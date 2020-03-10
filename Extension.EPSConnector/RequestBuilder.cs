using Microsoft.Dynamics.Commerce.HardwareStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Extension.EPSPaymentConnector
{
    public class RequestBuilder
    {
        public string BuildSampleCardRequestXML()
        {
            // string sampleXmlAsString1 = "<?xml version=\"1.0\"?> <CardServiceRequest xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\"  WorkstationID=\"Dave's Workstation\" RequestID=\"19\"> <POSdata> <POSTimeStamp>2020-02-05T15:02:59.6303603+01:00</POSTimeStamp> <ManualPAN>false</ManualPAN> </POSdata> < TotalAmount Currency = \"GBP\" > 9 </ TotalAmount > </ CardServiceRequest >";

            //{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}
            string sampleXmlAsString = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\" WorkstationID=\"TestPos\" RequestID=\"2\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> false </ManualPAN> <TransactionNumber> TXNDEMONo5 </TransactionNumber>   </POSdata>   <TotalAmount Currency=\"GBP\"> 7 </TotalAmount> </CardServiceRequest>";

            return sampleXmlAsString;
        }
        public string BuildPaymentRequest(AuthorizePaymentTerminalDeviceRequest paymentRequest)
        { 
            string cardServiceRequestXML= @"<?xml version=""1.0""?>
            <CardServiceRequest xmlns: xsi = ""http://www.w3.org/2001/XMLSchema-instance"" xmlns: xsd = ""http://www.w3.org/2001/XMLSchema"" 
                 RequestType = ""CardPayment"" WorkstationID = ""{0}"" RequestID = ""{1}"" xmlns = ""http://www.nrf-arts.org/IXRetail/namespace"" >
                    < POSdata >
                        < POSTimeStamp >{2}</ POSTimeStamp >
                        < ClerkID > {3}</ ClerkID >
                        < ManualPAN > {4} </ ManualPAN >
                        < TransactionNumber >{5}</ TransactionNumber >
                    </ POSdata >
                    < TotalAmount Currency = ""GBP"" > {6} </ TotalAmount >
            </ CardServiceRequest > ";

            var requestXMLWithValues = string.Format(cardServiceRequestXML, "1234", "Dave's station", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"), "1234", paymentRequest.IsManualEntry, paymentRequest.TransactionReferenceData.UniqueTransactionId, paymentRequest.Amount);

            return requestXMLWithValues;
        }

        public string BuildRefundRequest()
        {
            return "";
        }

        public string BuildAbortRequest()
        {
            return "";
        }
    }
}
