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
            //RecieveAndSendDeviceComs();
            RecieveAndSendDeviceComsDynamic();
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
                deviceResponseXML = $"<?xml version=\"1.0\"?><DeviceResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"{RequestProperties.RequestType}\" ApplicationSender=\"GSPOS\" RequestID=\"{RequestProperties.RequestID}\" OverallResult=\"Success\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"><Output OutDeviceTarget=\"{RequestProperties.OutDeviceTarget}\" OutResult=\"Success\" /></DeviceResponse>";
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
                int numberofCall = 0;
                // Enter the listening loop.
                while (numberofCall < 4)
                {
                    numberofCall = numberofCall + 1;
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
                    i = networkStream.Read(bytes, 0, dataSizeToRead);
                    Logger.WriteLog($"Data size excluding first 4 bytes: { i}");

                    data = Encoding.ASCII.GetString(bytes, 0, i);

                    //Parse the message
                    var deviceRequest = ParseDeviceRequestMessage(data);

                    //Build deviceResponseXML
                    var deviceResponseMessage = BuildDeviceResponse(deviceRequest);
                    byte[] message = Encoding.ASCII.GetBytes(deviceResponseMessage);

                    // Send back a response.
                    BinaryWriter binaryWriter = new BinaryWriter(networkStream);
                    var hosttoAddress = IPAddress.HostToNetworkOrder(message.Length);
                    binaryWriter.Write(hosttoAddress); //data size
                    binaryWriter.Write(message);       //actual data

                    //networkStream.Write(message, 0, message.Length);

                    Logger.WriteLog($"Sent deviceResponse message: {deviceResponseMessage}");

                    // Shutdown and end connection
                    // networkStream.Close(); //TODO: need to test using this as well?
                    client.Close();
                }

            }
            catch (SocketException e)
            {
                Logger.WriteLog($"SocketException in RecieveAndSendDeviceComs: {e}");
            }
        }

        private void RecieveAndSendDeviceComsDynamic()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(hostIP);
                tcpListener = new TcpListener(ipAddress, hostPort);
                tcpListener.Start();
                tcpListener.Server.ReceiveTimeout = 1000;
                tcpListener.Server.SendTimeout = 1000;

                // Buffer for reading data
                byte[] bytes = new byte[512];
                string data = null;
                System.Threading.Thread.Sleep(3000);
                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                while (true)
                {
                    Logger.WriteLog("Waiting for a connection...");

                    if (tcpListener.Pending())
                    {
                        TcpClient client = tcpListener.AcceptTcpClient();

                        //if (!client.Connected)
                        //    break;

                        Logger.WriteLog($"Connectection status : {client.Connected} and data available is :{client.Available}");

                        data = null;
                        NetworkStream networkStream = client.GetStream();
                        //specifies the amount of time, in milliseconds, that will elapse before a read operation fails
                        //If the read operation does not complete within the time specified by this property, the read operation throws an IOException.
                        networkStream.ReadTimeout = 5000;
                        int i;

                        Logger.WriteLog("Started reading !");
                        Logger.WriteLog($"Networkstream data available flag :{networkStream.DataAvailable}");
                        //get data size
                        byte[] first4Bytes = new byte[4];
                        try
                        {
                            var dataRead = networkStream.Read(first4Bytes, 0, 4);
                        }
                        catch (IOException ioException)
                        {
                            Logger.WriteLog($"An IO exception occoured while trying to read the data size : { ioException.StackTrace}");
                            break;
                        }

                        if (first4Bytes.Length > 0)
                        {
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(first4Bytes);

                            int dataSizeToRead = BitConverter.ToInt32(first4Bytes, 0);
                            Logger.WriteLog($"data size to read int: { dataSizeToRead}");

                            bytes = new byte[dataSizeToRead];

                            i = networkStream.Read(bytes, 0, dataSizeToRead);
                            Logger.WriteLog($"Data size excluding first 4 bytes: { i}");

                            data = Encoding.ASCII.GetString(bytes, 0, i);

                            //Parse the message
                            var deviceRequest = ParseDeviceRequestMessage(data);

                            //Build deviceResponseXML
                            var deviceResponseMessage = BuildDeviceResponse(deviceRequest);
                            byte[] message = Encoding.ASCII.GetBytes(deviceResponseMessage);

                            // Send back a response.
                            BinaryWriter binaryWriter = new BinaryWriter(networkStream);
                            var hosttoAddress = IPAddress.HostToNetworkOrder(message.Length);
                            binaryWriter.Write(hosttoAddress); //data size
                            binaryWriter.Write(message);       //actual data

                            Logger.WriteLog($"Sent deviceResponse message: {deviceResponseMessage}");
                        }
                        client.Close();
                    }
                    else
                    {
                        Logger.WriteLog($"TCP Listener Pending status is :{tcpListener.Pending()}");
                        if(stopWatch.Elapsed > new TimeSpan(0,0,30))
                        {
                            Logger.WriteLog($"You've just wasted {stopWatch.Elapsed } time of my life, so hanging up");
                            break;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }

            }
            catch (SocketException exSoc)
            {
                Logger.WriteLog($"SocketException in RecieveAndSendDeviceComsDynamic: {exSoc} , with error code: {exSoc.ErrorCode}");
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception in RecieveAndSendDeviceComsDynamic: {ex}");
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
