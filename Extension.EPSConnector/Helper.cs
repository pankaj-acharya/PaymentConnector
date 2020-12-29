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
            public int TimeOut { get; set; }
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
                    //return BuildAuthorizeRequest(POSTimeStamp,currency,amount, IsManualPAN);

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
        
    }

}
