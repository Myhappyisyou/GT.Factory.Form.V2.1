using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GT_Common
{
    [Serializable]
    public class DataItem
    {

        public DateTime Timestamp = DateTime.Now;

        /// <summary>
        /// 产品码
        /// </summary>
        public string Sn;
        /// <summary>
        /// 时间
        /// </summary>
        public string Do_time;
        /// <summary>
        /// 数据
        /// </summary>
        public string[] Data;
        /// <summary>
        /// 上限
        /// </summary>
        public string[] Data_up;
        /// <summary>
        /// 下限
        /// </summary>
        public string[] Data_down;
        /// <summary>
        /// 结果
        /// </summary>
        public string[] Data_result;
        /// <summary>
        /// 测试项
        /// </summary>
        public string[] Data_name;
        /// <summary>
        /// NG信息
        /// </summary>
        public string NgMsg = "";
        /// <summary>
        /// 结果
        /// </summary>
        public string Result;
        /// <summary>
        /// 子工序号
        /// </summary>
        public int Step = 1;
        /// <summary>
        /// 最后子工序标志
        /// </summary>
        public bool IsLastStep;

        public DataItem()
        {
            Data = new string[] { };
        }
    }

    [Serializable]
    public class ItemBuffer
    {
        public List<DataItem> Items;

        public ItemBuffer()
        {
            Items = new List<DataItem>();
        }

        public void Add(DataItem dataItem)
        {
            // 判断是否是相同步相同SN，如果是则更新数据
            bool isSame = false;

            foreach (var item in Items)
            {
                if (item.Sn == dataItem.Sn && item.Step == dataItem.Step)
                {
                    item.Data = dataItem.Data;
                    item.Data_up = dataItem.Data_up;
                    item.Data_down = dataItem.Data_down;
                    item.Data_result = dataItem.Data_result;
                    item.Data_name = dataItem.Data_name;
                    item.NgMsg = dataItem.NgMsg;
                    item.Result = dataItem.Result;
                    item.Do_time = dataItem.Do_time;
                    isSame = true;
                    break;
                }
            }
            if (!isSame)
            {
                Items.Add(dataItem);
            }
        }

        //  根据sn查询该产品数据锡
        public DataItem Search(string sn)
        {
            DataItem di = new DataItem();

            var re = from myValue in Items
                     where myValue.Sn == sn
                     orderby myValue.Step 
                     select myValue;

            int reCount = re.Count();
            if (re.Count() == 0)
            {
                return null;
            }

            for (int i = 0; i < re.Count(); i++)
            {
                int AA = re.ElementAt(i).Step;
                if (re.ElementAt(i).Step != i + 1)
                {
                    return null;
                }
            }

            di.Sn = sn;
            di.NgMsg = re.Last().NgMsg;
            di.Result = re.Last().Result;
            di.IsLastStep = re.Last().IsLastStep;
            di.Step = re.Last().Step;
            di.Do_time = re.Last().Do_time;

            List<string> allValue = new List<string>();
            List<string> Value_up = new List<string>();
            List<string> Value_down = new List<string>();
            List<string> value_name = new List<string>();
            List<string> value_result = new List<string>();
            foreach (var item in re)
            {
                allValue.AddRange(item.Data);
                Value_up.AddRange(item.Data_up);
                Value_down.AddRange(item.Data_down);
                value_name.AddRange(item.Data_name);
                value_result.AddRange(item.Data_result);
            }
            di.Data_name = value_name.ToArray();
            di.Data_result = value_result.ToArray();
            di.Data = allValue.ToArray();
            di.Data_up = Value_up.ToArray();
            di.Data_down = Value_down.ToArray();

            if (di.Result != "NG")
            {
                foreach (var item in re)
                {
                    Items.Remove(item);
                }
            }
            
            return di;
        }

        //  根据sn查询该产品数据锡
        public DataItem SearchData(string sn)
        {
            DataItem di = new DataItem();

            var re = from myValue in Items
                     where myValue.Sn == sn
                     orderby myValue.Step
                     select myValue;

            int reCount = re.Count();
            if (re.Count() == 0)
            {
                return null;
            }

            for (int i = 0; i < re.Count(); i++)
            {
                int AA = re.ElementAt(i).Step;
                if (re.ElementAt(i).Step != i + 1)
                {
                    return null;
                }
            }

            di.Sn = sn;
            di.NgMsg = re.Last().NgMsg;
            di.Result = re.Last().Result;
            di.IsLastStep = re.Last().IsLastStep;
            di.Step = re.Last().Step;
            di.Do_time = re.Last().Do_time;

            List<string> allValue = new List<string>();
            List<string> Value_up = new List<string>();
            List<string> Value_down = new List<string>();
            List<string> value_name = new List<string>();
            List<string> value_result = new List<string>();
            foreach (var item in re)
            {
                allValue.AddRange(item.Data);
                Value_up.AddRange(item.Data_up);
                Value_down.AddRange(item.Data_down);
                value_name.AddRange(item.Data_name);
                value_result.AddRange(item.Data_result);
            }
            di.Data_name = value_name.ToArray();
            di.Data_result = value_result.ToArray();
            di.Data = allValue.ToArray();
            di.Data_up = Value_up.ToArray();
            di.Data_down = Value_down.ToArray();

            return di;
        }
        //  查询指定步骤的结果
        public DataItem Search(string sn,int step)
        {
            DataItem di = new DataItem();

            var re = from myValue in Items
                     where myValue.Sn == sn && myValue.Step == step
                     select myValue;

            int reCount = re.Count();
            if (re.Count() == 0)
            {
                return null;
            }

            di.Sn = sn;
            di.NgMsg = re.Last().NgMsg;
            di.Result = re.Last().Result;
            di.IsLastStep = re.Last().IsLastStep;
            di.Step = re.Last().Step;
            di.Do_time = re.Last().Do_time;

            return di;
        }

        public void RemoveExpired(TimeSpan expireTime)
        {
            var now = DateTime.Now;
            Items.RemoveAll(x => now - x.Timestamp > expireTime);
        }

    }

    public class UploadManager
    {
        public event EventHandler<DataItem> ValueReady;

        public ItemBuffer Buffer = new ItemBuffer();

        private static readonly object GlobalFileLock = new object(); // 全局锁

        private object obj = new object();

        /// <summary>
        /// 程序打开加载
        /// </summary>
        public void Load()
        {
            lock (GlobalFileLock)
            {
                Buffer = SerializeHelper.LoadBinary<ItemBuffer>(PathCenter.HistoryFile(Path.Combine("UploadManager", "uploadManager.cache"))) ?? new ItemBuffer();
                Buffer.RemoveExpired(TimeSpan.FromDays(7));
            }
        }

        /// <summary>
        /// 不断上传数据
        /// </summary>
        /// <param name="item"></param>
        public DataItem Update(DataItem item)
        {
            lock (GlobalFileLock)
            {
                if (item == null)
                {
                    return null;
                }

                Buffer.Add(item);

                DataItem newItem = new DataItem();
                // 如果结果是ng或者是最后一步就触发上传事件
                if (item.IsLastStep || item.Result == "NG")
                {
                    // 寻找所有数据并拼接
                    newItem = Buffer.Search(item.Sn);

                    if (newItem != null)
                    {
                        OnValueReady(newItem);
                    }
                }
                else
                {
                    newItem = null;
                }
                // 序列化保存到本地
                SerializeHelper.SaveBinary<ItemBuffer>(Buffer, PathCenter.HistoryFile(Path.Combine("UploadManager", "uploadManager.cache")));
                return newItem;

            }
        }

        /// <summary>
        /// 不断上传数据
        /// </summary>
        /// <param name="item"></param>
        public DataItem CheckLoalDate(DataItem item)
        {
            lock (GlobalFileLock)
            {
                //if (item == null)
                //{
                //    return false;
                //}

                // 如果结果是ng或者是最后一步就触发上传事件
               
                    // 寻找所有数据并拼接
                    DataItem newItem = Buffer.Search(item.Sn,item.Step);

                return newItem;
            }
        }

        /// <summary>
        /// 不断上传数据
        /// </summary>
        /// <param name="item"></param>
        public bool LoadUpdate(DataItem item)
        {
            lock (GlobalFileLock)
            {
                if (item == null)
                {
                    return false;
                }

                // 如果结果是ng或者是最后一步就触发上传事件
                if (item.IsLastStep || item.Result == "NG")
                {
                    // 寻找所有数据并拼接
                    DataItem newItem = Buffer.Search(item.Sn);

                    if (newItem != null)
                    {
                        OnValueReady(newItem);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 引发需要上传事件
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnValueReady(DataItem item)
        {
            ValueReady?.Invoke(this, item);
        }
    }
}
