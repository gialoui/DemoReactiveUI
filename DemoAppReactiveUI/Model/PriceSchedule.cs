using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace DemoAppReactiveUI.Model
{
    [Table("price_schedule", Schema = "public")]
    public class PriceSchedule
    {
        [Column("id"), Key]
        public Int64 ID { get; set; }

        [Column("product_id")]
        public int productID { get; set; }

        [Column("price")]
        public double price { get; set; }

        [Column("price_cold")]
        public double priceCold { get; set; }

        [Column("start_on")]
        public DateTime scheduleDate { get; set; }

        public class PriceScheduleContext : MyDbContext
        {
            public PriceScheduleContext() : base()
            {
            }

            public DbSet<PriceSchedule> PriceSchedules { get; set; }
        }

        public PriceSchedule()
        {
        }

        public PriceSchedule(int productID, double price, double priceCold, DateTime scheduleDate)
        {
            this.productID = productID;
            this.price = price;
            this.priceCold = priceCold;
            this.scheduleDate = scheduleDate;
        }

        public string GetScheduleDateString()
        {
            return String.Format("{0:MMMM d, yyyy}", scheduleDate);
        }

        public string GetSellingPriceString()
        {
            return "Selling Price: " + price.ToString("0.00");
        }

        public string GetColdPriceString()
        {
            return "Cold Price: " + priceCold.ToString("0.00");
        }

        #region DATABASE

        public static PriceSchedule GetByProductID(int productID)
        {
            using (var db = new PriceScheduleContext())
            {
                var result = from p in db.PriceSchedules where p.productID == productID select p;
                if (result != null && result.Count() > 0)
                    return result.First();
                return null;
            }
        }

        public static List<PriceSchedule> GetAllByDate(DateTime date)
        {
            using (var db = new PriceScheduleContext())
            {
                var result = from p in db.PriceSchedules
                             where DbFunctions.TruncateTime(p.scheduleDate) <= DbFunctions.TruncateTime(DateTime.Now)
                             select p;
                if (result != null && result.Count() > 0)
                    return result.ToList();
                return null;
            }
        }

        public bool SaveToDB()
        {
            using (var db = new PriceScheduleContext())
            {
                db.PriceSchedules.Add(this);
                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        public bool UpdateToDB(double price, double priceCold, DateTime scheduleDate)
        {
            using (var db = new PriceScheduleContext())
            {
                this.price = price;
                this.priceCold = priceCold;
                this.scheduleDate = scheduleDate;

                db.PriceSchedules.Attach(this);
                db.Entry(this).Property("price").IsModified = true;
                db.Entry(this).Property("priceCold").IsModified = true;
                db.Entry(this).Property("scheduleDate").IsModified = true;

                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        public static bool DeleteFromDB(List<PriceSchedule> priceSchedules)
        {
            using (var db = new PriceScheduleContext())
            {
                foreach (var item in priceSchedules)
                {
                    db.PriceSchedules.Attach(item);
                    db.PriceSchedules.Remove(item);
                }
                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        public bool DeleteFromDB()
        {
            using (var db = new PriceScheduleContext())
            {
                db.PriceSchedules.Attach(this);
                db.PriceSchedules.Remove(this);
                var affectedRow = db.SaveChanges();
                if (affectedRow > 0) return true;
                return false;
            }
        }

        #endregion DATABASE
    }
}