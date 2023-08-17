// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;

namespace IoT.Device.AtModem.Http
{
    internal class Sim7080HttpClient : HttpClient
    {
        private const string RootCaFileName = "crt";

        // 1 is MQTT, so let's take 2 for HTTP
        private const int IndexSSL = 2;
        private ManualResetEvent _httpActionArrived = new ManualResetEvent(false);
        private HttpActionResult _httpActionResult = null;
        private X509Certificate _certAuth;
        private string _certName;

        /// <summary>
        /// Class with the result of a request.
        /// </summary>
        private class HttpActionResult
        {
            public HttpMethod Action { get; private set; }

            public int StatusCode { get; private set; }

            public int DataLenght { get; private set; }

            public HttpActionResult(HttpMethod action, int statusCode, int dataLenght)
            {
                Action = action;
                StatusCode = statusCode;
                DataLenght = dataLenght;
            }
        }

        /// <inheritdoc/>
        public override X509Certificate HttpsAuthentCert
        {
            get => _certAuth;
            set
            {
                _certAuth = value;
                if (_certAuth != null)
                {
                    // We need to setup the certificate in the native mode
                    // First store the certificate in the modem
                    int hash = HashHelper.ComputeHash(_certAuth.GetRawCertData());
                    _certName = RootCaFileName + hash + "." + RootCaFileName;
                    Modem.FileStorage.WriteFile(_certName, _certAuth.GetRawCertData());

                    // Open the file system to convert
                    Modem.Channel.SendCommand("AT+CFSINIT");

                    // Convert the certificate
                    Modem.Channel.SendCommand($"AT+CFSCONVERT=\"CONVERT\",2,\"{_certName}\"");

                    // And finalize it
                    Modem.Channel.SendCommand("AT+CFSTERM");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7080HttpClient"/> class.
        /// </summary>
        /// <param name="modem">A valid <see cref="ModemBase"/>.</param>
        public Sim7080HttpClient(ModemBase modem) : base(modem)
        {
            if (!Modem.Network.IsConnected)
            {
                throw new InvalidOperationException("Modem is not connected to network.");
            }
        }

        internal override HttpResponseMessage SendInternal(HttpRequestMessage request)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

            try
            {
                AtResponse response;
                long contentLength = 0;
                string line;
                int index;
                int retries = 5;

            Retry:
                if (request.RequestUri.Scheme == "https")
                {
                    // This is to read the current configuration, we don't need it now.
                    ////Modem.Channel.SendCommand($"AT+CSSLCFG=\"\"CTXINDEX\",{IndexSSL}");

                    switch (SslProtocols)
                    {
                        case System.Net.Security.SslProtocols.None:
                            break;
                        case System.Net.Security.SslProtocols.Tls:
                            Modem.Channel.SendCommand($"AT+CSSLCFG=\"SSLVERSION\",{IndexSSL},1");
                            break;
                        case System.Net.Security.SslProtocols.Tls11:
                            Modem.Channel.SendCommand($"AT+CSSLCFG=\"SSLVERSION\",{IndexSSL},2");
                            break;
                        case System.Net.Security.SslProtocols.Tls12:
                            Modem.Channel.SendCommand($"AT+CSSLCFG=\"SSLVERSION\",{IndexSSL},3");
                            break;
                        default:
                            break;
                    }

                    // If there is a certificate attached, we check it otherwise, we ignore (less secure)
                    if (_certAuth != null)
                    {
                        Modem.Channel.SendCommand($"AT+SHSSL={IndexSSL},\"{RootCaFileName}\"");
                    }
                    else
                    {
                        Modem.Channel.SendCommand($"AT+SHSSL={IndexSSL},\"\"");
                    }
                }

                response = Modem.Channel.SendCommand($"AT+SHCONF=\"URL\",\"{request.RequestUri.Scheme}://{request.RequestUri.Host}:{request.RequestUri.Port}\"");
                if (!response.Success)
                {
                    return result;
                }

                // Those need to be setup, setting them up with the max values
                Modem.Channel.SendCommand($"AT+SHCONF=\"BODYLEN\",4096");
                Modem.Channel.SendCommand($"AT+SHCONF=\"HEADERLEN\",350");

                response = Modem.Channel.SendCommand($"AT+SHCONN");
                if (!response.Success)
                {
                    if (retries-- > 0)
                    {
                        // Making sure we are in a good initial state
                        Modem.Channel.SendCommand("AT+SHDISC");
                        Thread.Sleep(1000);
                        goto Retry;
                    }

                    return result;
                }

                response = Modem.Channel.SendCommandReadSingleLine("AT+SHSTATE?", "+SHSTATE");
                if (!response.Success)
                {
                    return result;
                }

                if ((string)response.Intermediates[0] != "+SHSTATE: 1")
                {
                    return result;
                }

                // Clear HTTP header
                Modem.Channel.SendCommand("AT+SHCHEAD");

                if (request.Headers != null && request.Headers._headerStore.Count > 0)
                {
                    // Add all the headers
                    foreach (var header in request.Headers._headerStore.AllKeys)
                    {
                        Modem.Channel.SendCommand($"AT+SHAHEAD=\"{header}\",\"{request.Headers._headerStore[header]}\"");
                    }
                }

                // Set the content type
                if (request.Content != null)
                {
                    response = Modem.Channel.SendCommand($"AT+SHAHEAD=\"Content-Type\",\"{request.Content.Headers.ContentType}\"");
                    if (!response.Success)
                    {
                        return result;
                    }
                }

                // Let's say GET is default just in case
                int method = 1;
                if (request.Method == HttpMethod.Get)
                {
                    method = 1;
                }
                else if (request.Method == HttpMethod.Head)
                {
                    method = 2;
                }
                else if (request.Method == HttpMethod.Post)
                {
                    method = 3;
                }
                else if (request.Method == HttpMethod.Put)
                {
                    method = 4;
                }
                else if (request.Method == HttpMethod.Delete)
                {
                    method = 5;
                }
                else if (request.Method == HttpMethod.Connect)
                {
                    method = 6;
                }
                else if (request.Method == HttpMethod.Options)
                {
                    method = 7;
                }
                else if (request.Method == HttpMethod.Trace)
                {
                    method = 8;
                }
                else if (request.Method == HttpMethod.Patch)
                {
                    method = 9;
                }

                // send POST data
                // check method
                // check also if there is data in the Data field or in the request stream of the caller
                if (request.Method == HttpMethod.Post &&
                    request.Content != null)
                {
                    // We have some length in this case
                    request.Content.TryComputeLength(out contentLength);
                    int timeout = ((int)contentLength / 100) * 500;

                    // min allowed timeout is 1000
                    timeout = timeout < 1000 ? 2000 : timeout;

                    // We stop the thread reading continuously
                    Modem.Channel.Stop();

                    // We send a raw data the order to upload the data
                    Modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+SHBOD={contentLength},{(timeout > 120000 ? 120000 : timeout)}\r\n"));
#if DEBUG
                    Debug.WriteLine($"Out: AT+SHBOD={contentLength},{(timeout > 120000 ? 120000 : timeout)}");
#endif
                    // We read the response
                    line = Modem.Channel.ReadLine();
#if DEBUG
                    Debug.WriteLine($"In: {line}");
#endif
                    line = Modem.Channel.ReadLine(TimeSpan.FromMilliseconds(200));
#if DEBUG
                    Debug.WriteLine($"In: {line}");
#endif
                    if (!line.Contains(">"))
                    {
                        Modem.Channel.Clear();
                        Modem.Channel.Start();
                        return result;
                    }

                    // write data in chunks of 64 bytes because of UART buffer size
                    index = 0;
                    const int ChunkSize = 64;
                    int bytesToSend;
                    SpanByte toSend = request.Content.ReadAsByteArray();
                    while (index < contentLength)
                    {
                        bytesToSend = (int)(contentLength - index);
                        if (bytesToSend > ChunkSize)
                        {
                            bytesToSend = ChunkSize;
                        }

                        Modem.Channel.SendBytesWithoutAck(toSend.Slice(index, bytesToSend).ToArray());
                        index += bytesToSend;
                    }

                    // Send ctrl + z to validate the data
                    Modem.Channel.SendBytesWithoutAck(new byte[] { 0x1A });
                    Modem.Channel.Clear();
                    Modem.Channel.Start();
                }

                // Set the querry URL
                // URL = <scheme>://<host>(:<port>)/<path>
                // Do we have a last slash?
                string querryUrl = string.Empty;
                if (request.RequestUri.AbsoluteUri.LastIndexOf('/') > (request.RequestUri.AbsoluteUri.Length - request.RequestUri.Host.Length - request.RequestUri.Scheme.Length))
                {
                    querryUrl = request.RequestUri.AbsoluteUri.Substring(request.RequestUri.Host.Length + request.RequestUri.Scheme.Length);
                    querryUrl = querryUrl.Substring(querryUrl.IndexOf('/'));
                }

                Modem.GenericEvent += ModemGenericEvent;
                _httpActionArrived.Reset();

                response = Modem.Channel.SendCommand($"AT+SHREQ=\"{querryUrl}\",{method}");
                if (!response.Success)
                {
                    return result;
                }

                // this is the timeout for the loop to complete, it has to depend on the content length, with a minimum of 15 seconds
                int milisecondsTimeout = (int)(contentLength > 5000 ? contentLength * 1.6 : 15000);
                _httpActionArrived.WaitOne(milisecondsTimeout, true);

                if (_httpActionResult != null)
                {
                    // Setup the manual mode for the channel and then send the raw commands and read the raw elements
                    Modem.Channel.Stop();

                    Modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+SHREAD=0,{_httpActionResult.DataLenght}\r\n"));
#if DEBUG
                    Debug.WriteLine($"Out: AT+SHREAD=0,{_httpActionResult.DataLenght}");
#endif
                    int lengthRead = 0;
                    int lengthToRead = _httpActionResult.DataLenght;
                    int toRead = 0;
                    byte[] data = new byte[lengthToRead];
                    index = 0;
                    Thread.Sleep(20);

                    // Give it 2 minutes maximum
                    // TODO: maybe adjust all this a bit as a setting somewhere
                    CancellationTokenSource cts = new CancellationTokenSource(120000);
                    do
                    {
                        line = Modem.Channel.ReadLine();
#if DEBUG
                        Debug.WriteLine($"In: {line}");
#endif
                        if (line.Contains("+SHREAD: "))
                        {
                            toRead = int.Parse(line.Substring(9));

                            Thread.Sleep(20);

                            // Read chunks of 64 bytes
                            index = 0;
                            while (index < toRead)
                            {
                                var chunk = Modem.Channel.ReadRawBytes(Math.Min(64, toRead - index));
                                chunk.CopyTo(data, lengthRead + index);
                                index += chunk.Length;
                            }

                            lengthRead += index;
                        }
                    }
                    while ((lengthRead < lengthToRead) || cts.IsCancellationRequested);

                    // Restart everything
                    Modem.Channel.Clear();
                    Modem.Channel.Start();

                    result = new HttpResponseMessage(_httpActionResult == null ? HttpStatusCode.OK : (HttpStatusCode)_httpActionResult.StatusCode);
                    result.Content = new ByteArrayContent(data);
                }
            }
            catch (Exception ex)
            {
                result.Content = new StringContent(ex.Message);
            }
            finally
            {
                Modem.GenericEvent -= ModemGenericEvent;
                Modem.Channel.SendCommand("AT+SHDISC");
            }

            return result;
        }

        private void ModemGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message.StartsWith("+SHREQ:"))
            {
                string response = e.Message.Substring(7);
                string[] responseParts = response.Split(',');
                string methodTxt = responseParts[0].Trim('\"');
                HttpMethod httpMethod = new HttpMethod(methodTxt);

                if (responseParts.Length > 2)
                {
                    _httpActionResult = new HttpActionResult(httpMethod, int.Parse(responseParts[1]), int.Parse(responseParts[2]));
                }

                _httpActionArrived.Set();
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Modem.GenericEvent -= ModemGenericEvent;
            Modem.Channel.SendCommand("AT+SHDISC");
        }
    }
}
