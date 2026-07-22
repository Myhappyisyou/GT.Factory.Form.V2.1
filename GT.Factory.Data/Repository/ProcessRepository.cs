using Dapper;
using GT.Factory.Common.Enums;
using GT.Factory.Common.Helper;
using GT.Factory.Common.Models;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Data.Repository
{
    /// <summary>
    /// 生产数据仓储
    /// 负责生产测试数据上传
    /// 纯数据层
    /// </summary>
    public class ProcessRepository
    {
        private readonly IDatabase _primary;

        private readonly string processTable = TableNameResolver.Get(LogicalTable.ProcessProperty);

        private readonly string propertyParseTable = TableNameResolver.Get(LogicalTable.PropertyParse);

        private readonly string calibrationTable = TableNameResolver.Get(LogicalTable.Calibration);

        public ProcessRepository(DatabaseManager manager)
        {
            _primary = manager.Primary;
        }

        #region 生产数据上传

        /// <summary>
        /// 上传基础测试数据
        /// </summary>
        public async Task<bool> UploadBasicDataAsync(ProcessPropertyPayload propertyPayload)
        {
            if (string.IsNullOrWhiteSpace(propertyPayload.BarNo))
                return false;

            await InsertPrimaryAsync(
                _primary,
                propertyPayload);

            return true;
        }

        /// <summary>
        /// 上传基础测试数据 上限 下限 实际值 结果
        /// </summary>
        public async Task<bool> UploadFourAsync(ProcessPropertyFourPayload propertyPayload)
        {
            if (string.IsNullOrWhiteSpace(propertyPayload.BarNo))
                return false;

            await InsertProcessPropertyFourPrimaryAsync(
                _primary,
                propertyPayload);

            return true;
        }

        /// <summary>
        /// 上传点检
        /// </summary>
        public async Task<bool> UploadCalibrationAsync(
           string barNo,
           string processNo,
           string testItem,
           string testItemType,
           string testItemValue,
           string doTime)
        {
            var sql = $@"
                INSERT INTO {calibrationTable}
                (bar_no, process_no, do_time,
                 test_item, test_item_type,
                 test_item_up, test_item_down,
                 test_item_value, flag)
                VALUES
                (@bar_no, @process_no, @do_time,
                 @test_item, @test_item_type,
                 '', '',
                 @test_item_value, '0')";

            await _primary.ExecuteAsync(sql, new
            {
                bar_no = barNo,
                process_no = processNo,
                do_time = doTime,
                test_item = testItem,
                test_item_type = testItemType,
                test_item_value = testItemValue
            });

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task<bool> UploadPeriodTestInspectionAsync(
            string tableName,
            string barNo,
            string processNo,
            string doTime,
            string[] testItem,
            string[] testItemType,
            string[] testItemUp,
            string[] testItemDown,
            string[] testItemValue)
        {
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {calibrationTable} ");
            sb.Append("(bar_no,process_no,do_time,test_item,test_item_type,test_item_up,test_item_down,test_item_value,flag) VALUES ");

            var values = new List<string>();
            var param = new DynamicParameters();

            for (int i = 0; i < testItemValue.Length; i++)
            {
                values.Add($"(@bar{i},@process{i},@time{i},@item{i},@type{i},@up{i},@down{i},@value{i},'0')");

                param.Add($"@bar{i}", barNo);
                param.Add($"@process{i}", processNo);
                param.Add($"@time{i}", doTime);
                param.Add($"@item{i}", testItem[i]);
                param.Add($"@type{i}", testItemType[i]);
                param.Add($"@up{i}", testItemUp[i]);
                param.Add($"@down{i}", testItemDown[i]);
                param.Add($"@value{i}", testItemValue[i]);
            }

            sb.Append(string.Join(",", values));

            await _primary.ExecuteAsync(sb.ToString(), param);
            return true;
        }

        #endregion

        #region 生产数据查询

        /// <summary>
        /// 根据产品码查询最新一条NG信息
        /// </summary>
        public async Task<string> SelectNgCodeAsync(string barNo)
        {
            var sql = $@"
                SELECT TOP 1 ng_msg
                FROM {processTable}
                WHERE bar_no = @barNo
                ORDER BY do_time DESC";

            return await _primary.QuerySingleAsync<string>(sql, new { barNo });
        }

        /// <summary>
        /// 查询指定工序的最新一条NG信息
        /// </summary>
        public async Task<string> SelectProcessNgCodeAsync(string barNo, string processNo)
        {
            var sql = $@"
                SELECT TOP 1 ng_msg
                FROM {processTable}
                WHERE bar_no = @barNo
                  AND process_no = @processNo
                ORDER BY do_time DESC";

            return await _primary.QuerySingleAsync<string>(sql, new { barNo, processNo });
        }

        /// <summary>
        /// 查询产品上传次数及返工状态
        /// </summary>
        //public async Task<ReworkQueryResult> QueryDataUploadCountAsync(
        //    string processNo,
        //    string barNo,
        //    int reworkCountLimit)
        //{
        //    const string sql = @"
        //SELECT 
        //    COUNT(*) AS TotalCount,
        //    MAX(ng_msg) AS LastNgMsg
        //FROM GTProcessProperty
        //WHERE process_no = @processNo
        //  AND bar_no = @barNo";

        //    var result = await _primary.QuerySingleAsync<dynamic>(
        //        sql,
        //        new { processNo, barNo });

        //    var output = new ReworkQueryResult();

        //    if (result == null)
        //    {
        //        output.Status = 2;
        //        return output;
        //    }

        //    int totalCount = result.TotalCount;
        //    string lastNgMsg = result.LastNgMsg;

        //    if (totalCount == 0)
        //    {
        //        output.Status = 1;
        //        return output;
        //    }

        //    // ✅ 解析 NGCode
        //    if (!string.IsNullOrEmpty(lastNgMsg))
        //    {
        //        var firstPart = lastNgMsg.Split('_').FirstOrDefault();
        //        if (int.TryParse(firstPart, out int code))
        //        {
        //            output.NgCode = code;
        //        }
        //    }

        //    if (totalCount > reworkCountLimit)
        //    {
        //        output.ReworkFlag = 0;
        //        output.Status = 2;
        //    }
        //    else
        //    {
        //        output.ReworkFlag = 1;
        //        output.Status = 1;
        //    }

        //    return output;
        //}

        #endregion

        #region 动态字段

        public async Task<string> GetDynamicFieldsAsync(string processNo)
        {
            var sql = $@"
                SELECT ',' + CAST(field_name AS nvarchar(250))
                FROM {propertyParseTable}
                WHERE process_no = @processNo
                ORDER BY field_name FOR XML PATH('')";

            return await _primary.QuerySingleAsync<string>(sql, new { processNo });
        }

        /// <summary>
        /// 字段解析
        /// </summary>
        /// <param name="processNo"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetColumnMapAsync(string processNo)
        {
            var sql = $@"
                SELECT field_name, field_name_cn
                FROM {propertyParseTable}
                WHERE process_no = @processNo";

            var rows = await _primary.QueryAsync<dynamic>(
                sql,
                new { processNo });

            var map = new Dictionary<string, string>();

            foreach (var row in rows)
            {
                string en = row.field_name;
                string cn = row.field_name_cn;
                map[en] = string.IsNullOrWhiteSpace(cn) ? en : cn;
            }

            return map;
        }

        #endregion


        #region 私有方法

        /// <summary>
        /// 实际执行插入逻辑
        /// </summary>
        private async Task InsertPrimaryAsync(IDatabase db, ProcessPropertyPayload propertyPayload)
        {
            var fieldStr = GenerateFieldStr(propertyPayload.Data.Length);
            var paramNames = string.Join(",", propertyPayload.Data.Select((_, i) => $"@p{i}"));

            var sql = $@"
                INSERT INTO {processTable}
                (bar_no, process_no, do_time, vou_no,
                 ok_flag, ng_msg, user_id,
                 flag, eqpt_loc_id, major_state, second_state, aux_state,
                 {fieldStr})
                VALUES
                (@bar_no, @process_no, GETDATE(), @vou_no,
                 @ok_flag, @ng_msg, @user_id,
                 '', '', '0', '0', '0',
                 {paramNames})";

            var param = new DynamicParameters();

            param.Add("@bar_no", propertyPayload.BarNo);
            param.Add("@process_no", propertyPayload.ProcessNo);
            param.Add("@vou_no", propertyPayload.VouNo ?? "0");
            param.Add("@ok_flag", propertyPayload.OkFlag ?? "NG");
            param.Add("@ng_msg", propertyPayload.NgMsg ?? "NG");
            param.Add("@user_id", propertyPayload.UserId ?? "");

            for (int i = 0; i < propertyPayload.Data.Length; i++)
            {
                param.Add($"@p{i}", propertyPayload.Data[i] ?? "");
            }

            await db.ExecuteAsync(sql, param);
        }

        /// <summary>
        /// 实际执行插入4 上限 下限 实际值  结果 逻辑
        /// </summary>
        private async Task InsertProcessPropertyFourPrimaryAsync(IDatabase db, ProcessPropertyFourPayload processData)
        {

            // 取出四类数组（可能为 null）
            var data = processData.Data.Data ?? Array.Empty<string>();
            var dataUp = processData.Data.Data_up ?? Array.Empty<string>();
            var dataDown = processData.Data.Data_down ?? Array.Empty<string>();
            var dataResult = processData.Data.Data_result ?? Array.Empty<string>();

            // 按索引顺序合并，单个索引的顺序为：Data_up, Data_down, Data, Data_result
            int maxLen = Math.Max(Math.Max(data.Length, dataUp.Length), Math.Max(dataDown.Length, dataResult.Length));
            var allValues = new List<string>(capacity: maxLen * 4);
            for (int i = 0; i < maxLen; i++)
            {
                allValues.Add(GetAtOrEmpty(dataUp, i));
                allValues.Add(GetAtOrEmpty(dataDown, i));
                allValues.Add(GetAtOrEmpty(data, i));
                allValues.Add(GetAtOrEmpty(dataResult, i));
            }

            // 生成字段名与参数占位
            string fieldStr = GenerateFieldStr(allValues.Count); // data001,data002,...
            string paramNames = string.Join(",", allValues.Select((_, i) => $"@p{i}"));

            string sql =
                $"INSERT INTO GTProcessProperty (" +
                $"bar_no, process_no, do_time, vou_no, ok_flag, ng_msg, user_id, " +
                $"flag, eqpt_loc_id, major_state, second_state, aux_state, {fieldStr}) " +
                $"VALUES (@bar_no, @process_no, GETDATE(), @vou_no, @ok_flag, @ng_msg, @user_id, " +
                $"'', '', '0', '0', '0', {paramNames})";

            // 公共参数
            var param = new List<SqlParameter>
                    {
                        new SqlParameter("@bar_no", processData.BarNo),
                        new SqlParameter("@process_no", processData.ProcessNo),
                        new SqlParameter("@vou_no", (object)processData.VouNo ?? "0"),
                        new SqlParameter("@ok_flag", (object)processData.OkFlag ?? "NG"),
                        new SqlParameter("@ng_msg", (object)processData.NgMsg ?? (object)processData.Data.NgMsg ?? "NG"),
                        new SqlParameter("@user_id", (object)processData.UserId ?? "Unknown")
                    };

            // 数据参数（空值用空字符串）
            for (int i = 0; i < allValues.Count; i++)
            {
                param.Add(new SqlParameter($"@p{i}", allValues[i] ?? ""));
            }

            await db.ExecuteAsync(sql, param);
        }

        /// <summary>
        /// 生成 data001,data002...
        /// </summary>
        private string GenerateFieldStr(int count)
        {
            var list = new List<string>();

            for (int i = 1; i <= count; i++)
            {
                list.Add($"data{i:D3}");
            }

            return string.Join(",", list);
        }

        // 辅助：安全读取数组索引
        private static string GetAtOrEmpty(string[] arr, int idx)
        {
            if (arr == null || idx < 0 || idx >= arr.Length) return "";
            return arr[idx] ?? "";
        }

        #endregion
    }
}
