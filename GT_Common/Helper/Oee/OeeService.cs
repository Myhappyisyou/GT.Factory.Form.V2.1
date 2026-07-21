using GT_Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GT_Common.Helper.Oee
{
    public class OeeService
    {
        private readonly Timer _timer;

        public OeeService()
        {
            _timer = new Timer();
            _timer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds; // 每10分钟执行一次
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
        }

        public void Start() => _timer.Start();

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine("开始计算 OEE ...");

                // 1. 读取生产数量
                var (outputCount, goodCount) = GetProductionData();

                // 2. 读取最近 10 分钟报警时长（分钟）
                double alarmMinutes = GetAlarmDurationMinutes();

                // 3. 负荷时间 = 10 分钟（本次周期）
                double plannedTime = 10;

                // 4. 利用时间 = 负荷时间 − 报警时间
                double operatingTime = plannedTime - alarmMinutes;
                if (operatingTime < 0) operatingTime = 0;

                // 5. 设计速度
                double designSpeed = 3.0; // 件/分钟（根据你的设备参数）

                // 6. 计算 OEE
                double oee = OeeCalculator.CalcOee(
                    plannedTime,
                    operatingTime,
                    outputCount,
                    goodCount,
                    designSpeed
                );

                Console.WriteLine($"OEE = {oee:P2}");

                // 7. 保存数据库
                SaveOeeToDb(oee, operatingTime, alarmMinutes, outputCount, goodCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OEE 计算失败：" + ex.Message);
            }
        }

        // -------------------- ↓ 数据获取 ↓ ---------------------

        private (int outputCount, int goodCount) GetProductionData()
        {
            return UploadSql.GetProductionData();
        }

        /// <summary>
        /// 获取最近10分钟报警总时长（分钟）
        /// </summary>
        private double GetAlarmDurationMinutes()
        {
            double seconds = UploadSql.GetAlarmDurationMinutes();
            return seconds / 60.0; // 转成分钟
        }

        private void SaveOeeToDb(double oee, double operating, double alarmMinutes, int output, int good)
        {
            string connStr = "Server=.;Database=ProductionDB;User Id=sa;Password=123456;";

            //string sql = @"INSERT INTO OEERecord
            //           (record_time, oee_value, operating_time, alarm_minutes, output_count, good_count)
            //           VALUES(GETDATE(), @oee, @op, @alarm, @output, @good)";

            //using var conn = new SqlConnection(connStr);
            //using var cmd = new SqlCommand(sql, conn);

            //cmd.Parameters.AddWithValue("@oee", oee);
            //cmd.Parameters.AddWithValue("@op", operating);
            //cmd.Parameters.AddWithValue("@alarm", alarmMinutes);
            //cmd.Parameters.AddWithValue("@output", output);
            //cmd.Parameters.AddWithValue("@good", good);

            //conn.Open();
            //cmd.ExecuteNonQuery();
        }
    }

}
