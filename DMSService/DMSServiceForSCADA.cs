using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using OMSSCADACommon;
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
        public void ChangeOnSCADA(string mrID, OMSSCADACommon.States state)
        {
            ModelGdaDMS gda = new ModelGdaDMS();
            List<ResourceDescription> rdl = gda.GetExtentValuesExtended(ModelCode.DISCRETE);
            ResourceDescription rd = rdl.Where(r => r.GetProperty(ModelCode.IDOBJ_MRID).AsString() == mrID).FirstOrDefault();

            long res = rd.GetProperty(ModelCode.MEASUREMENT_PSR).AsLong();

            // ovde pozvati algoritam za energizaciju/deenergizaciju koji vraca ovu listu promena (GID, true/false)

            List<SCADAUpdateModel> energizedList = new List<SCADAUpdateModel>();

            bool energized = true;

            SCADAUpdateModel update = new SCADAUpdateModel(res, energized) { State = state };
            energizedList.Add(update);

            Publisher publisher = new Publisher();
            
            publisher.PublishUpdate(energizedList);
        }
    }
}
