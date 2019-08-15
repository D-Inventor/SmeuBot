using System.Threading.Tasks;

namespace SmeuArchief
{
    internal class Program
    {
        // entry point
        public static Task Main(string[] args) => Startup.RunAsync(args);
    }
}
