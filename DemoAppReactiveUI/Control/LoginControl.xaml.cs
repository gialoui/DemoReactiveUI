using DemoAppReactiveUI.DataAccess;
using DemoAppReactiveUI.Model;
using System.Windows;
using System.Windows.Controls;

namespace DemoAppReactiveUI.Control
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private void AddPIN(int number)
        {
            var PIN = updatePINText(number);

            if (PIN.Contains("-") == false && PIN.Length >= 4)
            {
                verifyPIN(PIN);
            }
        }

        private string updatePINText(int number)
        {
            var PIN = PINText.Text;
            // find the most left "-" index
            for (int i = 0; i < PIN.Length; i++)
            {
                if (PIN[i] == '-')
                {
                    PIN = PIN.Remove(i, 1).Insert(i, number + "");
                    break;
                }
            }
            PINText.Text = PIN;
            return PIN;
        }

        private void verifyPIN(string PIN)
        {
            var loginUser = UserDA.GetEnableUserByPIN(PIN);
            if (loginUser == null)
            {
                MessageBox.Show("Sorry, PIN number doesn't exist.");
                PINText.Text = "----";
                return;
            }

            ProcessNewUserLogin(loginUser);
        }

        private void ProcessNewUserLogin(User newUser)
        {
            Window.GetWindow(this).Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddPIN(1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AddPIN(2);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AddPIN(3);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            AddPIN(4);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            AddPIN(5);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            AddPIN(6);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            AddPIN(7);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            AddPIN(8);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            AddPIN(9);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            AddPIN(0);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            PINText.Text = "----";
        }
    }
}