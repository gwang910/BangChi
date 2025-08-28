using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Fields (Legacy)")]
    public InputField idField;
    public InputField pwField;

    [Header("Buttons")]
    public Button loginButton;
    public Button makeIdButton;
    public Button optionButton;

    [Header("Texts (Legacy)")]
    public Text messageText;

    [Header("Style")]
    public Font baeminFont;

    public GameObject optionWindow;  // �ɼ�â �г�

    void Awake()
    {
        loginButton.onClick.AddListener(OnClickLogin);
        makeIdButton.onClick.AddListener(OnClickMakeId);
        optionButton.onClick.AddListener(OnClickOption);

        // �Է�ĭ ��Ÿ��/���� �غ�
        PrepareInput(idField, isPassword: false);
        PrepareInput(pwField, isPassword: true);
        optionWindow.SetActive(false); // ���� �� ���� ����

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

        if (string.IsNullOrEmpty(id)) { ShowMsg("ID�� �Է��ϼ���."); idField.Select(); idField.ActivateInputField(); return; }
        if (string.IsNullOrEmpty(pw)) { ShowMsg("��й�ȣ�� �Է��ϼ���."); pwField.Select(); pwField.ActivateInputField(); return; }

        SetInteractable(false);
        StartCoroutine(CoLogin(id, pw));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // 2�� �� �ı�
    }

    IEnumerator CoLogin(string id, string pw)
    {
        ShowMsg("Ȯ�� ��...");
        yield return null; // �� ������ �纸(�ε� ����)
        bool ok = UserDatabase.Validate(id, pw);

        if (ok)
        {
            ShowMsg("�α��� ����!");
            PlayerPrefs.SetString("userId", id);
            if (GameManager.Instance != null)
                GameManager.Instance.userId = id;
            yield return new WaitForSeconds(0.15f);
            SceneManager.LoadScene("MainScene");
            DontDestroyOnLoad(gameObject); // ����� �ٲ� ����
            StartCoroutine(DestroyAfterDelay(2f)); // 2�� �� �ı�
        }
        else
        {
            ShowMsg("ID �Ǵ� ��й�ȣ�� �ùٸ��� �ʽ��ϴ�.");
            pwField.text = "";
            pwField.Select();
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
        bool isActive = optionWindow.activeSelf;
        optionWindow.SetActive(!isActive); // ���� ������ �ݰ�, ���� ������ ����
    }

    void PrepareInput(InputField f, bool isPassword)
    {
        // �ؽ�Ʈ ������Ʈ(���� ǥ�ÿ�)
        var tc = f.textComponent;                  // (Legacy) Text
        if (tc != null)
        {
            if (baeminFont) tc.font = baeminFont;  // ��Ʈ ����
            tc.alignment = TextAnchor.MiddleLeft;  // ���� ���� ����
            tc.supportRichText = false;            // ��ġ�ؽ�Ʈ ����
            tc.resizeTextForBestFit = false;       // �ڵ� ũ�� OFF(�ٹٲ�/���� ���� ����)
            if (tc.color.a < 0.99f) tc.color = new Color(tc.color.r, tc.color.g, tc.color.b, 1f); // ���� ����
        }

        // �÷��̽�Ȧ��
        var ph = f.placeholder as Text;            // (Legacy) Text
        if (ph != null)
        {
            if (baeminFont) ph.font = baeminFont;
            ph.alignment = TextAnchor.MiddleLeft;
            var c = ph.color; c.a = 0.5f; ph.color = c;  // ������
            ph.supportRichText = false;
            ph.resizeTextForBestFit = false;
        }

        // �Է� �ʵ� ����
        f.lineType = InputField.LineType.SingleLine;
        f.textComponent.horizontalOverflow = HorizontalWrapMode.Overflow; // ���� ��ũ�� ���
        f.textComponent.verticalOverflow = VerticalWrapMode.Truncate;

        if (isPassword)
        {
            f.contentType = InputField.ContentType.Password;
            f.asteriskChar = '?'; // ��Ʈ�� '?' �۸����� ������ '*'�� �ٲټ���.
        }
        else
        {
            f.contentType = InputField.ContentType.Standard;
        }

        // Ŀ��/���� ��(���ü�)
        f.caretBlinkRate = 0.85f;
        f.caretColor = new Color(0, 0, 0, 1);
        f.selectionColor = new Color(0.25f, 0.5f, 1f, 0.35f);
    }

    void WireClickToFocus(Graphic clickable, InputField target)
    {
        if (clickable == null || target == null) return;
        var btn = clickable.GetComponent<Button>();
        if (btn == null) btn = clickable.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None; // �ð�ȿ�� ���ʿ��ϸ� None
        btn.onClick.AddListener(() =>
        {
            target.Select();
            target.ActivateInputField();
        });
    }

}