using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace DemoAppReactiveUI.Model
{
    [Table("groups", Schema = "public")]
    public class Group
    {
        [Column("id"), Key]
        public int ID { get; set; }

        public ICollection<ProductGroup> productGroups { get; set; }

        public class GroupContext : MyDbContext
        {
            public GroupContext() : base()
            {
            }

            public DbSet<Group> Groups { get; set; }
        }

        public Group()
        {
        }

        //public Group(JArray serverProductIDs) {
        //    var serverIDs = serverProductIDs.ToObject<int[]>();
        //    foreach (int serverID in serverIDs)
        //    {
        //        var localProductId = Product.GetProductIDByServerID(serverID);
        //        var productGroup = new ProductGroup();
        //        productGroup.productID = localProductId;

        //        if (productGroups == null) productGroups = new List<ProductGroup> { };
        //        productGroups.Add(productGroup);
        //    }
        //}

        public static Group CloneGroup(Group original)
        {
            if (original == null)
            {
                return null;
            }

            Group newGroup = new Group();
            var originalProductGroups = original.productGroups;
            foreach (ProductGroup originalProductGroup in originalProductGroups)
            {
                var productGroup = new ProductGroup();
                productGroup.productID = originalProductGroup.productID;
                if (originalProductGroup.product != null) productGroup.product = originalProductGroup.product;
                if (newGroup.productGroups == null) newGroup.productGroups = new List<ProductGroup> { };
                newGroup.productGroups.Add(productGroup);
            }
            return newGroup;
        }

        #region DATABASE

        public static Group GetGroupByID(int groupID)
        {
            using (var db = new Group.GroupContext())
            {
                var result = from g in db.Groups.Include("productGroups") where g.ID == groupID select g;
                if (result != null && result.Count() > 0)
                {
                    var group = result.FirstOrDefault();
                    if (group == null) return null;

                    // get list of product IDs
                    var productIDs = new List<int> { };
                    foreach (var item in group.productGroups)
                    {
                        productIDs.Add(item.productID);
                    }
                    // get product detail which contain activeProductSupplier
                    var products = Product.GetProductsByIDs(productIDs);
                    if (products != null)
                    {
                        // loop through each product to set to productGroup object
                        foreach (var product in products)
                        {
                            foreach (var item in group.productGroups)
                            {
                                if (item.productID == product.ID)
                                {
                                    item.product = product;
                                    break;
                                }
                            }
                        }
                    }

                    return group;
                }
            }
            return null;
        }

        public bool CheckAndDeleteProductWithIDFromDB(int productID)
        {
            if (productGroups != null)
            {
                foreach (var item in productGroups)
                {
                    if (item.productID == productID) return item.DeleteFromDB();
                }
            }
            return false;
        }

        #endregion DATABASE
    }
}