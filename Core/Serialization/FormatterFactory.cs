using System;

using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Serialization.Formatters;

namespace EnergonSoftware.Core.Serialization
{
    /// <summary>
    /// Factory for formatters.
    /// </summary>
    public static class FormatterFactory
    {
        /// <summary>
        /// Creates a new formatter of the specified type.
        /// </summary>
        /// <param name="type">The formatter type.</param>
        /// <returns>The new formatter.</returns>
        /// <exception cref="System.ArgumentException">type</exception>
        public static IFormatter Create(string type)
        {
            switch(type)
            {
            case BinaryNetworkFormatter.FormatterType:
                return new BinaryNetworkFormatter();
            case JsonFormatter.FormatterType:
                return new JsonFormatter();
            case ProtoBufFormatter.FormatterType:
                return new ProtoBufFormatter();
            case XmlFormatter.FormatterType:
                return new XmlFormatter();
            }

            throw new ArgumentException(string.Format(Resources.ErrorUnsupportedFormatter, type), "type");
        }
    }
}
