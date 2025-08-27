using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField idField;
    public TMP_InputField pwField;
    public Button loginButton;
    public Button makeIdButton;
    public Button optionButton;
    public TextMeshProUGUI messageText;

    void Awake()
    {
        loginButton.onClick.AddListener(OnClickLogin);
        makeIdButton.onClick.AddListener(OnClickMakeId);
        optionButton.onClick.AddListener(OnClickOption);

        UserDatabase.EnsureReady(); // ù ���� �غ�
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnClickLogin();
    }

    void SetInteractable(bool v)
    {
        loginButton.interactable = v;
        makeIdButton.interactable = v;
        optionButton.interactable = v;
        idField.interactable = v;
        pwField.interactable = v;
    }

    void ShowMsg(string s) { if (messageText) messageText.text = s; Debug.Log(s); }

    public void OnClickLogin()
    {
        string id = idField.text.Trim();
        string pw = pwField.text;

        if (string.IsNullOrEmpty(id)) { ShowMsg("ID�� �Է��ϼ���."); idField.ActivateInputField(); return; }
        if (string.IsNullOrEmpty(pw)) { ShowMsg("��й�ȣ�� �Է��ϼ���."); pwField.ActivateInputField(); return; }

        SetInteractable(false);
        StartCoroutine(CoLogin(id, pw));
    }

    IEnumerator CoLogin(string id, string pw)
    {
        ShowMsg("Ȯ�� ��...");
        yield return null; // �� ������ �纸(�ε� ����)
        bool ok = UserDatabase.Validate(id, pw);

        if (ok)
        {
            ShowMsg("�α��� ����!");
            // PlayerPrefs.SetString("userId", id); // �ΰ����� ���� ����
            yield return new WaitForSeconds(0.15f);
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            ShowMsg("ID �Ǵ� ��й�ȣ�� �ùٸ��� �ʽ��ϴ�.");
            pwField.text = "";
            pwField.ActivateInputField();
            SetInteractable(true);
        }
    }

    // ȸ������ ��ư �� ���� JSON�� ���� �߰�
    void OnClickMakeId()
    {
        string id = idField.text.Trim();
        string pw = pwField.text;
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            ShowMsg("ID/��й�ȣ�� �Է��� �� �ٽ� ��������.");
            return;
        }
        if (UserDatabase.Exists(id))
        {
            ShowMsg("�̹� �����ϴ� ID�Դϴ�.");
            return;
        }
        bool ok = UserDatabase.Register(id, pw);
        ShowMsg(ok ? "ȸ������ �Ϸ�! ���� �α����ϼ���." : "ȸ������ ����.");
    }

    void OnClickOption()
    {
        ShowMsg("�ɼ� ����(�̱���)");
    }
}