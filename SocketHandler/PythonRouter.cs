using System.Net;
using System.Net.Sockets;
using System.Text;

public class PythonRouter
{
    public Socket client;

    public PythonRouter(IPAddress ipAddress)
    {
        // Connects to local client.
        IPEndPoint clientEndPoint = new IPEndPoint(ipAddress, 10000);
        client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(clientEndPoint);
        if (client.IsConnected())
        {
            Console.Write(" Connected");
        }
    }

    void CopySocketStream(Socket location, Socket target)
    {
        HttpHeader header = new HttpHeader(location);
        if (!target.IsConnected())
        {
            Console.Write("Target not connected");
            return;
        }
        target.Send(header.raw);
        if (header.contentLength == -1)
        {
            Console.Write(" No body");
            return;
        }
        
        byte[] buffer = new byte[header.contentLength];
        int readLength = 0;
        while (true)
        {
            if (!location.IsConnected())
            {
                Console.Write("\nLocation aborted");
                break;
            }
            int bytesLength = location.Receive(buffer);
            if (bytesLength <= 0)
            {
                Console.Write("\nStopped receiving data");
                break;
            }

            readLength += bytesLength; 
            Console.Write(Encoding.UTF8.GetString(buffer) + "(" + readLength + "/" + header.contentLength + ")");
            
            if (!target.IsConnected())
            {
                Console.Write("\nTarget aborted");
                break;
            }
            target.Send(buffer);

            if (readLength >= header.contentLength)
            {
                Console.Write("\nContent length reached.");
                break;
            }
        }
    }

    /*void CopySocketStream(Socket location, Socket target)
    {
        byte[] buffer = new byte[1024];
        string data = "";
        int readLength = 0;
        int? totalLength = null;
        while (true)
        {
            if (!location.IsConnected())
            {
                Console.Write("\nLocation aborted");
                break;
            }
            int bytesLength = location.Receive(buffer);
            string toAdd = Encoding.ASCII.GetString(buffer);
            Console.Write(toAdd);
            data += toAdd;
            if (!target.IsConnected())
            {
                Console.Write("\nTarget aborted");
                break;
            }
            target.Send(buffer);
            if (totalLength == null)
            {
                string[] parts = data.Split("\r\n");
                for (int i = 0; i < parts.Length; i++)
                {
                    string name = "Content-Length: ";
                    if (parts[i].StartsWith(name) && parts[i].Length > name.Length)
                    {
                        if (int.TryParse(parts[i].Substring(name.Length + 1), out int number))
                        {
                            totalLength = number;
                        }
                        break;
                    }
                }
            }
            if (bytesLength <= 0 || (totalLength == null && data.Contains("\r\n\r\n")) || (totalLength != null && readLength >= totalLength))
            {
                Console.Write("\nContent length reached.");
                break;
            }
        }
    }*/

    public void SendSocketStream(Socket socket)
    {
        if (!client.IsConnected())
        {
            return;
        }

        // Sends user data.
        CopySocketStream(socket, client);
        Console.Write(" Received");

        // Sends server data.
        CopySocketStream(client, socket);
        Console.WriteLine(" Sent");
    }

    public void Close()
    {
        if (client.IsConnected())
        {
            client.Shutdown(SocketShutdown.Both);
        }
        client.Close();
    }
}