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

        private Action<string> _RegistrationService;

        // Using a DependencyProperty as the backing store for ValidationErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationErrorMessageProperty =
            DependencyProperty.Register("ValidationErrorMessage", typeof(string), typeof(ClientRegistrationWindow), new PropertyMetadata(""));


        public ClientRegistrationWindow(Action<string> registrationService, string validationErrorMessage = "")
        {
            InitializeComponent();
            DataContext = this;
            ValidationErrorMessage = validationErrorMessage;
            _RegistrationService = registrationService;
        }

        private void _proceedBtn_Click(object sender, RoutedEventArgs e)
        {
            _RegistrationService?.Invoke(_loginIdBox.Text);
            Close();
        }
    }
}
