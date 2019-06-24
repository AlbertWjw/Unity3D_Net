using Net.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Net {
    class Packet {

        static JavaScriptSerializer Js = new JavaScriptSerializer();

        /// <summary>
        /// 编码
        /// </summary>
        /// <returns></returns>
        public static byte[] Encode(BaseProto message) {
            string s = Js.Serialize(message);
            //Console.WriteLine("编码：" + s);
            return Encoding.UTF8.GetBytes(s);
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <returns></returns>
        public static BaseProto Decode(string protoName, string message) {
            //string message = Encoding.UTF8.GetString(bytes).Replace("\0", null).Trim();
            //Console.WriteLine("准备解码：" + message);
            BaseProto date = Js.Deserialize(message,Type.GetType("Net.Proto."+protoName)) as BaseProto;
            return date;
        }
    }
}
