using DispatcherApp.Model;
using FTN.Common;
using OMSSCADACommon;
using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransactionManager
{
    public class MappingEngineTransactionManager
    {
        private static MappingEngineTransactionManager instance;
        public static MappingEngineTransactionManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MappingEngineTransactionManager();
                return instance;
            }
        }
        public MappingEngineTransactionManager()
        {
        }

        // to do: add for analog!
        public List<ResourceDescription> MappResult(Response response)
        {

            List<ResourceDescription> retVal = new List<ResourceDescription>();
            ResourceDescription rd;
            foreach (ResponseVariable rv in response.Variables)
            {               
                rd = new ResourceDescription();
                DigitalVariable dv = rv as DigitalVariable;
                rd = new ResourceDescription();
                rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, dv.Id));

                Console.WriteLine("Variable = {0}, STATE = {1}", dv.Id, dv.State);

                if (dv.State.ToString() == "CLOSED")
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_NORMVAL, 0));
                }
                else
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_NORMVAL, 1));
                }
                retVal.Add(rd);
            }

            //vratiti rezultat korisniku
            return retVal;
        }

        public Command MappCommand(TypeOfSCADACommand typeOfCommand, string mrid, CommandTypes command, float value)
        {
            switch (typeOfCommand)
            {
                case TypeOfSCADACommand.ReadAll:
                    return new ReadAll();
                case TypeOfSCADACommand.WriteAnalog:
                    return new WriteSingleAnalog() { Id = mrid, Value = value };
                case TypeOfSCADACommand.WriteDigital:
                    return new WriteSingleDigital() { Id = mrid, CommandType = command };
            }

            return null;
            ////naapirati klijentsku komandu na scada komandu
            //ReadAll readAllCommand = new ReadAll();
            //return readAllCommand;
        }

        public MappingEngineTransactionManager getInstanceForTest()
        {
            return Instance;
        }
    }
}
