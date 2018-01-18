using DemoAppReactiveUI.Model;
using DemoAppReactiveUI.View;
using DemoAppReactiveUI.ViewModel;
using ReactiveUI;
using System;
using System.Windows;

namespace DemoAppReactiveUI.Control
{
    /// <summary>
    /// Interaction logic for ProductInfoControl.xaml
    /// </summary>
    public partial class ProductInfoControl : IViewFor<ProductInfoViewModel>
    {
        public static readonly DependencyProperty _viewModel =
               DependencyProperty.Register("ViewModel", typeof(ProductInfoViewModel), typeof(ProductInfoControl));

        public ProductInfoViewModel ViewModel
        {
            get { return GetValue(_viewModel) as ProductInfoViewModel; }
            set { SetValue(_viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ProductInfoViewModel; }
        }

        public Product SelectedProduct;

        public ProductInfoControl()
        {
            InitializeComponent();
            ViewModel = new ProductInfoViewModel();
            this.WhenActivated(d => Binding(d));
        }

        public ProductInfoControl(Product product)
        {
            ViewModel.SelectedProduct = product;
        }

        private void Binding(Action<IDisposable> d)
        {
            d(this.BindCommand(ViewModel, vm => vm.ExecuteSearch, x => x.SearchBtn));

            d(this.Bind(ViewModel, vm => vm.SelectedProduct.ID, v => v.ProductID.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.categoryName, v => v.Category.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.price, v => v.SellingPrice.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.priceCold, v => v.ColdPrice.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.IsOpenPrice, v => v.IsOpenPrice.IsChecked));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.supplierCode, v => v.SupplierCode.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.barcodeEx1, v => v.Barcode1.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.barcodeEx2, v => v.Barcode2.Text));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct.barcodeEx3, v => v.Barcode3.Text));

            d(this.Bind(ViewModel, vm => vm.SelectedProduct.priceSchedule, v => v.SchedulePrice.Text));

            d(this.ViewModel.SearchProductResult.RegisterHandler(interacion =>
            {
                var searchDialog = new SearchDialog();

                // ShowDialog: When this method is called, the code following it is not executed until after the dialog box is closed.
                var result = searchDialog.ShowDialog();

                // This block will not be executed until dialog is closed
                if (result ?? false)
                {
                    interacion.SetOutput(searchDialog.SelectedProduct);
                }
                else
                {
                    interacion.SetOutput(null);
                }
            }));
        }
    }
}