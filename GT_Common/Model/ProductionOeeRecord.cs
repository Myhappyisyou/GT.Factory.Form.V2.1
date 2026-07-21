using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class ProductionOeeRecord
    {
        // --- 基本属性 ---
        public int Id { get; set; }
        public string WorkOrderNo { get; set; }
        public string ProductName { get; set; }

        private int _orderQuantity;
        private int _completedQuantity;
        private int _goodQuantity;

        /// <summary>
        /// 工单数量
        /// </summary>
        public int OrderQuantity
        {
            get => _orderQuantity;
            set
            {
                _orderQuantity = value;
                RecalculateRates();
            }
        }

        /// <summary>
        /// 完成数量
        /// </summary>
        public int CompletedQuantity
        {
            get => _completedQuantity;
            set
            {
                _completedQuantity = value;
                RecalculateRates();
            }
        }

        /// <summary>
        /// 合格数量
        /// </summary>
        public int GoodQuantity
        {
            get => _goodQuantity;
            set
            {
                _goodQuantity = value;
                RecalculateRates();
            }
        }

        // --- 自动计算属性 ---
        /// <summary>
        /// 完成率（0~1）
        /// </summary>
        public double CompletionRate { get; private set; }

        /// <summary>
        /// 合格率（0~1）
        /// </summary>
        public double QualityRate { get; private set; }

        /// <summary>
        /// 整线节拍（秒/件）
        /// </summary>
        public double LineCycle { get; set; }

        /// <summary>
        /// 线平衡（0~1）
        /// </summary>
        public double LineBalance { get; set; }

        /// <summary>
        /// OEE（0~1）
        /// </summary>
        public double Oee { get; private set; }

        /// <summary>
        /// 直通率（0~1）
        /// </summary>
        public double FirstPassRate { get; private set; }

        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string UpdateFlag { get; set; }

        // --- 构造函数 ---
        public ProductionOeeRecord(int orderQty, int completedQty, int goodQty)
        {
            _orderQuantity = orderQty;
            _completedQuantity = completedQty;
            _goodQuantity = goodQty;
            RecalculateRates();
        }

        public ProductionOeeRecord() { }

        // --- 重新计算比率 ---
        private void RecalculateRates()
        {
            CompletionRate = OrderQuantity > 0 ? (double)CompletedQuantity / OrderQuantity : 0;
            QualityRate = CompletedQuantity > 0 ? (double)GoodQuantity / CompletedQuantity : 0;
            FirstPassRate = QualityRate; // 可按业务规则调整
            Oee = CompletionRate * QualityRate * LineBalance; // 可按业务规则调整
        }

        /// <summary>
        /// 更新完成数量或合格数量后手动刷新比率
        /// </summary>
        public void RefreshRates()
        {
            RecalculateRates();
        }
    }
}
