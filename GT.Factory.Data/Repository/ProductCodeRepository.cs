using Dapper;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
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
    /// 产品码生成仓储。
    ///
    /// 负责：
    ///
    /// 1. 调用产品码生成存储过程
    /// 2. 获取生成结果
    ///
    /// 不负责：
    ///
    /// 1. 规则编辑
    /// 2. 产品码规则解析
    /// 3. UI
    ///
    /// </summary>
    public class ProductCodeRepository
    {

        private readonly IDatabase _db;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public ProductCodeRepository(
            DatabaseManager manager)
        {

            _db =
                manager.Primary
                ?? throw new Exception(
                    "Primary数据库未配置");

        }

        #region sp_GetProductCode

        /// <summary>
        /// 根据规则生成产品码。
        ///
        /// 调用：
        ///
        /// dbo.sp_GetProductCode
        ///
        /// </summary>
        public ProductCodeResult Generate(
            int ruleId)
        {

            DynamicParameters param =
                new DynamicParameters();

            param.Add(
                "@RuleId",
                ruleId);

            param.Add(
                "@ResultCode",
                dbType: DbType.String,
                size: 500,
                direction:
                ParameterDirection.Output);

            param.Add(
                    "@ErrorMessage",
                    dbType: DbType.String,
                    size: 500,
                    direction:
                    ParameterDirection.Output);


            _db.Execute(
                    "sp_GetProductCode",
                    param,
                    commandType:
                    CommandType.StoredProcedure);

            string code =
                param.Get<string>(
                    "@ResultCode");

            string error =
                param.Get<string>(
                    "@ErrorMessage");

            return new ProductCodeResult
            {
                Success =
                    string.IsNullOrEmpty(error),

                Code =
                    code,

                ErrorMessage =
                    error
            };

        }

        #endregion


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



        #region 流水号

        /// <summary>
        /// 获取指定产品码规则的下一个流水号。
        ///
        /// 该方法属于高并发敏感操作。
        ///
        /// 执行过程必须保证：
        ///
        /// 1. 读取当前流水号
        /// 2. 判断是否需要重置
        /// 3. 计算下一个流水号
        /// 4. 更新数据库
        /// 5. 提交事务
        ///
        /// 上述操作必须处于同一个 Serializable 事务中。
        ///
        /// 同时通过 UPDLOCK + HOLDLOCK 锁定当前流水号记录，
        /// 防止多个工位同时生成产品码时产生重复流水号。
        /// </summary>
        public long GetNextValue(
            int ruleId,
            string resetMode,
            DateTime generationTime)
        {
            
            IDatabaseTransaction transaction = null;

            try
            {
                transaction =
                    _db.BeginTransactionAsync(
                        IsolationLevel.Serializable)
                    .GetAwaiter()
                    .GetResult();


                const string selectSql = @"
                        SELECT
                            CurrentValue,
                            ResetMode,
                            LastResetDate
                        FROM dbo.GTCodeSerialNumber WITH (UPDLOCK, HOLDLOCK)
                        WHERE RuleId = @RuleId;";

                SerialNumberRecord record =
                    transaction.QuerySingle<SerialNumberRecord>(
                        selectSql,
                        new
                        {
                            RuleId = ruleId
                        });

                /*
                 * 第一次生成该规则的产品码。
                 *
                 * 新规则刚创建时，GTCodeSerialNumber
                 * 可能还没有对应记录。
                 *
                 * 此时从 1 开始创建流水号。
                 */
                if (record == null)
                {
                    const string insertSql = @"
                            INSERT INTO dbo.GTCodeSerialNumber
                            (
                                RuleId,
                                CurrentValue,
                                ResetMode,
                                LastResetDate
                            )
                            VALUES
                            (
                                @RuleId,
                                @CurrentValue,
                                @ResetMode,
                                @LastResetDate
                            );";

                    transaction.Execute(
                        insertSql,
                        new
                        {
                            RuleId = ruleId,
                            CurrentValue = 1L,
                            ResetMode = resetMode,
                            LastResetDate= generationTime
                        });

                    transaction.CommitAsync()
                        .GetAwaiter()
                        .GetResult();

                    return 1L;
                }

                /*
                 * 已经存在流水号记录。
                 */
                bool needReset =
                    NeedReset(
                        record.ResetMode,
                        record.LastResetDate,
                        generationTime);

                long nextValue;

                if (needReset)
                {
                    nextValue = 1L;
                }
                else
                {
                    nextValue = record.CurrentValue + 1L;
                }

                const string updateSql = @"
                       UPDATE dbo.GTCodeSerialNumber
                       SET
                           CurrentValue = @CurrentValue,
                           UpdateTime = @UpdateTime,
                           LastResetDate =
                               CASE
                                   WHEN @NeedReset = 1
                                       THEN @DatabaseNow
                                   ELSE LastResetDate
                               END
                       WHERE RuleId = @RuleId;";

                int affected =
                    transaction.Execute(
                        updateSql,
                        new
                        {
                            RuleId = ruleId,
                            CurrentValue = nextValue,
                            UpdateTime = generationTime,
                            NeedReset = needReset ? 1 : 0,
                            DatabaseNow = generationTime
                        });

                if (affected != 1)
                {
                    throw new InvalidOperationException(
                        "更新产品码流水号失败，RuleId=" + ruleId);
                }

                transaction.CommitAsync()
                    .GetAwaiter()
                    .GetResult();

                return nextValue;
            }
            catch
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.RollbackAsync()
                            .GetAwaiter()
                            .GetResult();
                    }
                    catch
                    {
                        // 保留原始异常。
                    }
                }

                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.DisposeAsync()
                        .GetAwaiter()
                        .GetResult();
                }
            }
        }


        private bool NeedReset(string resetMode, DateTime lastResetDate, DateTime now)
        {
            if (string.Equals(resetMode, "Daily", StringComparison.OrdinalIgnoreCase))
            {
                return lastResetDate.Date != now.Date;
            }

            if (string.Equals(resetMode, "Monthly", StringComparison.OrdinalIgnoreCase))
            {
                return lastResetDate.Year != now.Year || lastResetDate.Month != now.Month;
            }

            if (string.Equals(resetMode, "Yearly", StringComparison.OrdinalIgnoreCase))
            {
                return lastResetDate.Year != now.Year;
            }

            return false;
        }


        #endregion


        #region 产品码历史


        /// <summary>
        /// 保存产品码生成历史。
        ///
        /// 数据表：
        ///
        /// dbo.GTProductCodeHistory
        /// </summary>
        /// <param name="productCode">生成的产品码。</param>
        /// <param name="ruleId">规则ID。</param>
        /// <param name="generateTime">生成时间。</param>
        public void AddHistory(
            string productCode,
             int ruleId,
            DateTime generationTime)
        {
            string sql = @"
                    INSERT INTO dbo.GTProductCodeHistory
                    (
                        ProductCode,
                        RuleId,
                        GenerateTime
                    )
                    VALUES
                    (
                        @ProductCode,
                        @RuleId,
                        @GenerationTime
                    );";


            _db.Execute(
                sql,
                new
                {
                    ProductCode = productCode,
                    RuleId = ruleId,
                    GenerationTime = generationTime
                });
        }


        #endregion
    }

}

