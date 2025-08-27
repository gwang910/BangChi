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

        UserDatabase.EnsureReady(); // 첫 실행 준비
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

        if (string.IsNullOrEmpty(id)) { ShowMsg("ID를 입력하세요."); idField.ActivateInputField(); return; }
        if (string.IsNullOrEmpty(pw)) { ShowMsg("비밀번호를 입력하세요."); pwField.ActivateInputField(); return; }

        SetInteractable(false);
        StartCoroutine(CoLogin(id, pw));
    }

    IEnumerator CoLogin(string id, string pw)
    {
        ShowMsg("확인 중...");
        yield return null; // 한 프레임 양보(로딩 느낌)
        bool ok = UserDatabase.Validate(id, pw);

        if (ok)
        {
            ShowMsg("로그인 성공!");
            // PlayerPrefs.SetString("userId", id); // 민감정보 저장 지양
            yield return new WaitForSeconds(0.15f);
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            ShowMsg("ID 또는 비밀번호가 올바르지 않습니다.");
            pwField.text = "";
            pwField.ActivateInputField();
            SetInteractable(true);
        }
    }

    // 회원가입 버튼 → 로컬 JSON에 계정 추가
    void OnClickMakeId()
    {
        string id = idField.text.Trim();
        string pw = pwField.text;
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            ShowMsg("ID/비밀번호를 입력한 후 다시 누르세요.");
            return;
        }
        if (UserDatabase.Exists(id))
        {
            ShowMsg("이미 존재하는 ID입니다.");
            return;
        }
        bool ok = UserDatabase.Register(id, pw);
        ShowMsg(ok ? "회원가입 완료! 이제 로그인하세요." : "회원가입 실패.");
    }

    void OnClickOption()
    {
        ShowMsg("옵션 열기(미구현)");
    }
}