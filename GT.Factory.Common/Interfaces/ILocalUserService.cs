using GT.Factory.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Interfaces
{
    public interface ILocalUserService
    {
        Task<User> GetUserByCard(string cardId);

        Task<User> GetUserByLogin(string jobNub, string password);

        Task<User> GetUserByRoleId(string username, string password);

        void UpdateUserLog(User user);
    }
}
