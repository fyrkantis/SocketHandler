﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

static class Program
{
    // https://stackoverflow.com/a/722265/13347795
    public static bool IsConnected(this Socket socket)
    {
        try
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (SocketException)
        {
            return false;
        }
    }
    static void Main()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5000);
        Console.WriteLine("Listening on {0}.", localEndPoint.ToString());
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(localEndPoint);

        listener.Listen(10); // Starts listening on port with a max queue of 10.
        while (true)
        {
            Console.Write("Awaiting connection...");
            Socket handler = listener.Accept(); // Accepts connection on another port.
            Console.Write(" Starting");

            PythonRouter pythonRouter = new PythonRouter(ipAddress);
            pythonRouter.SendSocketStream(handler);

            if (!pythonRouter.client.IsConnected())
            {
                Console.WriteLine(" Couldn't connect to internal server.");
                continue;
            }

            if (handler.IsConnected())
            {
                handler.Shutdown(SocketShutdown.Both);
            }
            handler.Close();

            pythonRouter.Close();
        }
    }
}
