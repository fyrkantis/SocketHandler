using SocketHandler;
using System.Net.Sockets;
using System.Text;

public class HeaderClass
{
    public string? protocol;
    public Dictionary<string, string> headers = new Dictionary<string, string>();
}

public class HeaderGenerator : HeaderClass
{
    public string status;
    public string? body;

    public HeaderGenerator(FileInfo fileInfo) // For sending a file and standard message.
    {
        protocol = "HTTP/1.0";
        status = "200 All good buckaroo";
        AddFileInfo(fileInfo);
    }

    public HeaderGenerator(string message, string? plainBody) // For sending a message and no file.
    {
        protocol = "HTTP/1.0";
        status = message;
        if (plainBody != null)
        {
            body = plainBody;
            headers.Add("Content-length", Encoding.ASCII.GetByteCount(plainBody).ToString());
            headers.Add("Contnet-type", "text/plain; charset=ascii");
        }
    }

    public void AddFileInfo(FileInfo fileInfo)
    {
        headers.Add("Content-length", fileInfo.Length.ToString());
        headers.Add("Content-disposition", "inline; filename = " + fileInfo.Name);
        headers.Add("Content-type", MimeTypes.GetMimeType(fileInfo.Name) + "; charset=utf-8");
    }

    public string GetString()
    {
        string str = protocol + " " + status;
        foreach (KeyValuePair<string, string> header in headers)
        {
            str += "\r\n" + header.Key + ": " + header.Value;
        }
        str += "\r\n\r\n";
        if (body != null)
        {
            str += body;
        }
        Console.WriteLine(str);
        return str;
    }

    public byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(GetString());
    }
}

public class HeaderReader : HeaderClass
{
    public string? method;
    public Route? route;
    public byte[] raw = new byte[0];

    public HeaderReader(Socket socket)
    {
        Console.WriteLine();
        List<byte> rawList = new List<byte>();
        while (true)
        {
            byte[] rawData = ReadToNewline(socket);
            rawList.AddRange(rawData);
            string data = Encoding.ASCII.GetString(rawData);
            if (string.IsNullOrWhiteSpace(data))
            {
                break;
            }

            string[] dataParts = data.Split(':', 2);
            if (dataParts.Length >= 1 && !string.IsNullOrWhiteSpace(dataParts[0]))
            {
                if (dataParts.Length >= 2 && !string.IsNullOrWhiteSpace(dataParts[1]))
                {
                    headers.Add(dataParts[0].Trim().ToLower(), dataParts[1].Trim());
                }
                else
                {
                    dataParts = data.Split(" ", 3);
                    if (dataParts.Length >= 1 && !string.IsNullOrWhiteSpace(dataParts[0]))
                    {
                        method = dataParts[0];
                        if (dataParts.Length >= 2 && !string.IsNullOrWhiteSpace(dataParts[1]))
                        {
                            route = new Route(dataParts[1]);
                            if (dataParts.Length >= 3 && !string.IsNullOrWhiteSpace(dataParts[2]))
                            {
                                protocol = dataParts[2];
                            }
                        }
                    }
                }
            }
        }
        raw = rawList.ToArray();
    }

    byte[] ReadToNewline(Socket socket)
    {
        int bufferSize = 1;
        byte[] rawBuffer = new byte[bufferSize];
        List<byte> rawDataList = new List<byte>();

        while (true)
        {
            int bufferLength = socket.Receive(rawBuffer, bufferSize, SocketFlags.None);
            rawDataList.AddRange(rawBuffer);
            string buffer = Encoding.ASCII.GetString(rawBuffer);
            Console.Write(buffer);
            if (bufferLength == 0 || buffer[0] == '\n')
            {
                return rawDataList.ToArray();
            }
        }
    }
}