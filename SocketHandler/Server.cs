using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
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
		if (fileInfo.Extension != ".html")
		{
			socket.Send(new HeaderGenerator(fileInfo).GetBytes());
			socket.SendFile(path);
		}
		else
		{
			ScriptObject script = new ScriptObject();
			script.Add("teeest", "aaa");
			TemplateContext context = new TemplateContext();
			context.TemplateLoader = new MyTemplateLoader();
			context.PushGlobal(script);

			Template template = Template.Parse(File.ReadAllText(path, Encoding.UTF8));
			byte[] bytes = Encoding.UTF8.GetBytes(template.Render(context));

			socket.Send(new HeaderGenerator(fileInfo, bytes.Length).GetBytes());
			socket.Send(bytes);
		}
	}

	public class MyTemplateLoader : ITemplateLoader
	{
		public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
		{
			return Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName.TrimEnd('\\') + "\\Website\\" + templateName.TrimStart('\\');
		}

		public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
		{
			return File.ReadAllText(templatePath, Encoding.UTF8);
		}

		public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
		{
			throw new NotImplementedException();
		}
	}
}