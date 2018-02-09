

namespace SCADA.RealtimeDatabase.Model
{
    public class AnalogIn : ProcessVariable
    {
        public AnalogIn()
        {
            this.Type = VariableTypes.ANALOGIN;
        }

        public float Value { get; set; }
    }
}
