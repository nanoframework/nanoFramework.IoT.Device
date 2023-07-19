namespace HeboTech.ATLib.Events
{
    public class MissedCallEventArgs
    {
        public MissedCallEventArgs(string time, string phoneNumber)
        {
            Time = time;
            PhoneNumber = phoneNumber;
        }

        public string Time { get; }
        public string PhoneNumber { get; }

        public static MissedCallEventArgs CreateFromResponse(string response)
        {
#if NETSTANDARD2_0
            string[] split = response.Split(new char[] { ' ' }, 3);
#elif NETSTANDARD2_1_OR_GREATER
            string[] split = response.Split(' ', 3);
#endif
            return new MissedCallEventArgs(split[1], split[2]);
        }
    }
}
