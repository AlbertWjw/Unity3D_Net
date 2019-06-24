using Net.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolSet;
using UnityEngine;

/// <summary>
///网络操作类
/// </summary>
public class NetDispose : MonoBehaviour
{
    NetManager net = null;

    public Transform cubeParent = null;
    public Transform cube = null;

    public Dictionary<int,Vector3> playerGoList = new Dictionary<int,Vector3>();
    public List<Action> moveList = new List<Action>();
    public Queue<string[]> moveQueue = new Queue<string[]>();

    public int id = 0;

    void Start()
    {
        cubeParent = GameObject.Find("List").transform;
        cube = cubeParent.Find("item");
        NetEvent.AddEvent(enter, netEventEnum.Enter);
        NetEvent.AddEvent(leave, netEventEnum.Leave);
        NetEvent.AddEvent(move, netEventEnum.Move);
        NetEvent.AddEvent(list, netEventEnum.List);

        // 网络
        net = NetManager._instance;
        net.pingPongTime = 3;
        net.Connect("127.0.0.1", 9003);

    }

    void Update() {
        NetManager.Update();
        // 临时按键控制
        if (Input.GetKeyDown(KeyCode.X)) {
            net.Close();
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            var mp = new MoveProto();
            System.Random ran = new System.Random();
            mp.x = ran.Next(-10,10);
            mp.id = id;
            mp.Timestamp = Tool.GetTimestamp();
            string str = JsonUtility.ToJson(mp);
            net.Send(netEventEnum.Move,str);
        }
    }

    // 销毁
    private void OnDisable() {
        NetEvent.DelEvent(enter, netEventEnum.Enter);
        NetEvent.DelEvent(leave, netEventEnum.Leave);
        NetEvent.DelEvent(move, netEventEnum.Move);
        NetEvent.DelEvent(list, netEventEnum.List);
        net.Close();
    }

    // 连接完成处理
    public void enter(params object[] args) {
        id = int.Parse(args[0].ToString());
    }

    public void leave(params object[] args) {
        int leaveId = int.Parse(args[0].ToString());
        if (playerGoList.ContainsKey(leaveId)) {
            playerGoList.Remove(leaveId);
        }
        Transform child = cubeParent.Find(leaveId.ToString());
        if (child != null) {
            Destroy(child.gameObject);
        }
    }

    // 列表下发处理
    public void list(params object[] objs) {
        string[] args = new string[objs.Length];
        objs.CopyTo(args, 0);
        //string[] str = args[0].Split(';');
        string[] str = args;
        foreach (var item in str) {
            if (item == null || item == "")
                continue;
            string[] i = item.Split(',');
            if (!playerGoList.ContainsKey(int.Parse(i[0]))) {
                playerGoList.Add(int.Parse(i[0]), new Vector3(float.Parse(i[1]), float.Parse(i[2]), float.Parse(i[3])));
            }
        }

        // 玩家列表下发后刷新
        if (cubeParent.childCount - 1 < playerGoList.Count) {
            int[] keys = new int[playerGoList.Keys.Count];
            playerGoList.Keys.CopyTo(keys, 0);
            foreach (var i in keys) {
                Transform child = cubeParent.Find(i.ToString());
                if (child != null) {
                    continue;
                }
                GameObject go = Instantiate(this.cube, cubeParent).gameObject;
                go.name = i.ToString();
                if (playerGoList.ContainsKey(i) && playerGoList[i] != null) {
                    go.transform.position = playerGoList[i];
                } else {
                    go.transform.position = Vector3.zero;
                }
                go.SetActive(true);
            }
        }

    }

    // 移动处理
    public void move(params object[] objs) {
        string[] args = new string[objs.Length];
        objs.CopyTo(args, 0);
        lock (moveQueue) {
            moveQueue.Enqueue(args);
        }

        // 移动处理
        if (moveQueue.Count > 0) {
            string[] item = null;
            lock (moveQueue) {
                item = moveQueue.Peek();
            }
            if (item == null) return;
            if (item.Count() == 4) {
                int id = int.Parse(item[0]);
                Vector3 v3 = new Vector3(float.Parse(item[1]), float.Parse(item[2]), float.Parse(item[3]));
                if (playerGoList.ContainsKey(id)) {
                    playerGoList[id] = v3;
                    Transform tr = cubeParent.Find(id.ToString());
                    if (tr == null) return;
                    tr.position = v3;
                }
            }
            lock (moveQueue) {
                moveQueue.Dequeue();
            }
        }
    }
}
