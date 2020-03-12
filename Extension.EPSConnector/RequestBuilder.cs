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
        /// <summary>
        /// Returns dummy request XML for test
        /// </summary>
        /// <returns>string</returns>
        public string BuildSampleCardRequestXML()
        {
            string sampleXmlAsString = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\" WorkstationID=\"TestPos\" RequestID=\"2\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> false </ManualPAN> <TransactionNumber> TXNDEMONo5 </TransactionNumber>   </POSdata>   <TotalAmount Currency=\"GBP\"> 7 </TotalAmount> </CardServiceRequest>";

            return sampleXmlAsString;
        }
        public string BuildAuthorizePaymentRequest(AuthorizePaymentTerminalDeviceRequest paymentRequest,string workStationId)
        {
            
            string cardServiceRequestXML= $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\" WorkstationID=\"{workStationId}\" RequestID=\"2\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> {paymentRequest.IsManualEntry} </ManualPAN> <TransactionNumber> {paymentRequest.TransactionReferenceData.UniqueTransactionId} </TransactionNumber>   </POSdata>   <TotalAmount Currency=\"{paymentRequest.Currency}\"> {paymentRequest.Amount} </TotalAmount> </CardServiceRequest>";

            return cardServiceRequestXML;
        }

        public string BuildRefundPaymentRequest(RefundPaymentTerminalDeviceRequest refundRequest,string workStationId)
        {
            string refundRequestXML = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"PaymentRefund\" WorkstationID=\"{workStationId}\" RequestID=\"2\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> {refundRequest.IsManualEntry} </ManualPAN> </POSdata>   <TotalAmount Currency=\"{refundRequest.Currency}\"> {refundRequest.Amount} </TotalAmount> </CardServiceRequest>";

            return refundRequestXML;
        }

        public string BuildCancelPaymentRequest(VoidPaymentTerminalDeviceRequest cancelRequest, string workStationId)
        {
            string cancelRequestXML = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"PaymentRefund\" WorkstationID=\"{workStationId}\" RequestID=\"2\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN>False</ManualPAN> <TransactionNumber> </TransactionNumber>  </POSdata></CardServiceRequest>";
            return cancelRequestXML;
        }
    }
}
