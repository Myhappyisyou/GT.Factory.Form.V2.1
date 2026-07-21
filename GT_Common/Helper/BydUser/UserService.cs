using GT_Common;
using GTJPN2503006.Helper.ClientTask;
using GTJPN2503006.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTJPN2503006.Helper.BydUser
{
    public class UserService : IUserService
    {
        //private readonly AccessMdbHelper _db;

        private readonly AccessMdbHelper _db;


        public UserService(string dbPath,string userPath)
        {
            
            _db = new AccessMdbHelper(userPath);

        }

        //  通过UID号获取用户信息
        public User GetUserByCard(string cardId)
        {
            return UploadSql.Ac_SelectUsersInforByUID(_db, cardId);
        }

        //  通过账号密码获取用户信息
        public User GetUserByLogin(string username, string password)
        {
            return UploadSql.Ac_SelectUsersInforByJobNub(_db, username, password);
        }

        //  上传用户登录信息
        public void InsertUserSwipeLog(User user)
        {

        }

    }

}
