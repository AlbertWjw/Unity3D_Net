using Net.Proto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Net
{
    class Program
    {
        public static Server server = null;

        private static bool isStop = false;
        //private static bool isGameStart = false;

        static void Main(string[] args)
        {
            server = new Server();

            Thread getKey = new Thread(() => {
                while (true) {
                    if (Console.ReadKey().Key == ConsoleKey.Q) {
                        server.Close();
                        Console.WriteLine("关闭");
                        isStop = true;
                        break;
                    }
                }

            });
            getKey.Start();

            while (!isStop) {
                //if (Server.clients.Count >= 2 && !isGameStart) {
                //    Console.WriteLine("游戏开始");
                //    BaseProto bp = new BaseProto();
                //    bp.Timestamp = Tool.GetTimestamp();
                //    string str = Encoding.UTF8.GetString(Packet.Encode(bp));
                //    Server.broadcast(netEventEnum.Enter,str);
                //    isGameStart = true;
                //}
            }
        }
    }
}
