using CommunicationEngineContract;
using OMSSCADACommon;
using SCADA.ClientHandler;
using SCADA.CommAcqEngine;
using SCADA.RealtimeDatabase;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA
{
    public class Program
    {
        private static InstanceContext instanceContext;
        public static string commEngineAdress = "net.tcp://localhost:4100/CommunEngine";
        public static InstanceContext InstanceContext
        {
            get
            {
                return instanceContext;
            }

            set
            {
                instanceContext = value;
            }
        }
        static void Main(string[] args)
        {
            InstanceContext = new InstanceContext(new SCADACommuncEngineService());

            Digital d1 = new Digital()
            {
                RtuName = "RTU-1",
                Name = "MEAS_D_1",

                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { RealtimeDatabase.Catalogs.States.CLOSED, RealtimeDatabase.Catalogs.States.OPENED },
                Command = CommandTypes.OPEN,
                State = RealtimeDatabase.Catalogs.States.CLOSED,
                Address = 1000
            };

            Digital d2 = new Digital()
            {
                RtuName = "RTU-1",
                Name = "MEAS_D_2",

                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { RealtimeDatabase.Catalogs.States.CLOSED, RealtimeDatabase.Catalogs.States.OPENED },
                Command = CommandTypes.OPEN,
                State = RealtimeDatabase.Catalogs.States.CLOSED,
                Address = 1001
            };

            Digital d3 = new Digital()
            {
                RtuName = "RTU-1",
                Name = "MEAS_D_3",

                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { RealtimeDatabase.Catalogs.States.CLOSED, RealtimeDatabase.Catalogs.States.OPENED },
                Command = CommandTypes.OPEN,
                State = RealtimeDatabase.Catalogs.States.CLOSED,
                Address = 1002
            };


            //Digital d4 = new Digital()
            //{
            //    RtuName = "RTU-1",
            //    Name = "TEST1",

            //    Class = DigitalDeviceClasses.SWITCH,
            //    ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
            //    ValidStates = { RealtimeDatabase.Catalogs.States.CLOSED, RealtimeDatabase.Catalogs.States.OPENED, RealtimeDatabase.Catalogs.States.UNKNOWN },
            //    Command = CommandTypes.OPEN,
            //    State = RealtimeDatabase.Catalogs.States.CLOSED,
            //    Address = 1003
            //};

            Digital d4 = new Digital()
            {
                RtuName = "RTU-1",
                Name = "TEST1",

                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { RealtimeDatabase.Catalogs.States.CLOSED, RealtimeDatabase.Catalogs.States.OPENED },
                Command = CommandTypes.OPEN,
                State = RealtimeDatabase.Catalogs.States.CLOSED,
                Address = 1003
            };

            Digital d5 = new Digital()
            {
                RtuName = "RTU-1",
                Name = "TEST2",

                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { RealtimeDatabase.Catalogs.States.CLOSED, RealtimeDatabase.Catalogs.States.OPENED },
                Command = CommandTypes.OPEN,
                State = RealtimeDatabase.Catalogs.States.CLOSED,
                Address = 1004
            };


            DBContext context = new DBContext();
            context.AddProcessVariable(d1);
            context.AddProcessVariable(d2);
            context.AddProcessVariable(d3);

            // test
            //context.AddProcessVariable(d4);
            //context.AddProcessVariable(d5);

            Console.WriteLine("Setting PCCommEngine");
            PCCommunicationEngine PCCommEng = new PCCommunicationEngine();
            PCCommEng.Configure(); // mozda parametar da bude adresa datoteka...     

            Thread reqConsumer = new Thread(PCCommEng.StartProcessing);

            Console.WriteLine("Setting AcqEngine");
            ACQEngine AcqEngine = new ACQEngine();

            Thread reqProducer = new Thread(AcqEngine.StartAcquisition);
            Thread answConsumer = new Thread(AcqEngine.ProcessPCAnwers);

            reqConsumer.Start();
            reqProducer.Start();
            answConsumer.Start();


            try
            {
                Console.WriteLine("\n....");
                SCADAService ss = new SCADAService();
                ss.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("SCADA service failed.");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Press <Enter> to stop the service.");
            Console.ReadKey();
        }
    }
}
