using DemoAppReactiveUI.Model;
using DemoAppReactiveUI.View;
using DemoAppReactiveUI.ViewModel;
using ReactiveUI;
using System;
using System.Reactive;
using System.Windows;

namespace DemoAppReactiveUI.Control
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : IViewFor<SearchViewModel>
    {
        public static readonly DependencyProperty _viewModel =
               DependencyProperty.Register("ViewModel", typeof(SearchViewModel), typeof(SearchControl));

        public SearchViewModel ViewModel
        {
            get { return GetValue(_viewModel) as SearchViewModel; }
            set { SetValue(_viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as SearchViewModel; }
        }

        public SearchControl()
        {
            InitializeComponent();
            ViewModel = new SearchViewModel();
            this.WhenActivated(d => Binding(d));
        }

        private void Binding(Action<IDisposable> d)
        {
            d(this.OneWayBind(ViewModel, vm => vm.Categories, v => v.Category.ItemsSource));
            d(this.Bind(ViewModel, vm => vm.SelectedCategory, v => v.Category.SelectedItem));

            d(this.OneWayBind(ViewModel, vm => vm.Products, v => v.ProductList.ItemsSource));
            d(this.Bind(ViewModel, vm => vm.SelectedProduct, v => v.ProductList.SelectedItem));

            d(this.Bind(ViewModel, vm => vm.Barcode, v => v.Barcode.Text));
            d(this.OneWayBind(ViewModel, vm => vm.Suppliers, v => v.Supplier.ItemsSource));
            d(this.Bind(ViewModel, vm => vm.Name, v => v.Name.Text));

            d(this.BindCommand(ViewModel, vm => vm.ExecuteResetFilters, x => x.ResetFilters));
            d(this.BindCommand(ViewModel, vm => vm.ExecuteSearch, x => x.Search));

            // Handle [Cancel] button
            d(this.BindCommand(ViewModel, vm => vm.ExecuteCancel, x => x.Cancel));
            d(this.WhenAnyObservable(v => v.ViewModel.ExecuteCancel).Subscribe(_ =>
            {
                var productInfoWindow = new InformationWindow();
                Window.GetWindow(this).Close();
                productInfoWindow.Show();
            }));

            // Handle [Select Product] button
            d(this.BindCommand(ViewModel, vm => vm.ExecuteSelectProduct, x => x.SelectProduct));

            //d(ViewModel.SearchProductResult.RegisterHandler(handle =>
            //{
            //    handle.SetOutput(Unit.Default);
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        SelectedProduct = handle.Input;
            //    });
            //}));

            //d(this.WhenAnyObservable(v => v.ViewModel.ExecuteSelectProduct).Subscribe(_ =>
            //{
            //    var selectedProduct = ViewModel.SelectedProduct;

            //    //foreach (Window item in Application.Current.Windows)
            //    //{
            //    //    if (item.Title == "Product Information")
            //    //    {
            //    //        var gk = (InformationWindow)item;

            //    //        try
            //    //        {
            //    //            //gk.Content.ViewModel.SelectedProduct = ViewModel.SelectedProduct;
            //    //        }
            //    //        catch (Exception ex)
            //    //        {
            //    //        }
            //    //    }
            //    //}

            //    var productInfoWindow = new ProductInfoControl(ViewModel.SelectedProduct);
            //    Window.GetWindow(this).Close();

            //    //if (productInfoWindow.ShowDialog() == true)
            //    //{
            //    //}
            //    //ViewModel.SelectedProduct;
            //}));
        }
    }
}