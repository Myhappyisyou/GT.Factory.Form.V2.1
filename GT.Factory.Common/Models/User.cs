using GT.Factory.Common.Enums;
using GT.Factory.Common.Helper;
using System;
using System.ComponentModel;

namespace GT.Factory.Common.Models
{
    public class User
    {
        [DisplayName("ID")]
        public int ID { get; set; }

        [DisplayName("用户名")]
        public string UserName { get; set; }

        [DisplayName("用户密码")]
        public string UserPassword { get; set; }

        [DisplayName("用户权限")]
        public string UserRole { get; set; }

        [DisplayName("厂牌UID")]
        public string UID { get; set; }

        [DisplayName("工号")]
        public string JobNub { get; set; }

        [DisplayName("MES账号")]
        public string MesAccount { get; set; }

        [DisplayName("MES密码")]
        public string MesPassword { get; set; }

        // 是否启用
        public bool IsEnabled { get; set; } = true;

        // 创建时间
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 是否需要MES账号
        public MesCredential MesCredential { get; set; }

        public UserLevel LevelEnum
        {
            get => UserLevelHelper.Parse(UserRole);
            set => UserRole = UserLevelHelper.ToLevelString(value);
        }
    }

    public class MesCredential
    {
        public string Account { get; set; }

        public string Password { get; set; }
    }
}
