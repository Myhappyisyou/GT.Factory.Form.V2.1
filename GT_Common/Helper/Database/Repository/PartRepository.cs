using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using GT_Common.Helper.Logging;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Repository
{
    /// <summary>
    /// 零件绑定仓储
    /// 负责 GTProcessPart 表的操作
    /// </summary>
    public class PartRepository
    {
        private readonly IDatabase _db;
        private readonly string partTable = TableNameResolver.Get(LogicalTable.Part);

        public PartRepository(DatabaseManager manager)
        {
            _db = manager.Primary
                ?? throw new Exception("Primary 数据库未配置");
        }

        #region ✅ 绑定零件

        /// <summary>
        /// 绑定单个零件（不存在则插入，存在则更新）
        /// </summary>
        public async Task BindPartAsync(
            string mainBar,
            string processNo,
            string partName,
            string partBar)
        {
            var checkSql = $@"
                SELECT COUNT(1)
                FROM {partTable}
                WHERE main_bar = @mainBar
                  AND process_no = @processNo
                  AND part_name = @partName";

            int count = await _db.QuerySingleAsync<int>(
                checkSql,
                new { mainBar, processNo, partName });

            if (count == 0)
            {
                var insertSql = $@"
                    INSERT INTO {partTable}
                    (main_bar, process_no, part_name, part_bar)
                    VALUES
                    (@mainBar, @processNo, @partName, @partBar)";

                await _db.ExecuteAsync(insertSql,
                    new { mainBar, processNo, partName, partBar });
            }
            else
            {
                var updateSql = $@"
                    UPDATE {partTable}
                    SET part_bar = @partBar
                    WHERE main_bar = @mainBar
                      AND process_no = @processNo
                      AND part_name = @partName";

                await _db.ExecuteAsync(updateSql,
                    new { mainBar, processNo, partName, partBar });
            }
        }

        #endregion

        #region ✅ 查询

        /// <summary>
        /// 根据部件码查询最新主码
        /// </summary>
        public async Task<string> QueryMainBarByPartBarAsync(
            string processNo,
            string partBar)
        {
            var sql = $@"
                SELECT TOP 1 main_bar
                FROM {partTable}
                WHERE process_no = @processNo
                  AND part_bar = @partBar
                ORDER BY id DESC";

            return await _db.QuerySingleAsync<string>(
                sql,
                new { processNo, partBar });
        }

        /// <summary>
        /// 根据主码查询最新部件码
        /// </summary>
        public async Task<string> QueryPartBarByMainBarAsync(
            string processNo,
            string mainBar)
        {
            var sql = $@"
                SELECT TOP 1 part_bar
                FROM {partTable}
                WHERE process_no = @processNo
                  AND main_bar = @mainBar
                ORDER BY id DESC";

            return await _db.QuerySingleAsync<string>(
                sql,
                new { processNo, mainBar });
        }

        /// <summary>
        /// 根据部件码查询主码（兼容旧 SelectSn）
        /// </summary>
        public async Task<string> SelectSnAsync(
            string partBar,
            string processNo)
        {
            var sql = $@"
                SELECT TOP 1 main_bar
                FROM {partTable}
                WHERE part_bar = @partBar
                  AND process_no = @processNo
                ORDER BY id DESC";

            return await _db.QuerySingleAsync<string>(
                sql,
                new { partBar, processNo });
        }

        /// <summary>
        /// 根据主码查询指定零件
        /// </summary>
        public async Task<string> SelectQRDataAsync(
            string mainBar,
            string partName,
            string processNo)
        {
            var sql = $@"
                SELECT TOP 1 part_bar
                FROM {partTable}
                WHERE main_bar = @mainBar
                  AND part_name = @partName
                  AND process_no = @processNo
                ORDER BY id DESC";

            return await _db.QuerySingleAsync<string>(
                sql,
                new { mainBar, partName, processNo });
        }

        #endregion
    }
}
