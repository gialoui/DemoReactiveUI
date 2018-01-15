using DemoAppReactiveUI.Model;
using System;
using System.Collections.Generic;

namespace DemoAppReactiveUI.Helper
{
    public class MySupplierNameComparer : IEqualityComparer<Supplier>
    {
        public bool Equals(Supplier x, Supplier y)
        {
            return string.Equals(x.name, y.name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Supplier obj)
        {
            return obj.name.GetHashCode();
        }
    }

    public class MyCategoryNameComparer : IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            return string.Equals(x.name, y.name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Category obj)
        {
            return obj.name.GetHashCode();
        }
    }

    public class MyProductBarcodeComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product x, Product y)
        {
            return string.Equals(x.barcode.Trim(), y.barcode.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Product obj)
        {
            return obj.barcode.Trim().GetHashCode();
        }
    }
}