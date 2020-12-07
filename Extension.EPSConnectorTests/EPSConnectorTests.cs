using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Linq;
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
            var paymentInfo = responseMapper.MapPaymentResponse(xmlResponseString, "testconnector");
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

        [TestMethod]
        public void ParseDeviceResponseXML()
        {
            try
            {
                string XmlMessage = "<?xml version=\"1.0\"?><DeviceResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"Output\" ApplicationSender=\"GSPOS\" RequestID=\"39\" OverallResult=\"Success\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"><Output OutDeviceTarget=\"PrinterReceipt\" OutResult=\"Success\" /></DeviceResponse>";
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(XmlMessage);

                XmlNodeList deviceRequestNode = xmlDoc.GetElementsByTagName("DeviceResponse");
                var requestType = deviceRequestNode[0].Attributes["RequestType"].Value;

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void ParseDeviceRequestXML_Returns_IsPrinterReceipt()
        {
            try
            {
                string XmlMessage = "<?xml version=\"1.0\"?><DeviceRequest xmlns=\"http://www.w3.org/2001/XMLSchema-instance\" RequestType=\"Output\" ApplicationSender=\"GSPOS\" RequestID=\"39\"> <Output OutDeviceTarget=\"PrinterReceipt\" Immediate=\"true\"><TextLine>FIS_TYPE = CardholderReceipt </TextLine><TextLine> APPLICATION_IDENTIFIER = A00000031010 </TextLine ><TextLine> BATCH_ID = 004 </TextLine ><TextLine> ANOTHER_PROPERTY = 123456 </TextLine></Output ></DeviceRequest> ";
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(XmlMessage);

                XmlNodeList deviceRequestNode = xmlDoc.GetElementsByTagName("DeviceRequest");
                var requestType = deviceRequestNode[0].Attributes["RequestType"].Value;

                XmlNodeList outputNode = xmlDoc.GetElementsByTagName("Output");
                var OutDeviceTarget = outputNode[0].Attributes["OutDeviceTarget"].Value;

                var Found = outputNode[0].InnerText.ToLower().Contains("cardholderreceipt");

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        [TestMethod]
        public void ReadConnectorConfig()
        {

            var helper = new Helper();

            var config = helper.ReadConnectorConfiguration();

            //assert
            //Assert.IsNotNull(config.EndPointIp);
            //Assert.IsNotNull(config.OutgoingPort);
            //Assert.IsNotNull(config.IncomingPort);
            //Assert.IsNotNull(config.IncomingPortTimeout);

        }

        [TestMethod]
        public void EPSConnector_SendRequest()
        {
            var epsConnector = new EPSConnector();
            var results = epsConnector.SendRequestTcp("send this message");

            Assert.IsNotNull(results);
        }

    }
}