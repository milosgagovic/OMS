using System;

namespace OMSSCADACommon
{
    public abstract class Command
    {
        public IReceiver Receiver { get;  set; }

        public string Id { get; set; }

        public abstract void Execute();
    }
}
