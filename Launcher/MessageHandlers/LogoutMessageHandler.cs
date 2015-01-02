﻿using System.Windows;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        internal LogoutMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            ((App)Application.Current).Logout();
        }
    }
}
