using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Oee
{
    public class OeeCalculator
    {
        /// <summary>
        /// 计算时间利用率 Availability = 利用时间 / 负荷时间
        /// </summary>
        public static double CalcAvailability(double operatingTime, double plannedTime)
        {
            if (plannedTime <= 0) return 0;
            return operatingTime / plannedTime;
        }

        /// <summary>
        /// 计算性能利用率 Performance = 生产产品数 / (利用时间 × 设计生产速度)
        /// </summary>
        public static double CalcPerformance(double outputCount, double operatingTime, double designSpeed)
        {
            if (operatingTime <= 0 || designSpeed <= 0) return 0;
            return outputCount / (operatingTime * designSpeed);
        }

        /// <summary>
        /// 计算合格率 Quality = 合格品数 / 生产产品数
        /// </summary>
        public static double CalcQuality(double goodCount, double outputCount)
        {
            if (outputCount <= 0) return 0;
            return goodCount / outputCount;
        }

        /// <summary>
        /// 计算 OEE = 时间利用率 × 性能利用率 × 合格率
        /// </summary>
        public static double CalcOee(
            double plannedTime,      // 负荷时间
            double operatingTime,    // 利用时间
            double outputCount,      // 生产产品数
            double goodCount,        // 合格品数
            double designSpeed       // 设计生产速度
        )
        {
            var availability = CalcAvailability(operatingTime, plannedTime);
            var performance = CalcPerformance(outputCount, operatingTime, designSpeed);
            var quality = CalcQuality(goodCount, outputCount);

            return availability * performance * quality;
        }
    }

}
