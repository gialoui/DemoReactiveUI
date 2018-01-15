using DemoAppReactiveUI.Model;
using ReactiveUI;
using System.Reactive;

namespace DemoAppReactiveUI.ViewModel
{
    public class ProductInfoViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> ExecuteSearch { get; protected set; }

        private readonly Interaction<Unit, Product> _searchProduct;
        public Interaction<Unit, Product> SearchProductResult => _searchProduct;

        private Product _SelectedProduct;

        public Product SelectedProduct
        {
            get { return _SelectedProduct; }
            set { this.RaiseAndSetIfChanged(ref _SelectedProduct, value); }
        }

        public ProductInfoViewModel()
        {
            _searchProduct = new Interaction<Unit, Product>();
            ExecuteSearch = ReactiveCommand.Create(() => { });
        }
    }
}