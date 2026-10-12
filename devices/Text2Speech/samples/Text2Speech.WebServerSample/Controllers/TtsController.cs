// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Iot.Device.Text2Speech.Samples;
using nanoFramework.WebServer;

namespace Iot.Device.Text2Speech.WebServerSample
{
    /// <summary>
    /// Handles the Text2Speech browser interface and WAV file operations.
    /// </summary>
    public sealed class TtsController
    {
        private const string StorageRoot = "I:\\";
        private const string FilePrefix = "text2speech-";
        private const int MaximumRequestLength = 8192;
        private const int MaximumTextLength = 1024;

        private static readonly object OperationLock = new object();
        private static readonly TtsSegmenter EnglishSegmenter =
            new TtsSegmenter(EnglishTtsLanguage.Instance);

        private static readonly TtsSegmenter FrenchSegmenter =
            new TtsSegmenter(new FrenchTtsLanguage());

        private static readonly TtsSynthesizer EnglishSmoothVoice =
            new TtsSynthesizer(
                EnglishTtsLanguage.Instance,
                new VoiceOptions(
                    pitchShift: -1,
                    speedPercent: 90,
                    formantScalePercent: 98,
                    intonationPercent: 140,
                    fricativeNoisePercent: 80,
                    transitionSmoothingPercent: 180));

        private static readonly TtsSynthesizer EnglishFastBrightVoice =
            new TtsSynthesizer(
                EnglishTtsLanguage.Instance,
                new VoiceOptions(
                    pitchShift: 3,
                    speedPercent: 125,
                    formantScalePercent: 108,
                    intonationPercent: 160,
                    fricativeNoisePercent: 90,
                    transitionSmoothingPercent: 130));

        private static readonly TtsSynthesizer EnglishDeepVoice =
            new TtsSynthesizer(
                EnglishTtsLanguage.Instance,
                new VoiceOptions(
                    pitchShift: -4,
                    speedPercent: 100,
                    formantScalePercent: 90,
                    intonationPercent: 80,
                    fricativeNoisePercent: 110,
                    transitionSmoothingPercent: 120));

        private static readonly TtsSynthesizer FrenchSmoothVoice =
            new TtsSynthesizer(
                new FrenchTtsLanguage(),
                new VoiceOptions(
                    pitchShift: -1,
                    speedPercent: 90,
                    formantScalePercent: 98,
                    intonationPercent: 140,
                    fricativeNoisePercent: 80,
                    transitionSmoothingPercent: 180));

        private static readonly TtsSynthesizer FrenchFastBrightVoice =
            new TtsSynthesizer(
                new FrenchTtsLanguage(),
                new VoiceOptions(
                    pitchShift: 3,
                    speedPercent: 125,
                    formantScalePercent: 108,
                    intonationPercent: 160,
                    fricativeNoisePercent: 90,
                    transitionSmoothingPercent: 130));

        private static readonly TtsSynthesizer FrenchDeepVoice =
            new TtsSynthesizer(
                new FrenchTtsLanguage(),
                new VoiceOptions(
                    pitchShift: -4,
                    speedPercent: 100,
                    formantScalePercent: 90,
                    intonationPercent: 80,
                    fricativeNoisePercent: 110,
                    transitionSmoothingPercent: 120));

        private static readonly string PageHtml =
            "<!doctype html><html><head><meta charset='utf-8'>"
            + "<meta name='viewport' content='width=device-width,initial-scale=1'>"
            + "<title>Text2Speech</title><style>"
            + "body{font-family:sans-serif;max-width:760px;margin:auto;padding:20px}"
            + "textarea,select,button{font-size:1rem;margin:6px 0;padding:8px}"
            + "textarea{width:100%;box-sizing:border-box}label{display:block}"
            + "table{border-collapse:collapse;width:100%}td,th{padding:7px;border-bottom:1px solid #ccc}"
            + ".inline{display:inline}.status{padding:10px;background:#eee;min-height:1.2em}"
            + "</style></head><body><h1>Text2Speech</h1>"
            + "<form id='speech'><label>Text</label>"
            + "<textarea name='text' maxlength='1024' rows='8' required>Hello from Text2Speech.</textarea>"
            + "<p>Text can contain up to 1,024 characters. Long text is split between sentences or words "
            + "into synthesis rounds of at most 96 characters. A short pause can occur between rounds, "
            + "and saving creates one WAV file per round.</p>"
            + "<label>Language <select name='language'>"
            + "<option value='english'>English</option>"
            + "<option value='french'>French</option></select></label>"
            + "<label>Voice <select name='voice'>"
            + "<option value='smooth'>Smooth</option>"
            + "<option value='fast-bright'>Fast bright</option>"
            + "<option value='deep'>Deep</option></select></label>"
            + "<label>Playback <select name='mode'>"
            + "<option value='buffered'>Playback</option>"
            + "<option value='streaming'>Live stream</option></select></label>"
            + "<label><input type='checkbox' name='save' value='true' checked> Save WAV file</label>"
            + "<button type='submit'>Speak</button></form>"
            + "<h2>Sound</h2><form id='sound'><label>Volume <output id='volumeValue'>"
            + Esp32S3BoxLiteWavPlayer.DefaultVolumePercent.ToString()
            + "%</output><input id='volume' type='range' name='volume' min='0' max='100' value='"
            + Esp32S3BoxLiteWavPlayer.DefaultVolumePercent.ToString()
            + "' oninput=\"document.getElementById('volumeValue').textContent=this.value+'%'\"></label>"
            + "<label><input type='checkbox' name='muted' value='true'> Mute output</label>"
            + "<button type='submit'>Apply sound settings</button></form>"
            + "<p id='status' class='status'>Ready.</p><h2>Saved WAV files</h2>"
            + "<div id='files'>Loading...</div><script>"
            + "async function loadFiles(){let r=await fetch('/files');document.getElementById('files').innerHTML=await r.text();}"
            + "document.getElementById('speech').onsubmit=async function(e){e.preventDefault();"
            + "let s=document.getElementById('status');s.textContent='Processing...';"
            + "try{let r=await fetch('/speak',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},"
            + "body:new URLSearchParams(new FormData(this))});s.textContent=await r.text();await loadFiles();}"
            + "catch(x){s.textContent='Request failed: '+x;}};"
            + "document.getElementById('sound').onsubmit=async function(e){e.preventDefault();"
            + "let s=document.getElementById('status');s.textContent='Applying sound settings...';"
            + "try{let r=await fetch('/sound',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},"
            + "body:new URLSearchParams(new FormData(this))});s.textContent=await r.text();}"
            + "catch(x){s.textContent='Sound request failed: '+x;}};"
            + "async function playFile(f){let s=document.getElementById('status');s.textContent='Playing WAV...';"
            + "try{let r=await fetch('/play',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},"
            + "body:new URLSearchParams(new FormData(f))});s.textContent=await r.text();}"
            + "catch(x){s.textContent='Playback request failed: '+x;}return false;}"
            + "async function removeFile(f){if(!confirm('Delete this WAV file?'))return false;"
            + "let r=await fetch('/delete',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded'},"
            + "body:new URLSearchParams(new FormData(f))});document.getElementById('status').textContent=await r.text();"
            + "await loadFiles();return false;}loadFiles();</script></body></html>";

        private static Esp32S3BoxLiteWavPlayer _player;
        private static int _fileCounter = 1;

        /// <summary>
        /// Initializes the controller with the shared audio player.
        /// </summary>
        /// <param name="player">The initialized audio player.</param>
        /// <exception cref="ArgumentNullException"><paramref name="player" /> is <see langword="null" />.</exception>
        internal static void Initialize(Esp32S3BoxLiteWavPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException();
            }

            Directory.GetFiles(StorageRoot);
            _player = player;
        }

        /// <summary>
        /// Returns the static browser application.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("")]
        [Method("GET")]
        public void Index(WebServerEventArgs e)
        {
            WriteResponse(e.Context.Response, HttpStatusCode.OK, "text/html", PageHtml);
        }

        /// <summary>
        /// Synthesizes and plays submitted text.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("speak")]
        [Method("POST")]
        public void Speak(WebServerEventArgs e)
        {
            try
            {
                UrlParameter[] form = ReadForm(e.Context.Request);
                string text = GetFormValue(form, "text");
                string languageName = GetFormValue(form, "language");
                string voiceName = GetFormValue(form, "voice");
                string mode = GetFormValue(form, "mode");
                bool save = GetFormValue(form, "save") == "true";

                if (string.IsNullOrEmpty(text) || text.Length > MaximumTextLength)
                {
                    WriteResponse(
                        e.Context.Response,
                        HttpStatusCode.BadRequest,
                        "text/plain",
                        "Text must contain 1 to 1,024 characters.");
                    return;
                }

                TtsSynthesizer synthesizer = GetSynthesizer(languageName, voiceName);
                TtsSegmenter segmenter = GetSegmenter(languageName);
                if (synthesizer == null
                    || segmenter == null
                    || (mode != "buffered" && mode != "streaming"))
                {
                    WriteResponse(
                        e.Context.Response,
                        HttpStatusCode.BadRequest,
                        "text/plain",
                        "Invalid language, voice, or playback mode.");
                    return;
                }

                string[] segments;
                try
                {
                    segments = segmenter.Split(text);
                }
                catch (ArgumentException ex)
                {
                    WriteResponse(
                        e.Context.Response,
                        HttpStatusCode.BadRequest,
                        "text/plain",
                        ex.Message);
                    return;
                }

                int samples = 0;
                int rounds = segments.Length;
                StringBuilder savedFiles = new StringBuilder();
                lock (OperationLock)
                {
                    if (mode == "streaming")
                    {
                        string[] wavPaths = save ? new string[segments.Length] : null;
                        if (wavPaths != null)
                        {
                            for (int i = 0; i < wavPaths.Length; i++)
                            {
                                wavPaths[i] = GetNextWavPath();
                                AppendSavedFile(savedFiles, wavPaths[i]);
                            }
                        }

                        samples = _player.SpeakStreaming(synthesizer, segments, wavPaths);
                    }
                    else
                    {
                        for (int i = 0; i < segments.Length; i++)
                        {
                            string wavPath = save ? GetNextWavPath() : null;
                            samples += _player.Speak(synthesizer, segments[i], wavPath);
                            if (wavPath != null)
                            {
                                AppendSavedFile(savedFiles, wavPath);
                            }
                        }
                    }
                }

                string message = "Completed " + rounds.ToString() + " synthesis round";
                if (rounds != 1)
                {
                    message += "s";
                }

                message += " in " + languageName + " using " + voiceName + " " + mode
                    + " playback; generated "
                    + samples.ToString() + " samples.";
                if (rounds > 1)
                {
                    message += mode == "streaming"
                        ? " Long text was split between words and streamed continuously between rounds."
                        : " Long text was split between words; a short pause can occur between rounds.";
                }

                if (save)
                {
                    message += " Saved " + rounds.ToString() + " WAV file";
                    if (rounds != 1)
                    {
                        message += "s";
                    }

                    message += ": " + savedFiles.ToString() + ".";
                }

                WriteResponse(e.Context.Response, HttpStatusCode.OK, "text/plain", message);
            }
            catch (Exception ex)
            {
                WriteFailure(e.Context.Response, "Speech request failed", ex);
            }
        }

        /// <summary>
        /// Updates the shared codec volume and mute state.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("sound")]
        [Method("POST")]
        public void Sound(WebServerEventArgs e)
        {
            try
            {
                UrlParameter[] form = ReadForm(e.Context.Request);
                byte volume;
                if (!TryParseVolume(GetFormValue(form, "volume"), out volume))
                {
                    WriteResponse(
                        e.Context.Response,
                        HttpStatusCode.BadRequest,
                        "text/plain",
                        "Volume must be an integer from 0 through 100.");
                    return;
                }

                bool muted = GetFormValue(form, "muted") == "true";
                lock (OperationLock)
                {
                    _player.Volume = volume;
                    _player.Muted = muted;
                }

                string message = muted
                    ? "Sound muted at " + volume.ToString() + "% volume."
                    : "Volume set to " + volume.ToString() + "%.";
                WriteResponse(
                    e.Context.Response,
                    HttpStatusCode.OK,
                    "text/plain",
                    message);
            }
            catch (Exception ex)
            {
                WriteFailure(e.Context.Response, "Sound settings failed", ex);
            }
        }

        /// <summary>
        /// Returns an HTML fragment containing saved WAV files.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("files")]
        [Method("GET")]
        public void Files(WebServerEventArgs e)
        {
            try
            {
                string html = "<table><tr><th>File</th><th>Actions</th></tr>";
                int count = 0;
                lock (OperationLock)
                {
                    string[] paths = Directory.GetFiles(StorageRoot);
                    for (int i = 0; i < paths.Length; i++)
                    {
                        string name = GetFileName(paths[i]);
                        if (!IsWavFileName(name))
                        {
                            continue;
                        }

                        string encodedName = HttpUtility.UrlEncode(name);
                        string htmlName = HtmlEncode(name);
                        html += "<tr><td>" + htmlName + "</td><td>"
                            + "<form class='inline' onsubmit='return playFile(this)'>"
                            + "<input type='hidden' name='name' value='" + htmlName + "'>"
                            + "<button type='submit'>Play</button></form> "
                            + "<a href='/download?name=" + encodedName + "'>Download</a> "
                            + "<form class='inline' onsubmit='return removeFile(this)'>"
                            + "<input type='hidden' name='name' value='" + htmlName + "'>"
                            + "<button type='submit'>Delete</button></form></td></tr>";
                        count++;
                    }
                }

                html += "</table>";
                if (count == 0)
                {
                    html = "<p>No WAV files are available.</p>";
                }

                WriteResponse(e.Context.Response, HttpStatusCode.OK, "text/html", html);
            }
            catch (Exception ex)
            {
                WriteFailure(e.Context.Response, "File listing failed", ex);
            }
        }

        /// <summary>
        /// Plays a saved WAV file through the initialized audio output.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("play")]
        [Method("POST")]
        public void Play(WebServerEventArgs e)
        {
            try
            {
                string name = GetFormValue(ReadForm(e.Context.Request), "name");
                if (!IsWavFileName(name))
                {
                    WriteResponse(
                        e.Context.Response,
                        HttpStatusCode.BadRequest,
                        "text/plain",
                        "Invalid WAV file name.");
                    return;
                }

                lock (OperationLock)
                {
                    string path = StorageRoot + name;
                    if (!File.Exists(path))
                    {
                        WriteResponse(
                            e.Context.Response,
                            HttpStatusCode.NotFound,
                            "text/plain",
                            "WAV file was not found.");
                        return;
                    }

                    _player.Play(path);
                }

                WriteResponse(
                    e.Context.Response,
                    HttpStatusCode.OK,
                    "text/plain",
                    "Played " + name + ".");
            }
            catch (ArgumentException)
            {
                WriteResponse(
                    e.Context.Response,
                    HttpStatusCode.BadRequest,
                    "text/plain",
                    "WAV must be canonical unsigned 8-bit mono PCM at 8 kHz.");
            }
            catch (IOException)
            {
                WriteResponse(
                    e.Context.Response,
                    HttpStatusCode.BadRequest,
                    "text/plain",
                    "WAV file is incomplete.");
            }
            catch (Exception ex)
            {
                WriteFailure(e.Context.Response, "File playback failed", ex);
            }
        }

        /// <summary>
        /// Downloads a saved WAV file.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("download")]
        [Method("GET")]
        public void Download(WebServerEventArgs e)
        {
            try
            {
                string name = GetQueryValue(e.Context.Request.RawUrl, "name");
                if (!IsWavFileName(name))
                {
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                    return;
                }

                string path = StorageRoot + name;
                lock (OperationLock)
                {
                    if (!File.Exists(path))
                    {
                        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                        return;
                    }

                    e.Context.Response.Headers.Add(
                        "Content-Disposition",
                        "attachment; filename=\"" + name + "\"");
                    WebServer.SendFileOverHTTP(e.Context.Response, path, "audio/wav");
                }
            }
            catch (Exception ex)
            {
                WriteFailure(e.Context.Response, "File download failed", ex);
            }
        }

        /// <summary>
        /// Deletes a saved WAV file.
        /// </summary>
        /// <param name="e">The web server request.</param>
        [Route("delete")]
        [Method("POST")]
        public void Delete(WebServerEventArgs e)
        {
            try
            {
                string name = GetFormValue(ReadForm(e.Context.Request), "name");
                if (!IsWavFileName(name))
                {
                    WriteResponse(
                        e.Context.Response,
                        HttpStatusCode.BadRequest,
                        "text/plain",
                        "Invalid WAV file name.");
                    return;
                }

                lock (OperationLock)
                {
                    string path = StorageRoot + name;
                    if (!File.Exists(path))
                    {
                        WriteResponse(
                            e.Context.Response,
                            HttpStatusCode.NotFound,
                            "text/plain",
                            "WAV file was not found.");
                        return;
                    }

                    File.Delete(path);
                }

                WriteResponse(
                    e.Context.Response,
                    HttpStatusCode.OK,
                    "text/plain",
                    "Deleted " + name + ".");
            }
            catch (Exception ex)
            {
                WriteFailure(e.Context.Response, "File deletion failed", ex);
            }
        }

        private static UrlParameter[] ReadForm(HttpListenerRequest request)
        {
            if (request.ContentLength64 <= 0 || request.ContentLength64 > MaximumRequestLength)
            {
                throw new ArgumentException();
            }

            int length = (int)request.ContentLength64;
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = request.InputStream.Read(buffer, offset, length - offset);
                if (read == 0)
                {
                    throw new IOException();
                }

                offset += read;
            }

            string body = new string(Encoding.UTF8.GetChars(buffer));
            return WebServer.DecodeParam("?" + body);
        }

        private static string GetQueryValue(string rawUrl, string name)
        {
            return GetFormValue(WebServer.DecodeParam(rawUrl), name);
        }

        private static string GetFormValue(UrlParameter[] parameters, string name)
        {
            if (parameters == null)
            {
                return null;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Name.ToLower() == name)
                {
                    return HttpUtility.UrlDecode(parameters[i].Value);
                }
            }

            return null;
        }

        private static TtsSegmenter GetSegmenter(string languageName)
        {
            if (languageName == "english")
            {
                return EnglishSegmenter;
            }

            return languageName == "french" ? FrenchSegmenter : null;
        }

        private static TtsSynthesizer GetSynthesizer(string languageName, string voiceName)
        {
            if (languageName == "english")
            {
                if (voiceName == "smooth")
                {
                    return EnglishSmoothVoice;
                }

                if (voiceName == "fast-bright")
                {
                    return EnglishFastBrightVoice;
                }

                return voiceName == "deep" ? EnglishDeepVoice : null;
            }

            if (languageName == "french")
            {
                if (voiceName == "smooth")
                {
                    return FrenchSmoothVoice;
                }

                if (voiceName == "fast-bright")
                {
                    return FrenchFastBrightVoice;
                }

                return voiceName == "deep" ? FrenchDeepVoice : null;
            }

            return null;
        }

        private static void AppendSavedFile(StringBuilder savedFiles, string path)
        {
            if (savedFiles.Length != 0)
            {
                savedFiles.Append(", ");
            }

            savedFiles.Append(GetFileName(path));
        }

        private static string GetNextWavPath()
        {
            string path;
            do
            {
                path = StorageRoot + FilePrefix + _fileCounter.ToString() + ".wav";
                _fileCounter++;
            }
            while (File.Exists(path));

            return path;
        }

        private static string GetFileName(string path)
        {
            int separator = path.LastIndexOf('\\');
            return separator >= 0 ? path.Substring(separator + 1) : path;
        }

        private static bool TryParseVolume(string value, out byte volume)
        {
            volume = 0;
            if (string.IsNullOrEmpty(value) || value.Length > 3)
            {
                return false;
            }

            int parsed = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char digit = value[i];
                if (digit < '0' || digit > '9')
                {
                    return false;
                }

                parsed = (parsed * 10) + digit - '0';
            }

            if (parsed > 100)
            {
                return false;
            }

            volume = (byte)parsed;
            return true;
        }

        private static bool IsWavFileName(string name)
        {
            if (string.IsNullOrEmpty(name)
                || name.Length > 64
                || !name.ToLower().EndsWith(".wav"))
            {
                return false;
            }

            for (int i = 0; i < name.Length; i++)
            {
                char value = name[i];
                bool allowed = (value >= 'a' && value <= 'z')
                    || (value >= 'A' && value <= 'Z')
                    || (value >= '0' && value <= '9')
                    || value == '-'
                    || value == '_'
                    || value == '.';
                if (!allowed)
                {
                    return false;
                }
            }

            return name.IndexOf("..") < 0;
        }

        private static string HtmlEncode(string value)
        {
            StringBuilder encoded = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '&':
                        encoded.Append("&amp;");
                        break;
                    case '<':
                        encoded.Append("&lt;");
                        break;
                    case '>':
                        encoded.Append("&gt;");
                        break;
                    case '"':
                        encoded.Append("&quot;");
                        break;
                    case '\'':
                        encoded.Append("&#39;");
                        break;
                    default:
                        encoded.Append(value[i]);
                        break;
                }
            }

            return encoded.ToString();
        }

        private static void WriteResponse(
            HttpListenerResponse response,
            HttpStatusCode statusCode,
            string contentType,
            string content)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = contentType + "; charset=utf-8";
            WebServer.OutputAsStream(response, content);
        }

        private static void WriteFailure(
            HttpListenerResponse response,
            string message,
            Exception exception)
        {
            Debug.WriteLine(message + ": " + exception.ToString());
            WriteResponse(
                response,
                HttpStatusCode.InternalServerError,
                "text/plain",
                message + ". See the device debug output.");
        }
    }
}
