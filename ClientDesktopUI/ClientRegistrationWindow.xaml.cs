using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClientDesktopUI
{
    /// <summary>
    /// Interaction logic for ClientRegistrationWindow.xaml
    /// </summary>
    public partial class ClientRegistrationWindow : Window
    {


        public string ValidationErrorMessage
        {
            get { return (string)GetValue(ValidationErrorMessageProperty); }
            set { SetValue(ValidationErrorMessageProperty, value); }
        }

        private Action<string, string> _RegistrationService;
        private Action<string, string> _LoginService;

        // Using a DependencyProperty as the backing store for ValidationErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationErrorMessageProperty =
            DependencyProperty.Register("ValidationErrorMessage", typeof(string), typeof(ClientRegistrationWindow), new PropertyMetadata(""));


        public ClientRegistrationWindow(Action<string, string> signUpService, Action<string, string> loginService, string validationErrorMessage = "")
        {
            InitializeComponent();
            DataContext = this;
            ValidationErrorMessage = validationErrorMessage;
            _RegistrationService = signUpService;
            _LoginService = loginService;
            _loginEmailBox.Text = Properties.Settings.Default.Email;
            _loginPasswordBox.Password = Properties.Settings.Default.Password;
        }

        private void _signupBtn_Click(object sender, RoutedEventArgs e)
        {
            if(_signupEmailBox.Text.Length > 5 && _signupPasswordBox.Password.Length > 5)
            {
                _RegistrationService?.Invoke(_signupEmailBox.Text.Trim(), _signupPasswordBox.Password.Trim());
                if (_signupRememberMe.IsChecked ?? false)
                {
                    Properties.Settings.Default.Email = _signupEmailBox.Text.Trim();
                    Properties.Settings.Default.Password = _signupPasswordBox.Password.Trim();
                }
                else
                {
                    Properties.Settings.Default.Email = string.Empty;
                    Properties.Settings.Default.Password = string.Empty;
                }
                Close();
            }
            else
            {
                MessageBox.Show("Invalid entry to the text boxes");
            }
        }

        private void _loginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_loginEmailBox.Text.Length > 5 && _loginPasswordBox.Password.Length > 5)
            {
                _LoginService?.Invoke(_loginEmailBox.Text.Trim(), _loginPasswordBox.Password.Trim());
                if (_loginRememberMe.IsChecked ?? false)
                {
                    Properties.Settings.Default.Email = _loginEmailBox.Text.Trim();
                    Properties.Settings.Default.Password = _loginPasswordBox.Password.Trim();
                }
                else
                {
                    Properties.Settings.Default.Email = string.Empty;
                    Properties.Settings.Default.Password = string.Empty;
                }
                Close();
            }
            else
            {
                MessageBox.Show("Invalid entry to the text boxes");
            }
        }
    }
}
