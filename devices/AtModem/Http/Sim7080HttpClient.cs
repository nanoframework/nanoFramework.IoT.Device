// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Net.Http;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;

namespace IoT.Device.AtModem.Http
{
    internal class Sim7080HttpClient : HttpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sim800HttpClient"/> class.
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

                // TODO: port
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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Modem.GenericEvent -= ModemGenericEvent;
            Modem.Channel.SendCommand("AT+SHDISC");
        }
    }
}
