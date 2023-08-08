﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;

namespace IoT.Device.AtModem.Http
{
    /// <summary>
    /// Initializes a new instance of the HttpClient class for SIM800.
    /// </summary>
    public class Sim800HttpClient : HttpClient
    {
        private ManualResetEvent _httpActionArrived = new ManualResetEvent(false);
        private bool _waitingForResponse = false;
        private HttpActionResult _httpActionResult = null;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim800HttpClient"/> class.
        /// </summary>
        /// <param name="modem">A valid <see cref="ModemBase"/>.</param>
        public Sim800HttpClient(ModemBase modem) : base(modem)
        {
            Initilaize();
        }
        internal override HttpResponseMessage SendInternal(HttpRequestMessage request)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

            try
            {
                AtResponse response;
                long contentLength = 0;
                int index;
                int retry = 5;

                // Check the method used, only few supported.
                int method = -1;
                if (request.Method == HttpMethod.Get)
                {
                    method = 0;
                }
                else if (request.Method == HttpMethod.Post)
                {
                    method = 1;
                }
                else if (request.Method == HttpMethod.Head)
                {
                    method = 2;
                }
                else if (request.Method == HttpMethod.Delete)
                {
                    method = 3;
                }

                if (method == -1)
                {
                    return result;
                }

            Retry:
                response = Modem.Channel.SendCommand("AT+HTTPINIT");
                if (!response.Success)
                {
                    // Give it another try
                    if (retry-- > 0)
                    {
                        Thread.Sleep(500);
                        goto Retry;
                    }

                    return result;
                }

                // Set the content type
                if (request.Content != null)
                {
                    response = Modem.Channel.SendCommand($"AT+HTTPPARA=\"CONTENT\",\"{request.Content.Headers.ContentType}\"");
                    if (!response.Success)
                    {
                        return result;
                    }
                }

                // TODO: Set the user agent
                Modem.Channel.SendCommand($"AT+HTTPPARA=\"UA\",\"nanoFramework\"");

                // custom headers
                if (request.Headers != null && request.Headers._headerStore.Count > 0)
                {
                    // build string with headers
                    StringBuilder customHeaders = new(request.Headers._headerStore.ToString());

                    // escape the '"' char in the header fields 
                    customHeaders.Replace(@"""", @"\""");

                    // set custom headers removing last '|' separator
                    Modem.Channel.SendCommand($"AT+USERDATA={customHeaders.ToString().Substring(0, customHeaders.Length - 4)}");
                }
                else
                {
                    // clear user headers, just in case
                    Modem.Channel.SendCommand($"AT+USERDATA= ");
                }

                if (!response.Success)
                {
                    return result;
                }

                // Set SSL and don't check the answer as some models do not support.
                Modem.Channel.SendCommand($"AT+HTTPSSL={(request.RequestUri.Scheme == "https" ? 1 : 0)}");               

                // Set the URL
                response = Modem.Channel.SendCommand($"AT+HTTPPARA=\"URL\",\"{request.RequestUri.AbsoluteUri}\"");
                if (!response.Success)
                {
                    return result;
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
                    response = Modem.Channel.SendCommandReadSingleLine($"AT+HTTPDATA={contentLength},{(timeout > 120000 ? 120000 : timeout)}", "DOWNLOAD");
                    if (!response.Success)
                    {
                        return result;
                    }

                    // write data in chunks of 64 bytes because of UART buffer size
                    index = 0;
                    const int chunkSize = 64;
                    int bytesToSend;
                    SpanByte toSend = request.Content.ReadAsByteArray();
                    while (index < contentLength)
                    {
                        bytesToSend = (int)(contentLength - index);
                        if (bytesToSend > chunkSize)
                        {
                            bytesToSend = chunkSize;
                        }

                        Modem.Channel.SendBytesWithoutAck(toSend.Slice(index, bytesToSend).ToArray());
                        index += bytesToSend;
                    }
                }

                _httpActionArrived.Reset();
                _waitingForResponse = true;
                _httpActionResult = null;
                Modem.GenericEvent += ModemGenericEvent;

                // Send the actual action.
                response = Modem.Channel.SendCommand($"AT+HTTPACTION={method}");

                // this is the timeout for the loop to complete, it has to depend on the content length, with a minimum of 15 seconds
                int milisecondsTimeout = (int)(contentLength > 5000 ? contentLength * 1.6 : 15000);

                // Read if we have a response
                if (_waitingForResponse)
                {
                    _httpActionArrived.WaitOne(milisecondsTimeout, true);
                }

                // Stop the internal thread on AT Channel
                Modem.Channel.Stop();

                // Check if we have a response
                Modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes("AT+HTTPREAD\r\n"));

                // We will read a first line then a second one and find the +HTTPREAD: lengh, then we will read the data
                Modem.Channel.ReadLine();
                var line = Modem.Channel.ReadLine();
                if (!line.Contains("+HTTPREAD:"))
                {
                    return result;
                }

                line = line.Substring(11);
                var length = int.Parse(line);
                // Read by chunk of 64 bytes
                byte[] data = new byte[length];
                Thread.Sleep(20);
                index = 0;
                while (index < length)
                {
                    var chunk = Modem.Channel.ReadRawBytes(Math.Min(64, length - index));
                    chunk.CopyTo(data, index);
                    index += chunk.Length;
                }

                // Restart the internal thread on AT Channel
                Modem.Channel.Clear();
                Modem.Channel.Start();

                result = new HttpResponseMessage(_httpActionResult == null ? HttpStatusCode.OK : (HttpStatusCode)_httpActionResult.StatusCode);
                result.Content = new ByteArrayContent(data);
            }
            catch (Exception)
            {


            }
            finally
            {
                Modem.GenericEvent -= ModemGenericEvent;
                Modem.Channel.SendCommand("AT+HTTPTERM");
            }

            return result;
        }

        internal void Initilaize()
        {
            if (!Modem.Network.IsConnected)
            {
                throw new InvalidOperationException("Modem is not connected to network.");
            }

            // We are using the CID 1 for HTTP
            var response = Modem.Channel.SendCommand("AT+HTTPPARA=\"CID\",1");
            if (!response.Success)
            {
                // Give it another try
                Thread.Sleep(200);
                Modem.Channel.SendCommand("AT+HTTPPARA=\"CID\",1");
            }

            response = Modem.Channel.SendCommand("AT+HTTPPARA=\"REDIR\",1");
            if (!response.Success)
            {
                // Give it another try
                Thread.Sleep(200);
                Modem.Channel.SendCommand("AT+HTTPPARA=\"REDIR\",1");
            }
        }

        private void ModemGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message.Contains("+HTTPACTION:"))
            {
                string response = e.Message.Substring(13);
                string[] responseParts = response.Split(',');
                HttpMethod httpMethod;
                if (responseParts[0] == "0")
                {
                    httpMethod = HttpMethod.Get;
                }
                else if (responseParts[0] == "1")
                {
                    httpMethod = HttpMethod.Post;
                }
                else if (responseParts[0] == "2")
                {
                    httpMethod = HttpMethod.Head;
                }
                else
                {
                    httpMethod = HttpMethod.Delete;
                }

                _httpActionResult = new HttpActionResult(httpMethod, int.Parse(responseParts[1]), int.Parse(responseParts[2]));
                _httpActionArrived.Set();
                _waitingForResponse = false;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Modem.GenericEvent -= ModemGenericEvent;
            Modem.Channel.SendCommand("AT+HTTPTERM");
        }
    }
}
