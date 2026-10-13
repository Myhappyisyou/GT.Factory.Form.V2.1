using Serilog;
using GT.Factory.Server.Core;
using GT.Factory.Server.Core.ClientInfor;
using GT.Factory.Server.Core.LanModelSync;
using GT.Factory.Server.Help;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GT.Factory.Logging.Logging;
using GT.Factory.Common.Helpers;

namespace GT.Factory.Server
{
    public partial class UC_ChangeOrder : UserControl, IPageLifecycle
    {
        private ComboBox modelCombo;
        private Button switchButton;
        private DataGridView deviceGrid;
        private List<ProcessClient> lsProcessClients;
        private DeviceSyncService _syncService;
        ContextMenuStrip cmsMenu;
        ToolStripMenuItem switchItem;
        private bool _lastChangeEnable = false;
        JsonService jsonService = new JsonService();
        private Timer _timer;


        public UC_ChangeOrder(DeviceSyncService syncService)
        {
            _syncService = syncService;

            InitializeComponent();


            //lsProcessClients = UploadSql.SelectProcessClients(db);

            lsProcessClients = new List<ProcessClient>
            {
                new ProcessClient
                {
                    ID = 1,
                    ClientName = "默认客户端",
                    ClientDetail = "默认描述",
                    ClientIp = "127.0.0.1"
                }
            };

            Text = "型号切换主控端";

            Size = new Size(600, 400);

            modelCombo = new ComboBox { Dock = DockStyle.Top,Font= new Font("Microsoft Sans Serif", 20, FontStyle.Bold) };
            
            modelCombo.Items.AddRange(Directory.GetFiles(PathCenter.Models, "*.json")
                              .Select(Path.GetFileNameWithoutExtension)
                              .ToArray());

            modelCombo.SelectedIndex = 0;

            switchButton = new Button 
            { 
                Text = "切换所选设备型号", 
                Dock = DockStyle.Top,
                AutoSize = true, 
                Enabled = false, 
                ForeColor = Color.White, 
                Font = new Font("Microsoft Sans Serif", 20, FontStyle.Bold) 
            };
            switchButton.Click += (s, e) => SwitchModel();

            deviceGrid = new DataGridView { 
                Dock = DockStyle.Fill, ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                MultiSelect = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None // 先设置为None，然后分别设置
            };
            deviceGrid.Columns.Add("Process", "设备号");
            deviceGrid.Columns.Add("ProcessName", "设备名称");
            deviceGrid.Columns.Add("IP", "设备 IP");
            deviceGrid.Columns.Add("State", "状态");
            deviceGrid.Columns.Add("ModelName", "当前型号");

            // 为不同列设置不同的调整策略
            deviceGrid.Columns["Process"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            deviceGrid.Columns["ProcessName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            deviceGrid.Columns["IP"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            deviceGrid.Columns["State"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // 型号列填充剩余空间
            deviceGrid.Columns["ModelName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            Controls.Add(deviceGrid);
            Controls.Add(switchButton);
            Controls.Add(modelCombo);

            InitContextMenu();

            // ✅ 关键：取消所有默认选中行，防止选中颜色遮盖背景色
            deviceGrid.ClearSelection();

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;

            DisplayLog.Info("[ChangeOrder] 控件初始化完成",true);
            Log.Information("[ChangeOrder] 控件初始化完成");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 每秒刷新设备状态按钮可用性
            var changeEnable = GetSelectedIPs().All(ip =>
                _syncService.Monitor.DeviceStates.TryGetValue(ip, out var state) && state.Status == "Ready");

            if (changeEnable != _lastChangeEnable)
            {
                switchButton.Enabled = changeEnable;
                switchItem.Enabled = changeEnable;
                _lastChangeEnable = changeEnable;
            }
        }

        private void UpdateDeviceGrid(string ip, DeviceStatus state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateDeviceGrid(ip, state)));
                return;
            }

            try
            {
                ProcessClient processClient = lsProcessClients
                               .Where(t => t.ClientIp == ip)
                               .FirstOrDefault();
                string name = processClient?.ClientName ?? "未知工位";   // ?. 安全访问
                string detail = processClient?.ClientDetail ?? "未知工位";

                DeviceGridHelper.UpdateDeviceRow(deviceGrid, name, detail, ip, state.Status, state.ModelName);
            }           
            catch (Exception ex)
            {
                DisplayLog.Error($"未知工位：ip:{ip}--{state.Status}--{state.ModelName}",ex,true);
                
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private List<string> GetSelectedIPs()
        {
            return deviceGrid.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => new
                {
                    IP = r.Cells["IP"].Value?.ToString(),
                    State = r.Cells["State"].Value?.ToString()
                })
                .Where(x => !string.IsNullOrEmpty(x.IP) && !string.Equals(x.State, "offline", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.IP)
                .ToList();
        }

        private void InitContextMenu()
        {
            cmsMenu   = new ContextMenuStrip();
            switchItem = new ToolStripMenuItem("切换该设备型号");
            switchItem.Click += (s, e) =>
            {
                if (deviceGrid.CurrentRow != null)
                {
                    string ip = deviceGrid.CurrentRow.Cells["IP"].Value?.ToString();

                    SwitchModelTo(ip);
                }
            };
            cmsMenu.Items.Add(switchItem);
            deviceGrid.ContextMenuStrip = cmsMenu;
        }

        private void SwitchModelTo(string ip)
        {
            string selected = modelCombo.SelectedItem.ToString();

            var product = jsonService.LoadFromFile<ProductModel>(PathCenter.ModelFile(selected,$"{selected}.json"));

            ModelSender.SendModelToClients(product, new[] { ip }, 5555);
            MessageBox.Show($"已发送型号到设备 {ip}");
            Log.Information($"[ChangeOrder] 请求切换型号={selected} 设备={string.Join(",", ip)}", true);
        }

        private async void SwitchModel()
        {
            var selectedIPs = GetSelectedIPs();
            if (!selectedIPs.Any())
            {
                MessageBox.Show("请至少选择一个设备行");
                return;
            }

            string selected = modelCombo.SelectedItem.ToString();

            var product = jsonService.LoadFromFile<ProductModel>(PathCenter.ModelFile(selected, $"{selected}.json"));

            var confirm = MessageBox.Show(
                $"确认切换型号 [{selected}] 到以下设备？\n{string.Join("\n", selectedIPs)}",
                "确认型号切换",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                DisplayLog.Info("[ChangeOrder] 用户取消型号切换",true);
                return;
            }

            await ModelSender.SendModelToClientsAsync(product, selectedIPs.ToArray(), 5555);

            MessageBox.Show($"型号已发送至选中设备：{string.Join(", ", selectedIPs)}");

            Log.Information($"[ChangeOrder] 请求切换型号={selected} 设备={string.Join(",", selectedIPs)}",true);
        }

        public void OnPageShown()
        {
            modelCombo.Items.Clear();

            modelCombo.Items.AddRange(Directory.GetFiles(PathCenter.Models, "*.json")
                             .Select(Path.GetFileNameWithoutExtension)
                             .ToArray());

            modelCombo.SelectedIndex = 0;


            _syncService.Monitor.OnDeviceStateChanged -= UpdateDeviceGrid;

            _syncService.Monitor.OnDeviceStateChanged += UpdateDeviceGrid;
            // ✅ 初始化显示已有状态
            foreach (var kvp in _syncService.Monitor.DeviceStates)
            {
                if (kvp.Value != null)
                    UpdateDeviceGrid(kvp.Key, kvp.Value);
            }
            _timer.Start();
            DisplayLog.Info("[ChangeOrder] 页面显示，Timer启动");
        }

        public void OnPageHidden()
        {
            _timer.Stop();
            //_syncService.Monitor.OnDeviceStateChanged -= UpdateDeviceGrid;
            DisplayLog.Info("[ChangeOrder] 页面隐藏，Timer停止");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_syncService?.Monitor != null)
                    _syncService.Monitor.OnDeviceStateChanged -= UpdateDeviceGrid;
            }

            _timer?.Stop();
            _timer?.Dispose();

            components?.Dispose();
            base.Dispose(disposing);
        }
    }
}
