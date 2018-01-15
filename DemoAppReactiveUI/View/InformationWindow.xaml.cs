using DemoAppReactiveUI.Model;
using System.Windows;

namespace DemoAppReactiveUI.View
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationWindow : Window
    {
        public InformationWindow()
        {
            InitializeComponent();
        }

        public InformationWindow(Product pd)
        {
            InitializeComponent();

            this.ProductInfoControl.SelectedProduct = pd;
        }
    }
}