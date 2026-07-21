using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public partial class LoginForm : Form
    {
        private TabControl _tabControl;
        private readonly List<IIdentityProvider> _providers;

        public User CurrentUser { get; private set; }

        public LoginForm(List<IIdentityProvider> providers)
        {
            _providers = providers;
            InitUI();
            LoadProviders();
        }

        private void InitUI()
        {
            Text = "登录";
            Width = 420;
            Height = 300;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            _tabControl = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(_tabControl);
        }

        private void LoadProviders()
        {
            foreach (var provider in _providers)
            {
                var tab = new TabPage(provider.Name);

                tab.Controls.Add(provider.GetLoginControl());

                _tabControl.TabPages.Add(tab);

                provider.LoginSuccess += OnLoginSuccess;
                provider.Start();
            }
        }

        private void OnLoginSuccess(User user)
        {
            CurrentUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
