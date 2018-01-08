using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace DemoAppReactiveUI.Model
{
    [Table("users", Schema = "public")]
    public class User
    {
        [Key, Column("id")]
        public int ID { get; set; }

        [Column("role_id")]
        public int roleID { get; set; }

        [Column("name")]
        public string name { get; set; } = "";

        [Column("pin")]
        public string PIN { get; set; } = "";

        [Column("username")]
        public string username { get; set; }

        [Column("last_login")]
        public DateTime? lastLogin { get; set; }

        [Column("invalid_login_attempts")]
        public int invalidLoginAttempts { get; set; }

        [Column("does_require_password_change")]
        public bool doesRequirePasswordChange { get; set; }

        [Column("enable")]
        public bool Enable { get; set; } = true;

        public User()
        {
        }

        public class UserContext : MyDbContext
        {
            public UserContext() : base()
            {
            }

            public DbSet<User> Users { get; set; }
        }
    }
}
