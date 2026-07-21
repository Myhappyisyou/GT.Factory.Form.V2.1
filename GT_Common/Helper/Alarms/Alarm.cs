using GT_Common.Model;
using Newtonsoft.Json;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Alarms
{
    public  class Alarm
    {

        [DisplayName("ID")]
        public int Id { get; private set; }

        [DisplayName("工序号")]
        public string ProcessNo { get; private set; }

        [DisplayName("机台名称")]
        public string ProcessName { get; private set; }

        [DisplayName("故障发生工位")]
        public string AlarmStation { get; private set; }

        [DisplayName("故障类型")]
        public string AlarmGrade { get; private set; }
        [DisplayName("PLC地址")]
        public string PlcAddr { get; private set; }

        [DisplayName("报警信息")]
        public string Description { get; private set; }


        private bool lastValue;

        private DateTime alarmCreateTime;

        private DateTime alarmCloseTime;

        public event EventHandler<AlarmInfo> SampleEvent;

        private bool _handledThisCycle = false;

        public Alarm(int _id, string _processNo, string _alarmStation, string _processName, string _alarmGrade, string _plcAddr, string _des)
        {
            Id = _id;
            ProcessNo= _processNo;
            AlarmStation = _alarmStation;
            ProcessName = _processName;
            AlarmGrade = _alarmGrade;
            PlcAddr = _plcAddr;
            Description = _des;
        }

        public  void UpdateValue(bool value)
        {
            if (value != lastValue)
            {
                lastValue = value;

                //上升沿
                if (lastValue)
                {
                    alarmCreateTime = DateTime.Now;
                }
                else //下降沿
                {
                    // 下降沿：只允许一次
                    if (_handledThisCycle) return;

                    _handledThisCycle = false;

                    alarmCloseTime = DateTime.Now;
                    AlarmInfo info = new AlarmInfo
                    {
                        Id = Id,
                        ProcessNo = ProcessNo,
                        AlarmStation = AlarmStation,
                        ProcessName = ProcessName,
                        AlarmGrade = AlarmGrade,
                        PlcAddr = PlcAddr,
                        Description = Description,
                        CreateTime = alarmCreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        CloseTime = alarmCloseTime.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    SampleEvent?.Invoke(this, info);
                }          
            }
        }
    }

    public class AlarmInfo
    {
        //  ID
        public int Id { get;  set; }
        //  工序号
        public string ProcessNo { get; set; }

        //  故障发生工位
        public string AlarmStation { get;  set; }

        //  机台名称
        public string ProcessName { get;  set; }

        //  故障类型
        public string AlarmGrade { get;  set; }

        //  PLC地址
        public string PlcAddr { get; set; }

        //  故障描述
        public string Description { get;  set; }

        //  发生时间
        public string CreateTime { get;  set; }

        //  结束时间
        public string CloseTime { get;  set; }

        //  更新标识
        public string Flag { get;  set; } = "F";

    }

    public static class AlarmConfigManager
    {
        private static readonly string FilePath = Path.Combine(PathCenter.ConfigFile("alarms.json"));

        public static void Save(List<AlarmParse> lsAlarms)
        {
            var json = JsonConvert.SerializeObject(lsAlarms, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static List<AlarmParse> Load()
        {
            if (!File.Exists(FilePath)) return new List<AlarmParse>();

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<AlarmParse>>(json);
        }
    }
}
