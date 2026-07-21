using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GT_Common;
using GT_Common.Model;

namespace GT_Common.Helper.BydUser
{
    public class LocalUserService : ILocalUserService
    {
        private AccessMdbHelper db;

        public LocalUserService()
        {
            db = new AccessMdbHelper(Config.Instance.UserdbPath);
        }

        public User GetUserByCard(string cardId)
        {
            return UploadSql.Ac_SelectUsersInforByUID(db, cardId);

        }

        public User GetUserByLogin(string username, string password)
        {
            return UploadSql.Ac_SelectUsersInforByJobNub(db, username, password);

        }

        public void UpdateUserLog(User user)
        {
            UploadSql.Ac_InsertOrUpdateUser(db, user);

        }

    }
}
