using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Chat;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Chat
{
    internal sealed class FriendListMessageHandler : MessageHandler
    {
        internal FriendListMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            FriendListMessage friendListMessage = (FriendListMessage)message;
            ChatSession chatSession = (ChatSession)session;

            chatSession.SetFriendList(friendListMessage.Friends);
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
