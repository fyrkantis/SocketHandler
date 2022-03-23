using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;

public class PythonRouter
{
    public Socket server;

    public PythonRouter(IPAddress ipAddress)
    {
        // Connects to local server.
        IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, 10000);
        server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        server.Connect(serverEndPoint);
        if (server.IsConnected())
        {
            Console.Write(" Connected");
        }
    }

    void CopySocketStream(Socket location, Socket target, HeaderBytes headers)
    {
        if (!target.IsConnected())
        {
            Console.Write("Target not connected");
            return;
        }
        target.Send(headers.raw);
        int contentLength = -1;
        if (headers.headers.TryGetValue("content-length", out string? contentLengthString))
        {
            int.TryParse(contentLengthString, out contentLength);
        }
        Console.Write(" Content length: " + contentLength.ToString());

        if (contentLength == -1)
        {
            Console.Write(" No body");
            return;
        }

        byte[] buffer = new byte[1024];
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
            Console.Write(readLength.ToString() + ", ");

            if (!target.IsConnected())
            {
                Console.Write("\nTarget aborted");
                return;
            }
            target.Send(buffer);

            if (readLength >= contentLength)
            {
                Console.Write("\nContent length reached");
                break;
            }
        }
    }

    public HeaderBytes? SendSocketStream(Socket socket, HeaderBytes socketHeaders)
    {
        if (!server.IsConnected())
        {
            return null;
        }

        // Sends user data.
        CopySocketStream(socket, server, socketHeaders);
        Console.Write(" Received");

        HeaderBytes serverHeaders = new HeaderBytes(server);

        // Sends server data.
        CopySocketStream(server, socket, serverHeaders);
        Console.WriteLine(" Sent");

        return serverHeaders;
    }

    public void Close()
    {
        if (server.IsConnected())
        {
            server.Shutdown(SocketShutdown.Both);
        }
        server.Close();
    }
}