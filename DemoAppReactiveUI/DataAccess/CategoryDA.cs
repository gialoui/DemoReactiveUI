using DemoAppReactiveUI.Model;
using log4net;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DemoAppReactiveUI.DataAccess
{
    public partial class CategoryDA
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class CategoryContext : MyDbContext
        {
            public CategoryContext() : base()
            {
            }

            public DbSet<Category> Categories { get; set; }
        }

        private CategoryContext categoryContext;

        public CategoryDA()
        {
            categoryContext = new CategoryContext();
        }

        public void SetAutoDetectChanges(bool status)
        {
            categoryContext.Configuration.AutoDetectChangesEnabled = status;
        }

        public Task<int> SaveProductImportChanges()
        {
            return categoryContext.SaveChangesAsync();
        }

        public void AddCategoryImportEntry(Category category)
        {
            categoryContext.Categories.Add(category);
        }

        public Task<int> SaveCategoryImportChanges()
        {
            return categoryContext.SaveChangesAsync();
        }

        public static Dictionary<string, int> GetCategoryIDDict()
        {
            using (var db = new CategoryContext())
            {
                var result = from c in db.Categories where c.enable == true select c;
                return result.ToDictionary(c => c.name, c => c.ID);
            }
        }

        public static Dictionary<int, string> GetAllCategoryNameDict()
        {
            using (var db = new CategoryContext())
            {
                return db.Categories.ToDictionary(c => c.ID, c => c.name);
            }
        }
    }
}
