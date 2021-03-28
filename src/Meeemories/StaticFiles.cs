using System.IO;

namespace Meeemories
{
    public class StaticFiles
    {
        public static string Path(string file)
        {
            return System.IO.Path.Combine(Directory.GetParent(typeof(Startup).Assembly.Location).FullName, "../", file);
        }
    }
}