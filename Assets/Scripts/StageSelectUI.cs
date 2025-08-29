using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageSelectUI : MonoBehaviour
{
    public Dropdown dropdown;  // Canvas에 있는 uGUI Dropdown
    public Text infoText;      // "선택 후 이동" 같은 안내 문구(옵션)

    int pendingIndex;     // 사용자가 고른 값 임시 저장
    bool building;        // 옵션 구성 중에는 이벤트 무시

    void OnEnable()
    {
        BuildOptions();
    }

    public void BuildOptions()
    {
        if (!dropdown || GameManager.Instance == null) return;
        building = true;

        int max = GameManager.Instance.MaxSelectableStage;

        dropdown.options = new List<Dropdown.OptionData>();
        for (int i = 0; i <= max; i++)
            dropdown.options.Add(new Dropdown.OptionData($"Stage {i:00}"));

        // 현재 스테이지로 초기 선택 맞추기
        dropdown.value = Mathf.Clamp(GameManager.Instance.stats.stage, 0, max);
        dropdown.RefreshShownValue();

        pendingIndex = dropdown.value;
        if (infoText) infoText.text = $"선택 가능: 0 ~ {max}";

        building = false;
    }

    // Dropdown의 OnValueChanged에 연결
    public void OnSelectChanged(int index)
    {
        if (building) return;       // 옵션 구성 중엔 무시
        pendingIndex = index;       // 일단 값만 저장
    }

    // '이동' 버튼을 따로 둘 경우:
    public void OnClickGo()
    {
        GameManager.Instance.GoToStage(dropdown.value);
        BuildOptions();
    }
}
