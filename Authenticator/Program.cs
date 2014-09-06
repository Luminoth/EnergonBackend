using log4net;
using log4net.Config;

namespace EnergonSoftware.Authenticator
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        private static void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        static void Main(string[] args)
        {
            ConfigureLogging();

            _logger.Info("Hello World!");
        }
    }
}
