using System.Net;

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

        public void Authenticated(string username, string sessionId, EndPoint endpoint)
        {
        }

        private InstanceNotifier()
        {
        }
    }
}
