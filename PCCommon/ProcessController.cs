
namespace PCCommon
{
    /// <summary>
    /// Generic description of Process Controller (communication endoint)
    /// </summary>
    public class ProcessController
    {
        // ideja je da se ovde mogu dodati jos neki propertiji, 
        // ili klase izvedene iz ove, koje ce odredjivati da li pravimo
        // udp, tcp kanal...ili mozda napraviti zasebnu kanal klasu, to nekad kasnije razmisljati

        public int DeviceAddress { get; set; }

        // unique name  
        public string Name { get; set; }

        // to do: use this in future implementations...
        // the time in which the message must be sent
        // public int Timeout { get; set; }

        public string HostName { get; set; }

        public short HostPort { get; set; }
    }
}
