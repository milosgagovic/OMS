
namespace OMSSCADACommon
{
    public enum States
    {        
        CLOSED = 0,
        OPENED,
        UNKNOWN
    }

    public enum CrewResponse
    {
        ShortCircuit = 0,
        GroundFault,
        Overload,
    }
}
