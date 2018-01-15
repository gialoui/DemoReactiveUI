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

            d(this.WhenAnyObservable(v => v.ViewModel.ExecuteSearch).Subscribe(_ =>
            {
                var newDialog = new SearchDialog();
                Window.GetWindow(this).Close();
                newDialog.ShowDialog();
            }));

            //d(this.ViewModel.SearchProductResult.RegisterHandler(interacion =>
            //{
            //    var searchDialog = new SearchDialog();
            //    var result = searchDialog.ShowDialog();
            //    interacion.SetOutput(searchDialog.SearchControl.SelectedProduct);
            //}));
        }
    }
}
