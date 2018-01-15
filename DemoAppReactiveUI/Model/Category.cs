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
    [Table("category", Schema = "public")]
    public class Category : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        public bool isSaveLog = false;

        [JsonIgnore]
        public Dictionary<string, dynamic> ChangedPropertiesList { get; set; } = new Dictionary<string, dynamic>();

        // As Category ID not auto increase, we have to set the Opntion to let entity framework know
        [JsonIgnore]
        [Column("id"), Key]
        public int ID { get; set; }

        [JsonIgnore]
        [Column("parent_id")]
        public int parentID { get; set; }

        private string _name;

        [Column("name")]
        public string name
        {
            get { return _name; }
            set
            {
                var oldValue = _name;
                _name = value;
                if (oldValue != value)
                    NotifyPropertyChanged("name");
            }
        }

        private bool _enable;

        [JsonProperty(PropertyName = "active")]
        [Column("enable")]
        public bool enable
        {
            get { return _enable; }
            set
            {
                var oldValue = _enable;
                _enable = value;
                if (oldValue != value)
                    NotifyPropertyChanged("enable");
            }
        }

        [Column("does_require_approval")]
        public bool doesRequireApproval { get; set; }

        [JsonIgnore]
        [Column("is_prohibited_from_discount")]
        public bool isProhibitedFromDiscount { get; set; }

        [JsonIgnore]
        [Column("last_edit")]
        public DateTime lastEdit { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        [Column("uuid")]
        public Guid? UUID { get; set; }

        public class CategoryContext : MyDbContext
        {
            public CategoryContext() : base()
            {
            }

            public DbSet<Category> Categories { get; set; }
        }

        public Category()
        {
        }

        public Category(string name, int parentID = 0)
        {
            this.parentID = parentID;
            this.name = name.ToUpper();
            this.enable = true;
            this.isProhibitedFromDiscount = false;
            this.lastEdit = DateTime.Now;
            this.UUID = Guid.NewGuid();
        }

        /*
         * init from json object
         * parentID is very bad. We need to receive parent UUID
         *
         */

        public Category(JObject obj, DateTime lastEditDate)
        {
            name = Utility.GetStringValueForKey(obj, "name").ToUpper();
            enable = Utility.GetBoolValueForKey(obj, "active");
            UUID = Utility.GetUUIDValueForKey(obj, "uuid");
            lastEdit = lastEditDate;
        }

        #region Database

        public static List<Category> GetAllFromDB(bool ignoreEnableValue = false)
        {
            using (var db = new CategoryContext())
            {
                List<Category> result;

                if (ignoreEnableValue)
                    result = (from c in db.Categories orderby c.name select c).ToList();
                else result = (from c in db.Categories where c.enable == true orderby c.name select c).ToList();

                return result;
            }
        }

        public static Dictionary<int, string> GetCategoryDictFromDB()
        {
            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories orderby c.name where c.enable == true select c;
                if (result != null && result.Count() > 0)
                    return result.ToDictionary(c => c.ID, c => c.name);

                // return empty dict as default
                return new Dictionary<int, string>();
            }
        }

        public static Dictionary<string, int> GetCategoryNameDictFromDB()
        {
            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories orderby c.name where c.enable == true select c;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MyCategoryNameComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(c => c.name, c => c.ID);
                }
                // return empty dict as default
                return new Dictionary<string, int>();
            }
        }

        public static Dictionary<string, Guid?> GetCategoryNameAndUUIDDict()
        {
            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories orderby c.name where c.enable == true select c;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MyCategoryNameComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(c => c.name, c => c.UUID);
                }

                // return empty dict as default
                return new Dictionary<string, Guid?>();
            }
        }

        public static Dictionary<Guid, int> GetCategoryUuidAndIdDictFromDB()
        {
            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories orderby c.name where c.UUID != null && c.enable == true select c;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MyCategoryNameComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(c => c.UUID.Value, c => c.ID);
                }
                // return empty dict as default
                return new Dictionary<Guid, int>();
            }
        }

        public static int GetMaxID()
        {
            using (var db = new CategoryContext())
            {
                int? maxID = db.Categories.Max(c => (int?)c.ID);
                if (maxID == null) return 1;
                else return maxID.Value;
            }
        }

        /*
        NOTE: escapse % to prevent crash on database query
        */

        public static bool DoesCategoryNameExist(string categoryName)
        {
            using (var db = new CategoryContext())
            {
                categoryName = categoryName.Replace("%", "").Trim().ToUpper();
                var query = from c in db.Categories where c.name.ToUpper() == categoryName select c;
                return query.Any();
            }
        }

        /// <summary>
        /// Get all category which discount restriction
        /// </summary>
        /// <param name="isProbihitedFromDiscount">Boolean</param>
        /// <returns>List</returns>
        public static List<Category> GetRestrictionAllFromDB(bool isProbihitedFromDiscount = true)
        {
            using (var db = new CategoryContext())
            {
                List<Category> result;
                result = (from c in db.Categories
                          where c.isProhibitedFromDiscount == isProbihitedFromDiscount
                          orderby c.name
                          select c).ToList();
                return result;
            }
        }

        /// <summary>
        /// Check category is allow discount or not by id
        /// </summary>
        /// <param name="categoryId">Int32</param>
        /// <returns>bool</returns>
        public static bool IsCategoryDiscount(int? categoryId)
        {
            if (categoryId == null) return false;
            using (var db = new CategoryContext())
            {
                var query = (from c in db.Categories where c.ID == categoryId select c);
                var category = query.Single<Category>();
                if (category == null) return false;
                return category.isProhibitedFromDiscount;
            }
        }

        /// <summary>
        /// Get category by category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Category GetCategoryById(int? id)
        {
            if (id == null) return null;

            using (var db = new Category.CategoryContext())
            {
                var result = from c in db.Categories where c.ID == id select c;
                if (result != null && result.Count() > 0)
                {
                    return result.FirstOrDefault();
                }

                return null;
            }
        }

        public static Category GetCategoryByUUID(Guid? uuid)
        {
            if (uuid == null || uuid.HasValue == false) return null;

            using (var db = new Category.CategoryContext())
            {
                var result = from c in db.Categories where c.UUID.Value == uuid.Value select c;
                if (result != null && result.Count() > 0)
                {
                    return result.FirstOrDefault();
                }

                return null;
            }
        }

        public static Guid? GetCategoryUUIDByID(int id)
        {
            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories where c.ID == id select c.UUID;
                return result.FirstOrDefault();
            }
        }

        public static int? GetIDByUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return null;

            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories where c.UUID == uuid select c.ID;
                if (result.Count() <= 0) return null;

                return result.FirstOrDefault();
            }
        }

        public static bool DoesUUIDExist(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return false;

            using (var db = new Category.CategoryContext())
            {
                var result = from c in db.Categories where c.UUID.Value == uuid.Value select c;
                return result.Any();
            }
        }

        public static bool DisableCategoryWithUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return false;

            var cmd = new NpgsqlCommand("UPDATE category SET enable = false WHERE uuid = :uuidValue");
            cmd.Parameters.AddWithValue("uuidValue", NpgsqlDbType.Uuid, uuid.ToString());

            var affectedRow = DataConnection.ExecuteNonQuery(cmd);
            return (affectedRow > 0);
        }

        #endregion Database

        #region NOTIFY CHANGED

        private void NotifyPropertyChanged(string propName)
        {
            if (!isSaveLog) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

            PropertyInfo property = typeof(Category).GetProperty(propName);
            if (property == null) return;
            string propertyName = Utility.ConvertPropertyNameToDBName(propName);
            if (propertyName == "enable") propertyName = "active";
            dynamic propertyValue = property.GetValue(this);
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

        #endregion NOTIFY CHANGED
    }
}