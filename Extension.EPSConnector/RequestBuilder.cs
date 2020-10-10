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
            LastTransactionNumber = paymentRequest.TransactionReferenceData.UniqueTransactionId;
            string cardServiceRequestXML= $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\" WorkstationID=\"{workStationId}\" RequestID=\"{paymentRequest.TransactionReferenceData.UniqueTransactionId}\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> {paymentRequest.IsManualEntry} </ManualPAN> <TransactionNumber> {paymentRequest.TransactionReferenceData.UniqueTransactionId} </TransactionNumber>   </POSdata>   <TotalAmount Currency=\"GBP\"> {paymentRequest.Amount} </TotalAmount> </CardServiceRequest>";

            return cardServiceRequestXML;
        }

        public string BuildRefundPaymentRequest(RefundPaymentTerminalDeviceRequest refundRequest,string invoiceId,string workStationId)
        {
            string refundRequestXML = "";
            try
            {
                refundRequestXML = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"PaymentRefund\" WorkstationID=\"{workStationId}\" RequestID=\"{invoiceId}\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> false </ManualPAN> </POSdata>   <TotalAmount Currency=\"GBP\"> {refundRequest.Amount} </TotalAmount> </CardServiceRequest>";
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception in BuildRefundPaymentRequest() {ex}");
            }

            return refundRequestXML;
        }
        public string BuildVoidPaymentRequest(VoidPaymentTerminalDeviceRequest voidRequest, string invoiceId, string workStationId)
        {
            string refundRequestXML = "";
            try
            {
                refundRequestXML = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"PaymentRefund\" WorkstationID=\"{workStationId}\" RequestID=\"{invoiceId}\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ClerkID> 123456 </ClerkID> <ManualPAN> false </ManualPAN> </POSdata>   <TotalAmount Currency=\"GBP\"> {voidRequest.Amount} </TotalAmount> </CardServiceRequest>";
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception in BuildVoidPaymentRequest() {ex}");
            }

            return refundRequestXML;
        }

        public string BuildCancelPaymentRequest(string workStationId)
        {
            string customRequestIdForCancel = LastTransactionNumber + "89";
            string customTransactionNumberForCancel = LastTransactionNumber + "12";
            string cancelRequestXML = $"<?xml version =\"1.0\"?> <CardServiceRequest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"AbortRequest\" WorkstationID=\"{workStationId}\" RequestID=\"{customRequestIdForCancel}\" ReferenceNumber=\"{LastTransactionNumber}\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\">  <POSdata> <POSTimeStamp>{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}</POSTimeStamp> <ManualPAN>false</ManualPAN> </POSdata> </CardServiceRequest>";
            return cancelRequestXML;
        }

        public string BuildSampleCancelPaymentRequest(string workStationId)
        {
            string customRequestIdForCancel = LastTransactionNumber + "89";
            string customTransactionNumberForCancel = LastTransactionNumber + "12";
            string cancelRequestXML = $"<? xml version =\"1.0\"?><CardServiceRequest xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\" xmlns: xsd = \"http://www.w3.org/2001/XMLSchema\" RequestType = \"AbortRequest\" WorkstationID = \"TestPos\" RequestID = \"{customRequestIdForCancel}\" ReferenceNumber = \"{LastTransactionNumber}\" xmlns = \"http://www.nrf-arts.org/IXRetail/namespace\"> < POSdata >< POSTimeStamp > { DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")} </ POSTimeStamp >< ClerkID > 123456 </ ClerkID >< ManualPAN > false </ ManualPAN >< TransactionNumber > TXNNo3 </ TransactionNumber > </ POSdata ></ CardServiceRequest >";
            return cancelRequestXML;
        }

        /// <summary>
        /// Property to store last TransactionNumber .This is updated with every AuthorizePaymentRequest and is used for CancelPaymentRequest
        /// </summary>
        public string LastTransactionNumber { get; set; }
    }
}
