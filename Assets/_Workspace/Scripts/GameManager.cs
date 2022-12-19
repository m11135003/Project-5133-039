using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("主畫面")]
    public GameObject mainUI;


    [Header("選項介面")]
    public GameObject optionUI;


    [Header("內容介面")]
    public GameObject contentUI;


    [Header("選項按鈕顏色")]
    // 普通狀態
    public Color buttonNormal;

    // 被選中
    public Color buttonSelected;


    [Header("選項與內容對應")]
    public Content[] content;


    [Header("狀態")]
    public State state = State.Main;
    public int optionIndex;


    private float _unit;
    private SerialController _serialController;

    public enum State
    {
        Main,
        Options,
        Content
    }

    [System.Serializable]
    public class Content
    {
        public Button button;
        public GameObject content;
    }

    private void Start()
    {
        // 取得 Ardity 控制器
        _serialController = GetComponent<SerialController>();

        // 註冊按鈕點擊事件
        for (int i = 0; i < content.Length; i++)
        {
            var index = i;
            content[index].button.onClick.AddListener(() => DisplayContentUI(content[index].content));
        }

        // 計算可變電阻 (0~100) 分段為五按鈕的一單位數值
        _unit = 100f / (content.Length - 1);

        // 顯示主畫面
        DisplayMainUI();
    }

    private void Update()
    {
        // 接收 Arduino 字串
        var receive = _serialController.ReadSerialMessage();
        if (string.IsNullOrEmpty(receive))
            return;

        Debug.LogFormat("Receive Message: {0}", receive);

        // 轉型成整數
        var success = int.TryParse(receive, out int number);
        if (!success)
            return;

        // 當 number <= 100 為可變電阻作動
        if (number <= 100)
        {
            // 四捨五入並限制數值範圍為 0~4 整數，共 5 個選項
            optionIndex = Mathf.Clamp(Mathf.RoundToInt(number / _unit), 0, content.Length - 1);

            // 改變選中的按鈕顏色，並恢復未選中按鈕顏色
            for (int i = 0; i < content.Length; i++)
                content[i].button.GetComponent<Image>().color = i == optionIndex ? buttonSelected : buttonNormal;

            Debug.LogFormat("Option Index: {0}", optionIndex);
        }

        // 當 number == 101 為按鈕按下
        else if (number == 101)
        {
            switch (state)
            {
                // 如使用者在主畫面按下按鈕，進入選項介面
                case State.Main:
                    DisplayOptionUI();
                    break;

                // 如使用者在選項介面按下按鈕，觸發對應選項
                case State.Options:
                    content[optionIndex].button.onClick.Invoke();
                    break;

                // 如使用者在內容介面按下按鈕，回到選項介面
                case State.Content:
                    DisplayOptionUI();
                    break;
            }

            Debug.Log("Button Down");
        }

        // 當 number == 102 為按鈕放開
        // 目前無功能
        else if (number == 102)
        {
            Debug.Log("Button Up");
        }
    }


    // 顯示主畫面
    private void DisplayMainUI()
    {
        mainUI.SetActive(true);
        optionUI.SetActive(false);
        contentUI.SetActive(false);
        state = State.Main;
    }

    // 顯示選項介面
    private void DisplayOptionUI()
    {
        mainUI.SetActive(false);
        optionUI.SetActive(true);
        contentUI.SetActive(false);
        state = State.Options;
    }

    // 顯示內容介面
    private void DisplayContentUI(GameObject content)
    {
        mainUI.SetActive(false);
        optionUI.SetActive(false);
        contentUI.SetActive(true);
        state = State.Content;

        for (int i = 0; i < this.content.Length; i++)
            this.content[i].content.SetActive(this.content[i].content.Equals(content));

        Debug.LogFormat("Button Click: {0}", content.name);
    }
}