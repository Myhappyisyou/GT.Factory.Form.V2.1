using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.ProductCode
{
    public class ProductCodeRule
    {
        [Category("规则类型")]
        [DisplayName("码类型")]
        public CodeType CodeType { get; set; }

        [Category("规则类型")]
        [DisplayName("型号")]
        public string Model { get; set; }

        [Category("规则类型")]
        [DisplayName("产品码标识")]
        public string CodeMark { get; set; }

        [Category("规则类型")]
        [DisplayName("长度")]
        public int Length { get; set; }

        [Category("规则类型")]
        [DisplayName("部件名称")]
        public string PartName { get; set; }

        [Category("规则类型")]
        [DisplayName("是否启用")]
        public bool Enable { get; set; } = true;
    }

    public enum CodeType
    {
        [Description("管壳码")]
        Shell,

        [Description("扩散器码")]
        Diffuser,

        [Description("虚拟码")]
        Virtual,

        [Description("箱体码")]
        Housing
    }
}
