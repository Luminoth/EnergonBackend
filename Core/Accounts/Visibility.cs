using System.ComponentModel;

namespace EnergonSoftware.Core.Accounts
{
    public enum Visibility
    {
        [Description("Offline")]
        Offline,

        [Description("Online")]
        Online,

        [Description("Invisible")]
        Invisible,

        [Description("Away")]
        Away,

        [Description("Busy")]
        Busy,
    }

    public static class VisibilityExtensions
    {
        public static bool IsOnline(this Visibility visibility)
        {
            return Visibility.Offline != visibility && Visibility.Invisible != visibility;
        }
    }
}
