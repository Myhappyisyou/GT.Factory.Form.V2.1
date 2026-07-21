using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public static class UserMapper
    {
        public static GT_Common.Model.User ToLocalUser(
            this TaskContracts.Models.User user)
        {
            if (user == null)
                return null;

            return new GT_Common.Model.User
            {
                ID = user.ID,
                UserName = user.UserName,
                UserPassword = user.UserPassword,
                UserRole = user.UserRole,
                UID = user.UID,
                JobNub = user.JobNub,
                MesAccount = user.MesAccount,
                MesPassword = user.MesPassword,
                LevelEnum= (GT_Common.MyEnum.UserLevel)user.LevelEnum,
            };
        }

        public static TaskContracts.Models.User ToContractUser(
            this GT_Common.Model.User user)
        {
            if (user == null)
                return null;

            return new TaskContracts.Models.User
            {
                ID = user.ID,
                UserName = user.UserName,
                UserPassword = user.UserPassword,
                UserRole = user.UserRole,
                UID = user.UID,
                JobNub = user.JobNub,
                MesAccount = user.MesAccount,
                MesPassword = user.MesPassword,
                LevelEnum = (TaskContracts.Models.UserLevel)user.LevelEnum,

            };
        }
    }
}
