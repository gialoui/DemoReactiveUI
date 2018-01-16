using DemoAppReactiveUI.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DemoAppReactiveUI.DataAccess
{
    public partial class ProductDA
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class ProductContext : MyDbContext
        {
            public ProductContext() : base()
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        private ProductContext productContext;

        public ProductDA()
        {
            productContext = new ProductContext();
        }

        public void AttachPriceImportEntry(Product product, bool updateColdPrice = false)
        {
            productContext.Products.Attach(product);
            if (updateColdPrice) productContext.Entry(product).Property(e => e.priceCold).IsModified = true;
            productContext.Entry(product).Property(e => e.price).IsModified = true;
            productContext.Entry(product).Property(e => e.lastEdit).IsModified = true;
        }

        public void AttachProductImportEntry(Product product, bool updateColdPrice = false)
        {
            productContext.Products.Attach(product);
            if (updateColdPrice) productContext.Entry(product).Property(e => e.priceCold).IsModified = true;
            productContext.Entry(product).Property(e => e.price).IsModified = true;
            productContext.Entry(product).Property(e => e.categoryID).IsModified = true;
            productContext.Entry(product).Property(e => e.doesSaleByWeight).IsModified = true;
            productContext.Entry(product).Property(e => e.weightScaleID).IsModified = true;
            productContext.Entry(product).Property(e => e.lastEdit).IsModified = true;
        }

        public void AttachUpdatedItemOnHandEntry(Product product)
        {
            productContext.Products.Attach(product);
            productContext.Entry(product).Property(e => e.itemOnHand).IsModified = true;
            productContext.Entry(product).Property(e => e.lastEdit).IsModified = true;
        }

        public void AddProductImportEntry(Product product)
        {
            productContext.Products.Add(product);
        }

        public void SetAutoDetectChanges(bool status)
        {
            productContext.Configuration.AutoDetectChangesEnabled = status;
        }

        public Task<int> SaveImportChanges()
        {
            return productContext.SaveChangesAsync();
        }

        public static Dictionary<string, int> GetProductBarcodeDict()
        {
            using (var db = new ProductContext())
            {
                var result = from s in db.Products where s.enable == true select s;
                if (result != null && result.Count() > 0)
                {
                    try
                    {
                        var resultDicts = new List<Dictionary<string, int>>();
                        var resultList = result.ToList();
                        resultDicts.Add(resultList
                            .ToLookup(p => p.barcode, p => p.ID)
                            .Where(pair => !String.IsNullOrEmpty(pair.Key))
                            .ToDictionary(group => group.Key, group => group.First()));
                        resultDicts.Add(resultList
                            .ToLookup(p => p.barcodeEx1, p => p.ID)
                            .Where(pair => !String.IsNullOrEmpty(pair.Key))
                            .ToDictionary(group => group.Key, group => group.First()));
                        resultDicts.Add(resultList
                            .ToLookup(p => p.barcodeEx2, p => p.ID)
                            .Where(pair => !String.IsNullOrEmpty(pair.Key))
                            .ToDictionary(group => group.Key, group => group.First()));
                        resultDicts.Add(resultList
                            .ToLookup(p => p.barcodeEx3, p => p.ID)
                            .Where(pair => !String.IsNullOrEmpty(pair.Key))
                            .ToDictionary(group => group.Key, group => group.First()));

                        return resultDicts.SelectMany(dict => dict)
                            .ToLookup(pair => pair.Key, pair => pair.Value)
                            .ToDictionary(group => group.Key, group => group.First());
                    }
                    catch (Exception e) { logger.Debug(e); }
                }

                // return empty dict as default
                return new Dictionary<string, int>();
            }
        }

        public static Dictionary<string, int> GetProductNameDict()
        {
            using (var db = new ProductContext())
            {
                return (from s in db.Products where s.enable == true select s)
                    .ToLookup(p => p.weightScaleID, p => p.ID)
                    .Where(pair => !string.IsNullOrEmpty(pair.Key))
                    .ToDictionary(group => group.Key, group => group.First());
            }
        }

        public static Dictionary<int, string> GetProductIdAndNameDict()
        {
            using (var db = new ProductContext())
            {
                return db.Products.ToDictionary(x => x.ID, x => x.name);
            }
        }

        public static Dictionary<string, int> GetProductWeightScaleIDDict()
        {
            using (var db = new ProductContext())
            {
                return (from s in db.Products where s.enable == true select s)
                    .ToLookup(p => p.weightScaleID, p => p.ID)
                    .Where(pair => !string.IsNullOrEmpty(pair.Key))
                    .ToDictionary(group => group.Key, group => group.First());
            }
        }

        public static Dictionary<int, double> GetProductItemOnHandDict()
        {
            Dictionary<int, double> itemOnHandDict = new Dictionary<int, double>();
            using (var db = new ProductContext())
            {
                var result = from p in db.Products where p.enable == true select new { p.ID, p.itemOnHand };
                if (result != null && result.Count() > 0)
                {
                    try
                    {
                        itemOnHandDict = result.ToDictionary(p => p.ID, p => p.itemOnHand);
                    }
                    catch (Exception e) { logger.Error(e); }
                }

                // return empty dict as default
                return itemOnHandDict;
            }
        }
    }
}