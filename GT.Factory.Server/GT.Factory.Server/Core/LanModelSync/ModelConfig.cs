using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GT.Factory.Server.Core.LanModelSync
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProductModel
    {
        public ProductModel()
        {
            BaseInfo = new BaseInfo();
            DetailInfo = new DetailInfo();
        }

        [TreeNodeItem("基础信息", Order = 1)]
        [JsonProperty]
        [TypeConverter(typeof(ExpandableObjectConverter))] // 必须加这个！
        public BaseInfo BaseInfo { get; set; }

        [TreeNodeItem("详细信息", Order = 2)]
        [JsonProperty]
        [TypeConverter(typeof(ExpandableObjectConverter))] // 必须加这个！
        public DetailInfo DetailInfo { get; set; }

        [TreeNodeItem("配方路径", Order = 3)]
        [JsonProperty]
        [Browsable(false)]  // 可在 PropertyGrid 中显示

        public string FilePath { get; set; }

        public override string ToString() => BaseInfo?.ProductName ?? "新产品";
    }

    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BaseInfo
    {
        [DisplayName("产品代码")]
        [Description("产品代码")]
        [JsonProperty]
        [Category(Categories.Base)]
        public string ProductCode { get; set; }

        [DisplayName("产品名称")]
        [Description("产品名称")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行
        [Category(Categories.Base)]
        public string ProductName { get; set; }

        [DisplayName("产品编码")]
        [Description("产品编码")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行
        [Category(Categories.Base)]
        public string ProductNumber { get; set; }

        [DisplayName("配方号")]
        [Description("配方号")]
        [JsonProperty]
        [Category(Categories.Base)]
        public RecipeCode RecipeCode { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ProductName))
                return $"基础信息: {ProductName}";
            return "基础信息";
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DetailInfo
    {
        [DisplayName("点火药类型")]
        [Description("OP30点药类型")]
        [JsonProperty]
        [Browsable(true)]  // 添加这行
        [Category(Categories.Detail)]
        public GunpowderType Info1 { get; set; }

        [DisplayName("管径")]
        [Description("管径大小")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行
        [Category(Categories.Detail)]
        public PipeType Info2 { get; set; }

        [DisplayName("管壳长度")]
        [Description("管壳长度")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行
        [Category(Categories.Detail)]
        public ProductLength Info3 { get; set; }

        // 重写 ToString 方法，在 PropertyGrid 中显示有意义的信息
        public override string ToString()
        {
            return "详细信息";
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ParameterGroup
    {
        [DisplayName("参数组名称")]
        [JsonProperty] // 添加JsonProperty
        public string GroupName { get; set; }

        [DisplayName("参数列表")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonProperty] // 添加JsonProperty
        public ParameterItemCollection Parameters { get; set; } = new ParameterItemCollection();

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(GroupName)
                ? "参数组"
                : GroupName;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ParameterItem
    {
        [DisplayName("参数名")]
        [Description("参数的名称")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行

        public string Name { get; set; }

        [DisplayName("参数值")]
        [Description("参数的具体值")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行

        public string Value { get; set; }

        [DisplayName("类型")]
        [Description("参数的数据类型")]
        [JsonProperty] // 添加JsonProperty
        [Browsable(true)]  // 添加这行

        public string Type { get; set; }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name)
                ? "参数项"
                : Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ParameterItemCollection : List<ParameterItem>
    {
        public override string ToString() => $"参数项 ({Count} 个)";
    }

    public static class Categories
    {
        public const string Base = "基础信息";
        public const string Detail = "详细信息";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TreeNodeItemAttribute : Attribute
    {
        /// <summary>在 TreeView 中显示的文本</summary>
        public string DisplayName { get; }

        /// <summary>排序优先级</summary>
        public int Order { get; set; } = 0;

        /// <summary>是否默认展开</summary>
        public bool Expand { get; set; } = true;

        /// <summary>是否用于根节点显示</summary>
        public bool IsRootDisplay { get; set; } = false;

        /// <summary>构造函数，指定显示名称</summary>
        public TreeNodeItemAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }


    public static class TreeNodeBuilder
    {
        public static TreeNode Build(object model)
        {
            if (model == null) return null;

            string rootText = GetNodeText(model, isRoot: true);
            var rootNode = new TreeNode(rootText) { Tag = model };

            BuildChildren(rootNode, model);

            return rootNode;
        }

        private static string GetNodeText(object model, bool isRoot)
        {
            if (model == null) return string.Empty;

            var type = model.GetType();

            if (isRoot)
            {
                var rootProp = type.GetProperties()
                    .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<TreeNodeItemAttribute>() })
                    .FirstOrDefault(x => x.Attr != null && x.Attr.IsRootDisplay);

                if (rootProp != null)
                {
                    var val = rootProp.Prop.GetValue(model);
                    return val?.ToString() ?? rootProp.Attr.DisplayName;
                }
            }

            return model.ToString();
        }

        private static void BuildChildren(TreeNode parent, object model)
        {
            var props = model.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<TreeNodeItemAttribute>() })
                .Where(x => x.Attr != null)
                .OrderBy(x => x.Attr.Order);

            foreach (var item in props)
            {
                var value = item.Prop.GetValue(model);
                if (value == null) continue;

                TreeNode node;

                if (value is IEnumerable enumerable && !(value is string))
                {
                    node = new TreeNode(item.Attr.DisplayName) { Tag = value };
                    parent.Nodes.Add(node);
                    foreach (var child in enumerable)
                    {
                        var childNode = Build(child);
                        if (childNode != null) node.Nodes.Add(childNode);
                    }
                }
                else
                {
                    node = new TreeNode(item.Attr.DisplayName) { Tag = value };
                    parent.Nodes.Add(node);
                    BuildChildren(node, value);
                }

                if (item.Attr.Expand) node.Expand();
            }
        }

        public static void RefreshTreeNodeText(TreeNode node)
        {
            if (node == null || node.Tag == null) return;

            node.Text = GetNodeText(node.Tag, node.Parent == null);

            foreach (TreeNode child in node.Nodes)
            {
                RefreshTreeNodeText(child);
            }
        }
    }

    public enum RecipeCode
    {
        R1 = 1,
        R2 = 2,
        R3 = 3,
        R4 = 4,
        R5 = 5,
        R6 = 6,
        R7 = 7,
        R8 = 8,
        R9 = 9,
        R10 = 10
    }

    // 火药类型
    public enum GunpowderType
    {
        BK药 = 1,
        BQ药,
    }

    // 管径类型
    public enum PipeType
    {
        大 = 1,
        小,
    }

    public enum ProductLength
    {
        L1 = 1,
        L2 = 2,
        L3 = 3,
        L4 = 4,
        L5 = 5,
        L6 = 6,
        L7 = 7,
        L8 = 8,
        L9 = 9,
        L10 = 10
    }
}
