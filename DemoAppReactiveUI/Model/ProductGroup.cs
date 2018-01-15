using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace DemoAppReactiveUI.Model
{
    [Table("product_group", Schema = "public")]
    public class ProductGroup
    {
        [Column("product_id", Order = 0), Key]
        public int productID { get; set; }

        [Column("group_id", Order = 1), Key]
        public int groupID { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        // we use this property to insert ProductGroup to DB when editing a Group promotion
        [NotMapped]
        public bool existedInDB = false;

        [ForeignKey("productID")]
        public Product product { get; set; }

        [NotMapped]
        public string ProductName
        {
            get
            {
                if (product == null) return "";
                return product.name;
            }
        }

        [NotMapped]
        public string ProductBarcode
        {
            get
            {
                if (product == null) return "";
                return product.barcode;
            }
        }

        public class ProductGroupContext : MyDbContext
        {
            public ProductGroupContext() : base()
            {
            }

            public DbSet<ProductGroup> ProductGroups { get; set; }
        }

        public ProductGroup()
        {
        }

        #region DATABASE

        public static List<int> GetGroupIDsByProductID(int productID)
        {
            using (var db = new ProductGroup.ProductGroupContext())
            {
                var result = from pg in db.ProductGroups where pg.productID == productID select pg.groupID;
                if (result != null && result.Count() > 0)
                {
                    return result.ToList();
                }
            }
            return null;
        }

        public static bool UpdateToDB(List<ProductGroup> productGroups)
        {
            using (var db = new ProductGroup.ProductGroupContext())
            {
                foreach (var item in productGroups)
                {
                    if (item.existedInDB == false)
                    {
                        // insert new one
                        db.ProductGroups.Add(item);
                    }
                }

                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        public bool DeleteFromDB()
        {
            using (var db = new ProductGroup.ProductGroupContext())
            {
                db.ProductGroups.Attach(this);
                db.ProductGroups.Remove(this);

                var affectedRows = db.SaveChanges();
                if (affectedRows > 0) return true;
                else return false;
            }
        }

        public static List<ProductGroup> GetPacketByGroupID(int packetGroupID)
        {
            using (var db = new ProductGroup.ProductGroupContext())
            {
                var result = from pg in db.ProductGroups.Include("product") where pg.groupID == packetGroupID select pg;
                if (result != null && result.Count() > 0)
                {
                    return result.ToList();
                }
            }
            return null;
        }

        public static bool UpdatePacketToDB(ProductGroup productGroupItem)
        {
            using (var db = new ProductGroup.ProductGroupContext())
            {
                if (productGroupItem.existedInDB == false)
                {
                    // insert new one
                    db.ProductGroups.Add(productGroupItem);
                }
                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        #endregion DATABASE
    }
}