using SocketHandler;
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
            socket.Send(new HeaderGenerator("400 You did bad", "400: You did a bad request and I don't like it.").GetBytes());
            return;
        }
        if (projectDirectory == null)
        {
            socket.Send(new HeaderGenerator("500 AAAAAA", "500: Wtf did you just do?").GetBytes());
            return;
        }
        string path = projectDirectory.FullName.TrimEnd('\\') + "\\Website\\" + reader.route.raw.Replace('/', '\\').TrimStart('\\');

        if (!File.Exists(path))
        {
            socket.Send(new HeaderGenerator("404 Oh no", "404: Oh no, there's nothing there.").GetBytes());
            return;
        }

        FileInfo fileInfo = new FileInfo(path);

        socket.Send(new HeaderGenerator(fileInfo).GetBytes());
        socket.SendFile(path);
    }
}