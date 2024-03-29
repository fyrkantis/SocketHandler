﻿using System.Net;
using System.Net.Sockets;

static class Program
{
	static void Main()
	{
		Server server = new Server();
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

			HeaderReader reader = new HeaderReader(handler);

			if (reader.route == null)
			{
				Console.Write(" No route!");
				continue;
			}
			if (reader.route.parts.Length <= 0)
			{
				handler.Send(new HeaderGenerator("308 The thing is elsewhere", new Dictionary<string, string> { { "Location", "/index.html" } }).GetBytes());
				continue;
			}
			if (reader.route.parts[0].ToLower() == "wiki")
			{
				PythonRouter pythonRouter = new PythonRouter(ipAddress);
				if (!pythonRouter.server.IsConnected())
				{
					Console.WriteLine(" Couldn't connect to internal server.");
					continue;
				}
				pythonRouter.SendSocketStream(handler, reader);
				pythonRouter.Close();
			}
			else
			{
				server.GetFile(handler, reader);
			}

			if (handler.IsConnected())
			{
				handler.Shutdown(SocketShutdown.Both);
			}
			handler.Close();
		}
	}
}
