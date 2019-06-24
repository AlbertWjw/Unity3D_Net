using Net.Proto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Net {
    class Server {
        public static int port = 9003;
        public static string host = "127.0.0.1";  //服务器端ip地址

        IPAddress ip;
        IPEndPoint ipe;
        Socket serverSocket = null;
        Thread accept = null;

        private static bool isOpenAccept = true;
        public static List<Client> clients = new List<Client>();

        // 构造函数
        public Server() {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ip = IPAddress.Parse(host);
            ipe = new IPEndPoint(ip, port);
            serverSocket.Bind(ipe);
            accept = new Thread(() => {
                serverSocket.Listen(10);
                Console.WriteLine("监听已经打开，请等待");
                try {
                    while (isOpenAccept) {
                        Socket client = serverSocket.Accept();
                        clients.Add(new Client(client, clients.Count));

                        ListProto lp = new ListProto();
                        foreach (var item in clients) {
                            string[] strs = new string[4];
                            strs[0] = item.id.ToString();
                            item.pos.CopyTo(strs, 1);
                            lp.list.Add(string.Join(",",strs));
                        }
                        broadcast(netEventEnum.List, Encoding.UTF8.GetString(Packet.Encode(lp))); // 广播列表
                    }
                } catch (Exception e) {
                    //一般是断开连接
                    Console.WriteLine(e.ToString());
                }
            });
            accept.Start();
        }

        // 广播
        public static void broadcast(netEventEnum type, string message) {
            foreach (var i in clients) {
                i.Send(type, message);
            }
        }

        // 关闭
        public void Close() {
            isOpenAccept = false;
            serverSocket.Close();
            accept.Abort();

            // 关闭与所有客户端的连接
            foreach (var i in clients) {
                Console.WriteLine("id:" + i.id);
                i.Close();
            }
        }
    }
}
