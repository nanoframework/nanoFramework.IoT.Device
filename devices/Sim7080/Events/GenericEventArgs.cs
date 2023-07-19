namespace HeboTech.ATLib.Events
{
    public class GenericEventArgs
    {
        public GenericEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
