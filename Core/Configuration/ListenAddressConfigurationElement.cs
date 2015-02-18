using System.Configuration;
using System.Net;
using System.Text;

namespace EnergonSoftware.Core.Configuration
{
    public class ListenAddressConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name { get { return (string)this["name"]; } }

        [ConfigurationProperty("interface", IsRequired = true)]
        public string Interface { get { return (string)this["interface"]; } }
        public IPAddress InterfaceAddress { get { return IPAddress.Parse(Interface); } }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port { get { return(int)this["port"]; } }

        [ConfigurationProperty("multicastGroup")]
        public string MulticastGroup { get { return (string)this["multicastGroup"]; } }
        public IPAddress MulticastGroupIPAddress { get { return IPAddress.Parse(MulticastGroup); } }

        [ConfigurationProperty("multicastTTL", DefaultValue = 1)]
        public int MulticastTTL { get { return (int)this["multicastTTL"]; } }

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
