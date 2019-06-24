using Net.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ToolSet;
using UnityEngine;

/// <summary>
/// 网络模块
/// </summary>
public class NetManager {

    public static NetManager _instance = new NetManager();

    public static Socket client = null;  // 连接套接字
    public static IPEndPoint ipe;   // 服务器ip
    public int pingPongTime = 30;  //心跳间隔(秒)

    private static long lastPongTime = 0;  // 上次收到心跳的时间
    private bool isReceive = true;  // 是否接收消息
    private Thread pingThread = null;  // 心跳线程
    private static ByteArray readbuff = new ByteArray();  // 接收缓冲区
    private bool isPing = false;  // 是否开启心跳
    private static Queue<string> sendMgrQueue = new Queue<string>();  // 发送消息队列

    public static Queue<string[]> MessageQueue { get; set; } = new Queue<string[]>();

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip">服务器ip</param>
    /// <param name="port">服务端口</param>
    /// <param name="MgrCallback">消息接收回调</param>
    public void Connect(string ip,int port) {
        ipe = new IPEndPoint(IPAddress.Parse(ip), port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IAsyncResult result = null;
        try {
            result = client.BeginConnect(ipe, BeginConnectCallback, client);
        } catch (SocketException ex) {
            client.EndConnect(result);
        }
    }
    
    /// <summary>
    /// 发送方法
    /// </summary>
    /// <param name="type">消息类型</param>
    /// <param name="message">消息内容</param>
    public void Send(netEventEnum type,string message) {
        //消息构成 2byte总长，2byte消息类型，之后的都是消息内容

        // 消息类型bytes
        byte[] typeBytes = new byte[2];  
        typeBytes[0] = (byte)(((int)type) % 256);
        typeBytes[1] = (byte)(((int)type) / 256);
        // 打印消息类
        string strr = BitConverter.ToInt16(typeBytes, 0).ToString();
        //UILog.log.Add("发送消息类型字符串：" + strr);
        //UILog.log.Add("发送消息类型：" + type.ToString());
        // 消息类型字符串
        string str = Encoding.UTF8.GetString(typeBytes);  
        // 写入消息队列
        lock (sendMgrQueue) {
            sendMgrQueue.Enqueue(str+message);
        }

        // 发送消息拼装（加入长度）
        if (client.Connected) {
            if (sendMgrQueue.Count < 0) return;  // 待发送消息队列中没有消息跳过
            string msg = sendMgrQueue.Peek();  // 取出待发送消息
            // 消息长度
            byte[] sendbytes = Encoding.UTF8.GetBytes(msg);
            Int16 mgrLen = (Int16)sendbytes.Length;
            byte[] lenBytes = new byte[2];
            lenBytes[0] = (byte)(sendbytes.Length % 256);
            lenBytes[1] = (byte)(sendbytes.Length / 256);
            byte[] sendBytes = lenBytes.Concat(sendbytes).ToArray();  // 发送bytes
            //UILog.log.Add("发送："+Encoding.UTF8.GetString(sendbytes));
            client.BeginSend(sendBytes, 0, sendBytes.Length, 0, BeginSendCallback, client);
        } else {
            //UILog.log.Add("与服务器断开");
            Close();
        }
    }
    
    // BeginConnect回调
    public void BeginConnectCallback(IAsyncResult ar) {
        try {
            ((Socket)ar.AsyncState).EndConnect(ar);

            //开始心跳
            isPing = true;
            pingThread = new Thread(Ping);
            pingThread.Start();

            // 开启消息接收
            //UILog.log.Add("开始接收...");
            client.BeginReceive(readbuff.bytes, readbuff.writeIdx, readbuff.remain, 0, BeginReceiveCallback, client);
        } catch (SocketException ex) {
            //UILog.log.Add(ex.NativeErrorCode+":"+ex.ToString());
            // 重连
            //10061已经找到对方但是被对方拒绝
            if (ex.NativeErrorCode.Equals(10061)) {
                // UILog.log.Add(ipe.Address.ToString()+":"+ ipe.Port.ToString());
                client.BeginConnect(ipe, BeginConnectCallback, client);
            }
        }
    }

    // 接收回调
    public void BeginReceiveCallback(IAsyncResult ar) {
        try {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count <= 0) {  // 已断开
                //UILog.log.Add("与服务器断开" );
                Close();
                return;
            }
            //UILog.log.Add("接收长度："+ count);
            //UILog.log.Add("接收："+Encoding.UTF8.GetString(readbuff));
            readbuff.writeIdx += count;
            OnReceiveData();
            readbuff.MoveBytes();
            if (!isReceive) return;
            socket.BeginReceive(readbuff.bytes, readbuff.writeIdx, readbuff.remain, 0, BeginReceiveCallback, socket);
        } catch (SocketException ex) {
            UILog.log.Add(ex.ToString());
            Close();
        } catch (Exception ex) {
            UILog.log.Add(ex.ToString());
            Close();
        }
    }

    // 发送回调
    public void BeginSendCallback(IAsyncResult ar) {
        try {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            //UILog.log.Add("发送成功！发送长度："+count);
            lock (sendMgrQueue) {
                sendMgrQueue.Dequeue();
            }

        } catch (Exception ex) {
            //UILog.log.Add(ex.ToString());
        }
    }

    // 需要外部循环调用的
    // 消息下发(主线程)
    public static void Update() {
        // 从消息列表中获取消息
        if (NetManager.MessageQueue.Count > 0) {
            
            string[] item = null;
            lock (NetManager.MessageQueue) {
                item = NetManager.MessageQueue.Peek();
            }
            string type = item[0];
            string msg = item[1];
            UILog.log.Add("接收到的消息：" + msg);
            BaseProto date = JsonUtility.FromJson(msg, Type.GetType(type)) as BaseProto;
            NetEvent.SendEvent((netEventEnum)date.protoType, date.returnFun()); // 发送事件
            lock (NetManager.MessageQueue) {
                NetManager.MessageQueue.Dequeue();
            }
            UILog.log.Add("接收到的消息类型：" + date.protoType);
        }
    }

    // 处理接收到的消息 
    public static void OnReceiveData() {
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
        if ((netEventEnum)(int.Parse(type)) != netEventEnum.Pong) {  // 当收到的信息不是ping-pong时回调
            lock (MessageQueue) {
                MessageQueue.Enqueue(new string[2] { protoType, message });
            }
        } else {
            lastPongTime = Tool.GetTimestamp();
        }

        // 粘包的继续解析
        if (readbuff.Length > 2) {
            OnReceiveData();
        }
    }

    // 心跳
    private void Ping() {
        if (!client.Connected) {
            Close();
        }
        while (isPing) {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds; // 相差毫秒数
            var mp = new PingProto();
            mp.Timestamp = timeStamp;
            mp.protoType = netEventEnum.Ping;
            string str = JsonUtility.ToJson(mp);
            Send(netEventEnum.Ping, str);
            //lastPingTime = timeStamp;
            Thread.Sleep(pingPongTime * 1000);
        }

    }

    // 关闭
    public void Close() {
        isReceive = false;
        // 关闭心跳
        isPing = false;
        if (pingThread != null) {
            pingThread.Abort();
        }

        // 关闭套接字
        if (client != null) {
            client.Close();
        }
        sendMgrQueue.Clear();  // 清空消息队列
        //UILog.log.Add("关闭");
    }
}
