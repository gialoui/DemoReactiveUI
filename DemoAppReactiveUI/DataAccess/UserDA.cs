using DemoAppReactiveUI.Model;
using System.Collections.Generic;
using System.Linq;
using static DemoAppReactiveUI.Model.User;

namespace DemoAppReactiveUI.DataAccess
{
    public static class UserDA
    {
        public static IEnumerable<User> GetAll()
        {
            using (var db = new UserContext())
            {
                return db.Users.ToList();
            }
        }

        public static Dictionary<int, string> GetAllUsersDictForSecureLogin()
        {
            using (var db = new UserContext())
            {
                return db.Users?
                        .Where(x => x.username != null)
                        .OrderBy(x => x.username)
                        .ToDictionary(x => x.ID, x => x.username);
            }
        }

        #region PIN USAGE

        public static User GetEnableUserByPIN(string PIN)
        {
            using (var db = new UserContext())
            {
                var query = from u in db.Users where u.PIN == PIN && u.Enable == true select u;
                var result = query.FirstOrDefault();
                return result;
            }
        }

        #endregion PIN USAGE
    }
}