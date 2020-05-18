using Codebot.Web;
using Raspberry.Device;

namespace WebTest
{
    [DefaultPage("home.html", IsTemplate = true)]
    public class HomePage : PageHandler
    {
        static ConsoleLcd console;

        static void Start()
        {
            var lcd = new CharacterLcd(23, 24, 25, 8, 7, 12);
            console = new ConsoleLcd(lcd);
        }

        [MethodPage("console")]
        public void WriteConsole()
        {
            console.WriteLine(Read("message"));
        }

        public static void Main(string[] args)
        {
            Start();
            Website.Run(args);
        }
    }
}
