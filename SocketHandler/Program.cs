using System.Net;
using System.Net.Sockets;

static class Program
{
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

            HeaderBytes headers = new HeaderBytes(handler);

            PythonRouter pythonRouter = new PythonRouter(ipAddress);
            if (!pythonRouter.server.IsConnected())
            {
                Console.WriteLine(" Couldn't connect to internal server.");
                continue;
            }
            pythonRouter.SendSocketStream(handler, headers);

            if (handler.IsConnected())
            {
                handler.Shutdown(SocketShutdown.Both);
            }
            handler.Close();

            pythonRouter.Close();
        }
    }
}
