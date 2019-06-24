using Net.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Net
{
    class Client {

        public int id = 0;  // 玩家id
        private Socket socket = null; // 套接字
        private bool isReceive = true;  // 是否接收消息
        private ByteArray readbuff = new ByteArray();  // 接收缓冲区
        private Queue<string> sendMgrQueue = new Queue<string>();  // 发送消息队列

        private long lastPingTime = 0;  // 上次收到ping的时间

        public string[] pos = { "0", "0", "0" };

        // 构造
        public Client(Socket socket,int id) {
            Console.WriteLine("连接已经建立");
            this.socket = socket;
            this.id = id;

            // 连接成功，下发用户信息（id）
            BaseProto bp = new BaseProto();
            bp.protoType = netEventEnum.Enter;
            bp.id = id;
            Send(netEventEnum.Enter, Encoding.UTF8.GetString(Packet.Encode(bp))); 

            // 开始接收
            Console.WriteLine("开始接收...");
            SendMsg();
            socket.BeginReceive(readbuff.bytes, readbuff.writeIdx, readbuff.remain, 0, BeginReceiveCallback, socket);
        }

        /// <summary>
        /// 发送方法
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="message">消息内容</param>
        public void Send(netEventEnum type, string message) {
            //消息构成 2byte总长，2byte消息类型，之后的都是消息内容

            // 消息类型bytes
            byte[] typeBytes = new byte[2];
            typeBytes[0] = (byte)(((int)type) % 256);
            typeBytes[1] = (byte)(((int)type) / 256);
            // 打印消息类
            string strr = BitConverter.ToInt16(typeBytes, 0).ToString();
            //Console.WriteLine("发送消息类型字符串：" + strr);
            if(type != netEventEnum.Pong)
                Console.WriteLine(socket.RemoteEndPoint.ToString()+"发送消息类型：" + type.ToString());
            // 消息类型字符串
            string str = Encoding.UTF8.GetString(typeBytes);
            // 写入消息队列
            lock (sendMgrQueue) {
                sendMgrQueue.Enqueue(str + message);
            }
        }

        // 真正将消息发送出去的
        private void SendMsg() {
            // 发送消息拼装（加入长度）
            if (socket.Connected) {
                if (sendMgrQueue.Count <= 0) {
                    Thread.Sleep(100);
                    SendMsg();
                    return;
                }  // 待发送消息队列中没有消息跳过  todo
                string msg = "";
                lock (sendMgrQueue) {
                    msg = sendMgrQueue.Peek();  // 取出待发送消息
                }
                // 消息长度
                byte[] sendbytes = Encoding.UTF8.GetBytes(msg);
                Int16 mgrLen = (Int16)sendbytes.Length;
                byte[] lenBytes = new byte[2];
                lenBytes[0] = (byte)(sendbytes.Length % 256);
                lenBytes[1] = (byte)(sendbytes.Length / 256);
                byte[] sendBytes = lenBytes.Concat(sendbytes).ToArray();  // 发送bytes
                //Console.WriteLine("发送：" + Encoding.UTF8.GetString(sendbytes));
                socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, BeginSendCallback, socket);
            } else {
                Console.WriteLine("与服务器断开");
                Close();
            }
        }

        // 发送回调
        public void BeginSendCallback(IAsyncResult ar) {
            try {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar);
                //Console.WriteLine("发送成功！发送长度：" + count);
                lock (sendMgrQueue) {
                    sendMgrQueue.Dequeue();
                }
                SendMsg(); // 将消息队列中的消息发送给客户端

            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        // 接收回调
        public void BeginReceiveCallback(IAsyncResult ar) {
            try {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndReceive(ar);
                if (count <= 0) {  // 已断开
                    Console.WriteLine("与id：" + id + "的客户端断开");
                    Close();
                    return;
                }
                //UILog.log.Add("接收长度："+ count);
                //UILog.log.Add("接收："+Encoding.UTF8.GetString(readbuff));
                readbuff.writeIdx += count;
                OnReceiveData(); // 处理消息
                readbuff.MoveBytes();
                if (!isReceive) return;
                socket.BeginReceive(readbuff.bytes, readbuff.writeIdx, readbuff.remain, 0, BeginReceiveCallback, socket);
            } catch (SocketException ex) {
                Console.WriteLine("与id：" + id + "的客户端断开");
                Close();
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Close();
            }
        }

        // 处理接收到的消息 
        public void OnReceiveData() {
            // 半包的先不处理
            if (readbuff.Length <= 2) return;
            Int16 bodyLen = BitConverter.ToInt16(readbuff.bytes, 0);
            //UILog.log.Add("收到信息长度：" + bodyLen);
            if (readbuff.Length < 2 + bodyLen) return;

            // 处理消息
            string message = Encoding.UTF8.GetString(readbuff.bytes, 2, bodyLen);
            readbuff.readIdx += 2 + bodyLen;
            readbuff.CheckAndMoveBytes();
            Int16 typeLen = BitConverter.ToInt16(Encoding.UTF8.GetBytes(message), 0);
            string type = typeLen.ToString();
            message = message.Substring(2, message.Length - 2);
            //UILog.log.Add("收到信息类型：" + ((netEventEnum)(int.Parse(type))).ToString());
            string protoType = "Net.Proto." + ((netEventEnum)(int.Parse(type))).ToString() + "Proto";
            if ((netEventEnum)(int.Parse(type)) != netEventEnum.Ping) {
                Console.WriteLine("收到信息类型：" + ((netEventEnum)(int.Parse(type))).ToString());
                if ((netEventEnum)(int.Parse(type)) == netEventEnum.Move) {
                    MoveProto mp = Packet.Decode(((netEventEnum)(int.Parse(type))).ToString() + "Proto", message) as MoveProto;
                    pos[0] = mp.x.ToString();
                    pos[1] = mp.y.ToString();
                    pos[2] = mp.z.ToString();
                }
                Server.broadcast((netEventEnum)(int.Parse(type)), message);  // 广播
            } else {
                lastPingTime = Tool.GetTimestamp();
                PongProto pp = new PongProto();
                pp.Timestamp = Tool.GetTimestamp();
                Send(netEventEnum.Pong, Encoding.UTF8.GetString(Packet.Encode(pp)));
            }
            // 粘包的继续解析
            if (readbuff.Length > 2) {
                OnReceiveData();
            }
        }

        public void Close() {
            isReceive = false;
            Server.clients.Remove(this);
            if (socket != null) {
                socket.Close();
            }

            lastPingTime = Tool.GetTimestamp();
            LeaveProto lp = new LeaveProto();
            lp.Timestamp = Tool.GetTimestamp();
            lp.id = id;
            Server.broadcast(lp.protoType, Encoding.UTF8.GetString(Packet.Encode(lp)));  // 广播
        }
    }
}
