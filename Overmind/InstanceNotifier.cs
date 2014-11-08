using System.Net;

namespace EnergonSoftware.Overmind
{
    sealed class InstanceNotifier
    {
#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        public void Login(string username, EndPoint endpoint)
        {
        }

        public void Logout(string username)
        {
        }

        private InstanceNotifier()
        {
        }
    }
}
