using System.ComponentModel;

namespace EnergonSoftware.Backend.Accounts
{
    /// <summary>
    /// Account visibility types
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// The account is offline
        /// </summary>
        [Description("Offline")]
        Offline,

        /// <summary>
        /// The account is online and available
        /// </summary>
        [Description("Online")]
        Online,

        /// <summary>
        /// The account is online but appears offline
        /// </summary>
        [Description("Invisible")]
        Invisible,

        /// <summary>
        /// The account is away
        /// </summary>
        [Description("Away")]
        Away,

        /// <summary>
        /// The account is busy
        /// </summary>
        [Description("Busy")]
        Busy,
    }

    /// <summary>
    /// Useful extensions to the Visibility enumeration
    /// </summary>
    public static class VisibilityExtensions
    {
        /// <summary>
        /// Determines whether the account is online.
        /// </summary>
        /// <param name="visibility">The visibility.</param>
        /// <returns>True if the account is online</returns>
        public static bool IsOnline(this Visibility visibility)
        {
            return Visibility.Offline != visibility && Visibility.Invisible != visibility;
        }
    }
}
