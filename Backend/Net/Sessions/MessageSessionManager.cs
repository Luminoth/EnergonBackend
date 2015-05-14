using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Packet;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Backend.Net.Sessions
{
    public class MessageSessionManager : NetworkSessionManager
    {
        public async Task BroadcastMessageAsync(MessagePacket packet, string formatterType)
        {
            using(MemoryStream buffer = new MemoryStream()) {
                await packet.SerializeAsync(buffer, formatterType).ConfigureAwait(false);
                await BroadcastAsync(buffer).ConfigureAwait(false);
            }
        }
    }
}
