using System.IO;

using log4net;

namespace EnergonSoftware.Core
{
    public static class Common
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Common));

#region Auth Properties
        public const int AUTH_VERSION = 2;
#endregion

        // TODO: read this from the registry/CLI/whatever
        public static string HomeDir { get { return Path.Combine("/", "EnergonSoftware"); } }
        public static string BinDir { get { return Path.Combine(HomeDir, "bin"); } }
        public static string ConfDir { get { return Path.Combine(HomeDir, "etc"); } }
        public static string DataDir { get { return Path.Combine(HomeDir, "share"); } }

        public static void InitFilesystem()
        {
            _logger.Info("Initializing filesystem...");
            Directory.CreateDirectory(BinDir);
            Directory.CreateDirectory(ConfDir);
            Directory.CreateDirectory(DataDir);
        }
    }
}
