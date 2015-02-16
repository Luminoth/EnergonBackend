using System.Configuration;

namespace EnergonSoftware.Core.Configuration
{
    [ConfigurationCollection(typeof(ListenAddressConfigurationElement))]
    public class ListenAddressConfigurationElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        { get { return ConfigurationElementCollectionType.BasicMapAlternate; } }

        protected override string ElementName { get { return "listenAddress"; } }

        protected override ConfigurationElement CreateNewElement()
        { return new ListenAddressConfigurationElement(); }

        protected override object GetElementKey(ConfigurationElement element)
        { return ((ListenAddressConfigurationElement)element).Name; }
    }
}
