using System.Collections.Generic;

/// <summary>
/// 事件枚举
/// </summary>
public enum netEventEnum {

    None = 0,
    Ping = 1,
    Pong = 2,

    // 连接
    Enter = 11,  // 连接   
    Leave = 12,  // 断连

    List = 23,  // 下发玩家列表

    // 操控
    Move = 101,   // 移动
    Rotate = 102,  // 旋转
    Jump = 103,  // 跳跃
    Fire = 104,  // 开火

    max = 105,  // event最大值，添加enum时需要一同修改
}

/// <summary>
/// 事件管理
/// </summary>
public class NetEvent
{
    public delegate void Operation(params object[] handler);
    private static List<Operation>[] operation;

    public NetEvent(){
        operation = new List<Operation>[(int)netEventEnum.max];  // 初始化事件列表
    }

    /// <summary>
    /// 事件订阅
    /// </summary>
    public static void AddEvent(Operation handler, params netEventEnum[] events) {
        if (operation == null)
            operation = new List<Operation>[(int)netEventEnum.max];
        for (int i = 0; i < events.Length; i++) {
            //Debug.Log("事件订阅  "+i);
            int key = (int)events[i];
            if (i > operation.Length) continue;
            if (operation[key] == null) {
                operation[key] = new List<Operation>();
            }
            operation[key].Add(handler);
        }
    }

    /// <summary>
    /// 事件退订
    /// </summary>
    public static void DelEvent(Operation handler, netEventEnum e) {
        //Debug.Log("事件退订  "+(int)e);
        if ((int)e > operation.Length || operation[(int)e] == null) return;
        operation[(int)e].Remove(handler);
    }

    /// <summary>
    /// 事件派发
    /// </summary>
    public static void SendEvent(netEventEnum eventId, params object[] obj) {
        // Debug.Log("事件派发  "+(int)eventId);
        if (operation == null || operation[(int)eventId] == null) return;
        for (int i = 0; i < operation[(int)eventId].Count; i++) {
            operation[(int)eventId][i](obj);
        }
    }
}
