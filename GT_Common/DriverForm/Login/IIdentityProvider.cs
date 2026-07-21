using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    /// <summary>
    /// 身份识别提供者接口
    /// 每种登录方式实现该接口
    /// </summary>
    public interface IIdentityProvider
    {
        /// <summary>
        /// 显示名称（用于Tab标题）
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取登录UI控件
        /// </summary>
        Control GetLoginControl();

        /// <summary>
        /// 登录成功事件
        /// </summary>
        event Action<User> LoginSuccess;

        /// <summary>
        /// 启动身份识别（如启动刷卡监听）
        /// </summary>
        void Start();

        /// <summary>
        /// 停止身份识别
        /// </summary>
        void Stop();
    }
}
