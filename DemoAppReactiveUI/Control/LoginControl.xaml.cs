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
            ViewModel = new LoginViewModel();
            this.WhenActivated(d => Binding(d));
        }

        private void Binding(Action<IDisposable> d)
        {
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button0, Observable.Return(0)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button1, Observable.Return(1)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button2, Observable.Return(2)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button3, Observable.Return(3)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button4, Observable.Return(4)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button5, Observable.Return(5)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button6, Observable.Return(6)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button7, Observable.Return(7)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button8, Observable.Return(8)));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickNumPad, x => x.Button9, Observable.Return(9)));

            d(this.BindCommand(ViewModel, vm => vm.ExecuteClickClear, x => x.Clear));
            d(this.Bind(ViewModel, vm => vm.PINText, v => v.PINText.Text));
            d(this.WhenAnyValue(v => v.ViewModel.PINText).Where(PINText => !PINText.Contains("-"))
                .InvokeCommand(ViewModel.ExecuteVerifyPINText));

            d(this.WhenAnyObservable(v => v.ViewModel.ExecuteVerifyPINText).Where(loginUser => loginUser != null)
                .Subscribe(_ =>
                {
                    Window.GetWindow(this).Close();
                }));

            d(this.WhenAnyObservable(v => v.ViewModel.ExecuteVerifyPINText).Where(loginUser => loginUser == null)
                .Subscribe(_ =>
                {
                    MessageBox.Show("Sorry, PIN number doesn't exist.");
                }));
        }
    }
}