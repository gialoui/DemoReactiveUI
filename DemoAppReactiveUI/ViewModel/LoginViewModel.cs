using DemoAppReactiveUI.DataAccess;
using DemoAppReactiveUI.Model;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DemoAppReactiveUI.ViewModel
{
    public class LoginViewModel : ReactiveObject
    {
        public ReactiveCommand<int, Unit> ExecuteClickNumPad { get; protected set; }
        public ReactiveCommand<Unit, Unit> ExecuteClickClear { get; protected set; }
        public ReactiveCommand<string, User> ExecuteVerifyPINText { get; protected set; }

        private string _PINText = "----";

        public string PINText
        {
            get { return _PINText; }
            set { this.RaiseAndSetIfChanged(ref _PINText, value); }
        }

        public LoginViewModel()
        {
            ExecuteClickClear = ReactiveCommand.Create(ClearPINText);
            var canExecute = this.WhenAnyValue(v => v.PINText).Select(PINText => !PINText.Contains("-"));
            ExecuteClickNumPad = ReactiveCommand.Create<int>(
                number => AddPIN(number)
            );

            ExecuteVerifyPINText = ReactiveCommand.CreateFromTask<string, User>(
                pin => VerifyPIN(pin), canExecute
            );
        }

        private void AddPIN(int number)
        {
            var PIN = updatePINText(number);
        }

        private string updatePINText(int number)
        {
            var PIN = PINText;

            for (int i = 0; i < PIN.Length; i++)
            {
                if (PIN[i] == '-')
                {
                    PIN = PIN.Remove(i, 1).Insert(i, number + "");
                    break;
                }
            }
            PINText = PIN;
            return PIN;
        }

        private async Task<User> VerifyPIN(string PIN)
        {
            var loginUser = await Task.Run(() => UserDA.GetEnableUserByPIN(PIN));

            if (loginUser == null)
            {
                PINText = "----";
            }

            return loginUser;
        }

        private void ClearPINText()
        {
            PINText = "----";
        }
    }
}