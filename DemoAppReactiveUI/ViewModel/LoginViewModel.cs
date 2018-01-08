using ReactiveUI;
using System.Reactive;
using static System.Net.Mime.MediaTypeNames;

namespace DemoAppReactiveUI.ViewModel
{
    public class LoginViewModel : ReactiveObject
    {
        public ReactiveCommand<string, Unit> ExecuteClickNumPad { get; protected set; }

        ObservableAsPropertyHelper<Text> _PINText;
        public Text _PINText => _PINText.Value;

        public LoginViewModel()
        {
            
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
    }
}
