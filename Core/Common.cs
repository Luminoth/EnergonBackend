using System;
using System.IO;

namespace EnergonSoftware.Core
{
    public static class Common
    {
#region Auth Properties
        public const int AUTH_VERSION = 2;
        public const string DEFAULT_AUTH_REALM = "energonsoftware";
#endregion

#region Network Properties
        public const int DEFAULT_AUTH_PORT = 6788;
#endregion

        // TODO: read this from the registry/CLI/whatever
        public static string HomeDir { get { return Path.Combine("/", "EnergonSoftware"); } }
        public static string ConfDir { get { return Path.Combine(HomeDir, "etc"); } }
        public static string DataDir { get { return Path.Combine(HomeDir, "share"); } }

        public static void InitFilesystem()
        {
            Console.WriteLine("Initializing filesystem...");
            Directory.CreateDirectory(ConfDir);
            Directory.CreateDirectory(DataDir);
        }
    }
}
