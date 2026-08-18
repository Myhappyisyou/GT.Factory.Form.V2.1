namespace GT.Factory.Common.Models
{
    /// <summary>
    /// 查询产品上传次数及返工状态的结果
    /// </summary>
    public class ReworkQueryResult
    {
        /// <summary>
        /// 返工标志（1=允许返工，0=不允许返工）
        /// </summary>
        public int ReworkFlag { get; set; }

        /// <summary>
        /// 状态（1=正常，2=异常/超限/未找到）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        public int NgCode { get; set; }
    }
}