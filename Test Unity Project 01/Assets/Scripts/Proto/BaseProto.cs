using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 协议基类
/// </summary>
namespace Net.Proto {
    class BaseProto {
        public netEventEnum protoType = netEventEnum.None;
        public long Timestamp = 0;
        public int id = 0;

        public virtual object[] returnFun() {
            return null;
        }

        public static byte[] EnMsgType(netEventEnum type) {
            string str = type.ToString();
            return Encoding.UTF8.GetBytes(str);
        }
        public static netEventEnum DeMsgType(byte[] bytes) {
            string str = Encoding.UTF8.GetString(bytes);
            int typeNum = Convert.ToInt32(str,16);
            return (netEventEnum)typeNum;
        }
    }
}
