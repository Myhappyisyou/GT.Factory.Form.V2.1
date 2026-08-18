using GT.Factory.Common.Models;
using GT.Factory.Data.Core;
using GT.Factory.Station;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace GT.Factory.Station.Helper.BydUser
{
    public class LocalUserService : ILocalUserService
    {
        //private AccessMdbHelper db;
        public LocalUserService()
        {
            //db = new AccessMdbHelper(Config.Instance.UserdbPath);
        }

        public async Task<User> GetUserByCard(string cardId)
        {
            return await AppRuntime.Current
        .DataRuntime.Repositories.Access.GetByUIDAsync(cardId);
            //return UploadSql.Ac_SelectUsersInforByUID(db, cardId);

        }

        public async Task<User> GetUserByLogin(string jobNub, string password)
        {
            return await AppRuntime.Current
        .DataRuntime.Repositories.Access.GetByJobAsync(jobNub, password);

            //return UploadSql.Ac_SelectUsersInforByJobNub(db, username, password);

        }

        public async Task<User> GetUserByRoleId(string username, string password)
        {
            return await AppRuntime.Current
        .DataRuntime.Repositories.Access.GetByRoleIdAsync(username, password);

            //return UploadSql.Ac_SelectUsersInforByJobNub(db, username, password);

        }

        public async void UpdateUserLog(User user)
        {
            await AppRuntime.Current
        .DataRuntime.Repositories.Access.InsertOrUpdateUserAsync(user);

            //UploadSql.Ac_InsertOrUpdateUser(db, user);

        }

    }
}
