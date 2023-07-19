using System.Text.RegularExpressions;

namespace HeboTech.ATLib.Events
{
    public class UssdResponseEventArgs
    {
        public UssdResponseEventArgs(int status, string response, int codingScheme)
        {
            Status = status;
            Response = response;
            CodingScheme = codingScheme;
        }

        public int Status { get; }
        public string Response { get; }
        public int CodingScheme { get; }

        public static UssdResponseEventArgs CreateFromResponse(string response)
        {
            var match = Regex.Match(response, @"\+CUSD:\s(?<status>\d),""(?<message>(?s).*)"",(?<codingScheme>\d+)");
            if (match.Success)
            {
                int status = int.Parse(match.Groups["status"].Value);
                string message = match.Groups["message"].Value;
                int codingScheme = int.Parse(match.Groups["codingScheme"].Value);
                return new UssdResponseEventArgs(status, message, codingScheme);
            }
            return default;
        }
    }
}
