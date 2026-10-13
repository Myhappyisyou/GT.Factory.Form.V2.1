using GT.Factory.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GT.Factory.Server.Core.TreeEdit
{
    public  class TreeNodeFileConfiguration
    {
        // 从JSON文件加载树结构
        public static void LoadTreeFromJson(TreeView treeView, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"配置文件 {filePath} 不存在");

            string json = File.ReadAllText(filePath);

            // 修复：反序列化为列表而不是单个对象
            List<TreeNodeConfig> rootConfigs = JsonConvert.DeserializeObject<List<TreeNodeConfig>>(json);

            // 清除现有节点
            treeView.Nodes.Clear();

            // 遍历所有根节点配置
            foreach (var configNode in rootConfigs)
            {
                AddTreeNode(treeView.Nodes, configNode);
            }
        }

        // 保存 TreeView 到 JSON
        public static void SaveTreeToJson(TreeView treeView, string filePath)
        {
            // 创建根配置列表（支持多个根节点）
            var rootConfigs = new List<TreeNodeConfig>();

            // 遍历所有根节点
            foreach (TreeNode rootNode in treeView.Nodes)
            {
                rootConfigs.Add(ConvertTreeNodeToConfig(rootNode));
            }

            // 序列化并保存
            string json = JsonConvert.SerializeObject(rootConfigs, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // 递归转换 TreeNode 为配置对象
        private static TreeNodeConfig ConvertTreeNodeToConfig(TreeNode treeNode)
        {
            var config = new TreeNodeConfig
            {
                Name = treeNode.Text,
                Checked = treeNode.Checked,
                Tag = treeNode.Tag?.ToString(), // 处理 null 值
                Children = new List<TreeNodeConfig>()
            };

            // 递归处理子节点
            foreach (TreeNode childNode in treeNode.Nodes)
            {
                config.Children.Add(ConvertTreeNodeToConfig(childNode));
            }

            return config;
        }

        // 递归添加树节点
        private static void AddTreeNode(TreeNodeCollection nodes, TreeNodeConfig configNode)
        {
            TreeNode treeNode = new TreeNode(configNode.Name)
            {
                Checked = configNode.Checked,
                Tag = configNode.Tag
            };

            nodes.Add(treeNode);

            if (configNode.Children != null)
            {
                foreach (var child in configNode.Children)
                {
                    AddTreeNode(treeNode.Nodes, child);
                }
            }
        }
    }
}
