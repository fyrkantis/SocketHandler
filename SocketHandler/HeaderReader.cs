﻿using System.Net.Sockets;
using System.Text;

public class HeaderReader
{
    public string? method;
    public Route? route;
    public string? protocol;
    public byte[] raw = new byte[0];
    public Dictionary<string, string> headers = new Dictionary<string, string>();

    public HeaderReader(Socket socket)
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