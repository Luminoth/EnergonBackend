﻿using System;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class OvermindSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            OvermindSession session = null;
            try {
                session = new OvermindSession(socket);
                session.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
                return session;
            } catch(Exception) {
                if(null != session) {
                    session.Dispose();
                }

                throw;
            }
        }
    }

    internal sealed class OvermindSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        public override string Name { get { return "overmind"; } }

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new OvermindMessageHandlerFactory();

        protected override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }

        public OvermindSession(Socket socket) : base(socket)
        {
        }

        protected async override Task<Account> LookupAccountAsync(string accountName)
        {
            Logger.Debug("Looking up account for accountName=" + accountName);
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.AccountName == accountName select a;
                if(accounts.Count() < 1) {
                    return null;
                }

                await Task.Delay(0).ConfigureAwait(false);
                return accounts.First().ToAccount();
            }
        }

        protected override MessagePacket CreatePacket(IMessage message)
        {
            return new NetworkPacket();
        }

        public async Task PingAsync()
        {
            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
        }
    }
}
