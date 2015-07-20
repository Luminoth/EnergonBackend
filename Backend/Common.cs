namespace EnergonSoftware.Backend
{
    /// <summary>
    /// Common backend/frontend data
    /// </summary>
    public static class Common
    {
#region Auth Properties
        /// <summary>
        /// The authentication protocol version
        /// </summary>
        public const int AuthProtocolVersion = 2;
#endregion

        // TODO: all this garbage should be handled by the installer(s)
#if false
        public const string AppDir = "EnergonSoftware";

        // TODO: read this from the registry/CLI/whatever
        // and really this probably has no business existing in Core
        // maybe should go in some sort of "common" lib
        private static string _installDir = Path.Combine("/", AppDir);
        public static string InstallDir { get { return _installDir; } set { _installDir = value; } }

        public static string BinDir { get { return Path.Combine(InstallDir, "bin"); } }
        public static string EtcDir { get { return Path.Combine(InstallDir, "etc"); } }
        public static string UsrDir { get { return Path.Combine(InstallDir, "usr"); } }
        public static string VarDir { get { return Path.Combine(InstallDir, "var"); } }

        public static string ConfDir { get { return Path.Combine(EtcDir, AppDir); } }
        public static string DataDir { get { return Path.Combine(Path.Combine(UsrDir, "share"), AppDir); } }
        public static string LogDir { get { return Path.Combine(Path.Combine(VarDir, "log"), AppDir); } }

        public static async Task InitFilesystemAsync()
        {
            Logger.Info("Initializing filesystem...");

            await DirectoryExtensions.CreateDirectoryAsync(InstallDir).ConfigureAwait(false);

            await DirectoryExtensions.CreateDirectoryAsync(BinDir).ConfigureAwait(false);
            await DirectoryExtensions.CreateDirectoryAsync(EtcDir).ConfigureAwait(false);
            await DirectoryExtensions.CreateDirectoryAsync(UsrDir).ConfigureAwait(false);
            await DirectoryExtensions.CreateDirectoryAsync(VarDir).ConfigureAwait(false);

            await DirectoryExtensions.CreateDirectoryAsync(ConfDir).ConfigureAwait(false);
            await DirectoryExtensions.CreateDirectoryAsync(DataDir).ConfigureAwait(false);
            await DirectoryExtensions.CreateDirectoryAsync(LogDir).ConfigureAwait(false);
        }
#endif
    }
}
