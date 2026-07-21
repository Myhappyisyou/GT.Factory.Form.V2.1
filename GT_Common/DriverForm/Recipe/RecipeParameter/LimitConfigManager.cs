using GT_Common;
using GT_Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecipeParameter.RecipeParameter
{
    public class ProcessGroup
    {
        public string GroupName { get; set; }

        public string ParametersName { get; set; }

        [JsonConverter(typeof(ParameterListConverter))] // ✅ 作用于列表
        public List<ParameterBase> Parameters { get; set; } = new List<ParameterBase>();

        public override string ToString() => GroupName;
    }

    public class LimitConfig
    {
        public List<ProcessGroup> Groups { get; set; } = new List<ProcessGroup>();
    }

    public static class LimitConfigManager
    {
        //private static string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"limits\{Shared.productName}_limits.json");
        private static string FilePath = PathCenter.ConfigFile(Path.Combine("limits", $"{Shared.productName}_limits.json"));

        public static void Save(LimitConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        //public static LimitConfig Load()
        //{
        //    FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"limits\{Shared.productName}_limits.json");
        //    if (!File.Exists(FilePath)) return new LimitConfig();

        //    var json = File.ReadAllText(FilePath);
        //    return JsonConvert.DeserializeObject<LimitConfig>(json);
        //}

        public static LimitConfig Load()
        {
            FilePath = PathCenter.ConfigFile(Path.Combine("limits", $"{Shared.productName}_limits.json"));

            //FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"limits\{Shared.productName}_limits.json");

            // 如果指定文件存在，直接加载
            if (File.Exists(FilePath))
            {
                try
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonConvert.DeserializeObject<LimitConfig>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载配置文件失败: {ex.Message}");
                    // 继续执行，尝试使用模板文件
                }
            }

            // 如果指定文件不存在，查找其他存在的文件作为模板
            LimitConfig limitConfig =  LoadFromTemplate();
            Save(limitConfig);
            return limitConfig;
        }

        private static LimitConfig LoadFromTemplate()
        {
            //string limitsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "limits");

            string limitsDirectory = PathCenter.ConfigFile("limits");

            // 确保limits目录存在
            if (!Directory.Exists(limitsDirectory))
            {
                Directory.CreateDirectory(limitsDirectory);
                return new LimitConfig();
            }

            // 查找目录下的所有json文件
            var jsonFiles = Directory.GetFiles(limitsDirectory, "*_limits.json");
            if (jsonFiles.Length == 0)
            {
                return new LimitConfig();
            }

            // 如果有多个文件，让用户选择
            if (jsonFiles.Length > 1)
            {
                var selectedFile = ShowTemplateSelectionDialog(jsonFiles);
                if (selectedFile != null)
                {
                    return LoadConfigFromFile(selectedFile);
                }
                return new LimitConfig();
            }

            // 如果只有一个文件，直接使用
            return LoadConfigFromFile(jsonFiles[0]);
        }

        private static string ShowTemplateSelectionDialog(string[] jsonFiles)
        {
            using (var form = new Form())
            {
                form.Text = "选择模板文件";
                form.Width = 400;
                form.Height = 300;
                form.StartPosition = FormStartPosition.CenterScreen;

                var label = new Label
                {
                    Text = $"未找到 {Shared.productName}_limits.json，请选择一个模板文件:",
                    Dock = DockStyle.Top,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var listBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    DisplayMember = "FileName"
                };

                // 显示友好的文件名（不带路径）
                listBox.Items.AddRange(jsonFiles.Select(f => new
                {
                    FilePath = f,
                    FileName = Path.GetFileName(f)
                }).ToArray());

                var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
                var btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Left = 100, Top = 8 };
                var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Left = 10, Top = 8 };
                var btnNew = new Button { Text = "新建空配置", DialogResult = DialogResult.Abort, Left = 190, Top = 8 };

                panel.Controls.AddRange(new[] { btnOk, btnCancel, btnNew });

                form.Controls.AddRange(new Control[] { listBox, label, panel });

                listBox.SelectedIndex = 0;

                var result = form.ShowDialog();

                if (result == DialogResult.OK && listBox.SelectedItem != null)
                {
                    dynamic selected = listBox.SelectedItem;
                    return selected.FilePath;
                }
                else if (result == DialogResult.Abort)
                {
                    return null; // 返回null表示创建新配置
                }
                else
                {
                    return null;
                }
            }
        }

        private static LimitConfig LoadConfigFromFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<LimitConfig>(json);

                // 更新当前文件路径为选择的模板文件路径（可选）
                // FilePath = filePath;

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载模板文件失败: {ex.Message}");
                return new LimitConfig();
            }
        }

        // 获取所有可用的配置文件名称（用于状态显示）
        public static string[] GetAvailableConfigs()
        {
            //string limitsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "limits");
            string limitsDirectory = PathCenter.ConfigFile("limits");

            if (!Directory.Exists(limitsDirectory)) return new string[0];

            return Directory.GetFiles(limitsDirectory, "*_limits.json")
                           .Select(Path.GetFileNameWithoutExtension)
                           .ToArray();
        }

    }
}
