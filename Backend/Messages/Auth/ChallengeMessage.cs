﻿using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication challenge
    /// </summary>
    [Serializable]
    public sealed class ChallengeMessage : Message
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "challenge";

        public override string Type => MessageType;

        /// <summary>
        /// Gets or sets the challenge.
        /// </summary>
        /// <value>
        /// The challenge.
        /// </value>
        public string Challenge { get; set; } = string.Empty;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Challenge", Challenge).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            Challenge = await formatter.ReadStringAsync("Challenge").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"ChallengeMessage(Challenge={Challenge})";
        }
    }
}
