using System;
using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class CallEndedEventArgs
    {
        public CallEndedEventArgs(TimeSpan duration)
        {
            Duration = duration;
        }

        public TimeSpan Duration { get; }

        public static CallEndedEventArgs CreateFromResponse(string response)
        {
            var match = Regex.Match(response, @"VOICE CALL: END: (?<duration>\d+)");
            if (match.Success)
            {
                int durationSeconds = int.Parse(match.Groups["duration"].Value);
                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                return new CallEndedEventArgs(duration);
            }
            return default;
        }
    }
}
