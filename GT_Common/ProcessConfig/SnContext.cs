using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    public class SnContext
    {
        public string Sn { get; set; }

        /// <summary>
        /// 主码
        /// </summary>
        public string MainBar { get; set; }

        /// <summary>
        /// 部件码
        /// </summary>
        public string PartBar { get; set; }

        /// <summary>
        /// 工单
        /// </summary>
        public string WorkOrder { get; set; }

        /// <summary>
        /// MES校验结果
        /// </summary>
        public bool MesValidated { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// 测试数据
        /// </summary>
        public List<IMeasurement> Measurements { get; set; }
            = new List<IMeasurement>();

        /// <summary>
        /// 扩展字段
        /// </summary>
        public Dictionary<string, object> Items { get; }
            = new Dictionary<string, object>();

        public T Get<T>(string key)
        {
            return Items.TryGetValue(key, out var value)
                ? (T)value
                : default;
        }

        public void Set(string key, object value)
        {
            Items[key] = value;
        }

        public DateTime LastUpdateTime { get; set; }
            = DateTime.Now;

    }
}
