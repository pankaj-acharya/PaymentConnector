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
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseStr);

                XmlNodeList cardServiceResponseNode = xmlDoc.GetElementsByTagName("CardServiceResponse");
                string requestType = cardServiceResponseNode[0].Attributes["RequestType"].Value;
                string workstationID = cardServiceResponseNode[0].Attributes["WorkstationID"].Value;
                string requestID = cardServiceResponseNode[0].Attributes["RequestID"].Value;
                string overallResult = cardServiceResponseNode[0].Attributes["OverallResult"].Value;

                Logger.WriteLog($"OverallResult value is :{overallResult}");

                //If OverallResult is not Success , then read the error  node and return else proceed further
                if (overallResult.ToLower().Equals("success"))
                {
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

                    if (overallResult.Equals(OverallResult.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                        paymentInfo.IsApproved = true;

                    //Map all values to paymentinfo object
                    paymentInfo.ApprovedAmount = Convert.ToDecimal(totalAmount);
                    paymentInfo.CardNumberMasked = panprint;
                    paymentInfo.CardType = CardType.CustomerCard;//TODO:do we need to send this in response ?
                }
                else
                {
                    //EPS has not returned success 
                    var privateDataNode = xmlDoc.GetElementsByTagName("PrivateData");
                    string privateDataNodeValue = privateDataNode[0].InnerText;
                    Logger.WriteLog($"EPS returned overallResult : {overallResult} with message {privateDataNodeValue}");

                    var paymentError = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentError(Microsoft.Dynamics.Retail.PaymentSDK.Portable.ErrorCode.GeneralException, $"EPS returned overallResult : {overallResult} with message {privateDataNodeValue}",true);
                    var PaymentErrors = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentError[] { paymentError };
                    paymentInfo.Errors = PaymentErrors;
                    return paymentInfo;
                }
                    
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An exception occoured while trying to parse the Authorize response .Message :{ex.Message} ,InnerException :{ex.InnerException},StackTrace{ex.StackTrace}");
            }
            return paymentInfo;
        }

        public PaymentInfo MapRefundResponse(string responseStr)
        {
            PaymentInfo paymentInfo = new PaymentInfo
            {
                IsApproved = false
            };

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseStr);

                XmlNodeList cardServiceResponseNode = xmlDoc.GetElementsByTagName("CardServiceResponse");
                string requestType = cardServiceResponseNode[0].Attributes["RequestType"].Value;
                string workstationID = cardServiceResponseNode[0].Attributes["WorkstationID"].Value;
                string requestID = cardServiceResponseNode[0].Attributes["RequestID"].Value;
                string overallResult = cardServiceResponseNode[0].Attributes["OverallResult"].Value;

                Logger.WriteLog($"OverallResult value is :{overallResult}");

                //If OverallResult is not Success , then read the error  node and return else proceed further
                if (overallResult.ToLower().Equals("success"))
                {
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
                    
                    string actionCode = authorisationNode[0].Attributes["ActionCode"].Value;
                    string approvalCode = authorisationNode[0].Attributes["ApprovalCode"].Value;
                    string acquirerBatch = authorisationNode[0].Attributes["AcquirerBatch"].Value;
                    string panprint = authorisationNode[0].Attributes["PANprint"].Value;
                    string merchant = authorisationNode[0].Attributes["Merchant"].Value;
                    string authorisationType = authorisationNode[0].Attributes["AuthorisationType"].Value;
                    string captureReference = authorisationNode[0].Attributes["CaptureReference"].Value;

                    if (overallResult.Equals(OverallResult.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                        paymentInfo.IsApproved = true;

                    //Map all values to paymentinfo object
                    paymentInfo.ApprovedAmount = Convert.ToDecimal(totalAmount);
                    paymentInfo.CardNumberMasked = panprint;
                    paymentInfo.CardType = CardType.CustomerCard;//TODO:do we need to send this in response ?
                }
                else
                {
                    //EPS has not returned success 
                    var privateDataNode = xmlDoc.GetElementsByTagName("PrivateData");
                    string privateDataNodeValue = privateDataNode[0].InnerText;
                    Logger.WriteLog($"EPS returned overallResult : {overallResult} with message {privateDataNodeValue}");

                    var paymentError = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentError(Microsoft.Dynamics.Retail.PaymentSDK.Portable.ErrorCode.GeneralException, $"EPS returned overallResult : {overallResult} with message {privateDataNodeValue}", true);
                    var PaymentErrors = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentError[] { paymentError };
                    paymentInfo.Errors = PaymentErrors;
                    return paymentInfo;
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An exception occoured while trying to parse Refund response .Message :{ex.Message} ,InnerException :{ex.InnerException},StackTrace{ex.StackTrace}");
            }
            return paymentInfo;
        }

        public PaymentInfo MapVoidResponse(string responseStr)
        {
            var paymentInfo= new PaymentInfo();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseStr);

                XmlNodeList cardServiceResponseNode = xmlDoc.GetElementsByTagName("CardServiceResponse");
                string requestType = cardServiceResponseNode[0].Attributes["RequestType"].Value;
                string workstationID = cardServiceResponseNode[0].Attributes["WorkstationID"].Value;
                string requestID = cardServiceResponseNode[0].Attributes["RequestID"].Value;
                string overallResult = cardServiceResponseNode[0].Attributes["OverallResult"].Value;

                Logger.WriteLog($"OverallResult value is :{overallResult}");

                //If OverallResult is not Success , then read the error  node and return else proceed further
                if (overallResult.ToLower().Equals("success"))
                {
                    var terminalNode = xmlDoc.GetElementsByTagName("Terminal");
                    string terminalID = terminalNode[0].Attributes["TerminalID"].Value;
                    string terminalBatch = terminalNode[0].Attributes["TerminalBatch"].Value;

                    //var tenderNode = xmlDoc.GetElementsByTagName("Tender");

                    //var totalAmountNode = xmlDoc.GetElementsByTagName("TotalAmount");
                    //string totalAmount = totalAmountNode[0].InnerText;

                    //var authorisationNode = xmlDoc.GetElementsByTagName("Authorisation");
                    //string acquirerID = authorisationNode[0].Attributes["AcquirerID"].Value;
                    //string startDate = authorisationNode[0].Attributes["StartDate"].Value;
                    //string expiryDate = authorisationNode[0].Attributes["ExpiryDate"].Value;
                    //string timeStamp = authorisationNode[0].Attributes["TimeStamp"].Value;

                    //string actionCode = authorisationNode[0].Attributes["ActionCode"].Value;
                    //string approvalCode = authorisationNode[0].Attributes["ApprovalCode"].Value;
                    //string acquirerBatch = authorisationNode[0].Attributes["AcquirerBatch"].Value;
                    //string panprint = authorisationNode[0].Attributes["PANprint"].Value;
                    //string merchant = authorisationNode[0].Attributes["Merchant"].Value;
                    //string authorisationType = authorisationNode[0].Attributes["AuthorisationType"].Value;
                    //string captureReference = authorisationNode[0].Attributes["CaptureReference"].Value;

                    //if (overallResult.Equals(OverallResult.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                    //    paymentInfo.IsApproved = true;

                    ////Map all values to paymentinfo object
                    //paymentInfo.ApprovedAmount = Convert.ToDecimal(totalAmount);
                    //paymentInfo.CardNumberMasked = panprint;
                    //paymentInfo.CardType = CardType.CustomerCard;//TODO:do we need to send this in response ?
                }
                else
                {
                    //EPS has not returned success 
                    var privateDataNode = xmlDoc.GetElementsByTagName("PrivateData");
                    string privateDataNodeValue = privateDataNode[0].InnerText;
                    Logger.WriteLog($"EPS returned overallResult : {overallResult} with message {privateDataNodeValue}");

                    var paymentError = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentError(Microsoft.Dynamics.Retail.PaymentSDK.Portable.ErrorCode.GeneralException, $"EPS returned overallResult : {overallResult} with message {privateDataNodeValue}", true);
                    var PaymentErrors = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentError[] { paymentError };
                    paymentInfo.Errors = PaymentErrors;
                    return paymentInfo;
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An exception occoured while trying to parse VoidResponse response .Message :{ex.Message} ,InnerException :{ex.InnerException},StackTrace{ex.StackTrace}");
            }
            return paymentInfo;
        }
    }
    enum OverallResult
    {
        Success,
        CommunicationError
    }
}
