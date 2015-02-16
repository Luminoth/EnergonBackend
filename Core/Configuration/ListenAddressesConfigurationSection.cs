using System.Configuration;

namespace EnergonSoftware.Core.Configuration
{
    public class ListenAddressesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("maxConnections", DefaultValue = -1)]
        public int MaxConnections { get { return (int)this["maxConnections"]; } }

        [ConfigurationProperty("backlog", DefaultValue = 10)]
        public int Backlog { get { return (int)this["backlog"]; } }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ListenAddressConfigurationElementCollection ListenAddresses
        {
            get { return (ListenAddressConfigurationElementCollection)base[string.Empty]; }
        }
    }
}
