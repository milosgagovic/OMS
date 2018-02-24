
namespace PCCommon
{
    /// <summary>
    /// Generic description of Process Controller (communication endoint)
    /// </summary>
    public class ProcessController
    {
        public int DeviceAddress { get; set; }

        // unique name  
        public string Name { get; set; }

        //public int AcqPeriod { get; set; }

        public string HostName { get; set; }

        public short HostPort { get; set; }
    }
}
