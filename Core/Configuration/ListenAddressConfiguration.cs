using System;
using System.Configuration;
using System.Net;

namespace EnergonSoftware.Core.Configuration
{
    public class ListenAddressConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name { get { return (string)this["name"]; } }

        [ConfigurationProperty("address", IsRequired=true)]
        public string Address { get { return (string)this["address"]; } }
        public IPAddress IPAddress { get { return IPAddress.Parse(Address); } }

        [ConfigurationProperty("port", IsRequired=true)]
        public int Port { get { return(int)this["port"]; } }

        public override string ToString()
        {
            return "ListenAddress(Name=" + Name + ", Address=" + Address + ":" + Port + ")";
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
            get { return (ListenAddressConfigurationElementCollection)base[""]; }
        }
    }
}
