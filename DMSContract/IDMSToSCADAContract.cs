using OMSSCADACommon;
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
        void ChangeOnSCADADigital(string mrID, States state);

        [OperationContract]
        void ChangeOnSCADAAnalog(string mrID, float value);
    }
}
