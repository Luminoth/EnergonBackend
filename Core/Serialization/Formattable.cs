using System.Threading.Tasks;

namespace EnergonSoftware.Core.Serialization
{
    public interface IFormattable
    {
        string Type { get; }

        Task SerializeAsync(IFormatter formatter);
        Task DeserializeAsync(IFormatter formatter);
    }
}
