using System.Net.Sockets;
using System.Text;

public class HttpHeader
{
    public string raw = "";
    public int contentLength = -1;

    public HttpHeader(Socket socket)
    {
        GetHeaders(socket);
    }

    void GetHeaders(Socket socket)
    {
        while (true)
        {
            string data = ReadToNewline(socket);
            if (string.IsNullOrWhiteSpace(data))
            {
                break;
            }
            raw += data;
            if (tryGetHeaderValue("Content-Length: ", data, out string valueString))
            {
                if (int.TryParse(valueString, out int number))
                {
                    contentLength = number;
                }
            }
        }
    }

    string ReadToNewline(Socket socket)
    {
        int bufferSize = 1;
        byte[] buffer = new byte[bufferSize];
        string data = "";

        while (true)
        {
            socket.Receive(buffer, bufferSize, SocketFlags.None);
            string toAdd = Encoding.ASCII.GetString(buffer);
            Console.Write(toAdd);
            data += toAdd;
            if (toAdd[0] == '\n')
            {
                return data;
            }
        }
    }

    bool tryGetHeaderValue(string name, string row, out string valueString)
    {
        if (row.StartsWith(name) && row.Length > name.Length)
        {
            valueString = row.Substring(name.Length + 1);
            return true;
        }
        valueString = "";
        return false;
    }
}