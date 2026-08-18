using GT.Factory.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.Helper.BydUser
{
    //public interface IUserService
    //{
    //    //  通过UID号获取用户信息
    //    User GetUserByCard(string cardId);

    //    //  通过账号密码获取用户信息
    //    User GetUserByLogin(string username, string password);

    //    //  上传用户登录信息
    //    void InsertUserSwipeLog(User user);
    //}

    public interface IUserService
    {
        //  通过UID号获取用户信息
        Task<User> GetUserByCard(string cardId);

        //  通过账号密码获取用户信息
        Task<User> GetUserByLogin(string username, string password);

        //  通过账号密码获取用户信息
        Task<User> GetUserByRoleId(string username, string password);

        //  上传用户登录信息
        Task InsertUserSwipeLog(User user);
    }
}
