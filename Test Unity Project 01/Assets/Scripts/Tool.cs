using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 工具类
/// </summary>
namespace ToolSet {
    class Tool {

        /// <summary>
        /// 获取当前时间戳(毫秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp() {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds; // 相差毫秒数
            return timeStamp;
        }
    }
}
