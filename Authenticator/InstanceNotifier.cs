using System.Net;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Authenticator
{
    sealed class InstanceNotifier
    {
#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        public void Authenticating(string username, EndPoint endpoint)
        {
        }

        public void Authenticated(string username, SessionId sessionId, EndPoint endpoint)
        {
        }

        private InstanceNotifier()
        {
        }
    }
}
