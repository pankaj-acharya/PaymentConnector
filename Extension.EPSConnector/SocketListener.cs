using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hardware.Extension.EPSPaymentConnector
{
    public static class SocketListener
    {
        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            IPHostEntry host = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9900);
            // Create a Socket that will use Tcp protocol      
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket handler=new Socket(new SocketInformation());

            try
            {
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 10 requests at a time  
                listener.Listen(10);

                Logger.WriteLog("Waiting for a connection on 9900...");
                handler = listener.Accept();

                // Incoming data from the client.    
                string data = null;
                byte[] bytes = null;

                //while (true)
                //{
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                Logger.WriteLog($"Recieved {bytesRec} bytes of data on port 9900");

                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                //    if (data.Length > 1)
                //    {
                //        break;
                //    }
                //}

                Logger.WriteLog($"Text received : {data}");
            }
            catch (Exception e)
            {
                Logger.WriteLog($"Exception in SocketListener.cs >> StartServer() .Message :{e.ToString()} ");
            }
            finally
            {
                handler.Shutdown(SocketShutdown.Receive);
                handler.Close();
                Logger.WriteLog($"Port 9900 closed");
            }

        }
    }
}
