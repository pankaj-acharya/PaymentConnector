using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Net;
using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;

namespace Hardware.Extension.EPSPaymentConnector
{
    public class Helper
    {
        //Ctor
        public Helper()
        {

        }

        public string BuildCardRequestXML()
        {
            string sampleXmlAsString = "<?xml version=\"1.0\"?> <CardServiceRequest xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\"  WorkstationID=\"Dave's Workstation\" RequestID=\"19\"> <POSdata> <POSTimeStamp>2020-02-05T15:02:59.6303603+01:00</POSTimeStamp> <ManualPAN>false</ManualPAN> </POSdata> < TotalAmount Currency = \"GBP\" > 9 </ TotalAmount > </ CardServiceRequest >";

            return sampleXmlAsString;
        }
        public string BuildSampleCardRequestXML()
        {
            string sampleXmlAsString = "<?xml version=\"1.0\"?> <CardServiceRequest xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"CardPayment\"  WorkstationID=\"Dave's Workstation\" RequestID=\"19\"> <POSdata> <POSTimeStamp>2020-02-05T15:02:59.6303603+01:00</POSTimeStamp> <ManualPAN>false</ManualPAN> </POSdata> < TotalAmount Currency = \"GBP\" > 9 </ TotalAmount > </ CardServiceRequest >";

            return sampleXmlAsString;
        }

        public ConnectorConfig ReadConnectorConfiguration()
        {
            var config = new ConnectorConfig();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load("connectorconfig.xml");
                XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/Config");

                foreach (XmlNode node in nodeList)
                {
                    config.EndPointIp = node.FirstChild.InnerText;
                    config.Port = node.LastChild.InnerText;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception in : ReadConnectorConfiguration() .InnerException :{ex.InnerException} ", true);
            }
            return config;
        }

        public class ConnectorConfig
        {
            public ConnectorConfig()
            {
                EndPointIp = "";
                Port = "";
            }
            public string EndPointIp { get; set; }
            public string Port { get; set; }
        }
        public enum RequestType
        {
            Authorize,
            Capture,
            Refund,
            Void
        }
        public PaymentInfo PostXMLData(RequestType requestType,DateTime POSTimeStamp,string ClerkId,bool IsManualPAN,string TransactionNumber, string currency,string amount)
        {
            switch (requestType)
            {
                case RequestType.Authorize:
                    return BuildAuthorizeRequest(POSTimeStamp,currency,amount, IsManualPAN);

                case RequestType.Capture:
                    break;
                case RequestType.Refund:
                    break;
                case RequestType.Void:
                    break;
                default:
                    break;
            }
            return null;
        }

        private PaymentInfo BuildAuthorizeRequest(DateTime POSTimeStamp, string currency, string amount,bool IsManualPAN)
        {
            Logger.WriteLog($"Entered method :BuildAuthorizeRequest()");
            PaymentInfo paymentInfo = new PaymentInfo();
            #region PaymentInfo_Object_Structure
            //PaymentInfo paymentInfo = new PaymentInfo()
            //{
            //    CardNumberMasked = cardNumberMasked,
            //    CardType = cardType,
            //    SignatureData = signatureData,
            //    PaymentSdkData = str,
            //    CashbackAmount = cashbackAmount,
            //    ApprovedAmount = approvedAmount,
            //    AvailableBalanceAmount = availableBalanceAmount,
            //    IsApproved = isApproved,
            //    Errors = paymentErrorArray,
            //    PaymentSdkContentType = paymentSdkContentType
            //};
            #endregion

            //Build Request
            try
            {
                string urlwithPortAndPath = "http://127.0.0.1:8900/Capture";//_config.EndPointIp + destinationPath;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlwithPortAndPath);
                byte[] bytes;
                string _xmlString = "";
                bytes = System.Text.Encoding.ASCII.GetBytes(_xmlString);
                request.ContentType = "text/xml; encoding='utf-8'";
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    Logger.WriteLog($"Response :{responseStr}", true);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception in : postXMLData() .Exception message : {ex.Message + ex.StackTrace} .InnerException :{ex.InnerException} ", true);
            }

            //ParseResponse
            return paymentInfo;
        }
    }

}
