using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hardware.Extension.EPSPaymentConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.HardwareStation;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace Hardware.Extension.EPSPaymentConnector.Tests
{
    [TestClass()]
    public class EPSConnectorTests
    {

        [TestMethod()]
        public void ParseXMLDataTest()
        {
            //Arrange
            EPSConnector _connector = new EPSConnector();
            ResponseMapper responseMapper = new ResponseMapper();
            string xmlResponseString = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><CardServiceResponse xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\" RequestType = \"CardPayment\" WorkstationID = \"Dave's Workstation\" RequestID = \"19\" OverallResult = \"Success\"><Terminal TerminalID = \"02052317\" TerminalBatch = \"006\"/><Tender><TotalAmount> 9.000000 </TotalAmount><Authorisation AcquirerID = \"000000\" StartDate = \"0401\" ExpiryDate = \"1412\" TimeStamp = \"2012-07-25T15:02:59.000+01:00\" ActionCode = \"000\" ApprovalCode = \"011860\" AcquirerBatch = \"006\" PANprint = \"541333******0036\" Merchant = \"2100112192\" AuthorisationType = \"Online\" CaptureReference = \"0092\"/></Tender></CardServiceResponse >";

            //Act
            var paymentInfo = responseMapper.MapPaymentResponse(xmlResponseString);
            //Assert
            Assert.IsNotNull(paymentInfo);
        }

        [TestMethod()]
        public void SendStringRequest_ValidRequest_ThrowsNoError()
        {

            //Arrange
            EPSConnector _connector = new EPSConnector();
            RequestBuilder requestBuilder = new RequestBuilder();
            var requestXML = requestBuilder.BuildSampleCardRequestXML();

            //Act
            var response = _connector.SendRequestTcp(requestXML);

            //Assert
            Assert.IsNotNull(response);
        }

        [TestMethod()]
        public void ParseDeviceRequestMasterTest()
        {
            
                //Arrange
                string firstRequestXML = string.Empty;
                string secondRequestXML = string.Empty;
                string thirdRequestXML = string.Empty;
                string masterXmlAsString= System.IO.File.ReadAllText("DeviceRequestMaster.txt");

                //Act
                DeviceComsHandler deviceComsHandler = new DeviceComsHandler();
                deviceComsHandler.SplitDeviceRequestMessages(masterXmlAsString, out firstRequestXML, out secondRequestXML, out thirdRequestXML);
                
                //Assert
                Assert.IsTrue(firstRequestXML.Length > 0);
        }

        [TestMethod()]
        public void TestStringEncodeDecodeDuration()
        {

            //Arrange
            string firstRequestXML = string.Empty;
            string secondRequestXML = string.Empty;
            string thirdRequestXML = string.Empty;
            string masterXmlAsString = System.IO.File.ReadAllText("DeviceRequestMaster.txt");

            //Act
            DeviceComsHandler deviceComsHandler = new DeviceComsHandler();
            deviceComsHandler.SplitDeviceRequestMessages(masterXmlAsString, out firstRequestXML, out secondRequestXML, out thirdRequestXML);

            Logger.WriteLog("started operation");

            byte[] encodedMessage = Encoding.ASCII.GetBytes(firstRequestXML);
            Logger.WriteLog("finished encoding");

            var decodedMessage = Encoding.ASCII.GetString(encodedMessage);

            Logger.WriteLog("finished decoding");

            //Assert
            Assert.IsTrue(firstRequestXML.Length > 0);
        }

    }
}