﻿using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            OvermindSession overmindSession = session as OvermindSession;
            if(null == overmindSession) {
                return;
            }

            await ClientState.Instance.OnLoggedInAsync(true).ConfigureAwait(false);
        }
    }
}
