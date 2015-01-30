using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace EnergonSoftware.Launcher
{
    internal enum AuthenticationStage
    {
        NotAuthenticated,
        Begin,
        Challenge,
        Finalize,
        Authenticated,
    }

    internal sealed class ClientState : INotifyPropertyChanged
    {
        public static readonly ClientState Instance = new ClientState();

        public bool UseDummyNetwork { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["dummyNetwork"]); } }

        public AuthenticationStage AuthStage { get; set; }
        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        // *** move these
        private string _news = "Checking for news updates...";
        public string News { get { return _news; } set { _news = value; NotifyPropertyChanged(); } }

        private string _username;
        public string Username { get { return _username; } set { _username = value; NotifyPropertyChanged(); } }

        private string _password;
        public string Password { get { return _password; } set { _password = value; NotifyPropertyChanged(); } }

        public string Ticket { get; set; }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private ClientState()
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
        }
    }
}
