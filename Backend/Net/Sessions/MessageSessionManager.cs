using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Packet;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Backend.Net.Sessions
{
    public class MessageSessionManager : NetworkSessionManager
    {
        public async Task BroadcastMessageAsync(IMessage message)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _sessions.ForEach(session => {
                    MessageSession messageSession = (MessageSession)session;
                    tasks.Add(messageSession.SendMessageAsync(message));
                });
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
