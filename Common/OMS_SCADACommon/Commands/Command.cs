using System;

namespace OMS_SCADACommon
{
    public abstract class Command
    {
        public IReceiver Receiver { get;  set; }

        public string Id { get; set; }

        public abstract void Execute();
    }
}
