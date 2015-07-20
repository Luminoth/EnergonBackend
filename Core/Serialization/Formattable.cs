using System.Threading.Tasks;

namespace EnergonSoftware.Core.Serialization
{
    /// <summary>
    /// Marks an object as formattable.
    /// </summary>
    public interface IFormattable
    {
        /// <summary>
        /// Gets the object type.
        /// </summary>
        /// <value>
        /// The object type.
        /// </value>
        string Type { get; }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="formatter">The formatter to use.</param>
        Task SerializeAsync(IFormatter formatter);

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="formatter">The formatter to use.</param>
        Task DeserializeAsync(IFormatter formatter);
    }
}
