// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using nanoFramework.Runtime.Native;

namespace WiFiAP
{
    /// <summary>
    /// Webserver.
    /// </summary>
    internal class WebServer
    {
        private HttpListener _listener;
        private Thread _serverThread;

        // The tags used in the page
        private static class TagsInPAges
        {
            public const string Ssid = "{ssid}";

            public const string Message = "{message}";
        }

        private static string ReplaceMessage(string page, string message, string ssid)
        {
            string retpage;
            int index = page.IndexOf(TagsInPAges.Ssid);
            if (index >= 0)
            {
                retpage = page.Substring(0, index) + ssid + page.Substring(index + TagsInPAges.Ssid.Length);
            }
            else
            {
                retpage = page;
            }

            index = retpage.IndexOf(TagsInPAges.Message);
            if (index >= 0)
            {
                return retpage.Substring(0, index) + message + retpage.Substring(index + TagsInPAges.Message.Length);
            }

            return retpage;
        }

        private static void OutPutResponse(HttpListenerResponse response, string responseString)
        {
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
            OutPutByteResponse(response, System.Text.Encoding.UTF8.GetBytes(responseString));
        }

        private static void OutPutByteResponse(HttpListenerResponse response, byte[] responseBytes)
        {
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }

        private static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));
        }

        private static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new Hashtable();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');
                hash.Add(nameValue[0], nameValue[1]);
            }

            return hash;
        }

        private static string CreateMainPage(string message)
        {
            return "<!DOCTYPE html><html><body>" +
                    "<h1>NanoFramework</h1>" +
                    "<form method='POST'>" +
                    "<fieldset><legend>Wireless configuration</legend>" +
                    "Ssid:</br><input type='input' name='ssid' value='' ></br>" +
                    "Password:</br><input type='password' name='password' value='' >" +
                    "<br><br>" +
                    "<input type='submit' value='Save'>" +
                    "</fieldset>" +
                    "<b>" + message + "</b>" +
                    "</form></body></html>";
        }

        /// <summary>
        /// Starts the web server.
        /// </summary>
        public void Start()
        {
            if (_listener == null)
            {
                _listener = new HttpListener("http");
                _serverThread = new Thread(RunServer);
                _serverThread.Start();
            }
        }

        /// <summary>
        /// Stops the web server.
        /// </summary>
        public void Stop()
        {
            if (_listener != null)
            {
                _listener.Stop();
            }
        }

        private void RunServer()
        {
            _listener.Start();

            while (_listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    if (context != null)
                    {
                        ProcessRequest(context);
                    }
                }
                catch
                {
                    //// On purpose, we don't want the server to stop because a client gets disconnected
                }
            }

            _listener.Close();
            _listener = null;
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string responseString;
            string ssid = null;
            string password = null;
            bool isApSet = false;

            Debug.WriteLine($"reqest methode: {request.HttpMethod}");
            Debug.WriteLine($"reqest url: {request.RawUrl}");
            string[] url = request.RawUrl.Split('?');
            switch (request.HttpMethod)
            {
                case "GET":
                    if (url[0] == "/favicon.ico")
                    {
                        response.ContentType = "image/png";
                        byte[] responseBytes = Resources.GetBytes(Resources.BinaryResources.favicon);
                        OutPutByteResponse(response, responseBytes);
                    }
                    else
                    {
                        response.ContentType = "text/html";
                        if (url[0] == "/config")
                        {
                            responseString = ReplaceMessage(Resources.GetString(Resources.StringResources.config), string.Empty, string.Empty);
                        }
                        else
                        {
                            responseString = ReplaceMessage(Resources.GetString(Resources.StringResources.main), $"{DateTime.UtcNow.AddHours(8)}", string.Empty);
                        }

                        OutPutResponse(response, responseString);
                    }

                    break;

                case "POST":
                    if (url[0] == "/config")
                    {
                        // Pick up POST parameters from Input Stream
                        Hashtable hashPars = ParseParamsFromStream(request.InputStream);

                        ssid = (string)hashPars["ssid"];
                        password = (string)hashPars["password"];

                        Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");

                        string message = "<p>SSID can not be empty</p>";
                        if (ssid != null)
                        {
                            if (ssid.Length >= 1)
                            {
                                message = "<p>New settings saved.</p><p>Rebooting device to put into normal mode</p>";

                                // responseString = CreateMainPage(message);
                                isApSet = true;
                            }
                        }

                        responseString = ReplaceMessage(Resources.GetString(Resources.StringResources.config), message, ssid);
                    }
                    else
                    {
                        responseString = ReplaceMessage(Resources.GetString(Resources.StringResources.main), $"{DateTime.UtcNow.AddHours(8)}", string.Empty);
                    }

                    OutPutResponse(response, responseString);
                    break;
            }

            response.Close();

            if (isApSet && (!string.IsNullOrEmpty(ssid)) && (!string.IsNullOrEmpty(password)))
            {
                // Enable the Wireless station interface
                Wireless80211.Configure(ssid, password);

                // Disable the Soft AP
                WirelessAP.Disable();
                Thread.Sleep(200);
                Power.RebootDevice();
            }
        }
    }
}
