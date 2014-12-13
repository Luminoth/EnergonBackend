using System;
using System.Configuration;
using System.Net;
using System.Text;

namespace EnergonSoftware.Core.Configuration
{
    public class ListenAddressConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name { get { return (string)this["name"]; } }

        [ConfigurationProperty("interface", IsRequired=true)]
        public string Interface { get { return (string)this["interface"]; } }
        public IPAddress InterfaceAddress { get { return IPAddress.Parse(Interface); } }

        [ConfigurationProperty("port", IsRequired=true)]
        public int Port { get { return(int)this["port"]; } }

        [ConfigurationProperty("multicastGroup")]
        public string MulticastGroup { get { return (string)this["multicastGroup"]; } }
        public IPAddress MulticastGroupIPAddress { get { return IPAddress.Parse(MulticastGroup); } }

        [ConfigurationProperty("multicastTTL", DefaultValue=1)]
        public int MulticastTTL { get { return (int)this["multicastTTL"]; } }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("ListenAddress(Username=" + Name + ", Interface=" + Interface + ":" + Port);
            if(!string.IsNullOrEmpty(MulticastGroup)) {
                builder.Append(", MulticastGroup=" + MulticastGroup + ", MulticastTTL=" + MulticastTTL);
            }
            builder.Append(")");
            return builder.ToString();
        }
    }

    [ConfigurationCollection(typeof(ListenAddressConfigurationElement))]
    public class ListenAddressConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override string ElementName { get { return "listenAddress"; } }

        public override ConfigurationElementCollectionType CollectionType
        { get { return ConfigurationElementCollectionType.BasicMapAlternate; } }

        protected override ConfigurationElement CreateNewElement()
        { return new ListenAddressConfigurationElement(); }

        protected override object GetElementKey(ConfigurationElement element)
        { return ((ListenAddressConfigurationElement)element).Name; }
    }

    public class ListenAddressesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection=true)]
        public ListenAddressConfigurationElementCollection ListenAddresses
        {
            get { return (ListenAddressConfigurationElementCollection)base[string.Empty]; }
        }
    }
}
