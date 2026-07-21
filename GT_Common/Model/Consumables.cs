using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class Consumables
    {
        [DisplayName("ID")]
        public int ID { get; set; }
        [DisplayName("易损件所在工位")]
        public string ProcessName { get; set; }
        [DisplayName("机台名称")]
        public string StationName { get; set; }
        [DisplayName("易损件所在位置")]
        public string Location { get; set; }
        [DisplayName("易损件名称")]
        public string Name { get; set; }
        [DisplayName("易损件理论使用次数")]
        public int TheoreticalCount { get; set; }
        [DisplayName("易损件已使用次数")]
        public int UsedCount { get; set; }
        [DisplayName("易损件剩余使用次数")]
        public int RemainderCount { get; set; }

        // 获取剩余百分比
        public double GetRemainderPercentage()
        {
            if (TheoreticalCount == 0) return 0;
            return (double)RemainderCount / TheoreticalCount * 100;
        }

        // 获取警告级别
        public string GetWarningLevel()
        {
            double percentage = GetRemainderPercentage();

            if (percentage <= 5)
                return "严重警告";
            else if (percentage <= 10)
                return "警告";
            else if (percentage <= 20)
                return "注意";
            else
                return "正常";
        }

        // 检查是否需要警告
        public bool NeedsWarning()
        {
            return GetRemainderPercentage() <= 20;
        }

        // 获取警告颜色
        public Color GetWarningColor()
        {
            double percentage = GetRemainderPercentage();

            if (percentage <= 5)
                return Color.Red;
            else if (percentage <= 10)
                return Color.Orange;
            else if (percentage <= 20)
                return Color.Yellow;
            else
                return Color.White;
        }
    }
}
