﻿using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    sealed class FailureMessageHandler : MessageHandler
    {
        private readonly AuthSession _session;

        internal FailureMessageHandler(AuthSession session)
        {
            _session = session;
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    FailureMessage failure = (FailureMessage)message;
                    _session.AuthFailed(failure.Reason);
                }
            );
        }
    }
}
