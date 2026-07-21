using GT_Common.Helper;
using GT_Common.Helper.QueryClient;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Order
{
    public partial class BydMesForm : Form
    {
        private TextBox txtOrderNo, txtItem, txtItemDesc, txtRouter, txtOrderNum, txtCompNum, txtCompRate;
        private TextBox txtBindOrder;

        private TextBox txtCheckBar, txtUpDataBar, txtTestItems;
        private Panel pnlCheckLamp;
        private CheckBox cbBarResult;
        private Panel pnlUpDataLamp;

        // ===== 离散装配 =====
        private TextBox txtAssembleParent;
        private TextBox txtAssemblePart;
        private TextBox txtQueryAssemble;
        private DataGridView dgvAssemble;

        private QueryClient MesApiclient;

        public BydMesForm()
        {
            InitUI();
        }

        private void InitUI()
        {
            Text = "MES测试页面";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;

            var tab = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 120
            };

            tab.TabPages.Add(CreateOrderTab());
            tab.TabPages.Add(CreateBarTab());
            tab.TabPages.Add(CreateResourceTab());
            tab.TabPages.Add(CreateDeviceTab());  // 新增 Device Tab

            var resultPanel = CreateResultPanel();
            resultPanel.Dock = DockStyle.Top;

            dgvAssemble = CreateAssembleGrid();
            dgvAssemble.Dock = DockStyle.Fill;

            var bindPanel = CreateBindPanel();
            bindPanel.Dock = DockStyle.Bottom;

            Controls.Add(dgvAssemble);
            Controls.Add(bindPanel);
            Controls.Add(resultPanel);
            Controls.Add(tab);

            MesApiclient = new QueryClient(Config.Instance.ApiUrl);

        }

        #region Tabs
        private TabPage CreateOrderTab()
        {
            var page = new TabPage("工单查询");
            var txt = CreateTextBox();
            var btn = CreateButton("查询");
            btn.Click += async (s, e) => await QueryByOrder(txt.Text);

            page.Controls.Add(CreateFlow("工单号", txt, btn));
            return page;
        }

        private TabPage CreateBarTab()
        {
            var page = new TabPage("产品码查询");
            var txt = CreateTextBox();
            var btn = CreateButton("查询");
            btn.Click += async (s, e) => await QueryByBar(txt.Text);

            page.Controls.Add(CreateFlow("产品码", txt, btn));
            return page;
        }

        private TabPage CreateResourceTab()
        {
            var page = new TabPage("资源查询");
            var txt = CreateTextBox();
            var btn = CreateButton("查询");

            txt.Text = Config.Instance.Resource;
            btn.Click += async (s, e) => await QueryByResource(txt.Text);

            page.Controls.Add(CreateFlow("资源", txt, btn));
            return page;
        }

        private TabPage CreateDeviceTab()
        {
            var page = new TabPage("设备集成");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(10)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            // ===== SFC 条码 =====
            var txtSfc = CreateTextBox();
            var lblSfc = CreateLabel("SFC条码");
            layout.Controls.Add(lblSfc, 0, 0);
            layout.Controls.Add(txtSfc, 1, 0);

            // ===== 单条测试项 =====
            var txtTestItem = CreateTextBox();
            var lblTestItem = CreateLabel("测试项");
            layout.Controls.Add(lblTestItem, 0, 1);
            layout.Controls.Add(txtTestItem, 1, 1);

            // ===== 获取设备信息按钮 =====
            var btnGetDeviceInfo = CreateButton("测试设备项");
            layout.Controls.Add(btnGetDeviceInfo, 2, 1);

            // ===== 结果显示 =====
            var rtbResult = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Height = 200
            };
            layout.Controls.Add(rtbResult, 0, 2);
            layout.SetColumnSpan(rtbResult, 3);

            // ===== 按钮点击事件 =====
            btnGetDeviceInfo.Click += async (s, e) =>
            {
                var sfc = txtSfc.Text.Trim();
                var testItemText = txtTestItem.Text.Trim();

                if (string.IsNullOrEmpty(sfc))
                {
                    MessageBox.Show("请输入SFC条码");
                    return;
                }

                if (string.IsNullOrEmpty(testItemText))
                {
                    MessageBox.Show("请输入测试项内容");
                    return;
                }

                var testItems = new List<TestItemInfo> { new TestItemInfo { TestItem = testItemText } };

                try
                {
                    bool ok = false;
                    string mes="", xmlOut="";

                    var assemblySnTestInfoRequest = new GetAssemblySnTestInfoRequest
                    {
                        accessToken = Config.Instance.Token,
                        appId = Config.Instance.AppId,
                        requestId = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        data = new RequestData
                        {
                            assemblyNumber = sfc,
                            stationCode = Config.Instance.StationCode,
                            testItem = Config.Instance.TestItem
                        },
                    };

                    var testInfoResponse = await MesApiclient.CallAsync<TestInfoResponse, GetAssemblySnTestInfoRequest>(Config.Instance.GetAssemblySnTestInfo, assemblySnTestInfoRequest);

                    MessageBox.Show(testInfoResponse.GetDisplayText());

                    //var response = await Task.Run(() =>
                    //    BydMesCom.获取设备集成信息(sfc, testItems, out ok, out mes, out xmlOut));

                    //rtbResult.Text = FormatDeviceResponse(response, ok, mes, xmlOut);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"获取设备集成信息失败: {ex.Message}");
                }
            };

            page.Controls.Add(layout);
            return page;
        }

        #endregion

        #region Result Panel
        private Panel CreateResultPanel()
        {
            var panel = new Panel
            {
                Height = 120,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 2
            };

            for (int i = 0; i < 7; i++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2f));

            table.Controls.Add(CreateLabel("工单号"), 0, 0);
            table.Controls.Add(CreateLabel("物料"), 1, 0);
            table.Controls.Add(CreateLabel("描述"), 2, 0);
            table.Controls.Add(CreateLabel("工艺路线"), 3, 0);
            table.Controls.Add(CreateLabel("数量"), 4, 0);
            table.Controls.Add(CreateLabel("完成数"), 5, 0);
            table.Controls.Add(CreateLabel("完成率"), 6, 0);

            txtOrderNo = CreateTextBox();
            txtItem = CreateTextBox();
            txtItemDesc = CreateTextBox();
            txtRouter = CreateTextBox();
            txtOrderNum = CreateTextBox();
            txtCompNum = CreateTextBox();
            txtCompRate = CreateTextBox();

            table.Controls.Add(txtOrderNo, 0, 1);
            table.Controls.Add(txtItem, 1, 1);
            table.Controls.Add(txtItemDesc, 2, 1);
            table.Controls.Add(txtRouter, 3, 1);
            table.Controls.Add(txtOrderNum, 4, 1);
            table.Controls.Add(txtCompNum, 5, 1);
            table.Controls.Add(txtCompRate, 6, 1);

            panel.Controls.Add(table);
            return panel;
        }
        #endregion

        #region Bind Panel
        private Panel CreateBindPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 220,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 6
            };

            // ===== 工单绑定 =====
            txtBindOrder = CreateTextBox();
            var btnBind = CreateButton("绑定");
            btnBind.Click += BtnBind_Click;

            layout.Controls.Add(CreateLabel("工单号"), 0, 0);
            layout.Controls.Add(txtBindOrder, 1, 0);
            layout.Controls.Add(btnBind, 2, 0);

            // ===== 入站 =====
            txtCheckBar = CreateTextBox();
            var btnCheck = CreateButton("入站");
            btnCheck.Click += BtnCheckBar_Click;

            pnlCheckLamp = CreateLamp();

            layout.Controls.Add(CreateLabel("入站码"), 0, 1);
            layout.Controls.Add(txtCheckBar, 1, 1);
            layout.Controls.Add(btnCheck, 2, 1);
            layout.Controls.Add(pnlCheckLamp, 3, 1);

            // ===== 出站 =====
            txtUpDataBar = CreateTextBox();
            var btnUp = CreateButton("出站");
            btnUp.Click += BtnUpData_Click;

            pnlUpDataLamp = CreateLamp();

            layout.Controls.Add(CreateLabel("出站码"), 0, 2);
            layout.Controls.Add(txtUpDataBar, 1, 2);
            layout.Controls.Add(btnUp, 2, 2);
            layout.Controls.Add(pnlUpDataLamp, 3, 2);

            // ===== 测试项 =====
            txtTestItems = CreateTextBox();
            txtTestItems.Text = "!测试人,工号,4111348!节拍,秒,10.12!电阻,1~2,1.51";

            cbBarResult = new CheckBox();

            layout.Controls.Add(CreateLabel("测试项"), 0, 3);
            layout.Controls.Add(txtTestItems, 1, 3);
            layout.Controls.Add(cbBarResult, 2, 3);

            // ===== 离散装配 =====
            txtAssembleParent = CreateTextBox();
            txtAssemblePart = CreateTextBox();
            var btnAssemble = CreateButton("装配");
            btnAssemble.Click += BtnAssemble_Click;

            layout.Controls.Add(CreateLabel("总成码"), 0, 4);
            layout.Controls.Add(txtAssembleParent, 1, 4);
            layout.Controls.Add(txtAssemblePart, 2, 4);
            layout.Controls.Add(btnAssemble, 3, 4);

            // ===== 查询装配 =====
            txtQueryAssemble = CreateTextBox();
            var btnQuery = CreateButton("查询装配");
            btnQuery.Click += BtnQueryAssemble_Click;

            layout.Controls.Add(CreateLabel("查询码"), 0, 5);
            layout.Controls.Add(txtQueryAssemble, 1, 5);
            layout.Controls.Add(btnQuery, 2, 5);

            panel.Controls.Add(layout);
            return panel;
        }
        #endregion

        #region Lamp
        private Panel CreateLamp() => new Panel
        {
            Width = 14,
            Height = 14,
            BackColor = Color.Gray,
            Margin = new Padding(5)
        };

        private void SetLamp(Panel lamp, bool ok)
        {
            lamp.BackColor = ok ? Color.LimeGreen : Color.Red;
        }


        private void SetLamp(bool ok)
        {
            pnlCheckLamp.BackColor = ok ? Color.LimeGreen : Color.Red;
        }

        private void SetUpLamp(bool ok)
        {
            pnlUpDataLamp.BackColor = ok ? Color.LimeGreen : Color.Red;
        }
        #endregion

        #region Events

        private async void BtnAssemble_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    BydMesCom.离散装配(
                        txtAssembleParent.Text,
                        txtAssemblePart.Text,
                        out bool ok,
                        out string mes,
                        out string xml);

                    if (!ok) throw new Exception(mes);
                });

                MessageBox.Show("装配成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BtnQueryAssemble_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await Task.Run(() =>
                {
                    BydMesCom.查询离散装配(
                        txtQueryAssemble.Text,
                        out AssembleSfcResponse resp,
                        out string mes,
                        out string xml);

                    return resp;
                });

                FillGrid(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillGrid(AssembleSfcResponse resp)
        {
            dgvAssemble.Rows.Clear();

            if (resp?.Components == null) return;

            foreach (var c in resp.Components)
            {
                dgvAssemble.Rows.Add(c.Resource, c.Operation, c.Sfc, c.Item, c.DataField);
            }
        }

        #endregion

        #region Grid
        private DataGridView CreateAssembleGrid()
        {
            var dgv = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.Columns.Add("Res", "资源");
            dgv.Columns.Add("Op", "工序");
            dgv.Columns.Add("Sfc", "条码");
            dgv.Columns.Add("Item", "物料");
            dgv.Columns.Add("Type", "装配类型");

            return dgv;
        }
        #endregion

        #region Helpers
        private FlowLayoutPanel CreateFlow(string label, Control input, Control btn)
        {
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill };
            flow.Controls.Add(CreateLabel(label));
            flow.Controls.Add(input);
            flow.Controls.Add(btn);
            return flow;
        }

        private Label CreateLabel(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(5)
        };

        private TextBox CreateTextBox() => new TextBox
        {
            Width = 160,
            Margin = new Padding(5)
        };

        private Button CreateButton(string text) => new Button
        {
            Text = text,
            Width = 80
        };
        #endregion

        #region existing logic placeholders

        private async void BtnCheckBar_Click(object sender, EventArgs e)
        {
            var bar = txtCheckBar.Text?.Trim();

            if (string.IsNullOrWhiteSpace(bar))
            {
                SetLamp(false);
                MessageBox.Show("请输入产品码");
                return;
            }

            try
            {
                BydMesCom.条码验证(bar, out bool ok, out string mes, out string xml);

                SetLamp(ok);

                if (!ok)
                    MessageBox.Show("产品码不合法");
            }
            catch
            {
                SetLamp(false);
                MessageBox.Show("校验失败");
            }
        }

        private async void BtnBind_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBindOrder.Text))
            {
                MessageBox.Show("请输入工单号");
                return;
            }

            await Task.Run(() =>
            {
                BydMesCom.工单绑定(txtBindOrder.Text, out string mes, out string xml);
            });

            MessageBox.Show("绑定成功");
        }

        private async void BtnUpData_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUpDataBar.Text))
            {
                SetLamp(false);

                MessageBox.Show("请输入出站码");
                return;
            }
            try
            {
                BydMesCom.条码上传(cbBarResult.Checked, txtUpDataBar.Text, txtTestItems.Text, out bool result, out string mes, out string xml);
                SetUpLamp(result);
            }
            catch (Exception)
            {

                SetUpLamp(false);
                MessageBox.Show("上传失败");
                return;

            }

            MessageBox.Show("上传成功");
        }

        private async Task QueryByOrder(string order) => await Query(order: order);
        private async Task QueryByBar(string bar) => await Query(bar: bar);
        private async Task QueryByResource(string res) => await Query(resource: res);

        private async Task Query(string order = "", string resource = "", string bar = "")
        {
            try
            {
                var info = await Task.Run(() =>
                {

                    BydMesCom.工单信息查询(order, resource, bar,
                        out ShopOrderInfo result,
                        out string msg,
                        out string xml);

                    return result;
                });

                Fill(info);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void Fill(ShopOrderInfo info)
        {
            if (info == null)
            {
                Clear();
                return;
            }

            txtOrderNo.Text = info.Order ?? "";
            txtItem.Text = info.Item ?? "";
            txtItemDesc.Text = info.ItemDesc ?? "";
            txtRouter.Text = info.Router ?? "";
            txtOrderNum.Text = info.OrderNum ?? "";
            txtCompNum.Text = info.CompNum ?? "";
            txtCompRate.Text = info.CompRate ?? "";
        }

        private void Clear()
        {
            txtOrderNo.Clear();
            txtItem.Clear();
            txtItemDesc.Clear();
            txtRouter.Clear();
            txtOrderNum.Clear();
            txtCompNum.Clear();
            txtCompRate.Clear();
        }

        #endregion

        #region private methode
        private string FormatDeviceResponse(DeviceIntegrationResponse resp, bool ok, string mes, string xmlOut)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"验证结果: {(ok ? "PASS" : "FAIL")}");
            sb.AppendLine($"SFC: {resp.SFC}");
            sb.AppendLine($"操作: {resp.Operation}");
            sb.AppendLine($"资源: {resp.Resource}");
            sb.AppendLine($"ISOK: {resp.IsOk}");
            sb.AppendLine($"时间: {resp.Time}");
            sb.AppendLine("测试项:");
            foreach (var t in resp.TestItems)
                sb.AppendLine($"  {t.TestItem} | {t.TestParameter} | {t.TestValue}");
            sb.AppendLine($"MES反馈原文:\n{mes}");
            sb.AppendLine($"XML输出:\n{xmlOut}");
            return sb.ToString();
        }
        #endregion
    }
}