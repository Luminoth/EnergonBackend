﻿using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class VisibilityMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(Message message, NetworkSession session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
