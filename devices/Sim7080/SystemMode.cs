namespace IoT.Device.Sim7080
{
    public enum SystemMode
    {
        NoService = 0,

        /// <summary>
        /// Global System for Mobile Communications (GSM)
        /// </summary>
        GSM = 1,

        /// <summary>
        /// Enhanced General Packet Radio Service (EGPRS)
        /// </summary>
        EGPRS = 3,

        /// <summary>
        /// Long Term Evolution (LTE) enhanced Machine Type Communication
        /// </summary>
        LTE_M1 = 7,

        /// <summary>
        /// Long Term Evolution (LTE) NarrowBand 
        /// </summary>
        LTE_NB = 9
    }
}
