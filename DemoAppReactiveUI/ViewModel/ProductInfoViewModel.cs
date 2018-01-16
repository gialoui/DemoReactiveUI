using DemoAppReactiveUI.Model;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;

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
            ExecuteSearch = ReactiveCommand.CreateFromObservable(SearchImp);
        }

        public IObservable<Unit> SearchImp()
        {
            return Observable.Start(() =>
            {
                _searchProduct.Handle(Unit.Default).SubscribeOn(RxApp.MainThreadScheduler).Subscribe(p => {
                    SelectedProduct = p;
                });
            });
        }
    }
}