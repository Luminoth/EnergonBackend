﻿using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    [Serializable]
    public sealed class SuccessMessage : IMessage
    {
        public const string MessageType = "success";
        public string Type { get { return MessageType; } }

        public string SessionId { get; set; }

        public SuccessMessage()
        {
            SessionId = string.Empty;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "SuccessMessage(SessionId=" + SessionId + ")";
        }
    }
}
