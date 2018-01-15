using DemoAppReactiveUI.Helper;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace DemoAppReactiveUI.Model
{
    [Table("product", Schema = "public")]
    public partial class Product : INotifyPropertyChanged
    {
        // Log
        public static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public Dictionary<string, dynamic> ChangedPropertiesList = new Dictionary<string, dynamic>();

        [JsonIgnore]
        public bool IsSaveLog = false;

        /// <summary>
        /// enumeration of product type
        /// </summary>
        ///
        public enum PRODUCTTYPE
        {
            NORMAL = 0,
            PACKET = 1,
        }

        #region VARIABLES

        private int? _categoryID;
        private string _name;
        private string _nameChinese;
        private string _barcode;
        private string _barcodeEx1;
        private string _barcodeEx2;
        private string _barcodeEx3;
        private double _itemOnHand;
        private double _price;
        private double _priceCold;
        private bool _enable;
        private bool _isCarton;
        private bool _isNonTaxable;
        private bool _isOpenPrice;
        private bool _doesSaleByWeight;
        private string _weightScaleID;
        private int _type;
        private int? _cartonProductID;
        private double _cartonQuantity;

        [JsonIgnore]
        [Column("id"), Key]
        public int ID { get; set; }

        [JsonIgnore]
        [Column("category_id")]
        public int? categoryID
        {
            get { return _categoryID; }
            set
            {
                var oldValue = _categoryID;
                _categoryID = value;
                if (oldValue != value)
                    NotifyPropertyChanged("categoryID");
            }
        }

        [JsonProperty(PropertyName = "category_uuid")]
        [NotMapped]
        public Guid? categoryUUID
        {
            get
            {
                if (categoryID == null) return null;
                else
                {
                    if (category != null) return category.UUID;
                    else return Category.GetCategoryUUIDByID(categoryID.Value);
                }
            }
            set
            {
                var cateID = Category.GetIDByUUID(value);
                categoryID = cateID;
            }
        }

        [JsonProperty(PropertyName = "name_chinese")]
        [Column("name_chinese")]
        public string nameChinese
        {
            get { return _nameChinese; }
            set
            {
                var oldValue = _nameChinese;
                _nameChinese = value;
                if (oldValue != value)
                    NotifyPropertyChanged("nameChinese");
            }
        }

        [JsonProperty(PropertyName = "name")]
        [Column("name")]
        public string name
        {
            get { return _name; }
            set
            {
                var oldValue = name;
                _name = value;
                if (oldValue != value)
                    NotifyPropertyChanged("name");
            }
        }

        [JsonProperty(PropertyName = "barcode")]
        [Column("barcode")]
        public string barcode
        {
            get { return _barcode; }
            set
            {
                var oldValue = _barcode;
                _barcode = value;
                if (oldValue != value)
                    NotifyPropertyChanged("barcode");
            }
        }

        [JsonProperty(PropertyName = "barcode_extra1")]
        [Column("barcode_extra1")]
        public string barcodeEx1
        {
            get { return _barcodeEx1; }
            set
            {
                var oldValue = _barcodeEx1;
                _barcodeEx1 = value;
                if (oldValue != value)
                    NotifyPropertyChanged("barcodeEx1");
            }
        }

        [JsonProperty(PropertyName = "barcode_extra2")]
        [Column("barcode_extra2")]
        public string barcodeEx2
        {
            get { return _barcodeEx2; }
            set
            {
                var oldValue = _barcodeEx2;
                _barcodeEx2 = value;
                if (oldValue != value)
                    NotifyPropertyChanged("barcodeEx2");
            }
        }

        [JsonProperty(PropertyName = "barcode_extra3")]
        [Column("barcode_extra3")]
        public string barcodeEx3
        {
            get { return _barcodeEx3; }
            set
            {
                var oldValue = _barcodeEx3;
                _barcodeEx3 = value;
                if (oldValue != value)
                    NotifyPropertyChanged("barcodeEx3");
            }
        }

        [JsonIgnore]
        [Column("last_edit")]
        public DateTime lastEdit { get; set; }

        [JsonProperty(PropertyName = "item_on_hand")]
        [Column("item_on_hand")]
        public double itemOnHand
        {
            get { return _itemOnHand; }
            set
            {
                var oldValue = _itemOnHand;
                _itemOnHand = value;
                if (oldValue != value)
                    NotifyPropertyChanged("itemOnHand");
            }
        }

        [JsonProperty(PropertyName = "unit_price")]
        [Column("price_unit")]
        public double price
        {
            get { return _price; }
            set
            {
                var oldValue = _price;
                _price = value;
                oldValue = Utility.RoundDoubleValue(oldValue, 2);
                value = Utility.RoundDoubleValue(value, 2);
                if (oldValue != value)
                    NotifyPropertyChanged("price");
            }
        }

        [JsonProperty(PropertyName = "price_cold")]
        [Column("price_cold")]
        public double priceCold
        {
            get { return _priceCold; }
            set
            {
                var oldValue = _priceCold;
                _priceCold = value;
                oldValue = Utility.RoundDoubleValue(oldValue, 2);
                value = Utility.RoundDoubleValue(value, 2);
                if (oldValue != value)
                    NotifyPropertyChanged("priceCold");
            }
        }

        [JsonProperty(PropertyName = "active")]
        [Column("enable")]
        public bool enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
            }
        }

        [JsonProperty(PropertyName = "uuid")]
        [Column("uuid")]
        public Guid? UUID { get; set; }

        // Carton
        [JsonProperty(PropertyName = "is_carton")]
        [Column("is_carton")]
        public bool isCarton
        {
            get { return _isCarton; }
            set
            {
                var oldValue = _isCarton;
                _isCarton = value;
                if (oldValue != value)
                    NotifyPropertyChanged("isCarton");
            }
        }

        [JsonProperty(PropertyName = "carton_quantity")]
        [Column("carton_quantity")]
        public double cartonQuantity
        {
            get
            {
                return _cartonQuantity;
            }
            set
            {
                var oldValue = _cartonQuantity;
                _cartonQuantity = value;
                if (oldValue != value)
                    NotifyPropertyChanged("cartonQuantity");
            }
        }

        [JsonIgnore]
        [Column("carton_product_id")]
        public int? cartonProductID
        {
            get
            {
                return _cartonProductID;
            }
            set
            {
                var oldValue = _cartonProductID;
                _cartonProductID = value;
                if (oldValue != value)
                    NotifyPropertyChanged("cartonProductID");
            }
        }

        [JsonIgnore]
        [ForeignKey("categoryID")]
        public Category category { get; set; }

        // Open price
        [JsonIgnore]
        [Column("is_open_price")]
        public bool isOpenPrice
        {
            get { return _isOpenPrice; }
            set
            {
                var oldValue = _isOpenPrice;
                _isOpenPrice = value;
                if (oldValue != value)
                    NotifyPropertyChanged("isOpenPrice");
            }
        }

        [JsonProperty(PropertyName = "is_non_taxable")]
        [Column("is_non_taxable")]
        public bool isNonTaxable
        {
            get { return _isNonTaxable; }
            set
            {
                var oldValue = _isNonTaxable;
                _isNonTaxable = value;
                if (oldValue != value)
                    NotifyPropertyChanged("isNonTaxable");
            }
        }

        // Product sale by weight
        [JsonProperty(PropertyName = "is_weightscale")]
        [Column("does_sale_by_weight")]
        public bool doesSaleByWeight
        {
            get { return _doesSaleByWeight; }
            set
            {
                var oldValue = _doesSaleByWeight;
                _doesSaleByWeight = value;
                if (oldValue != value)
                    NotifyPropertyChanged("doesSaleByWeight");
            }
        }

        [JsonProperty(PropertyName = "weightscale_id")]
        [Column("weight_scale_id")]
        public string weightScaleID
        {
            get { return _weightScaleID; }
            set
            {
                var oldValue = _weightScaleID;
                _weightScaleID = value;
                if (oldValue != value)
                    NotifyPropertyChanged("weightScaleID");
            }
        }

        // Product type
        [JsonIgnore]
        [Column("type")]
        public int type
        {
            get { return _type; }
            set
            {
                var oldValue = _type;
                _type = value;
                if (oldValue != value)
                    NotifyPropertyChanged("type");
            }
        }

        [JsonIgnore]
        [Column("packet_group_id")]
        public int? packetGroupID { get; set; }

        [JsonIgnore]
        [ForeignKey("packetGroupID")]
        public Group packetGroup { get; set; }

        #endregion VARIABLES

        #region PROPERTIES METHODS

        [JsonIgnore]
        [NotMapped]
        public string LastUpdated
        {
            get
            {
                return lastEdit.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
        }

        [JsonIgnore]
        [NotMapped]
        public ProductSupplier activeProductSupplier { get; set; }

        [JsonIgnore]
        [NotMapped]
        public PriceSchedule priceSchedule { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Product cartonRelatedProduct;

        [JsonIgnore]
        [NotMapped]
        public List<ProductSupplier> productSuppliers { get; set; }

        [JsonIgnore]
        [NotMapped]
        public bool isChecked { get; set; } = false;

        [JsonIgnore]
        [NotMapped]
        public double PriceRealValue
        {
            set
            {
                price = value;
            }

            get
            {
                // for open price product when getting PriceValue, we return 0
                if (IsOpenPrice) return 0;

                return price;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsChecked
        {
            set
            {
                isChecked = value;
                this.NotifyPropertyChanged("IsChecked");
            }

            get { return isChecked; }
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsOpenPrice
        {
            set
            {
                isOpenPrice = value;
                this.NotifyPropertyChanged("IsOpenPrice");
                this.NotifyPropertyChanged("IsPriceEnabled");
            }

            get { return isOpenPrice; }
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsPriceEnabled
        {
            get { return !isOpenPrice; }
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsPacketProduct
        {
            get { return type == (int)PRODUCTTYPE.PACKET; }
        }

        // We need to base on the active supplier for product.
        [JsonIgnore]
        [NotMapped]
        public double cost
        {
            get
            {
                if (activeProductSupplier != null) return activeProductSupplier.realCost;
                else return 0.0;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string supplierName
        {
            get
            {
                if (activeProductSupplier != null) return activeProductSupplier.supplier.name;
                else return "";
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string supplierCode
        {
            get
            {
                if (activeProductSupplier != null) return activeProductSupplier.supplierCode;
                else return "";
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string cartonRelatedProductBarcode
        {
            get
            {
                if (cartonRelatedProduct != null) return cartonRelatedProduct.barcode;
                else return "";
            }
        }

        // Support show on view
        [JsonIgnore]
        [NotMapped]
        public string categoryName
        {
            get
            {
                if (category == null) return "";
                return category.name;
            }
        }

        [NotMapped]
        [JsonIgnore]
        public bool DoesRequireApproval
        {
            get
            {
                if (category == null) return false;
                return category.doesRequireApproval;
            }
        }

        [NotMapped]
        [JsonIgnore]
        public bool tax_included
        {
            get
            {
                // Tax is always included in Price
                return true;
            }
        }

        [NotMapped]
        [JsonIgnore]
        public string ItemOnHandDisplay
        {
            get
            {
                return Utility.FormatDoubleWithOptionalDecimalNumbers(this.itemOnHand);
            }
        }

        [NotMapped]
        [JsonIgnore]
        public double ItemOnHandAddValue { get; set; }

        #endregion PROPERTIES METHODS

        public class ProductContext : MyDbContext
        {
            public ProductContext() : base()
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        public Product()
        {
        }

        // Parse data from JSON object
        public Product(JObject obj, DateTime lastEditDate)
        {
            name = Utility.GetStringValueForKey(obj, "name").Trim().ToUpper();
            nameChinese = Utility.GetStringValueForKey(obj, "name_chinese").Trim().ToUpper();
            price = Utility.GetDoubleValueForKey(obj, "unit_price");
            priceCold = Utility.GetDoubleValueForKey(obj, "price_cold");
            itemOnHand = Utility.GetDoubleValueForKey(obj, "item_on_hand");
            barcode = Utility.GetStringValueForKey(obj, "barcode").Trim().ToUpper();
            barcodeEx1 = Utility.GetStringValueForKey(obj, "barcode_extra1").Trim().ToUpper();
            barcodeEx2 = Utility.GetStringValueForKey(obj, "barcode_extra2").Trim().ToUpper();
            barcodeEx3 = Utility.GetStringValueForKey(obj, "barcode_extra3").Trim().ToUpper();
            enable = Utility.GetBoolValueForKey(obj, "active");
            UUID = Utility.GetUUIDValueForKey(obj, "uuid");

            // get and convert date string to object
            var lastEditTimestampString = Utility.GetStringValueForKey(obj, "update_time");
            var dateObj = Utility.ConvertGMTDateStringToLocalDate(lastEditTimestampString);
            if (dateObj != null) lastEdit = (DateTime)dateObj;

            // carton
            isCarton = Utility.GetBoolValueForKey(obj, "is_carton");
            cartonQuantity = Utility.GetDoubleValueForKey(obj, "carton_quantity");

            isOpenPrice = Utility.GetBoolValueForKey(obj, "is_open_price");
            isNonTaxable = Utility.GetBoolValueForKey(obj, "is_non_taxable");
            doesSaleByWeight = Utility.GetBoolValueForKey(obj, "is_weightscale");
            weightScaleID = Utility.GetStringValueForKey(obj, "weightscale_id");
            lastEdit = lastEditDate;
        }

        #region DATABASE GET/CHECK

        public static int? GetLocalIDByUUID(Guid? _uuid)
        {
            if (_uuid == null) return null;

            using (var db = new ProductContext())
            {
                var result = from p in db.Products where p.UUID == _uuid select p.ID;
                if (result != null && result.Count() > 0)
                    return result.First();
                return null;
            }
        }

        public static List<Product> GetAllProducts()
        {
            using (var db = new ProductContext())
            {
                return db.Products.Where(x => x.enable).OrderBy(x => x.name).ToList();
            }
        }

        public static double GetInventoryByProductID(int productID)
        {
            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products where p.ID == productID select p.itemOnHand;
                if (result.Count() > 0)
                {
                    return result.FirstOrDefault();
                }
            }
            return 0.0;
        }

        public Product GetProductItemForThisCarton()
        {
            //for carton product, we have to update the inventory of the related product
            if (cartonProductID == null) return null;

            return GetProductByID((int)cartonProductID);
        }

        public List<Product> GetCartonProducts()
        {
            using (var db = new Product.ProductContext())
            {
                // look for carton related with ID
                var result = from p in db.Products
                             where p.cartonProductID == this.ID
                             select p;
                if (result != null && result.Count() > 0)
                {
                    return result.ToList();
                }
                return null;
            }
        }

        public static Product GetProductByUUID(Guid uuid)
        {
            if (uuid == null) return null;
            using (var db = new ProductContext())
            {
                var product = db.Products.Where(x => x.UUID.Value == uuid);
                if (product == null) return null;
                return product.FirstOrDefault();
            }
        }

        public static Guid? GetProductUUIDByLocalID(int productLocalID)
        {
            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products where p.enable == true && p.ID == productLocalID select p.UUID;
                if (result != null && result.Count() > 0)
                {
                    Guid? uuid = result.FirstOrDefault();
                    if (uuid != null) return uuid;
                    return null;
                }
            }
            return null;
        }

        public static Product GetProductByBarcode(string barcode)
        {
            if (barcode == null) return null;

            using (var db = new Product.ProductContext())
            {
                // as the barcode contain UPPER case letter, we should uppercase the search string also
                // Just select product without category some product don't have category
                barcode = barcode.ToUpper();
                var result = from p in db.Products.Include("category")
                             where p.enable == true
                             && (p.barcode == barcode
                             || p.barcodeEx1 == barcode
                             || p.barcodeEx2 == barcode
                             || p.barcodeEx3 == barcode)
                             select p;
                if (result != null && result.Count() > 0)
                {
                    Product product = result.First();
                    var temp = ProductSupplier.GetActiveSupplierForProduct(product);
                    if (temp != null) product.activeProductSupplier = temp;
                    //else Console.WriteLine("ERROR GetProductByBarcode -> this product has no active supplier");

                    return product;
                }
            }
            return null;
        }

        public static int GetProductIDByBarcode(string barcode)
        {
            if (IsValidBarcode(barcode) == false) return -1;

            using (var db = new Product.ProductContext())
            {
                // as the barcode contain UPPER case letter, we should uppercase the search string also
                barcode = barcode.ToUpper();
                var result = from p in db.Products
                             where p.enable == true && (p.barcode == barcode
                             || p.barcodeEx1 == barcode
                             || p.barcodeEx2 == barcode || p.barcodeEx3 == barcode)
                             select p.ID;
                if (result.Count() > 0)
                {
                    return result.FirstOrDefault();
                }
            }
            return -1;
        }

        public static int GetProductIDByWeightScaleID(string weightScaleID)
        {
            using (var db = new Product.ProductContext())
            {
                // as the barcode contain UPPER case letter, we should uppercase the search string also
                var result = from p in db.Products
                             where p.enable == true && (p.weightScaleID == weightScaleID)
                             select p.ID;
                if (result.Count() > 0)
                {
                    return result.FirstOrDefault();
                }
            }
            return -1;
        }

        public static Product GetProductByID(int productID)
        {
            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products.Include("category") where p.enable == true && p.ID == productID select p;
                if (result != null && result.Count() > 0)
                {
                    Product product = result.First();
                    var temp = ProductSupplier.GetActiveSupplierForProduct(product);
                    if (temp != null) product.activeProductSupplier = temp;

                    return product;
                }
            }
            return null;
        }

        public static Product GetProductByUUID(Guid? uuid)
        {
            if (uuid == null || !uuid.HasValue) return null;

            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products
                             where p.enable == true && p.UUID.Value == uuid.Value
                             select p;

                if (result != null && result.Count() > 0)
                {
                    Product product = result.FirstOrDefault();
                }
            }
            return null;
        }

        public static Product GetProductByWeightScaleID(string weightScaleID)
        {
            if (weightScaleID == null) return null;

            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products.Include("category")
                             where p.enable == true && p.weightScaleID == weightScaleID
                             select p;

                if (result != null && result.Count() > 0)
                {
                    Product product = result.First();
                    var temp = ProductSupplier.GetActiveSupplierForProduct(product);

                    if (temp != null)
                        product.activeProductSupplier = temp;

                    return product;
                }
            }
            return null;
        }

        public static List<Product> GetProductsByIDs(List<int> IDs)
        {
            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products.Include("category") where p.enable == true && IDs.Contains(p.ID) orderby p.ID select p;
                if (result.Count() > 0)
                {
                    var products = result.ToList();
                    var activeProductSuppliers = ProductSupplier.GetActiveSupplierByProductIDs(IDs);
                    // populate productsupplier data
                    if (activeProductSuppliers != null)
                    {
                        foreach (var product in products)
                        {
                            foreach (var productSupplier in activeProductSuppliers)
                            {
                                if (product.ID == productSupplier.productID)
                                {
                                    // we need this to get the sale price
                                    productSupplier.productSellingPrice = product.PriceRealValue;
                                    product.activeProductSupplier = productSupplier;
                                    continue;
                                }
                            }
                        }
                    }
                    return products;
                }
            }
            return null;
        }

        public static List<Product> GetProductByCategoryID(int cateID)
        {
            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products where p.enable == true && p.categoryID == cateID select p;
                return result.ToList();
            }
        }

        public static List<Product> GetAllProductsByCategoryID(int cateID)
        {
            using (var db = new Product.ProductContext())
            {
                var result = from p in db.Products where p.categoryID == cateID select p;
                return result.ToList();
            }
        }

        public static List<Product> GetProductsByListID(List<Int64> listID)
        {
            using (var db = new ProductContext())
            {
                List<Product> listProducts;
                listProducts = (from p in db.Products
                                where listID.Contains(p.ID)
                                orderby p.name
                                select p).ToList();

                return listProducts;
            }
        }

        public static bool DoesUUIDExist(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return false;

            using (var db = new ProductContext())
            {
                var result = from p in db.Products where p.UUID.Value == uuid.Value select p;
                return result.Any();
            }
        }

        public static bool DisableProductWithUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return false;

            var cmd = new NpgsqlCommand("UPDATE product SET enable = false WHERE uuid = :uuidValue");
            cmd.Parameters.AddWithValue("uuidValue", NpgsqlDbType.Uuid, uuid.ToString());

            var affectedRow = DataConnection.ExecuteNonQuery(cmd);
            return (affectedRow > 0);
        }

        public static int? GetIDByUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return null;

            using (var db = new ProductContext())
            {
                var result = from p in db.Products where p.UUID == uuid select p.ID;
                if (result.Count() <= 0) return null;

                return result.FirstOrDefault();
            }
        }

        #endregion DATABASE GET/CHECK

        #region HELPERS

        public static Dictionary<Guid, int> GetProductUuidAndIdDictFromDB()
        {
            using (var db = new ProductContext())
            {
                var result = from c in db.Products orderby c.name where c.UUID != null && c.enable == true select c;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MyProductBarcodeComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(c => c.UUID.Value, c => c.ID);
                }
                // return empty dict as default
                return new Dictionary<Guid, int>();
            }
        }

        public static Dictionary<string, int> GetProductBarcodeDict()
        {
            using (var db = new ProductContext())
            {
                var result = from s in db.Products where s.enable == true select s;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MyProductBarcodeComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(s => s.barcode.Trim(), s => s.ID);
                }

                // return empty dict as default
                return new Dictionary<string, int>();
            }
        }

        /*
        reference: http://eposhq.com:8102/project/epos-minimart/us/27?kanban-status=45
        we enforce that each product can have only 1 promotion rule.
        */

        public static bool CheckBarcodeExisted(string barcode)
        {
            using (var db = new Product.ProductContext())
            {
                // as the barcode contain UPPER case letter, we should uppercase the search string also
                barcode = barcode.ToUpper();
                var query = from p in db.Products
                            where p.enable == true
                                && (p.barcode.ToUpper() == barcode
                                || p.barcodeEx1.ToUpper() == barcode
                                || p.barcodeEx2.ToUpper() == barcode
                                || p.barcodeEx3.ToUpper() == barcode)
                            select p;
                var existed = query.Any();
                return existed;
            }
        }

        public static bool CheckWeightScaleIDExisted(string weightScaleID)
        {
            using (var db = new Product.ProductContext())
            {
                // as the barcode contain UPPER case letter, we should uppercase the search string also
                var query = from p in db.Products
                            where p.weightScaleID == weightScaleID
                            select p;
                var existed = query.Any();
                return existed;
            }
        }

        public static string[] InvalidBarcodes(IEnumerable<string> barcodes)
        {
            var upperBarcodes = barcodes.Select(x => x.ToUpper());
            using (var db = new ProductContext())
            {
                var allValidProducts = from p in db.Products
                                       where p.enable == true
                                       select p;
                var allBarcodes = from p in allValidProducts
                                  select p.barcode;
                var allBarcodesEx1 = from p in allValidProducts
                                     select p.barcodeEx1;
                var allBarcodesEx2 = from p in allValidProducts
                                     select p.barcodeEx2;
                var allBarcodesEx3 = from p in allValidProducts
                                     select p.barcodeEx3;

                allBarcodes = allBarcodes.Union(allBarcodesEx1).Union(allBarcodesEx2).Union(allBarcodesEx3);
                return upperBarcodes.Except(allBarcodes).ToArray();
            }
        }

        public static bool CheckCartonProductByBarcode(string barcode)
        {
            using (var db = new Product.ProductContext())
            {
                // as the barcode contain UPPER case letter, we should uppercase the search string also
                barcode = barcode.ToUpper();
                var query = from p in db.Products
                            where p.enable == true
                                && (p.barcode == barcode
                                || p.barcodeEx1 == barcode
                                || p.barcodeEx2 == barcode
                                || p.barcodeEx3 == barcode)
                            select p.isCarton;
                return query.FirstOrDefault();
            }
        }

        public bool IsEmpty()
        {
            if (ID <= 0) return true;
            return false;
        }

        public static bool IsValidBarcode(string barcode)
        {
            if (barcode == null) return false;
            if (barcode.Trim().Length <= 0) return false;

            return true;
        }

        public static JArray CreateProductsItemOnHandJSONArray()
        {
            using (var db = new Product.ProductContext())
            {
                var productList = db.Products.Where(p => p.UUID != null).ToList();
                var result = new JArray(from p in productList
                                        select new JObject(
                      new JProperty("uuid", p.UUID), new JProperty("quantity", p.itemOnHand)));
                return result;
            }
        }

        public static int GetMaxID()
        {
            using (var db = new ProductContext())
            {
                int? maxID = db.Products.Max(c => (int?)c.ID);
                if (maxID == null) return 0;
                else return maxID.Value;
            }
        }

        public double CalculatePacketCost()
        {
            if (IsPacketProduct == false || packetGroupID == null) return 0.00;
            var productGroups = ProductGroup.GetPacketByGroupID((int)packetGroupID);
            if (productGroups == null) return 0.0;

            var packetCost = 0.0;
            foreach (var pg in productGroups)
            {
                if (pg.product == null) continue;

                var temp = ProductSupplier.GetActiveSupplierForProduct(pg.product);
                if (temp != null) pg.product.activeProductSupplier = temp;
                packetCost += pg.product.cost * pg.Quantity;
            }

            return packetCost;
        }

        public bool CheckPacketProductEligibleAndPrepareForCreatingInventory(out List<ProductGroup> productGroups)
        {
            productGroups = null;
            if (IsPacketProduct == false || packetGroupID == null) return false;

            productGroups = ProductGroup.GetPacketByGroupID((int)packetGroupID);
            if (productGroups == null) return false;

            foreach (var pg in productGroups)
            {
                if (pg.product == null) return false;

                var temp = ProductSupplier.GetActiveSupplierForProduct(pg.product);
                if (temp != null) pg.product.activeProductSupplier = temp;
                else return false;
            }

            return true;
        }

        #endregion HELPERS

        #region Notify property change

        private void NotifyAllPropertyChanged()
        {
            this.NotifyPropertyChanged("Name");
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

            // The below is saving changeLog, if we dont need. Just return
            if (!IsSaveLog) return;

            // Filter unwanted changed properties
            if (propName.Equals("Name") ||
                propName.Equals("IsOpenPrice") ||
                propName.Equals("IsPriceEnable") ||
                propName.Equals("isPriceEnable") ||
                propName.Equals("itemOnHand")) return;

            PropertyInfo property = typeof(Product).GetProperty(propName);
            if (property == null) return;

            dynamic propertyValue = property.GetValue(this);
            string propertyName = Utility.ConvertPropertyNameToDBName(propName);
            if (propName.Equals("categoryID"))
            {
                property = typeof(Product).GetProperty("categoryUUID");
                propertyValue = property.GetValue(this);
                propertyName = "category_uuid";
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

        #endregion Notify property change
    }

    public static partial class DataExtensions
    {
        public static IEnumerable<IEnumerable<Product>> Partition(this List<Product> list, int partitionSize)
        {
            var numRows = Math.Ceiling((double)list.Count);
            for (var i = 0; i < numRows / partitionSize; i++)
            {
                yield return Partition(list, i * partitionSize, i * partitionSize + partitionSize);
            }
        }

        private static IEnumerable<Product> Partition(List<Product> list, int index, int endIndex)
        {
            for (var i = index; i < endIndex && i < list.Count; i++)
            {
                yield return list[i];
            }
        }
    }
}