namespace HeboTech.ATLib.Events
{
    public class ErrorEventArgs
    {
        public ErrorEventArgs(string error)
        {
            Error = error;
        }

        public string Error { get; }
    }
}
