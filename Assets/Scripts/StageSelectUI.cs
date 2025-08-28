using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageSelectUI : MonoBehaviour
{
    public Dropdown dropdown;  // Canvas에 있는 uGUI Dropdown
    public Text infoText;      // "선택 후 이동" 같은 안내 문구(옵션)

    void OnEnable()
    {
        BuildOptions();
    }

    public void BuildOptions()
    {
        if (GameManager.Instance == null || dropdown == null) return;

        int max = GameManager.Instance.MaxSelectableStage;

        var opts = new List<Dropdown.OptionData>();
        for (int i = 0; i <= max; i++)
            opts.Add(new Dropdown.OptionData($"Stage {i:00}"));

        dropdown.options = opts;
        // 현재 스테이지로 선택값 맞추기
        dropdown.value = Mathf.Clamp(GameManager.Instance.stats.stage, 0, max);
        dropdown.RefreshShownValue();

        if (infoText)
            infoText.text = $"선택 가능: 0 ~ {max}";
    }

    // Dropdown의 OnValueChanged에 연결
    public void OnSelectChanged(int index)
    {
        // 미리보기만 하고 이동은 버튼으로 하려면 이 함수에서 아무 것도 안 해도 됨
        // 즉시 이동하려면 아래 줄을 사용:
        GameManager.Instance.GoToStage(index);
        // 선택 후 현재 값으로 다시 구성(옵션)
        BuildOptions();
    }

    // '이동' 버튼을 따로 둘 경우:
    public void OnClickGo()
    {
        GameManager.Instance.GoToStage(dropdown.value);
        BuildOptions();
    }
}
