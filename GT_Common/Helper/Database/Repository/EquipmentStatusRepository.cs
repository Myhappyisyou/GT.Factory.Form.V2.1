using GT_Common.Helper.Alarms;
using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Repository
{
    /// <summary>
    /// 设备状态仓储
    /// 负责 GTProcessAlarm、GTEquipmentRun 等表
    /// </summary>
    public class EquipmentStatusRepository
    {
        private readonly IDatabase _db;
        private readonly string alarmTable = TableNameResolver.Get(LogicalTable.Alarm);
        private readonly string alarmParseTable = TableNameResolver.Get(LogicalTable.AlarmParse);

        public EquipmentStatusRepository(DatabaseManager manager)
        {
            _db = manager.Primary
                ?? throw new Exception("Primary 数据库未配置");
        }

        #region ✅ 上传报警

        /// <summary>
        /// 插入报警信息
        /// </summary>
        public async Task InsertAlarmAsync(AlarmInfo info)
        {
            var sql = $@"
                INSERT INTO {alarmTable}
                (process_no, create_time,
                 event_type, event_code,
                 event_name, event_content,
                 close_time, andon_flag, flag)
                VALUES
                (@process_no, @create_time,
                 'SC', @event_code,
                 @event_name, @event_content,
                 @close_time, 0, 0)";

            await _db.ExecuteAsync(sql, new
            {
                process_no = info.ProcessNo,
                create_time = info.CreateTime,
                event_code = info.PlcAddr,
                event_name = info.AlarmGrade,
                event_content = info.Description,
                close_time = info.CloseTime
            });
        }

        #endregion

        #region ✅ 查询报警统计

        /// <summary>
        /// 获取最近时间段报警总时长（分钟）
        /// </summary>
        public async Task<double> GetAlarmDurationMinutesAsync()
        {
            var sql = @"
                SELECT SUM(alarm_duration)
                FROM {alarmTable}
                WHERE alarm_time BETWEEN DATEADD(MINUTE, -10, GETDATE())
                                      AND GETDATE()";

            var result = await _db.QuerySingleAsync<double?>(sql);

            return result ?? 0d;
        }

        #endregion

        #region ✅ 上传接口调用日志

        public async Task InsertInterfaceCallAsync(
            string processNo,
            string partBar,
            string returnResult,
            string feedbackMsg)
        {
            const string sql = @"
                INSERT INTO SHProcessInterfaceCall
                (process_no, part_bar, do_time,
                 return_result, feedback_msg)
                VALUES
                (@process_no, @part_bar, GETDATE(),
                 @return_result, @feedback_msg)";

            await _db.ExecuteAsync(sql, new
            {
                process_no = processNo,
                part_bar = partBar,
                return_result = returnResult,
                feedback_msg = feedbackMsg
            });
        }

        #endregion

        #region ✅ 记录节拍

        public async Task InsertBeatAsync(
            string barNo,
            string processNo,
            double beat)
        {
            const string sql = @"
                INSERT INTO SHProcessNoBeat
                (bar_no, process_no, beat, do_time)
                VALUES
                (@bar_no, @process_no, @beat, GETDATE())";

            await _db.ExecuteAsync(sql, new
            {
                bar_no = barNo,
                process_no = processNo,
                beat
            });
        }

        #endregion
    }
}
