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
    [Table("supplier", Schema = "public")]
    public class Supplier : INotifyPropertyChanged
    {
        public static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _displayname;
        private bool _enable;

        [JsonIgnore]
        public bool isSaveLog = false;

        [JsonIgnore]
        public Dictionary<string, dynamic> ChangedPropertiesList = new Dictionary<string, dynamic>();

        [JsonIgnore]
        [Column("id"), Key]
        public int ID { get; set; }

        [JsonProperty(PropertyName = "display_name")]
        [Column("name")]
        public string name
        {
            get { return _name; }
            set
            {
                var oldValue = _name;
                _name = value;
                if (oldValue != value) NotifyPropertyChanged("name");
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string DisplayName
        {
            get { return name; }
            set
            {
                _displayname = value;
            }
        }

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

        [JsonIgnore]
        [Column("last_edit")]
        public DateTime lastEdit { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        [Column("uuid")]
        public Guid? UUID { get; set; }

        public class SupplierContext : MyDbContext
        {
            public SupplierContext() : base()
            {
            }

            public DbSet<Supplier> Suppliers { get; set; }

            public int GetNextSequenceValue()
            {
                var rawQuery = Database.SqlQuery<int>("SELECT nextval('supplier_id_seq'::regclass); ");
                var task = rawQuery.SingleAsync();
                int nextVal = task.Result;
                return nextVal;
            }
        }

        public Supplier()
        {
        }

        public Supplier(string name)
        {
            this.name = name;
            this.enable = true;
            this.lastEdit = DateTime.Now;
            UUID = Guid.NewGuid();
        }

        public Supplier(int ID, string name)
        {
            this.ID = ID;
            this.name = name;
            this.enable = true;
            this.lastEdit = DateTime.Now;
            UUID = Guid.NewGuid();
        }

        /*
        Overwrite ToString to support auto complete supplier input
        */

        public override string ToString()
        {
            return name;
        }

        public Supplier(JObject obj, DateTime lastEditDate)
        {
            name = Utility.GetStringValueForKey(obj, "display_name").ToUpper();
            enable = Utility.GetBoolValueForKey(obj, "active");
            UUID = Utility.GetUUIDValueForKey(obj, "uuid");
            lastEdit = lastEditDate;
        }

        #region ACTIONS

        public static int? GetLocalIDByUUID(Guid? _uuid)
        {
            if (_uuid == null) return null;

            using (var db = new SupplierContext())
            {
                var result = from s in db.Suppliers where s.UUID == _uuid select s.ID;
                if (result != null && result.Count() > 0)
                    return result.First();
                return null;
            }
        }

        public static bool DoesUUIDExist(Guid? uuid)
        {
            if (uuid == null || uuid.HasValue == false) return false;

            using (var db = new SupplierContext())
            {
                var result = from s in db.Suppliers where s.UUID.Value == uuid.Value select s;
                return result.Any();
            }
        }

        public static bool DisableSupplierWithUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return false;

            var cmd = new NpgsqlCommand("UPDATE supplier SET enable = false WHERE uuid = :uuidValue");
            cmd.Parameters.AddWithValue("uuidValue", NpgsqlDbType.Uuid, uuid.ToString());

            var affectedRow = DataConnection.ExecuteNonQuery(cmd);
            return (affectedRow > 0);
        }

        public static int? GetIDByUUID(Guid? uuid)
        {
            if (!uuid.HasValue || uuid.Value == Guid.Empty) return null;

            using (var db = new SupplierContext())
            {
                var result = from s in db.Suppliers where s.UUID == uuid select s.ID;
                if (result.Count() <= 0) return null;

                return result.FirstOrDefault();
            }
        }

        #endregion ACTIONS

        #region HELPERS

        public static Dictionary<Guid, int> GetSupplierUuidAndIdDictFromDB()
        {
            using (var db = new SupplierContext())
            {
                var result = from c in db.Suppliers orderby c.name where c.UUID != null && c.enable == true select c;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MySupplierNameComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(c => c.UUID.Value, c => c.ID);
                }
                // return empty dict as default
                return new Dictionary<Guid, int>();
            }
        }

        /*
        NOTE: escapse % to prevent crash on database query
        */

        public static bool DoesSupplierNameExist(string supplierName)
        {
            using (var db = new Supplier.SupplierContext())
            {
                supplierName = supplierName.Replace("%", "").Trim().ToUpper();
                var query = from s in db.Suppliers where s.name.ToUpper() == supplierName select s;
                return query.Any();
            }
        }

        public static List<Supplier> GetAllSuppliers(bool ignoreEnableValue = false)
        {
            using (var db = new Supplier.SupplierContext())
            {
                List<Supplier> suppliers;

                if (ignoreEnableValue)
                    suppliers = (from s in db.Suppliers select s).OrderBy(s => s.name).ToList();
                else suppliers = (from s in db.Suppliers where s.enable == true select s).OrderBy(s => s.name).ToList();
                return suppliers;
            }
        }

        /*
         * We agree that, saved data is finalize.
         * We shouldn't do any trim or upper, lower case for data return from database
         */

        public static Dictionary<string, int> GetSupplierDictNameFromDB()
        {
            using (var db = new SupplierContext())
            {
                var result = from s in db.Suppliers
                             orderby s.name
                             where s.enable == true
                             select s;
                if (result != null && result.Count() > 0)
                {
                    var comparer = new MySupplierNameComparer();
                    return result.AsEnumerable().Distinct(comparer).ToDictionary(s => s.name, s => s.ID);
                }
                // return empty dict as default
                return new Dictionary<string, int>();
            }
        }

        public static Supplier GetSupplierByID(int? supplierID)
        {
            if (supplierID != null)
            {
                using (var db = new Supplier.SupplierContext())
                {
                    var result = from s in db.Suppliers where s.ID == supplierID select s;
                    if (result != null && result.Count() > 0)
                    {
                        return result.First();
                    }
                }
            }
            return null;
        }

        public static Supplier GetSupplierByUUID(Guid? UUID)
        {
            if (UUID != null && UUID.HasValue)
            {
                using (var db = new Supplier.SupplierContext())
                {
                    var result = from s in db.Suppliers where s.UUID.Value == UUID.Value select s;
                    if (result != null && result.Count() > 0)
                    {
                        return result.First();
                    }
                }
            }
            return null;
        }

        public static Guid? GetSupplierUUIDByLocalID(int? supplierID)
        {
            if (supplierID != null)
            {
                using (var db = new Supplier.SupplierContext())
                {
                    var result = from s in db.Suppliers where s.ID == supplierID select s.UUID;
                    if (result != null && result.Count() > 0)
                    {
                        return result.FirstOrDefault();
                    }
                }
            }
            return null;
        }

        public bool UpdateSupplierUUID()
        {
            using (var db = new SupplierContext())
            {
                if (UUID == null)
                {
                    UUID = Guid.NewGuid();
                    db.Suppliers.Attach(this);
                    db.Entry(this).Property("UUID").IsModified = true;
                }
                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        #endregion HELPERS

        #region NOTIFY CHANGED

        private void NotifyPropertyChanged(string propName)
        {
            if (!isSaveLog) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

            PropertyInfo property = typeof(Supplier).GetProperty(propName);
            if (property == null) return;
            string propertyName = Utility.ConvertPropertyNameToDBName(propName);
            dynamic propertyValue = property.GetValue(this);
            if (propertyName == "enable") propertyName = "active";
            if (propertyName == "name") propertyName = "display_name";
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