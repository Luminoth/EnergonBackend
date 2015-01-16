﻿using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public interface IMessageSerializable
    {
        string Type { get; }

        // these may throw MessageException on error
        Task SerializeAsync(IMessageFormatter formatter);
        Task DeSerializeAsync(IMessageFormatter formatter);
    }
}
