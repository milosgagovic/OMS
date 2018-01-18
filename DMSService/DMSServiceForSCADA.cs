using DMSCommon.Model;
using DMSContract;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSService
{
    public class DMSServiceForSCADA : IDMSToSCADAContract
    {
        public void ChangeOnSCADA(string mrID, string state)
        {
            Publisher publisher = new Publisher();
            SCADAUpdateModel update = new SCADAUpdateModel(mrID, state);
            publisher.PublishUpdate(update);
        }
    }
}
