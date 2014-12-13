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
}
