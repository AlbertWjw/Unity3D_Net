using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 移动协议
/// </summary>
namespace Net.Proto {
    class MoveProto: BaseProto {
        public float x = 0; 
        public float y = 0; 
        public float z = 0;

        public MoveProto() {
            Console.WriteLine("移动协议 - 构造函数");
            protoType = netEventEnum.Move;
        }

        public override object[] returnFun() {
            List<object> objList = new List<object>();
            objList.Add(id.ToString());
            objList.Add(x.ToString());
            objList.Add(y.ToString());
            objList.Add(z.ToString());
            object[] obj = objList.ToArray();
            return obj;
        }

    }

    class EnterProto : BaseProto {

        public EnterProto() {
            Console.WriteLine("Enter协议 - 构造函数");
            protoType = netEventEnum.Enter;
        }

        public override object[] returnFun() {
            object[] objs = new object[1] { id };
            return objs;
        }

    }

    class LeaveProto : BaseProto {

        public LeaveProto() {
            Console.WriteLine("Leave协议 - 构造函数");
            protoType = netEventEnum.Leave;
        }

        public override object[] returnFun() {
            object[] objs = new object[1] { id };
            return objs;
        }

    }

    class ListProto : BaseProto {

        public List<string> list = new List<string>();
        //public string list = null;

        public ListProto() {
            Console.WriteLine("List协议 - 构造函数");
            protoType = netEventEnum.List;
        }

        public override object[] returnFun() {
            List<object> objList = list.ConvertAll(s => (object)s);
            object[] obj = objList.ToArray();
            return obj;
        }

    }

    class PingProto : BaseProto {

        public PingProto() {
            Console.WriteLine("ping协议 - 构造函数");
            protoType = netEventEnum.Ping;
        }

    }

    class PongProto : BaseProto {

        public PongProto() {
            Console.WriteLine("pong协议 - 构造函数");
            protoType = netEventEnum.Pong;
        }

    }
}
