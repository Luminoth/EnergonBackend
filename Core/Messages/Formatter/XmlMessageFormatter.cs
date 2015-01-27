﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public sealed class XmlMessageFormatter : IMessageFormatter
    {
        private const string Prefix = "msg";
        private const string Namespace = "energonsoftware";

        public const string FormatterType = "xml";
        public string Type { get { return FormatterType; } }

        private Stream _stream;
        private XmlWriter _writer;
        private XmlReader _reader;

        internal XmlMessageFormatter()
        {
        }

        public void Attach(Stream stream)
        {
            _stream = stream;

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Async = true;
            _writer = XmlWriter.Create(_stream, writerSettings);

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.Async = true;
            _reader = XmlReader.Create(_stream, readerSettings);
        }

#region Writing
        public async Task FlushAsync()
        {
            await _writer.FlushAsync().ConfigureAwait(false);
        }

        public async Task StartDocumentAsync()
        {
            //await _writer.WriteStartDocumentAsync().ConfigureAwait(false);
            await StartElementAsync("message").ConfigureAwait(false);
        }

        public async Task EndDocumentAsync()
        {
            await EndElementAsync().ConfigureAwait(false);
            //await _writer.WriteEndDocumentAsync().ConfigureAwait(false);
        }

        public async Task StartElementAsync(string name)
        {
            await _writer.WriteStartElementAsync(Prefix, name, Namespace).ConfigureAwait(false);
        }

        public async Task EndElementAsync()
        {
            await _writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        public async Task WriteAsync<T>(string name, IReadOnlyCollection<T> values) where T : IMessageSerializable
        {
            await StartElementAsync(name).ConfigureAwait(false);
            foreach(T value in values) {
                await WriteAsync(value).ConfigureAwait(false);
            }
            await EndElementAsync().ConfigureAwait(false);
        }

        public async Task WriteAsync(IMessageSerializable value)
        {
            await StartElementAsync(value.Type).ConfigureAwait(false);
            await value.SerializeAsync(this).ConfigureAwait(false);
            await EndElementAsync().ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, string value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, byte value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value.ToString()).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, bool value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value.ToString()).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, int value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value.ToString()).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, long value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value.ToString()).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, float value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value.ToString()).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, double value)
        {
            await _writer.WriteElementStringAsync(Prefix, name, Namespace, value.ToString()).ConfigureAwait(false);
        }

        public async Task WriteAsync(byte[] value, int offset, int count)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
#endregion

#region Reading
        public Task<List<T>> ReadListAsync<T>(string name) where T : IMessageSerializable, new()
        {
            throw new NotImplementedException();
        }

        public Task<T> ReadAsync<T>(string name) where T : IMessageSerializable, new()
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadStringAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<byte> ReadByteAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReadBoolAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadIntAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<long> ReadLongAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<float> ReadFloatAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<double> ReadDoubleAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task ReadAsync(byte[] value, int offset, int count)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
#endregion
    }
}