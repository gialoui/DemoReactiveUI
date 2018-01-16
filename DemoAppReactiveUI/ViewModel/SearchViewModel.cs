using DemoAppReactiveUI.Model;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace DemoAppReactiveUI.ViewModel
{
    public class SearchViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> ExecuteResetFilters { get; protected set; }
        public ReactiveCommand<Unit, Unit> ExecuteSelectProduct { get; protected set; }
        public ReactiveCommand<Unit, Unit> ExecuteSearch { get; protected set; }
        public ReactiveCommand<Unit, Unit> ExecuteCancel { get; protected set; }

        private readonly Interaction<Product, Unit> _searchResult;
        public Interaction<Product, Unit> SearchProductResult => _searchResult;

        private string _Barcode;

        public string Barcode
        {
            get { return _Barcode; }
            set { this.RaiseAndSetIfChanged(ref _Barcode, value); }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { this.RaiseAndSetIfChanged(ref _Name, value); }
        }

        private Product _SelectedProduct;

        public Product SelectedProduct
        {
            get { return _SelectedProduct; }
            set { this.RaiseAndSetIfChanged(ref _SelectedProduct, value); }
        }

        public ReactiveList<Product> Products = new ReactiveList<Product>();

        private readonly ReactiveList<Supplier> _Supplier = new ReactiveList<Supplier>();

        public IReadOnlyReactiveList<Supplier> Suppliers
        {
            get => _Supplier;
        }

        private Category _SelectedCategory;

        public Category SelectedCategory
        {
            get { return _SelectedCategory; }
            set { this.RaiseAndSetIfChanged(ref _SelectedCategory, value); }
        }

        private readonly ReactiveList<Category> _Category = new ReactiveList<Category>();

        public IReadOnlyReactiveList<Category> Categories
        {
            get => _Category;
        }

        // Constructor
        public SearchViewModel()
        {
            _searchResult = new Interaction<Product, Unit>();
            GetAllCategories();
            GetAllProducts();

            ExecuteResetFilters = ReactiveCommand.Create(ResetFilters);
            ExecuteSearch = ReactiveCommand.Create(SearchProduct);
            ExecuteSelectProduct = ReactiveCommand.Create(() => { });
            ExecuteCancel = ReactiveCommand.Create(() => { });
        }

        private void GetAllCategories()
        {
            var categoryList = Category.GetAllFromDB(ignoreEnableValue: true);
            categoryList.ForEach(x => _Category.Add(x));
        }

        private void GetAllProducts()
        {
            var productList = Product.GetAllProducts();
            productList.ForEach(x => Products.Add(x));
        }

        private void SearchProduct()
        {
            // Get the full list of Products
            Products.Clear();
            GetAllProducts();

            if (!string.IsNullOrEmpty(Name))
            {
                var pds = Products.Where(p => !p.name.ToLower().Contains(Name.Trim().ToLower())).ToList();

                if (pds != null)
                {
                    Products.RemoveAll(pds);
                }
            }

            if (!string.IsNullOrEmpty(Barcode))
            {
                var pds = Products.Where(p => !p.barcode.Contains(Barcode)).ToList();

                if (pds != null)
                {
                    Products.RemoveAll(pds);
                }
            }

            if (SelectedCategory != null)
            {
                var pds = Products.Where(p => !p.categoryID.ToString().Contains(SelectedCategory.ID.ToString())).ToList();

                if (pds != null)
                {
                    Products.RemoveAll(pds);
                }
            }
        }

        private void ResetFilters()
        {
            Name = "";
            Barcode = "";

            SelectedCategory = null;
        }
    }
}