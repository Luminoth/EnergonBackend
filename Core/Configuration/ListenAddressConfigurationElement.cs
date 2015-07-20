using System.Configuration;
using System.Net;
using System.Text;

namespace EnergonSoftware.Core.Configuration
{
    /// <summary>
    /// A listenAddress configuration element
    /// </summary>
    public class ListenAddressConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the listen address name.
        /// </summary>
        /// <value>
        /// The listen address name name.
        /// </value>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name => (string)this["name"];

        /// <summary>
        /// Gets the interface (IP address) to listen on.
        /// </summary>
        /// <value>
        /// The interface (IP address) to listen on.
        /// </value>
        [ConfigurationProperty("interface", IsRequired = true)]
        public string Interface => (string)this["interface"];

        /// <summary>
        /// Gets the interface as an IPAddress.
        /// </summary>
        /// <value>
        /// The interface as an IPAddress.
        /// </value>
        public IPAddress InterfaceAddress => IPAddress.Parse(Interface);

        /// <summary>
        /// Gets the port to listen on.
        /// </summary>
        /// <value>
        /// The port to listen on.
        /// </value>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port => (int)this["port"];

        /// <summary>
        /// Gets the multicast group (IP address) to join.
        /// </summary>
        /// <value>
        /// The multicast group (IP address) to join.
        /// </value>
        [ConfigurationProperty("multicastGroup")]
        public string MulticastGroup => (string)this["multicastGroup"];

        /// <summary>
        /// Gets the multicast group as an IPAddress.
        /// </summary>
        /// <value>
        /// The multicast group as an IPAddress.
        /// </value>
        // ReSharper disable once InconsistentNaming
        public IPAddress MulticastGroupIPAddress => IPAddress.Parse(MulticastGroup);

        /// <summary>
        /// Gets the multicast time to live (TTL).
        /// </summary>
        /// <value>
        /// The multicast time to live (TTL).
        /// </value>
        [ConfigurationProperty("multicastTTL", DefaultValue = 1)]
        // ReSharper disable once InconsistentNaming
        public int MulticastTTL => (int)this["multicastTTL"];

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("ListenAddress(Name=" + Name + ", Interface=" + Interface + ":" + Port);
            if(!string.IsNullOrEmpty(MulticastGroup)) {
                builder.Append(", MulticastGroup=" + MulticastGroup + ", MulticastTTL=" + MulticastTTL);
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}
