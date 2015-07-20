using System.Configuration;

namespace EnergonSoftware.Core.Configuration
{
    /// <summary>
    /// A listenAddresses configuration section.
    /// Should contain a set of listenAddress elements.
    /// </summary>
    public class ListenAddressesConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets the maximum number of connections to allow.
        /// For an unlimited number of connections, set to -1.
        /// </summary>
        /// <value>
        /// The maximum number of connections to allow.
        /// </value>
        [ConfigurationProperty("maxConnections", DefaultValue = -1)]
        public int MaxConnections => (int)this["maxConnections"];

        /// <summary>
        /// Gets the socket backlog.
        /// </summary>
        /// <value>
        /// The socket backlog.
        /// </value>
        [ConfigurationProperty("backlog", DefaultValue = 10)]
        public int Backlog => (int)this["backlog"];

        /// <summary>
        /// Gets the listen addresses.
        /// </summary>
        /// <value>
        /// The listen addresses.
        /// </value>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ListenAddressConfigurationElementCollection ListenAddresses => (ListenAddressConfigurationElementCollection)base[string.Empty];
    }
}
