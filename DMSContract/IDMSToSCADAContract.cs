using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DMSContract
{
    [ServiceContract]
    public interface IDMSToSCADAContract
    {
        [OperationContract]
        void ChangeOnSCADA(string mrID, string state);
    }
}
