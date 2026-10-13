using System;
using System.Diagnostics;
using System.Threading;
using GT.Factory.PLC.PlcComm;

namespace GT.Factory.Station.Helper
{
    public class FacilityState
    {
        private readonly PatchReader plc;
        private readonly string index;
        private string processNo;
        private ushort lastState;

        private readonly Stopwatch runtimeStopwatch = new Stopwatch();
        private readonly Stopwatch alarmStopwatch = new Stopwatch();

        Stopwatch swUpDate = new Stopwatch();

        private string dateCache = "";
        private string shiftCache = "";

        private Thread monitorThread;
        private CancellationTokenSource cancellationTokenSource;

        public FacilityState(PatchReader plc, string index)
        {
            this.plc = plc;
            this.index = index;            
        }

        //  初始化
        public void Init(string processno)
        {
            processNo = processno;
            cancellationTokenSource = new CancellationTokenSource();
            monitorThread = new Thread(() => MonitorLoop(cancellationTokenSource.Token))
            {
                IsBackground = true
            };
            monitorThread.Start();
        }

        //  停止
        public void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();

                // 等待线程退出
                monitorThread?.Join();

                // 最后一次数据上传
                UpdateEquipmentValue(processNo, DateTime.Now, DateTime.Now, lastState);
               
                // 重置 Stopwatch
                runtimeStopwatch.Reset();
                alarmStopwatch.Reset();
            }
        }

        //0：运行中 1：返工 4：故障 6：停机 8点检
       
        private void MonitorLoop(CancellationToken token)
        {
            GetDateAndShift(out dateCache, out shiftCache);

            swUpDate.Start();

            lastState = plc.Data.US(index);

            while (!token.IsCancellationRequested)
            {
                ushort currentState;

                try
                {
                    currentState = plc.Data.US(index);
                }
                catch
                {
                    Thread.Sleep(1000);
                    continue;
                }

               GetDateAndShift(out string date, out string shift);

                bool needUpload =
                    currentState != lastState ||
                    swUpDate.Elapsed.TotalSeconds > 600 ||
                    dateCache != date ||
                    shiftCache != shift;

                if (needUpload)
                {
                    UpdateEquipmentValue(processNo, DateTime.Now, DateTime.Now, lastState);

                    lastState = currentState;
                    dateCache = date;
                    shiftCache = shift;

                    swUpDate.Restart();
                }

                Thread.Sleep(500);
            }
        }

        //  获取数据库时间
        //public DateTime GetSqlDate()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    DataSet dataSet = new DataSet();
        //    sb.Append("SELECT GETDATE()");

        //    try
        //    {
        //        dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sb.ToString());
        //        return DateTime.Parse(dataSet.Tables[0].Rows[0][0].ToString());
        //    }
        //    catch (Exception)
        //    {
        //        return DateTime.Now;
        //    }
        //}

        //  上传当前状态
        private void UpdateEquipmentValue(string processno, DateTime starttime, DateTime endtime, int state)
        {
            AppRuntime.Current.DataRuntime.Repositories.Alarms.InsertEquipmentRunAsync(processno, starttime, endtime, state).ConfigureAwait(false);

            //string sql = $"EXEC equipment_run_state_record @eqpt_id = '{processno}',@state_start_time = GETDATE(), @state_end_time = GETDATE(), @state = {state},@run_data = N'{{}}' ";
            //try
            //{
            //    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql);
            //}
            //catch { }
        }

        //  获取当前班次
        private void GetDateAndShift(out string date, out string shift)
        {
            DateTime dt = DateTime.Now;
            
            if (dt.Hour >= 8 && dt.Hour < 20)
            {
                date = dt.ToString("yyyy-MM-dd");
                shift = "d";
            }
            else if (dt.Hour < 8)
            {
                date = dt.AddDays(-1).ToString("yyyy-MM-dd");
                shift = "n";
            }
            else
            {
                date = dt.ToString("yyyy-MM-dd");
                shift = "n";
            }
        }
    }
}
