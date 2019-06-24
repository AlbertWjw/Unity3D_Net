using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILog : MonoBehaviour
{
    static GameObject textGo = null;
    static GameObject gridGo = null;
    static GameObject ScrollbarVertical = null;

    public static List<string> log = new List<string>();

    void Start() {
        GameObject view = GameObject.Find("Canvas/Scroll View/Viewport");
        ScrollbarVertical = GameObject.Find("Canvas/Scroll View/Scrollbar Vertical");
        textGo = view.transform.Find("Text").gameObject;
        gridGo = view.transform.Find("Content").gameObject;
        Application.logMessageReceived += (a, b, c) => {
            log.Add(b+":"+a);
        };
    }

    // Update is called once per frame
    void Update() {
        if (log.Count > gridGo.transform.childCount) {
            insText(gridGo.transform.childCount);
        }
    }

    // 生成log
    public static void insText(int gc) {
        GameObject go = Instantiate(textGo, gridGo.transform);
        go.GetComponent<Text>().text = log[gc];
        go.SetActive(true);
        float height = gridGo.transform.childCount * go.GetComponent<RectTransform>().sizeDelta.y;
        gridGo.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        ScrollbarVertical.gameObject.GetComponent<Scrollbar>().value = 0;
    }
}
