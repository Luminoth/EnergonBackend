using System.Configuration;

namespace EnergonSoftware.Core.Configuration
{
    /// <summary>
    /// A collection of ListenAddressConfigurationElements.
    /// </summary>
    [ConfigurationCollection(typeof(ListenAddressConfigurationElement))]
    public class ListenAddressConfigurationElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => "listenAddress";

        protected override ConfigurationElement CreateNewElement()
        { return new ListenAddressConfigurationElement(); }

        protected override object GetElementKey(ConfigurationElement element)
        { return ((ListenAddressConfigurationElement)element).Name; }
    }
}
