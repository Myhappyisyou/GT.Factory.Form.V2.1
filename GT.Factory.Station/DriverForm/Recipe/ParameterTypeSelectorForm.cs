using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RecipeParameter.RecipeParameter;

namespace RecipeParameter
{
    public partial class ParameterTypeSelectorForm : Form
    {
        private ComboBox comboBox;
        private Button btnOK;

        public string SelectedType => comboBox.SelectedItem?.ToString();

        public ParameterTypeSelectorForm()
        {
            Text = "选择参数类型";
            Width = 250;
            Height = 120;
            this.StartPosition = FormStartPosition.CenterScreen;

            comboBox = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBox.Items.AddRange(new[] { "Limit", "Boolean", "Enum", "Text","Int" });
            comboBox.SelectedIndex = 0;

            btnOK = new Button { Text = "确定", Dock = DockStyle.Bottom };
            btnOK.Click += (s, e) => DialogResult = DialogResult.OK;

            Controls.Add(comboBox);
            Controls.Add(btnOK);
        }

        public ParameterBase CreateParameter()
        {
            switch (SelectedType)
            {
                case "Limit":
                    return new LimitParameter
                    {
                        Name = "新限值",
                        LowerLimit = new LimitPlcInfo { Address = "", ValueType = "int", Value = 123 },
                        UpperLimit = new LimitPlcInfo { Address = "", ValueType = "int", Value = 123 },
                        Unit = ""
                    };

                case "Boolean":
                    return new BooleanParameter
                    {
                        Name = "新布尔",
                        Value = false,
                        Unit = ""

                    };

                case "Enum":
                    return new EnumParameter
                    {
                        Name = "新枚举",
                        Value = "",
                        Options = new List<string> { "选项1", "选项2" },
                        Unit = ""

                    };

                case "Text":
                    return new TextParameter
                    {
                        Name = "新文本",
                        Value = "",
                        Unit = ""

                    };
                case "Int":
                    return new IntParameter
                    {
                        Name = "值",
                        Value = new LimitPlcInfo { Address = "", ValueType = "int", Value = 123 },
                        Unit = ""

                    };

                default:
                    return new TextParameter
                    {
                        Name = "未识别",
                        Value = "",
                        Unit = ""

                    };
            }
        }

    }
}
