using DemoAppReactiveUI.DataAccess;
using DemoAppReactiveUI.Model;
using DemoAppReactiveUI.ViewModel;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace DemoAppReactiveUI.Control
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : IViewFor<LoginViewModel>
    {
        public static readonly DependencyProperty _viewModel =
               DependencyProperty.Register("ViewModel", typeof(LoginViewModel), typeof(LoginControl));

        public LoginViewModel ViewModel
        {
            get { return GetValue(_viewModel) as LoginViewModel; }
            set { SetValue(_viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as LoginViewModel; }
        }

        public LoginControl()
        {
            InitializeComponent();
            this.WhenActivated(d => Binding(d));
        }

        private void Binding(Action<IDisposable> d)
        {
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button0, Observable.Return("0")));
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            PINText.Text = "----";
        }
    }
}