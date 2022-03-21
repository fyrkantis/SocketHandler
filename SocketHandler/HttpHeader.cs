using System.Net.Sockets;
using System.Text;

public class HttpHeader
{
    public byte[] raw = new byte[0];
    public int contentLength = -1;

    public HttpHeader(Socket socket)
    {
        GetHeaders(socket);
    }

    void GetHeaders(Socket socket)
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

            if (tryGetHeaderValue("Content-Length: ", data, out string valueString))
            {
                if (int.TryParse(valueString, out int number))
                {
                    contentLength = number;
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
            if (bufferLength == 0 || buffer[0] == '\n')
            {
                return rawDataList.ToArray();
            }
        }
    }

    bool tryGetHeaderValue(string name, string row, out string valueString)
    {
        if (row.StartsWith(name) && row.Length > name.Length)
        {
            valueString = row.Substring(name.Length);
            return true;
        }
        valueString = "";
        return false;
    }
}