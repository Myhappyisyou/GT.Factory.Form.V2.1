using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace GT.Factory.Server.Core
{
    public static class DeviceGridHelper
    {
        /// <summary>
        /// 更新 DataGridView 中的设备行，包括设备名、IP、状态、背景色
        /// </summary>
        /// <param name="grid">目标 DataGridView</param>
        /// <param name="deviceName">工位或设备名称</param>
        /// <param name="ip">设备 IP</param>
        /// <param name="state">当前状态，如 Ready、Busy、Offline</param>
        public static void UpdateDeviceRow(DataGridView grid, string deviceName, string deviceDetail, string ip, string state,  string modelName)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke((MethodInvoker)(() => UpdateDeviceRow(grid, deviceName, deviceDetail,ip, state, modelName)));
                return;
            }

            bool isNewRow = false;

            var row = grid.Rows
                         .Cast<DataGridViewRow>()
                         .FirstOrDefault(r => r.Cells["IP"].Value?.ToString() == ip);

            if (row == null)
            {
                grid.Rows.Add(deviceName, deviceDetail,ip, state, modelName);
                row = grid.Rows.Cast<DataGridViewRow>().Last();
                isNewRow = true;
            }

            // 始终更新设备名
            row.Cells["Process"].Value = deviceName;
            row.Cells["ModelName"].Value = modelName;

            string oldState = row.Cells["State"].Value?.ToString();
            if (isNewRow || oldState != state)
            {
                row.Cells["State"].Value = state;

                // 设置背景色
                switch (state)
                {
                    case "Offline":
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        break;
                    case "Ready":
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                    case "Busy":
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        break;
                    default:
                        row.DefaultCellStyle.BackColor = Color.White;
                        break;
                }
            }

            // 取消默认选中状态，确保颜色显示
            if (isNewRow)
                grid.ClearSelection();
        }
    }

}
