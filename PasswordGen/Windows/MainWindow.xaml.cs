using System.Windows;

using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.PasswordGen.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

#region UI Event Handlers
        private async void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateButton.IsEnabled = false;

            MD5.Text = await new MD5().DigestPasswordAsync(UserName.Text, Realm.Text, Password.Password);
            SHA512.Text = await new SHA512().DigestPasswordAsync(UserName.Text, Realm.Text, Password.Password);

            Password.Clear();
            GenerateButton.IsEnabled = true;
        }
#endregion
    }
}
