using CommunicationEngineContract;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicEngineTest
{
    [TestFixture]
    public class CommunicEngineProxyTest
    {
        private CommunicEngineProxy clientUnderTest;
        [OneTimeSetUp]
        public void SetupTest()
        {
            clientUnderTest = new CommunicEngineProxy();
            clientUnderTest.Factory = Substitute.For<ICommunicationEngineContract>();
            clientUnderTest.Factory.ReceiveValue().Returns(true);
        }
        [Test]
        public void ReceiveValueOk()
        {
            bool result = clientUnderTest.Factory.ReceiveValue();
            Assert.IsTrue(result);
        }

        [Test]
        public void SendCommandeException()
        {
            Assert.DoesNotThrow(() => clientUnderTest.Factory.SendCommand());
        }
    }
}
