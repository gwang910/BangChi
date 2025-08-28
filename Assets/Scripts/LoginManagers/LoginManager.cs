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

    public GameObject optionWindow;  // 옵션창 패널

    void Awake()
    {
        loginButton.onClick.AddListener(OnClickLogin);
        makeIdButton.onClick.AddListener(OnClickMakeId);
        optionButton.onClick.AddListener(OnClickOption);

        // 입력칸 스타일/동작 준비
        PrepareInput(idField, isPassword: false);
        PrepareInput(pwField, isPassword: true);
        optionWindow.SetActive(false); // 시작 시 닫힘 상태

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

        if (string.IsNullOrEmpty(id)) { ShowMsg("ID를 입력하세요."); idField.Select(); idField.ActivateInputField(); return; }
        if (string.IsNullOrEmpty(pw)) { ShowMsg("비밀번호를 입력하세요."); pwField.Select(); pwField.ActivateInputField(); return; }

        SetInteractable(false);
        StartCoroutine(CoLogin(id, pw));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // 2초 후 파괴
    }

    IEnumerator CoLogin(string id, string pw)
    {
        ShowMsg("확인 중...");
        yield return null; // 한 프레임 양보(로딩 느낌)
        bool ok = UserDatabase.Validate(id, pw);

        if (ok)
        {
            ShowMsg("로그인 성공!");
            PlayerPrefs.SetString("userId", id);
            if (GameManager.Instance != null)
                GameManager.Instance.userId = id;
            yield return new WaitForSeconds(0.15f);
            SceneManager.LoadScene("MainScene");
            DontDestroyOnLoad(gameObject); // 장면이 바뀌어도 유지
            StartCoroutine(DestroyAfterDelay(2f)); // 2초 뒤 파괴
        }
        else
        {
            ShowMsg("ID 또는 비밀번호가 올바르지 않습니다.");
            pwField.text = "";
            pwField.Select();
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
        bool isActive = optionWindow.activeSelf;
        optionWindow.SetActive(!isActive); // 열려 있으면 닫고, 닫혀 있으면 열기
    }

    void PrepareInput(InputField f, bool isPassword)
    {
        // 텍스트 컴포넌트(내용 표시용)
        var tc = f.textComponent;                  // (Legacy) Text
        if (tc != null)
        {
            if (baeminFont) tc.font = baeminFont;  // 폰트 지정
            tc.alignment = TextAnchor.MiddleLeft;  // 왼쪽 정렬 강제
            tc.supportRichText = false;            // 리치텍스트 비사용
            tc.resizeTextForBestFit = false;       // 자동 크기 OFF(줄바꿈/정렬 꼬임 방지)
            if (tc.color.a < 0.99f) tc.color = new Color(tc.color.r, tc.color.g, tc.color.b, 1f); // 투명 방지
        }

        // 플레이스홀더
        var ph = f.placeholder as Text;            // (Legacy) Text
        if (ph != null)
        {
            if (baeminFont) ph.font = baeminFont;
            ph.alignment = TextAnchor.MiddleLeft;
            var c = ph.color; c.a = 0.5f; ph.color = c;  // 반투명
            ph.supportRichText = false;
            ph.resizeTextForBestFit = false;
        }

        // 입력 필드 동작
        f.lineType = InputField.LineType.SingleLine;
        f.textComponent.horizontalOverflow = HorizontalWrapMode.Overflow; // 가로 스크롤 허용
        f.textComponent.verticalOverflow = VerticalWrapMode.Truncate;

        if (isPassword)
        {
            f.contentType = InputField.ContentType.Password;
            f.asteriskChar = '?'; // 폰트에 '?' 글리프가 없으면 '*'로 바꾸세요.
        }
        else
        {
            f.contentType = InputField.ContentType.Standard;
        }

        // 커서/선택 색(가시성)
        f.caretBlinkRate = 0.85f;
        f.caretColor = new Color(0, 0, 0, 1);
        f.selectionColor = new Color(0.25f, 0.5f, 1f, 0.35f);
    }

    void WireClickToFocus(Graphic clickable, InputField target)
    {
        if (clickable == null || target == null) return;
        var btn = clickable.GetComponent<Button>();
        if (btn == null) btn = clickable.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None; // 시각효과 불필요하면 None
        btn.onClick.AddListener(() =>
        {
            target.Select();
            target.ActivateInputField();
        });
    }

}