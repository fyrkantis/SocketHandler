using System.Net.Sockets;
using System.Text;

class Server
{
    DirectoryInfo? projectDirectory;
    public Server()
    {
        projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
    }
    public void GetFile(Socket socket, HeaderReader reader)
    {
        
        if (reader.route == null)
        {
            socket.Send(Encoding.ASCII.GetBytes("HTTP / 1.0 400 You did bad\r\nContent-length: 47\r\n\r\n400: You did a bad request and I don't like it."));
            return;
        }
        if (projectDirectory == null)
        {
            socket.Send(Encoding.ASCII.GetBytes("HTTP / 1.0 500 AAAAAA\r\nContent-length: 25\r\n\r\n500: Wtf did you just do?"));
            return;
        }
        string path = projectDirectory.FullName.TrimEnd('\\') + "\\Website\\" + reader.route.raw.Replace('/', '\\').TrimStart('\\');
        Console.WriteLine(path);
        Console.WriteLine(Directory.Exists(path));
        if (!File.Exists(path))
        {
            socket.Send(Encoding.ASCII.GetBytes("HTTP / 1.0 404 Oh no\r\nContent-length: 34\r\n\r\n404: Oh no, there's nothing there."));
            return;
        }

        long fileSize = new FileInfo(path).Length;
        socket.Send(Encoding.ASCII.GetBytes("HTTP/1.0 200 All good buckaroo!\r\nContent-length: " + fileSize.ToString() + "\r\n\r\n"));
        socket.SendFile(path);
    }
}