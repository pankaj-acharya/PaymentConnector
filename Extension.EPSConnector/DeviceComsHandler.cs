using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hardware.Extension.EPSPaymentConnector
{
    public class DeviceComsHandler
    {
        #region LocalVariables

        string hostIP = "127.0.0.1";
        int hostPort = 9900;
        
        TcpListener tcpListener = null;
        #endregion

        public DeviceComsHandler()
        {
            Logger.WriteLog("DeviceComsHandler constructor");
            
        }

        public void StopTcpServer()
        {
            try
            {
                //tcpListener.Stop();
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception occourded at StopTcpServer(). Message :{ex.InnerException}");
            }
        }
        public void DeviceRequestOneHandler()
        {
            Logger.WriteLog("Entered method DeviceRequestOneHandler");
            RecieveAndSendDeviceComs();
        }

        public void DeviceRequestTwoHandler()
        {
            Logger.WriteLog("Entered method DeviceRequestTwoHandler");
            RecieveAndSendDeviceComs();
        }
        public void DeviceRequestThreeHandler()
        {
            Logger.WriteLog("Entered method DeviceRequestThreeHandler");
            RecieveAndSendDeviceComs();

        }
        public void DeviceRequestFourHandler()
        {
            Logger.WriteLog("Entered method DeviceRequestFourHandler");
            RecieveAndSendDeviceComs();
        }
        public void SplitDeviceRequestMessages(string masterXmlAsString, out string firstRequestXML, out string secondRequestXML, out string thirdRequestXML)
        {
            string customSeparator = "</DeviceRequest>";

            int charsToReadFirstXML = masterXmlAsString.IndexOf(customSeparator);
            charsToReadFirstXML = charsToReadFirstXML + customSeparator.Length;
            firstRequestXML = masterXmlAsString.Substring(0, charsToReadFirstXML);

            int charsToReadSecondXML = masterXmlAsString.IndexOf(customSeparator, charsToReadFirstXML);
            charsToReadSecondXML = charsToReadSecondXML + customSeparator.Length;
            secondRequestXML = masterXmlAsString.Substring(charsToReadFirstXML, (charsToReadSecondXML - charsToReadFirstXML));

            int charsToReadThirdXML = masterXmlAsString.IndexOf(customSeparator, charsToReadSecondXML);
            charsToReadThirdXML = charsToReadThirdXML + customSeparator.Length;
            thirdRequestXML = masterXmlAsString.Substring(charsToReadSecondXML, (charsToReadThirdXML - charsToReadSecondXML));

        }
        private DeviceRequest ParseDeviceRequestMessage(string XmlMessage)
        {
            var deviceRequestValues = new DeviceRequest();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(XmlMessage);

                XmlNodeList deviceRequestNode = xmlDoc.GetElementsByTagName("DeviceRequest");
                deviceRequestValues.RequestType = deviceRequestNode[0].Attributes["RequestType"].Value;
                deviceRequestValues.ApplicationSender = deviceRequestNode[0].Attributes["ApplicationSender"].Value;
                deviceRequestValues.RequestID = deviceRequestNode[0].Attributes["RequestID"].Value;

                XmlNodeList outputNode = xmlDoc.GetElementsByTagName("Output");
                deviceRequestValues.OutDeviceTarget = outputNode[0].Attributes["OutDeviceTarget"].Value;

            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An exception occoured in ParseDeviceRequestMessage with message: {ex.Message}");
            }

            return deviceRequestValues;
        }

        private string BuildDeviceResponse(DeviceRequest RequestProperties)
        {
            string deviceResponseXML = string.Empty;
            try
            {
                deviceResponseXML = $"<?xml version=\"1.0\"?><DeviceResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"{RequestProperties.RequestType}\" ApplicationSender=\"{RequestProperties.ApplicationSender}\" RequestID=\"{RequestProperties.RequestID}\" OverallResult=\"Success\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"><Output OutDeviceTarget=\"{RequestProperties.OutDeviceTarget}\" OutResult=\"Success\" /></DeviceResponse>";
                //Logger.WriteLog(deviceResponseXML);
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An exception occoured in DeviceComsHandler.cs >> BuildDeviceResponse : {ex.Message}");
            }

            return deviceResponseXML;
        }
        private void RecieveAndSendDeviceComs()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(hostIP);
                tcpListener = new TcpListener(ipAddress, hostPort);
                tcpListener.Start();

                // Buffer for reading data
                byte[] bytes = new byte[512];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    Logger.WriteLog("Waiting for a connection...");

                    // Perform a blocking call to accept requests.
                    // You could also user tcpListener.AcceptSocket() here.
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Logger.WriteLog("Connected!");

                    data = null;
                    NetworkStream networkStream = client.GetStream();
                    int i;

                    Logger.WriteLog("Started reading !");

                    //BEGIN get data size
                    byte[] first4Bytes = new byte[4];
                    var dataRead = networkStream.Read(first4Bytes, 0, 4);

                    // If the system architecture is little-endian (that is, little end first),
                    // reverse the byte array.
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(first4Bytes);

                    int dataSizeToRead = BitConverter.ToInt32(first4Bytes, 0);
                    Logger.WriteLog($"data size to read int: { dataSizeToRead}");

                    bytes = new byte[dataSizeToRead];
                    //END
                    //while ((i = networkStream.Read(bytes, 0, bytes.Length)) != 0)
                    //{
                    //    data = Encoding.ASCII.GetString(bytes, 4, (i - 4));//ignore the first 4 bytes which is the size of data
                    //}
                    i = networkStream.Read(bytes, 0,dataSizeToRead);
                    data = Encoding.ASCII.GetString(bytes, 4, i);

                    //Parse the message
                    var deviceRequest = ParseDeviceRequestMessage(data);

                    //Build deviceResponseXML
                    var deviceResponseMessage = BuildDeviceResponse(deviceRequest);
                    byte[] message = Encoding.ASCII.GetBytes(deviceResponseMessage);

                    // Send back a response.
                    //TODO: Add code to add HOSTTONETWORK ORDER BIGIndian notation
                    //Note : we probably should not need below linees as the stream.Write is
                    //       already passing the number of bytes as third parameter.So test and confirm 
                    //BinaryWriter binaryWriter = new BinaryWriter(networkStream);
                    //var hosttoAddress = IPAddress.HostToNetworkOrder(message.Length);
                    //binaryWriter.Write(hosttoAddress);
                    //binaryWriter.Write(message);

                    networkStream.Write(message, 0, message.Length);

                    Logger.WriteLog($"Sent deviceResponse message: {deviceResponseMessage}");

                    // Shutdown and end connection
                   // networkStream.Close();
                    client.Close();
                }
               
            }
            catch (SocketException e)
            {
                Logger.WriteLog($"SocketException in RecieveAndSendDeviceComs: {e}");
            }
        }
    }
    public class DeviceRequest
    {
        public string RequestType { get; set; }
        public string ApplicationSender { get; set; }
        public string RequestID { get; set; }
        public string OutDeviceTarget { get; set; }
    }
}
