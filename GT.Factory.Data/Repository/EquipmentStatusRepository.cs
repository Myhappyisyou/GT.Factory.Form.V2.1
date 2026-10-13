using GT.Factory.Common.Enums;
using GT.Factory.Common.Helpers;
using GT.Factory.Common.Models;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GT.Factory.Data.Repository
{
    /// <summary>
    /// 设备状态仓储
    /// 负责 GTProcessAlarm、GTEquipmentRun 等表
    /// </summary>
    public class EquipmentStatusRepository
    {
        private readonly IDatabase _db;
        private readonly string alarmTable = TableNameResolver.Get(LogicalTable.Alarm);
        private readonly string equipmentRunTable = TableNameResolver.Get(LogicalTable.EquipmentRun);

        public EquipmentStatusRepository(DatabaseManager manager)
        {
            _db = manager.Primary
                ?? throw new Exception("Primary 数据库未配置");
        }



        #region 服务器时间

        /// <summary>
        /// 获取 SQL Server 当前时间。
        ///
        /// 正式产品码生成开始时调用一次。
        ///
        /// 该时间会作为本次产品码生成的统一时间，
        /// 后续 Date、Shift、Serial 和 History
        /// 均使用这个时间。
        /// </summary>
        public DateTime GetDatabaseTime()
        {
            const string sql = @"
                SELECT GETDATE();";

            return _db.QuerySingle<DateTime>(
                sql,
                null);
        }

        #endregion

        #region 班次

        public string GetCurrentShift(DateTime generationTime)
        {

            const string sql = @"
                SELECT TOP 1 ShiftCode
                FROM dbo.GTShiftConfig
                WHERE Enabled = 1
                AND
                (
                    (
                        StartTime < EndTime
                        AND CAST(@GenerationTime AS time) >= StartTime
                        AND CAST(@GenerationTime AS time) < EndTime
                    )
                    OR
                    (
                        StartTime > EndTime
                        AND
                        (
                            CAST(@GenerationTime AS time) >= StartTime
                            OR CAST(@GenerationTime AS time) < EndTime
                        )
                    )
                )
                ORDER BY StartTime;";

            return _db.QuerySingle<string>(
                sql,
                new
                {
                    GenerationTime = generationTime
                });

        }

        #endregion



        #region 设备状态

        public async Task InsertEquipmentRunAsync(string processno, DateTime starttime, DateTime endtime, int state)
        {
            string sql = $"EXEC equipment_run_state_record @eqpt_id = '{processno}',@state_start_time = GETDATE(), @state_end_time = GETDATE(), @state = {state},@run_data = N'{{}}' ";


            await _db.ExecuteAsync(sql);
        }


        #endregion


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
