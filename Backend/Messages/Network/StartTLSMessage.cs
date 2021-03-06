﻿using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Network
{
    /// <summary>
    /// Requests TLS authentication
    /// </summary>
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public sealed class StartTLSMessage : Message
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "starttls";

        public override string Type => MessageType;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "StartTLSMessage()";
        }
    }
}
