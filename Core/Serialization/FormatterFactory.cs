using System;

using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Serialization.Formatters;

namespace EnergonSoftware.Core.Serialization
{
    public static class FormatterFactory
    {
        public static IFormatter Create(string type)
        {
            switch(type)
            {
            case BinaryNetworkFormatter.FormatterType:
                return new BinaryNetworkFormatter();
            case XmlFormatter.FormatterType:
                return new XmlFormatter();
            }

            throw new ArgumentException(string.Format(Resources.ErrorUnsupportedFormatter, type), "type");
        }
    }
}
