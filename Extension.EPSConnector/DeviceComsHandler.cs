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
        //TODO : Move these to config file
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
                tcpListener.Stop();
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Exception occourded at StopTcpServer(). Message :{ex.InnerException}");
            }
        }
        public void DeviceRequestHandler()
        {
            Logger.WriteLog("Entered method DeviceRequestOneHandler");
            RecieveAndSendDeviceComsDynamic();
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

                if (deviceRequestValues.OutDeviceTarget.ToLower().Equals("printerreceipt"))
                {
                    if (outputNode[0].InnerText.ToLower().Contains("cardholderreceipt"))
                    {
                        deviceRequestValues.IsCardholderReceipt = true;
                    }
                }

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
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"An exception occoured in DeviceComsHandler.cs >> BuildDeviceResponse : {ex.Message}");
            }

            return deviceResponseXML;
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
                        DeviceRequest deviceRequest = new DeviceRequest();
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
                            deviceRequest = ParseDeviceRequestMessage(data);

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
                        if (deviceRequest.IsCardholderReceipt) //Once you have recieved cardholderReceipt , break out and start listening to port 8900
                            break;

                    }
                    else
                    {
                        Logger.WriteLog($"TCP Listener Pending status is :{tcpListener.Pending()}");
                        if (stopWatch.Elapsed > new TimeSpan(0, 0, 30)) //TODO : move the timeout value  into config
                        {
                            Logger.WriteLog($"Listened for {stopWatch.Elapsed } before hanging up");
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
            finally
            {
                Logger.WriteLog($"Stopping tcp listener in finally block");
                tcpListener.Stop();
            }
        }

    }

    public class DeviceRequest
    {
        public string RequestType { get; set; }
        public string ApplicationSender { get; set; }
        public string RequestID { get; set; }
        public string OutDeviceTarget { get; set; }
        public bool IsCardholderReceipt { get; set; }
    }
}
