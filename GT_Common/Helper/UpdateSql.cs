using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GT_Common.Helper
{
    [Serializable]
    public class SqlDataItem
    {
        public string Sn;
        public string Process;
        public string SqlData;
    }


    [Serializable]
    public class ItemBuffer
    {
        public List<SqlDataItem> Items;

        public ItemBuffer()
        {
            Items = new List<SqlDataItem>();
        }

        public void Add(SqlDataItem dataItem)
        {
            // 判断是否是相同步相同SN，如果是则更新数据

            Items.Add(dataItem);

        }


        public SqlDataItem Search(string sn,string process_no)
        {
            SqlDataItem di = new SqlDataItem();

            // 获取匹配的项
            var matchingItems = Items.Where(myValue => (myValue.Sn == sn && myValue.Process == process_no)).ToList();

            // 如果没有找到，返回 null
            if (matchingItems.Count == 0)
            {
                return null;
            }

            // 设置返回的 DataItem 属性
            di.Sn = sn;
            di.SqlData = matchingItems.Last().SqlData;

            if (matchingItems.Count > 2)
            {
                // 移除所有匹配的项
                Items.RemoveAll(item => (item.Sn == sn && item.Process == process_no));
                return null;
            }
           
            return di;
        }

        public SqlDataItem SearchRemove(string sn,string process_no)
        {
            SqlDataItem di = new SqlDataItem();

            // 获取匹配的项
            var matchingItems = Items.Where(myValue => (myValue.Sn == sn && myValue.Process == process_no)).ToList();

            // 如果没有找到，返回 null
            if (matchingItems.Count == 0)
            {
                return null;
            }

            // 设置返回的 DataItem 属性
            di.Sn = sn;
            di.SqlData = matchingItems.Last().SqlData;

            // 移除所有匹配的项
            Items.RemoveAll(item => item.Sn == sn);

            return di;
        }
    }


    public class UpdateSql
    {
        private string StationName = "updateSql";

        public event EventHandler<SqlDataItem> SqlValueReady;

        private ItemBuffer Buffer = new ItemBuffer();

        private object obj = new object();

        /// <summary>
        /// 程序打开加载
        /// </summary>
        public void Load()
        {
            Buffer = SerializeHelper.LoadBinary<ItemBuffer>(PathCenter.HistoryFile(Path.Combine("updateSql", "updateSql.errsql")));
            //Buffer = SerializeHelper.LoadBinary<ItemBuffer>($"{StationName}.errsql");

        }

        /// <summary>
        /// 不断上传数据
        /// </summary>
        /// <param name="item"></param>
        public void Update(SqlDataItem item)
        {
            lock (obj)
            {
                if (item == null)
                {
                    return;
                }

                Buffer.Add(item);

                // 寻找所有数据并拼接
                SqlDataItem newItem = Buffer.Search(item.Sn, item.Process);

                if (newItem != null)
                {
                    OnSqlValueReady(newItem);
                }

                // 序列化保存到本地
                //SerializeHelper.SaveBinary<ItemBuffer>(Buffer, $"{StationName}.errsql");
                SerializeHelper.SaveBinary<ItemBuffer>(Buffer, PathCenter.HistoryFile(Path.Combine("updateSql", "updateSql.errsql")));

            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="item"></param>
        public void Deletedate(SqlDataItem item)
        {
            lock (obj)
            {
                if (item == null)
                {
                    return;
                }

                // 寻找所有数据并拼接
               Buffer.SearchRemove(item.Sn,item.Process);

                // 序列化保存到本地 (PathCenter.HistoryFile(Path.Combine("updateSql", "updateSql.errsql")))
                SerializeHelper.SaveBinary<ItemBuffer>(Buffer, PathCenter.HistoryFile(Path.Combine("updateSql", "updateSql.errsql")));
            }
        }

        /// <summary>
        /// 引发需要上传事件
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnSqlValueReady(SqlDataItem item)
        {
            SqlValueReady?.Invoke(this, item);
        }
    }
}
