using System.Net.Http.Headers;
using System.Net.Sockets;

static class SocketMethods {
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

    /*public static HttpRequestHeaders ReadHeaders(this Socket socket)
    {
        
    }*/
}
