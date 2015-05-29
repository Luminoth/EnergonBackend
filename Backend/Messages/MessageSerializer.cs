using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages
{
    public static class MessageSerializer
    {
        public static async Task SerializeAsync(IMessage message, IFormatter formatter)
        {
            await formatter.BeginAsync("message").ConfigureAwait(false);
            await message.SerializeAsync(formatter).ConfigureAwait(false);
            await formatter.FinishAsync().ConfigureAwait(false);
            await formatter.FlushAsync().ConfigureAwait(false);
        }
    }
}
