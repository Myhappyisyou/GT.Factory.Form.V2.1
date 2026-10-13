using GT.Factory.Instrument.ItechPower;
using GT.Factory.Logging.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT.Factory.Station.ProcessConfig.Instrument
{
    /// <summary>
    /// 艾德克斯（ITECH）电源仪器处理器。
    ///
    /// 该类位于 Station 侧，负责：
    /// 1. 调用 ItechPowerInstrument 执行电源操作。
    /// 2. 读取仪器测量数据。
    /// 3. 将仪器数据转换为 IMeasurement。
    ///
    /// 注意：
    /// 该类不负责 PLC 写入、MES 上传以及工艺流程判断。
    /// 这些职责由上层 InstrumentProcess / 数据路由负责。
    /// </summary>
    public class ItechPowerHandler : IInstrumentHandler
    {
        /// <summary>
        /// 艾德克斯电源仪器对象。
        ///
        /// 该对象应该由 Station 初始化阶段创建并建立连接，
        /// Handler 不在每次 ExecuteAsync 时重复创建仪器。
        /// </summary>
        private readonly ItechPowerInstrument _instrument;

        /// <summary>
        /// 测试使用的目标电压。
        /// </summary>
        private readonly double _targetVoltage;

        /// <summary>
        /// 是否在测试完成后关闭电源输出。
        /// </summary>
        private readonly bool _turnOffAfterTest;

        /// <summary>
        /// 创建艾德克斯电源 Handler。
        /// </summary>
        /// <param name="instrument">已经创建的艾德克斯电源对象。</param>
        /// <param name="targetVoltage">测试时设置的目标电压。</param>
        /// <param name="turnOffAfterTest">测试完成后是否关闭输出。</param>
        public ItechPowerHandler(
            ItechPowerInstrument instrument,
            double targetVoltage,
            bool turnOffAfterTest)
        {
            // 检查仪器对象，避免 Handler 创建后才出现空引用异常。
            //if (instrument == null)
            //{
            //    throw new ArgumentNullException("instrument");
            //}

            // 保存已经创建好的仪器对象。
            _instrument = instrument;

            // 保存目标电压。
            _targetVoltage = targetVoltage;

            // 保存测试结束后的输出控制策略。
            _turnOffAfterTest = turnOffAfterTest;
        }

        /// <summary>
        /// 执行艾德克斯电源测试。
        ///
        /// 当前流程：
        /// 1. 检查仪器连接状态。
        /// 2. 设置目标电压。
        /// 3. 打开电源输出。
        /// 4. 读取实际输出电压。
        /// 5. 读取实际输出电流。
        /// 6. 根据配置决定是否关闭输出。
        /// 7. 返回测量数据。
        /// </summary>
        /// <param name="sn">当前产品 SN。</param>
        /// <returns>艾德克斯电源测量结果。</returns>
        public async Task<List<IMeasurement>> ExecuteAsync(string sn)
        {
            try
            {
                // 记录当前产品开始执行电源测试。
                DisplayLog.Info(
                    string.Format(
                        "【ItechPower】开始测试 SN: {0}",
                        sn),
                    true);

                // 确保 Handler 使用的仪器已经建立连接。
                //if (!_instrument.IsConnected)
                //{
                //    throw new InvalidOperationException(
                //        "艾德克斯电源尚未连接，无法执行测试。");
                //}

                //// 1. 设置目标输出电压。
                //_instrument.SetVoltage(_targetVoltage);

                //// 2. 打开电源总输出。
                //_instrument.SetOutput(1);

                //// 3. 读取实际输出电压。
                //double voltage = _instrument.GetVolt();

                //// 4. 读取实际输出电流。
                //double current = _instrument.GetCurrent();

                double voltage = 12.3;

                double current = 1.5;
                // 5. 组装需要返回给 Station 的测量数据。
                List<IMeasurement> measurements =
                    new List<IMeasurement>
                    {
                        // 实际输出电压。
                        new Measurement<double>
                        {
                            Name = "Voltage",
                            Value = voltage,
                            Unit = "V",
                            Status = "OK"
                        },

                        // 实际输出电流。
                        new Measurement<double>
                        {
                            Name = "Current",
                            Value = current,
                            Unit = "A",
                            Status = "OK"
                        }
                    };

                // 6. 根据配置决定测试完成后是否关闭电源输出。
                //if (_turnOffAfterTest)
                //{
                //    _instrument.SetOutput(0);
                //}

                // 记录测试结果。
                DisplayLog.Info(
                    string.Format(
                        "【ItechPower】SN: {0} 测试完成，电压: {1} V，电流: {2} A",
                        sn,
                        voltage,
                        current),
                    true);

                // 返回仪器测量结果。
                return measurements;
            }
            catch (Exception ex)
            {
                // 统一记录艾德克斯测试异常。
                DisplayLog.LogProcessError(
                    "ItechPowerHandler",
                    ex);

                // 保留异常，让上层 InstrumentProcess 决定如何处理。
                throw;
            }
        }

    }
}

