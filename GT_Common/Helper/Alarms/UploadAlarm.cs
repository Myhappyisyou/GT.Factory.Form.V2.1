using GT_Common.Helper.Alarms;
using GT_Common.Helper.PlcComm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using GT_Common;
using GT_Common.Model;
using System.Diagnostics;
using GT_Common.Helper.Logging;
using System.Linq;
using GT_Common.Helper.ClientTask;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public class UploadAlarm
    {
        public PatchReader Reader { get; private set; }

        //读取报警信息线程
        private Thread thAlarm;

        private CancellationTokenSource _cts;

        //报警
        private List<Alarm> alarms = new List<Alarm>();
       
        public string StartAddress { get; private set; }

        public ushort ReadLength { get; private set; }

        private List<int> listAlarm = new List<int>();

        private bool[] boolAlarm = new bool[1344];

        private short[] bAlarm = new short[1536];


        private List<AlarmParse> alarmParses = new List<AlarmParse>();

        private volatile bool isRunning = false;

        // 报警处理队列（核心）
        private readonly BlockingCollection<AlarmInfo> _alarmQueue =
            new BlockingCollection<AlarmInfo>(new ConcurrentQueue<AlarmInfo>());

        // 报警处理后台任务
        private Task _alarmWorker;

        // 客户端发送器
        private ClientTaskSender _sender;


        public UploadAlarm(PatchReader reader, string startAddress, ushort length, ClientTaskSender clientTaskSender)
        {
            Reader = reader;
            StartAddress = startAddress;
            ReadLength = length;
            _sender = clientTaskSender;
        }

        /// <summary>
        /// 加载报警
        /// </summary>

        public void Init()
        {
            LoadAlarm();
            boolAlarm = new bool[ReadLength];
            StartAlarmWorker();   // ⭐ 新增
            Start();            
        }


        private void StartAlarmWorker()
        {
            _alarmWorker = Task.Run(() =>
            {
                Console.WriteLine($"报警数量：{_alarmQueue.Count}");
                foreach (var alarm in _alarmQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        // 1 发给服务端
                        _sender.SendTaskAsync("InsertAlarm", new
                        {
                            ProcessName = alarm.ProcessName,
                            AlarmStation = alarm.AlarmStation,
                            AlarmGrade = alarm.AlarmGrade,
                            Description = alarm.Description,
                            CreateTime = alarm.CreateTime,
                            CloseTime = alarm.CloseTime,
                        }).ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                DisplayLog.Error("发送报警失败", t.Exception);
                            }
                        });

                        // 1 更新报警结束等状态
                        UploadSql.UpdateAlarmInfor(alarm);
                        // 本地Access
                        DatabaseSessionManager.EnsureDatabase();

                        var db = DbContext.CurrentDb;
                        UploadSql.Ac_InsertAlarmInfor(db, alarm);
                    }
                    catch (Exception ex)
                    {
                        DisplayLog.Error("报警处理失败", ex);
                    }
                }
            });
        }


        /// <summary>
        /// 触发线程
        /// </summary>
        public void Start()
        {
            if (isRunning) return;
            _cts = new CancellationTokenSource();
            isRunning = true;
            thAlarm = new Thread(() => ReadAlarm(_cts.Token))
            {
                IsBackground = true
            };
            thAlarm.Start();
        }

 
        public void Stop()
        {
            try
            {
                if (!isRunning) return;

                _cts.Cancel();
                _alarmQueue.CompleteAdding();

                foreach (var alarm in alarms)
                {
                    alarm.SampleEvent -= Alarm_SampleEvent; // ⭐ 必须解绑
                }

                thAlarm?.Join();          // PLC 线程退出
                _alarmWorker?.Wait();    // 报警处理线程退出

                isRunning = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("停止报警模块失败: " + ex.Message);
            }
        }


        /// <summary>
        /// 从数据库获取报警解析
        /// </summary>
        private void LoadAlarm()
        {
            DataSet dataAlarm = new DataSet();            
            try
            {
                alarmParses =AlarmConfigManager.Load();
                alarms.Clear();
                foreach (var item in alarmParses)
                {
                    Alarm alarm = new Alarm(item.Id,item.ProcessNo, item.AlarmStation, item.ProcessName, item.AlarmGrade, item.PlcAddr, item.Description);
                    alarm.SampleEvent += Alarm_SampleEvent;
                    alarms.Add(alarm);
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error("加载报警信息解析失败", ex);
            }
        }

        private void ReadAlarm(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {

                    // 读取PLC报警
                    boolAlarm = Reader.Plc.ReadBool(StartAddress, ReadLength).Content;//ok

                    listAlarm.Clear();
                    //判断报警是否触发 true报警状态 false 无报警
                    List<bool> listAllAlarm = new List<bool>();

                    listAllAlarm.AddRange(boolAlarm);

                    for (int i = 0; i < listAllAlarm.Count; i++)
                    {
                        if (listAllAlarm[i] == true)
                        {
                            listAlarm.Add(i);
                        }
                        else
                        {
                            listAlarm.Remove(i);
                        }
                    }

                    for (int i = 0; i < alarms.Count; i++)
                    {
                        alarms[i].UpdateValue(listAllAlarm[i]);
                    }
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    DisplayLog.Error("读取PLC报警异常", ex);
                }
            }
        }

        /// <summary>
        /// 触发报警上传数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void Alarm_SampleEvent(object sender, AlarmInfo e)
        //{
        //    //数据库长传
        //    try
        //    {
        //        //UploadSql.Ac_Server_InsertAlarmInfor(_db, e);

        //        UploadSql.UpdateAlarmInfor(e);
        //    }
        //    catch (Exception ex)
        //    {
        //        DisplayLog.Error("上传报警信息异常", ex);
        //    }
        //}

        private void Alarm_SampleEvent(object sender, AlarmInfo e)
        {
            if (!_alarmQueue.IsAddingCompleted)
            {
                _alarmQueue.Add(e);   // ⭐ 核心：入队
                Console.WriteLine("添加报警列队");
            }
        }

    }
}
