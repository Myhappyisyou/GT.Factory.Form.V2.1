using Dapper;
using GT.Factory.Common.Enums;
using GT.Factory.Common.Helper;
using GT.Factory.Common.Helpers;
using GT.Factory.Common.Models;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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

        private readonly string fileTable = TableNameResolver.Get(LogicalTable.ProcessFile);

        private readonly string resultCodeParse = TableNameResolver.Get(LogicalTable.ProcessResultCodeParse);

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
        /// 上传抽检
        /// </summary>
        public async Task<bool> UploadPeriodTestInspectionAsync(
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
        /// 查询数据库（DataSet）
        /// </summary>
        public async Task<List<dynamic>> SearchDataAsync(SearchDataPayload payload)
        {
            string fields =
                await GetDynamicFieldsAsync(payload.ProcessNo);

            var sb = new StringBuilder();

            sb.Append(
                $"SELECT bar_no, vou_no, ok_flag, ng_msg, do_time, user_id {fields} ");

            sb.Append(
                $"FROM {processTable} WHERE process_no=@process_no ");

            var parameters = new DynamicParameters();


            parameters.Add(
                "process_no",
                payload.ProcessNo);

            //var ps = new List<SqlParameter>
            //{
            //    new SqlParameter("@process_no", payload.ProcessNo)
            //};

            if (!string.IsNullOrEmpty(payload.FilterColumn) &&
                !string.IsNullOrEmpty(payload.FilterValue))
            {
                sb.Append(
                    $" AND {payload.FilterColumn} LIKE @filterValue");

                //ps.Add(
                //    new SqlParameter(
                //        "@filterValue",
                //        $"%{payload.FilterValue}%"));
                parameters.Add(
            "filterValue",
            $"%{payload.FilterValue}%");
            }

            if (payload.StartTime.HasValue &&
               payload.EndTime.HasValue)
            {
                sb.Append(
                    " AND do_time BETWEEN @start AND @end");

                //ps.Add(
                //    new SqlParameter(
                //        "@start",
                //        payload.StartTime.Value));

                //ps.Add(
                //    new SqlParameter(
                //        "@end",
                //        payload.EndTime.Value));
                parameters.Add(
         "start",
         payload.StartTime.Value);


                parameters.Add(
                    "end",
                    payload.EndTime.Value);
            }

            if (!string.IsNullOrEmpty(payload.OkFlag) && payload.OkFlag != "All")
            {
                sb.Append(
                    " AND ok_flag=@ok_flag");

                //ps.Add(
                //    new SqlParameter(
                //        "@ok_flag",
                //        payload.OkFlag));
                parameters.Add(
       "ok_flag",
       payload.OkFlag);
            }

            sb.Append(
                " ORDER BY do_time DESC");
            
            var rows =
                await _primary.QueryAsync<dynamic>(
                    sb.ToString(),
                    parameters);
            return rows;
        }

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
        public async Task<ReworkQueryResult> QueryDataUploadCountAsync(
            string processNo, string barNo, int reworkCountLimit)
        {
            var sql = $@"
                         SELECT 
                             COUNT(*) AS TotalCount,
                             MAX(ng_msg) AS LastNgMsg
                         FROM {processTable}
                         WHERE process_no = @processNo
                           AND bar_no = @barNo";

            var result = await _primary.QuerySingleAsync<dynamic>(
                sql,
                new { processNo, barNo });

            var output = new ReworkQueryResult();

            if (result == null)
            {
                output.Status = 2;
                return output;
            }

            int totalCount = result.TotalCount;
            string lastNgMsg = result.LastNgMsg;

            if (totalCount == 0)
            {
                output.Status = 1;
                return output;
            }

            // ✅ 解析 NGCode
            if (!string.IsNullOrEmpty(lastNgMsg))
            {
                var firstPart = lastNgMsg.Split('_').FirstOrDefault();
                if (int.TryParse(firstPart, out int code))
                {
                    output.NgCode = code;
                }
            }

            if (totalCount > reworkCountLimit)
            {
                output.ReworkFlag = 0;
                output.Status = 2;
            }
            else
            {
                output.ReworkFlag = 1;
                output.Status = 1;
            }

            return output;
        }

        /// <summary>
        /// 查询指定工站指定字段数值
        /// </summary>
        public async Task<float> SelectValueData(string process_no, string bar_no, string FieldName)
        {
            string sql = $"select {FieldName} from GTProcessProperty where process_no ='{process_no}' AND bar_no = '{bar_no}' order by do_time desc";

            var result = await _primary.QuerySingleAsync<object>(sql);

            return Convert.ToSingle(result);

        }

        #endregion

        #region 点检抽检查询

        /// <summary>
        /// 查询点检配置项
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public async Task<CalibrationOrPeriodInspection> SelectCalibrationItem(string inspection, string station_id)
        {
            CalibrationOrPeriodInspection calibrationOrPeriodInspection = new CalibrationOrPeriodInspection();
            List<string> LsTest_item = new List<string>();
            List<string> LsTest_item_type = new List<string>();
            List<string> LsTest_item_up = new List<string>();
            List<string> LsTest_item_down = new List<string>();
            string sql = $"select test_item,inspection,test_item_type,test_item_up,test_item_down from SHProcessCalibrationInspection where inspection = @inspection and station_id = @station_id";

            var rows =
               await _primary.QueryAsync<dynamic>(
                   sql,
                   new { inspection, station_id });


            calibrationOrPeriodInspection.CalibrationOrPeriodStationId = station_id;
            foreach (DataRow item in rows)
            {
                LsTest_item.Add(item["test_item"].ToString());
                LsTest_item_type.Add(item["test_item_type"].ToString());
                LsTest_item_up.Add(item["test_item_up"].ToString());
                LsTest_item_down.Add(item["test_item_down"].ToString());
            }
            calibrationOrPeriodInspection.Test_item = LsTest_item.ToArray();
            calibrationOrPeriodInspection.Test_item_type = LsTest_item_type.ToArray();
            calibrationOrPeriodInspection.Test_item_up = LsTest_item_up.ToArray();
            calibrationOrPeriodInspection.Test_item_down = LsTest_item_down.ToArray();
            return calibrationOrPeriodInspection;
        }

        /// <summary>
        /// 查询工序所有点检或抽检 station_id
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="process_no"></param>
        /// <returns></returns>
        public async Task<List<dynamic>> SelectCalibration(string process_no, DateTime? start_time, DateTime? end_time)
        {
            var sqlQuery = $@"select bar_no,do_time,test_item,test_item_up,test_item_down,test_item_value,case when flag = 1 then 'OK' else 'NG' end as flag 
                            FROM {calibrationTable} 
                            WHERE process_no = @process_no and do_time between @start_time and @end_time order by do_time desc";

            var parameters = new DynamicParameters();


            parameters.Add("process_no", process_no);
            parameters.Add("start_time", start_time.Value);
            parameters.Add("end_time", end_time.Value);

            //var parameters = new SqlParameter[]
            //{
            //     new SqlParameter("@process_no", process_no),
            //     new SqlParameter("@start_time", start_time.Value),
            //     new SqlParameter("@end_time", end_time.Value),
            //};

            var rows =
          await _primary.QueryAsync<dynamic>(
              sqlQuery,
              parameters);
            return rows;
        }

        /// <summary>
        /// 批量上传三花点检/抽检表数据
        /// </summary>
        public async Task<bool> UpdateSHPeriodOrCalibrationValues(string bar_no, string process_no, string do_time, List<SaveItem> saveItems)
        {
            if (string.IsNullOrWhiteSpace(bar_no))
                return false;

            StringBuilder sb = new StringBuilder();

            sb.Append($"insert into {calibrationTable} (bar_no,process_no,do_time,test_item,test_item_type,test_item_up,test_item_down,test_item_value,flag) values ");

            List<string> list = new List<string>();

            foreach (var item in saveItems)
            {
                string[] values = new string[] { bar_no, process_no, do_time, item.Test_item_name, "Property", item.Test_item_up, item.Test_item_down, item.Test_item_value, item.Ok_flag };
                list.Add($"({string.Join(",", values.Select(t => $"'{t}'"))})");
            }

            sb.Append($"{string.Join(",", list.ToArray())}");
            sb.Append(';');

            await _primary.ExecuteAsync(sb.ToString());

            return true;
        }


        #endregion

        #region 工艺参数表

        /// <summary>
        /// 查询工艺代码
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public async Task<List<ResultCodeParse>> SelectResultCodeParse(string process_no)
        {
            string sql = $"select ng_code,ng_msg from {resultCodeParse} where process_no = @process_no ";
            
            await _primary.ExecuteAsync(sql, new { process_no });

            var rows = await _primary.QueryAsync<ResultCodeParse>(sql, new { process_no });

            return rows;
           
        }


        #endregion

        #region 文件

        /// <summary>
        /// 上传文件路径，包含图片，曲线
        /// </summary>
        public async Task<bool> UploadProcessFile(ProcessFilePayload payload)
        {
            if (string.IsNullOrWhiteSpace(payload.BarNo))
                return false;

                string sql = $@"insert into {fileTable} (bar_no,process_no,file_type,name,do_time,path,flag) values (@bar_no,@process_no,@fileType,@fileName,GETDATE(),@filePath,'0')";

                await _primary.ExecuteAsync(sql, new
                {
                    bar_no = payload.BarNo,
                    process_no = payload.ProcessNo,
                    fileType = payload.FileType,
                    fileName = payload.FileName,
                    filePath = payload.FilePath
                });

                return true;
        }


        #endregion

        #region 字段解析表

        //  更新字段
        public async Task SaveFieldAsync(string processNo, string fieldName, string fieldNameCn)
        {
            string sql = $@"
                            UPDATE {propertyParseTable}
                            SET field_name_cn=@field_name_cn,
                                data_type='varchar'
                            WHERE process_no=@process_no
                            AND field_name=@field_name;
                            
                            IF @@ROWCOUNT = 0
                            BEGIN
                                INSERT INTO GTProcessPropertyParse
                                (
                                    process_no,
                                    field_name,
                                    field_name_cn,
                                    data_type
                                )
                                VALUES
                                (
                                    @process_no,
                                    @field_name,
                                    @field_name_cn,
                                    'varchar'
                                )
                            END";

            var param = new DynamicParameters();
            param.Add("@process_no", processNo);
            param.Add("@field_name", fieldName);
            param.Add("@field_name_cn", fieldNameCn);

            await _primary.ExecuteAsync(sql, param);
        }


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
            // 固定列
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "bar_no",  "产品码" },
                { "vou_no",  "工单号" },
                { "ok_flag", "结果" },
                { "ng_msg",  "结果信息" },
                { "do_time", "测试时间" },
                { "user_id", "操作员" }
            };

            var sql = $@"
                SELECT field_name, field_name_cn
                FROM {propertyParseTable}
                WHERE process_no = @processNo";

            var rows = await _primary.QueryAsync<dynamic>(
                sql,
                new { processNo });


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
                $"INSERT INTO {processTable} (" +
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
