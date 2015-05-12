using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Chat;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Chat
{
    internal sealed class FriendListMessageHandler : MessageHandler
    {
        internal FriendListMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            FriendListMessage friendListMessage = (FriendListMessage)message;
            ChatSession chatSession = (ChatSession)session;

            chatSession.SetFriendList(friendListMessage.Friends);
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
