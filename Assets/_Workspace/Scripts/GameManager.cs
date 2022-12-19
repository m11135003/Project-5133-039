using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("�D�e��")]
    public GameObject mainUI;


    [Header("�ﶵ����")]
    public GameObject optionUI;


    [Header("���e����")]
    public GameObject contentUI;


    [Header("�ﶵ���s�C��")]
    // ���q���A
    public Color buttonNormal;

    // �Q�襤
    public Color buttonSelected;


    [Header("�ﶵ�P���e����")]
    public Content[] content;


    [Header("���A")]
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
        // ���o Ardity ���
        _serialController = GetComponent<SerialController>();

        // ���U���s�I���ƥ�
        for (int i = 0; i < content.Length; i++)
        {
            var index = i;
            content[index].button.onClick.AddListener(() => DisplayContentUI(content[index].content));
        }

        // �p��i�ܹq�� (0~100) ���q�������s���@���ƭ�
        _unit = 100f / (content.Length - 1);

        // ��ܥD�e��
        DisplayMainUI();
    }

    private void Update()
    {
        // ���� Arduino �r��
        var receive = _serialController.ReadSerialMessage();
        if (string.IsNullOrEmpty(receive))
            return;

        Debug.LogFormat("Receive Message: {0}", receive);

        // �૬�����
        var success = int.TryParse(receive, out int number);
        if (!success)
            return;

        // �� number <= 100 ���i�ܹq���@��
        if (number <= 100)
        {
            // �|�ˤ��J�í���ƭȽd�� 0~4 ��ơA�@ 5 �ӿﶵ
            optionIndex = Mathf.Clamp(Mathf.RoundToInt(number / _unit), 0, content.Length - 1);

            // ���ܿ襤�����s�C��A�ë�_���襤���s�C��
            for (int i = 0; i < content.Length; i++)
                content[i].button.GetComponent<Image>().color = i == optionIndex ? buttonSelected : buttonNormal;

            Debug.LogFormat("Option Index: {0}", optionIndex);
        }

        // �� number == 101 �����s���U
        else if (number == 101)
        {
            switch (state)
            {
                // �p�ϥΪ̦b�D�e�����U���s�A�i�J�ﶵ����
                case State.Main:
                    DisplayOptionUI();
                    break;

                // �p�ϥΪ̦b�ﶵ�������U���s�AĲ�o�����ﶵ
                case State.Options:
                    content[optionIndex].button.onClick.Invoke();
                    break;

                // �p�ϥΪ̦b���e�������U���s�A�^��ﶵ����
                case State.Content:
                    DisplayOptionUI();
                    break;
            }

            Debug.Log("Button Down");
        }

        // �� number == 102 �����s��}
        // �ثe�L�\��
        else if (number == 102)
        {
            Debug.Log("Button Up");
        }
    }


    // ��ܥD�e��
    private void DisplayMainUI()
    {
        mainUI.SetActive(true);
        optionUI.SetActive(false);
        contentUI.SetActive(false);
        state = State.Main;
    }

    // ��ܿﶵ����
    private void DisplayOptionUI()
    {
        mainUI.SetActive(false);
        optionUI.SetActive(true);
        contentUI.SetActive(false);
        state = State.Options;
    }

    // ��ܤ��e����
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