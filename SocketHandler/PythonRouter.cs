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

	void CopySocketStream(Socket location, Socket target, HeaderReader reader)
	{
		if (!target.IsConnected())
		{
			Console.Write("Target not connected");
			return;
		}
		target.Send(reader.raw);
		int contentLength = -1;
		if (reader.headers.TryGetValue("content-length", out string? contentLengthString))
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

	public HeaderReader? SendSocketStream(Socket socket, HeaderReader socketReader)
	{
		if (!server.IsConnected())
		{
			return null;
		}

		// Sends user data.
		CopySocketStream(socket, server, socketReader);
		Console.Write(" Received");

		HeaderReader serverReader = new HeaderReader(server);

		// Sends server data.
		CopySocketStream(server, socket, serverReader);
		Console.WriteLine(" Sent");

		return serverReader;
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