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
        string hostname = "127.0.0.1";
        int port = 9900;

        public void DeviceRequestOneHandler()
        {
            TcpListener server = null;
            try
            {
                Logger.WriteLog("Entered method DeviceRequestOneHandler");
                // Set the TcpListener on port 9900.
                IPAddress localAddr = IPAddress.Parse(hostname);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Logger.WriteLog("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Logger.WriteLog("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        Logger.WriteLog($"The devicerequest data in bytes : {bytes} ");

                        data = Encoding.ASCII.GetString(bytes, 0, i);
                    }

                    Logger.WriteLog($"Received: {data}");

                    string firstRequestXML = string.Empty;
                    string secondRequestXML = string.Empty;
                    string thirdRequestXML = string.Empty;

                    if (!string.IsNullOrEmpty(data))
                        SplitDeviceRequestMessages(data, out firstRequestXML, out secondRequestXML, out thirdRequestXML);

                    //Parse the message
                    var firstDeviceRequest = ParseDeviceRequestMessage(firstRequestXML);
                    var secondDeviceRequest = ParseDeviceRequestMessage(secondRequestXML);
                    var thirdDeviceRequest = ParseDeviceRequestMessage(thirdRequestXML);

                    //Build deviceResponseXML
                    var firstDeviceResponseMsg = BuildDeviceResponse(firstDeviceRequest);
                    var secondDeviceResponseMsg = BuildDeviceResponse(secondDeviceRequest);
                    var thirdDeviceResponseMsg = BuildDeviceResponse(thirdDeviceRequest);

                    byte[] firstMsg = Encoding.ASCII.GetBytes(firstDeviceResponseMsg);
                    byte[] secondMsg = Encoding.ASCII.GetBytes(secondDeviceResponseMsg);
                    byte[] thirdMsg = Encoding.ASCII.GetBytes(thirdDeviceResponseMsg);

                    // Send back a response.
                    stream.Write(firstMsg, 0, firstMsg.Length);
                    stream.Write(secondMsg, 0, secondMsg.Length);
                    stream.Write(thirdMsg, 0, thirdMsg.Length);

                    Logger.WriteLog($"Sent deviceResponse data");

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Logger.WriteLog($"SocketException in DeviceRequestOneHandler: {e}");
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

        }
        public void DeviceRequestTwoHandler()
        {
            TcpListener server = null;
            try
            {
                Logger.WriteLog("Entered method DeviceRequestTwoHandler");
                // Set the TcpListener on port 9900.
                IPAddress localAddr = IPAddress.Parse(hostname);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Logger.WriteLog("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Logger.WriteLog("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Logger.WriteLog($"Received: {data}");

                        //Parse the message
                        var deviceRequest = ParseDeviceRequestMessage(data);

                        //Build deviceResponseXML
                        var deviceResponseXml = BuildDeviceResponse(deviceRequest);
                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(deviceResponseXml);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Logger.WriteLog($"Sent data");
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Logger.WriteLog($"SocketException in DeviceRequestTwoHandler: {e}");
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
        public void DeviceRequestThreeHandler()
        {
            TcpListener server = null;
            try
            {
                Logger.WriteLog("Entered method DeviceRequestThreeHandler");
                // Set the TcpListener on port 9900.
                IPAddress localAddr = IPAddress.Parse(hostname);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Logger.WriteLog("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Logger.WriteLog("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Logger.WriteLog($"Received: {data}");

                        //Parse the message
                        var deviceRequest = ParseDeviceRequestMessage(data);

                        //Build deviceResponseXML
                        var deviceResponseXml = BuildDeviceResponse(deviceRequest);
                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(deviceResponseXml);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Logger.WriteLog($"Sent: {msg}");
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Logger.WriteLog($"SocketException in DeviceRequestThreeHandler: {e}");
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
        public void DeviceRequestFourHandler()
        {
            TcpListener server = null;
            try
            {
                Logger.WriteLog("Entered method DeviceRequestFourHandler");
                // Set the TcpListener on port 9900.
                IPAddress localAddr = IPAddress.Parse(hostname);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Logger.WriteLog("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Logger.WriteLog("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        Logger.WriteLog($"Raw bytes : {bytes}");
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Logger.WriteLog($"Received data encoded: {data}");

                        //Parse the message
                        //var deviceRequest = ParseDeviceRequestMessage(data);

                        //Build deviceResponseXML
                        //var deviceResponseXml = BuildDeviceResponse(deviceRequest);

                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        //byte[] msg = System.Text.Encoding.ASCII.GetBytes(deviceResponseXml);

                        // Send back a response.
                        //stream.Write(msg, 0, msg.Length);
                        //Logger.WriteLog($"Sent: {data}");
                    }
                    

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Logger.WriteLog($"SocketException in DeviceRequestFourHandler: {e}");
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
        public void SplitDeviceRequestMessages(string masterXmlAsString,out string firstRequestXML,out string secondRequestXML,out string thirdRequestXML)
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
            string deviceResponseXML = $"<?xml version=\"1.0\"?><DeviceResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" RequestType=\"{RequestProperties.RequestType}\" ApplicationSender=\"{RequestProperties.ApplicationSender}\" RequestID=\"{RequestProperties.RequestID}\" OverallResult=\"Success\" xmlns=\"http://www.nrf-arts.org/IXRetail/namespace\"><Output OutDeviceTarget=\"{RequestProperties.OutDeviceTarget}\" OutResult=\"Success\" /></DeviceResponse>";
            Logger.WriteLog(deviceResponseXML);
            return deviceResponseXML;
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
