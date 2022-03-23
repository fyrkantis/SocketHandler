public class Route
{
    public string raw;
    public string[] parts;

    public Route(string thing)
    {
        raw = thing;
        string[] rawParts = thing.Split('/');
        List<string> partsList = new List<string>();
        for (int i = 0; i < rawParts.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(rawParts[i]))
            {
                partsList.Add(rawParts[i]);
            }
        }
        parts = partsList.ToArray();
    }
}
