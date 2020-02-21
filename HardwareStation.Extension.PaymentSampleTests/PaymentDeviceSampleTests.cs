using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contoso.Commerce.HardwareStation.PaymentSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.HardwareStation;
using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
using System.Xml;

namespace Contoso.Commerce.HardwareStation.PaymentSample.Tests
{
    [TestClass()]
    public class PaymentDeviceSampleTests
    {
        [TestMethod()]
        public void OpenPaymentTerminalDeviceRequestTest()
        {
            string _token = "sdfdsfsfdsfdsf1234566";
            string _deviceName = "myDevice001";
            SettingsInfo _settingInfo = new SettingsInfo
            {
                SignatureCaptureMinimumAmount=30,
                MinimumAmountAllowed=10,
                MaximumAmountAllowed=500,
                DebitCashbackLimit=20,
                Locale="en-GB",
                TerminalId="6687"
            };

            Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PeripheralConfiguration _peripheralConfiguration = new Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PeripheralConfiguration();
            Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.ExtensionTransaction _extensionTransaction = new Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.ExtensionTransaction();
            var _paymentDeviceSample = new PaymentDeviceSample();

            var paymentTerminalDeviceRequest = new OpenPaymentTerminalDeviceRequest(_token,_deviceName, _settingInfo, _peripheralConfiguration, _extensionTransaction);
            var response = _paymentDeviceSample.Execute(paymentTerminalDeviceRequest);

            Assert.IsNotNull(response);
        }

        [TestMethod()]
        public void BeginTransactionTest()
        {
            string _token = "sdfdsfsfdsfdsf1234566";
            string _paymentConnectorName = "PAYMENTCONNECTOR";
            string _merchantInfo = "MERCHANTINFO";
            string _invoiceNumber = "01234";
            

            Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.ExtensionTransaction _extensionTransaction = new Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.ExtensionTransaction();
            var _paymentDeviceSample = new PaymentDeviceSample();

            var paymentTerminalDeviceRequest = new BeginTransactionPaymentTerminalDeviceRequest(_token, _paymentConnectorName, _merchantInfo, _invoiceNumber,true, _extensionTransaction);
            var response = _paymentDeviceSample.Execute(paymentTerminalDeviceRequest);

            Assert.IsNotNull(response);
        }

        [TestMethod()]
        public void AuthorizePaymentTerminalDeviceRequestTest()
        {
            string _token = "sdfdsfsfdsfdsf1234566";
            string _paymentConnectorName = "PAYMENTCONNECTOR";
            
            Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities.TenderInfo _tenderInfo = new Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities.TenderInfo();
            PaymentTransactionReferenceData _transactionReference=new PaymentTransactionReferenceData();
            Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.ExtensionTransaction _extensionTransaction = new Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.ExtensionTransaction();
            var _paymentDeviceSample = new PaymentDeviceSample();

            var paymentTerminalDeviceRequest = new AuthorizePaymentTerminalDeviceRequest(_token, _paymentConnectorName, 10,"GBP",_tenderInfo,null,false, _transactionReference, _extensionTransaction);
            var response = _paymentDeviceSample.Execute(paymentTerminalDeviceRequest);

            Assert.IsNotNull(response);
        }

        [TestMethod()]
        public void ReadConnectorConfigurationReturnsValue()
        {
            var _paymentDeviceSample = new PaymentDeviceSample();

            var configurationObject= _paymentDeviceSample.ReadConnectorConfiguration();

            Assert.IsNotNull(configurationObject.EndPointIp);
        }

        [TestMethod()]
        public void BuildSampleCardRequestXMLReturnsValidXmlAndConvertToXML()
        {
            var _paymentDeviceSample = new PaymentDeviceSample();

            var xmlAsString = _paymentDeviceSample.BuildSampleCardRequestXML();

            // Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlAsString);

            Assert.IsTrue(doc.ChildNodes.Count > 0);
        }

        [TestMethod()]
        public void PostXMLDataReturnsResponse()
        {
            
            var _paymentDeviceSample = new PaymentDeviceSample();
            //get the sampleXML
            var xmlAsString = _paymentDeviceSample.BuildSampleCardRequestXML();

            //PostTheXML
            var cardServiceResponse = _paymentDeviceSample.postXMLData("CardService", xmlAsString);

            Assert.IsNotNull(cardServiceResponse);
        }

    }
}