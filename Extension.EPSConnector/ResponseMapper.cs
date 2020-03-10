using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;

namespace Hardware.Extension.EPSPaymentConnector
{
    public class ResponseMapper
    {
        public PaymentInfo MapPaymentResponse(string responseStr)
        {
            PaymentInfo paymentInfo = new PaymentInfo
            {
                IsApproved = false
            };

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseStr);

            XmlNodeList cardServiceResponseNode = xmlDoc.GetElementsByTagName("CardServiceResponse");
            string requestType = cardServiceResponseNode[0].Attributes["RequestType"].Value;
            string workstationID = cardServiceResponseNode[0].Attributes["WorkstationID"].Value;
            string requestID = cardServiceResponseNode[0].Attributes["RequestID"].Value;
            string overallResult = cardServiceResponseNode[0].Attributes["OverallResult"].Value;

            var terminalNode = xmlDoc.GetElementsByTagName("Terminal");
            string terminalID = terminalNode[0].Attributes["TerminalID"].Value;
            string terminalBatch = terminalNode[0].Attributes["TerminalBatch"].Value;

            var tenderNode = xmlDoc.GetElementsByTagName("Tender");

            var totalAmountNode = xmlDoc.GetElementsByTagName("TotalAmount");
            string totalAmount = totalAmountNode[0].InnerText;

            var authorisationNode = xmlDoc.GetElementsByTagName("Authorisation");
            string acquirerID = authorisationNode[0].Attributes["AcquirerID"].Value;
            string startDate = authorisationNode[0].Attributes["StartDate"].Value;
            string expiryDate = authorisationNode[0].Attributes["ExpiryDate"].Value;
            string timeStamp = authorisationNode[0].Attributes["TimeStamp"].Value;

            #region Different Action codes as per EPS doc
            // '004' is undefined by IFSF so should be treated as a decline.   
            //'000' Approved 
            //'001' Honour, with Identification Approved 
            //'002' Approved for partial amount Approved 
            //'003' Approved(VIP) Approved 
            //'005' Approved, account type specified by card issuer 
            //'006' Approved for partial amount, account Approved 
            //'007' Approved, update ICC Approved
            #endregion

            string actionCode = authorisationNode[0].Attributes["ActionCode"].Value;
            string approvalCode = authorisationNode[0].Attributes["ApprovalCode"].Value;
            string acquirerBatch = authorisationNode[0].Attributes["AcquirerBatch"].Value;
            string panprint = authorisationNode[0].Attributes["PANprint"].Value;
            string merchant = authorisationNode[0].Attributes["Merchant"].Value;
            string authorisationType = authorisationNode[0].Attributes["AuthorisationType"].Value;
            string captureReference = authorisationNode[0].Attributes["CaptureReference"].Value;

            if (overallResult.Equals(OverallResult.Success))
                paymentInfo.IsApproved = true;

            return paymentInfo;
        }

        public PaymentInfo MapRefundResponse(string responseStr)
        {
            return new PaymentInfo();
        }

        public PaymentInfo MapVoidResponse(string responseStr)
        {
            return new PaymentInfo();
        }
    }
    enum OverallResult
    {
        Success,
        CommunicationError
    }
}
