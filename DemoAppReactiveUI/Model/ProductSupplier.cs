using DemoAppReactiveUI.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace DemoAppReactiveUI.Model
{
    [Table("product_supplier", Schema = "public")]
    public class ProductSupplier : INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static double DEFAULT_INVALID_COST_PRICE = -1;

        [JsonIgnore]
        public Dictionary<string, dynamic> ChangedPropertiesList = new Dictionary<string, dynamic>();

        [JsonIgnore]
        public bool IsSaveLog = false;

        private double _cost;
        private bool _active;
        private bool _enable;
        private string _supplierCode;
        private string _info;
        private bool _taxApplied;

        [JsonIgnore]
        [Column("product_id", Order = 0), Key]
        public int productID { get; set; }

        [JsonIgnore]
        [Column("supplier_id", Order = 1), Key]
        public int supplierID { get; set; }

        [JsonIgnore]
        [Column("user_id")]
        public int? userID { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        [Column("uuid")]
        public Guid? UUID { get; set; }

        [JsonProperty(PropertyName = "cost")]
        [Column("cost")]
        public double cost
        {
            get
            {
                return _cost;
            }
            set
            {
                double oldValue = _cost;
                _cost = value;
                if (oldValue != value)
                    NotifyPropertyChanged("cost");
            }
        }

        [JsonProperty(PropertyName = "tax_applied")]
        [Column("tax_applied")]
        public bool taxApplied
        {
            get { return _taxApplied; }
            set
            {
                bool oldValue = _taxApplied;
                _taxApplied = value;
                if (oldValue != value) NotifyPropertyChanged("taxApplied");
            }
        }

        [JsonProperty(PropertyName = "active")]
        [Column("active")]
        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                bool oldValue = _active;
                _active = value;
                if (oldValue != value) NotifyPropertyChanged("active");
            }
        }

        [JsonIgnore]
        [Column("last_edit")]
        public DateTime lastEdit { get; set; }

        [JsonProperty(PropertyName = "supplier_code")]
        [Column("supplier_code")]
        public string supplierCode
        {
            get
            {
                return _supplierCode;
            }
            set
            {
                string oldValue = _supplierCode;
                _supplierCode = value;
                if (oldValue != value) NotifyPropertyChanged("supplierCode");
            }
        }

        [JsonProperty(PropertyName = "info")]
        [Column("info")]
        public string info
        {
            get { return _info; }
            set
            {
                string oldValue = _info;
                _info = value;
                if (oldValue != value) NotifyPropertyChanged("info");
            }
        }

        [Column("enable")]
        public bool enable
        {
            get { return _enable; }
            set
            {
                bool oldValue = _enable;
                _enable = value;
                if (oldValue != value) NotifyPropertyChanged("enable");
            }
        }

        [JsonIgnore]
        [ForeignKey("supplierID")]
        public Supplier supplier { get; set; }

        [JsonIgnore]
        [ForeignKey("productID")]
        public Product product { get; set; }

        [JsonIgnore]
        [ForeignKey("userID")]
        public User user { get; set; }

        [JsonProperty(PropertyName = "product_uuid")]
        [NotMapped]
        public Guid ProductUUID
        {
            get
            {
                if (product != null)
                {
                    if (product.UUID == null) return Guid.Empty;
                    return product.UUID.Value;
                }
                else
                {
                    var result = Product.GetProductUUIDByLocalID(productID);
                    if (result == null) return Guid.Empty;
                    else return result.Value;
                }
            }
            set
            {
                var valueID = Product.GetIDByUUID(value);
                if (valueID.HasValue == false) productID = 0;
                else productID = valueID.Value;
            }
        }

        [JsonProperty(PropertyName = "supplier_uuid")]
        [NotMapped]
        public Guid SupplierUUID
        {
            get
            {
                if (supplier != null)
                {
                    if (supplier.UUID == null) return Guid.Empty;
                    return supplier.UUID.Value;
                }
                else
                {
                    var result = Supplier.GetSupplierUUIDByLocalID(supplierID);
                    if (result == null) return Guid.Empty;
                    else return result.Value;
                }
            }
            set
            {
                var valueID = Supplier.GetIDByUUID(value);
                if (valueID.HasValue == false) supplierID = 0;
                else supplierID = valueID.Value;
            }
        }

        // For View
        [JsonIgnore]
        [NotMapped]
        public bool IsActive
        {
            set
            {
                _active = value;
                this.NotifyPropertyChanged("IsActive");
            }

            get { return _active; }
        }

        [JsonIgnore]
        [NotMapped]
        private double _productSellingPrice { get; set; }

        [JsonIgnore]
        [NotMapped]
        public double productSellingPrice
        {
            get
            {
                if (product != null) return product.price;
                else return _productSellingPrice;
            }
            set
            {
                _productSellingPrice = value;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public double realCost
        {
            get
            {
                var realCost = Utility.CalculateRealCostPrice(cost: cost, taxApplied: taxApplied);
                return realCost;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string profitMargin
        {
            get
            {
                // support open price product
                if (productSellingPrice <= 0) return "--";

                var sellingPrice = Utility.CalculateSellPriceWithoutGST(productSellingPrice);
                if (realCost == 0)
                {
                    return sellingPrice.ToString("0.00") + " (100%)";
                }
                // we have to calculate base on realCost
                var profitAmount = sellingPrice - realCost;
                var profitMargin = profitAmount / realCost;
                return profitAmount.ToString("0.00") + " (" + profitMargin.ToString("0.##%") + ")";
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string creator
        {
            get
            {
                if (user == null) return "Admin";
                return user.name;
            }
        }

        public ProductSupplier()
        {
        }

        public ProductSupplier(JObject obj, DateTime lastEditDate)
        {
            //ProductUUID = Utility.GetUUIDValueForKey(obj, "product_uuid").Value;
            //SupplierUUID = Utility.GetUUIDValueForKey(obj, "supplier_uuid").Value;
            cost = Utility.GetDoubleValueForKey(obj, "cost");
            supplierCode = Utility.GetStringValueForKey(obj, "supplier_code");
            taxApplied = Utility.GetBoolValueForKey(obj, "tax_applied");
            active = Utility.GetBoolValueForKey(obj, "active");
            enable = Utility.GetBoolValueForKey(obj, "enable");
            info = Utility.GetStringValueForKey(obj, "info");
            UUID = Utility.GetUUIDValueForKey(obj, "uuid");
            lastEdit = lastEditDate;
        }

        public ProductSupplier(int productID, int supplierID, double cost, bool taxApplied)
        {
            this.productID = productID;
            //this.ProductUUID = Product.GetProductUUIDByLocalID(productID).Value;
            this.supplierID = supplierID;
            //this.SupplierUUID = Supplier.GetSupplierUUIDByLocalID(supplierID).Value;
            this.cost = cost;
            this.taxApplied = taxApplied;
            this.lastEdit = DateTime.Now;
            this.enable = true;
        }

        public ProductSupplier(Product product, Supplier supplier)
        {
            this.productID = product.ID;
            this.supplierID = supplier.ID;
            //this.ProductUUID = product.UUID.Value;
            //this.SupplierUUID = supplier.UUID.Value;
            this.productSellingPrice = product.PriceRealValue;
            this.supplier = supplier;
            this.lastEdit = DateTime.Now;
            this.enable = true;
        }

        public class ProductSupplierContext : MyDbContext
        {
            public ProductSupplierContext() : base()
            {
            }

            public DbSet<ProductSupplier> ProductSuppliers { get; set; }
        }

        #region DATABASE

        public static List<ProductSupplier> GetAllByProduct(Product product)
        {
            var productSuppliers = new List<ProductSupplier> { };
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers.Include("supplier").Include("user")
                             where ps.enable == true && ps.productID == product.ID
                             select ps;
                if (result != null && result.Count() > 0)
                {
                    foreach (var productSupplier in result.ToList())
                    {
                        productSupplier.productSellingPrice = product.PriceRealValue;
                        productSuppliers.Add(productSupplier);
                    }
                }
            }

            return productSuppliers;
        }

        public static List<ProductSupplier> GetAllBySupplier(Supplier supplier)
        {
            var productSuppliers = new List<ProductSupplier> { };
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers.Include("product").Include("user")
                             where ps.enable == true && ps.supplierID == supplier.ID
                             select ps;
                if (result != null && result.Count() > 0)
                {
                    foreach (var productSupplier in result.ToList())
                    {
                        productSupplier.supplier = supplier;
                        productSuppliers.Add(productSupplier);
                    }
                }
            }

            return productSuppliers;
        }

        public static ProductSupplier GetProductSupplierByUUID(Guid? UUID)
        {
            using (var db = new ProductSupplierContext())
            {
                var result = (from ps in db.ProductSuppliers
                              where ps.UUID.Value == UUID.Value
                              select ps);
                if (result != null && result.Count() > 0)
                    return result.FirstOrDefault();
                else return null;
            }
        }

        public static List<ProductSupplier> GetAll()
        {
            List<ProductSupplier> list;
            using (var db = new ProductSupplierContext())
            {
                list = (from s in db.ProductSuppliers
                        select s).ToList();
            }
            return list;
        }

        public static List<ProductSupplier> GetDisableProductSuppliers(List<Product> products, int sID)
        {
            var productSuppliers = new List<ProductSupplier> { };
            var productIDs = new List<int> { };

            if (products == null) return productSuppliers;

            // need to get product ID list
            foreach (var product in products)
                productIDs.Add(product.ID);

            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers.Include("supplier").Include("user")
                             where ps.enable == false && ps.supplierID == sID && productIDs.Contains(ps.productID)
                             select ps;
                if (result != null && result.Count() > 0)
                    return result.ToList();
                return null;
            }
        }

        /*
        We use this to support Update product and supplier by product and all related
        carton
        */

        public static List<ProductSupplier> GetByProductsAndSupplierIgnoreEnableValue(List<Product> products, Supplier supplier)
        {
            var productSuppliers = new List<ProductSupplier> { };
            var productIDs = new List<int> { };

            if (products == null || supplier == null) return productSuppliers;

            // need to get product ID list
            foreach (var product in products)
                productIDs.Add(product.ID);

            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers
                             where ps.supplierID == supplier.ID && productIDs.Contains(ps.productID)
                             select ps;
                if (result != null && result.Count() > 0)
                    return result.ToList();
                return null;
            }
        }

        public static List<ProductSupplier> GetActiveSupplierByProductIDs(List<int> IDs)
        {
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers.Include("supplier").Include("user")
                             where ps.enable == true && ps.active == true && IDs.Contains(ps.productID)
                             select ps;
                if (result.Count() > 0)
                {
                    return result.ToList();
                }
            }

            return null;
        }

        public static ProductSupplier GetActiveSupplierForProduct(Product product)
        {
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers.Include("supplier").Include("user")
                             where ps.enable == true && ps.active == true && ps.productID == product.ID
                             select ps;
                if (result.Count() > 0)
                {
                    var productSupplier = result.First();
                    productSupplier.productSellingPrice = product.PriceRealValue;
                    return productSupplier;
                }
            }
            return null;
        }

        public static Tuple<string, double> GetActiveSupplierNameByProductBarcode(string barcode)
        {
            using (var db = new ProductSupplierContext())
            {
                var result = (from ps in db.ProductSuppliers.Include("supplier").Include("product")
                              where ps.enable == true
                              && ps.active == true
                              && ps.product.barcode.ToLower().Trim() == barcode.ToLower().Trim()
                              select ps).FirstOrDefault();
                if (result != null)
                    return new Tuple<string, double>(result.supplier.name, result.cost);
                else return null;
            }
        }

        public static Tuple<string, double> GetSupplierNameCostPricesByProductBarcode(string barcode)
        {
            using (var db = new ProductSupplierContext())
            {
                var result = (from ps in db.ProductSuppliers.Include("supplier").Include("product")
                              where ps.enable == true
                              && ps.product.barcode.ToLower().Trim() == barcode.ToLower().Trim()
                              select ps).FirstOrDefault();
                if (result != null)
                    return new Tuple<string, double>(result.supplier.name, result.cost);
                else return null;
            }
        }

        public static double GetCostPriceByProductBarcodeAndSupplierName(string barcode, string supplierName)
        {
            using (var db = new ProductSupplierContext())
            {
                var result = (from ps in db.ProductSuppliers.Include("supplier").Include("product")
                              where ps.enable == true &&
                              ps.product.barcode.ToLower().Trim() == barcode.ToLower().Trim() &&
                              ps.supplier.name.Trim().ToLower() == supplierName.ToLower()
                              select ps).FirstOrDefault();
                if (result != null)
                    return result.cost;
                return DEFAULT_INVALID_COST_PRICE;
            }
        }

        public static double GetCostPriceByProductIdAndSupplierId(int productId, int supplierId)
        {
            using (var db = new ProductSupplierContext())
            {
                var result = (from ps in db.ProductSuppliers
                              where ps.enable == true &&
                                ps.productID == productId &&
                                ps.supplierID == supplierId
                              select ps).FirstOrDefault();
                if (result != null) return result.cost;
                return DEFAULT_INVALID_COST_PRICE;
            }
        }

        public static bool CheckExistedIgnoreEnableValue(int pID, int sID)
        {
            return CheckExisted(pID, sID, shouldCheckEnable: false);
        }

        public static bool CheckExistedAndIsEnable(int pID, int sID)
        {
            return CheckExisted(pID, sID, shouldCheckEnable: true);
        }

        public static bool CheckExisted(int pID, int sID, bool shouldCheckEnable)
        {
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                IQueryable<ProductSupplier> result;
                if (shouldCheckEnable)
                {
                    result = from ps in db.ProductSuppliers
                             where ps.enable == true && ps.supplierID == sID && ps.productID == pID
                             select ps;
                }
                else
                {
                    result = from ps in db.ProductSuppliers
                             where ps.supplierID == sID && ps.productID == pID
                             select ps;
                }
                return result.Any();
            }
        }

        public static bool CheckExistedActiveSupplierByProductID(int pID)
        {
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers
                             where ps.enable == true && ps.productID == pID && ps.active == true
                             select ps;
                return result.Any();
            }
        }

        public static List<int> GetProductIdBySupplierCode(string supplierCode, string supplierId)
        {
            supplierId = supplierId == "-1" ? "" : supplierId;
            using (var db = new ProductSupplier.ProductSupplierContext())
            {
                return db.ProductSuppliers.Where((s => s.enable == true)).Where(s =>
                ((supplierCode == "") || (s.supplierCode.ToUpper().Contains(supplierCode)))).Where(
                (s => ((supplierId == "") || (supplierId == s.supplierID.ToString())))).Select(x =>
               x.productID).ToList();
            }
        }

        public static bool DoesUUIDExist(Guid? uuid)
        {
            if (uuid == null || uuid.HasValue == false) return false;

            using (var db = new ProductSupplierContext())
            {
                var result = from ps in db.ProductSuppliers where ps.UUID.Value == uuid.Value select ps;
                return result.Any();
            }
        }

        public static bool DisableProductSupplierWithUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return false;

            var cmd = new NpgsqlCommand("UPDATE product_supplier SET enable = false WHERE uuid = :uuidValue");
            cmd.Parameters.AddWithValue("uuidValue", NpgsqlDbType.Uuid, uuid.ToString());

            var affectedRow = DataConnection.ExecuteNonQuery(cmd);
            return (affectedRow > 0);
        }

        #endregion DATABASE

        #region Notify property change

        private void NotifyPriceChangeChanged()
        {
            this.NotifyPropertyChanged("profitMargin");
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if (!IsSaveLog) return;
            PropertyInfo property = typeof(ProductSupplier).GetProperty(propName);
            if (property == null) return;
            string propertyName = Utility.ConvertPropertyNameToDBName(propName);
            dynamic propertyValue = property.GetValue(this);

            if (propName.Equals("productID"))
            {
                property = typeof(Product).GetProperty("productUUID");
                propertyValue = property.GetValue(this);
                propertyName = "product_uuid";
            }

            if (propName.Equals("supplierID"))
            {
                property = typeof(Product).GetProperty("supplierUUID");
                propertyValue = property.GetValue(this);
                propertyName = "supplier_uuid";
            }

            if (ChangedPropertiesList == null) ChangedPropertiesList = new Dictionary<string, dynamic>();
            if (ChangedPropertiesList.ContainsKey(propertyName))
            {
                ChangedPropertiesList[propertyName] = propertyValue;
            }
            else
            {
                ChangedPropertiesList.Add(propertyName, propertyValue);
            }
        }

        public int CompareTo(Object obj)
        {
            ProductSupplier other = (ProductSupplier)obj;
            // Sort based on last edit
            if (this.lastEdit > other.lastEdit)
                return 1;
            else if (other.lastEdit > this.lastEdit)
                return -1;
            else
                return 0;
        }

        #endregion Notify property change
    }

    public static class DataExtensionsPS
    {
        public static IEnumerable<IEnumerable<ProductSupplier>> Partition(this List<ProductSupplier> list, int partitionSize)
        {
            var numRows = Math.Ceiling((double)list.Count);
            for (var i = 0; i < numRows / partitionSize; i++)
            {
                yield return Partition(list, i * partitionSize, i * partitionSize + partitionSize);
            }
        }

        private static IEnumerable<ProductSupplier> Partition(List<ProductSupplier> list, int index, int endIndex)
        {
            for (var i = index; i < endIndex && i < list.Count; i++)
            {
                yield return list[i];
            }
        }
    }
}