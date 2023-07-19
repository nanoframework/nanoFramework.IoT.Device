using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class SmsReceivedEventArgs
    {
        public SmsReceivedEventArgs(string storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        public string Storage { get; }
        public int Index { get; }

        public static SmsReceivedEventArgs CreateFromResponse(string response)
        {
            var match = Regex.Match(response, @"\+CMTI:\s""(?<storage>[A-Z]+)"",(?<index>\d+)");
            if (match.Success)
            {
                string storage = match.Groups["storage"].Value;
                int index = int.Parse(match.Groups["index"].Value);
                return new SmsReceivedEventArgs(storage, index);
            }
            return default;
        }
    }
}
