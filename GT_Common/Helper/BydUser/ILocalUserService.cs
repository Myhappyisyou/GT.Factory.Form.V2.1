using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.BydUser
{
    public interface ILocalUserService
    {
        User GetUserByCard(string cardId);

        User GetUserByLogin(string username, string password);

        void UpdateUserLog(User user);
    }
}
