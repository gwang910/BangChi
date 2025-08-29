using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageSelectUI : MonoBehaviour
{
    public Dropdown dropdown;  // Canvas�� �ִ� uGUI Dropdown
    public Text infoText;      // "���� �� �̵�" ���� �ȳ� ����(�ɼ�)

    int pendingIndex;     // ����ڰ� �� �� �ӽ� ����
    bool building;        // �ɼ� ���� �߿��� �̺�Ʈ ����

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

        // ���� ���������� �ʱ� ���� ���߱�
        dropdown.value = Mathf.Clamp(GameManager.Instance.stats.stage, 0, max);
        dropdown.RefreshShownValue();

        pendingIndex = dropdown.value;
        if (infoText) infoText.text = $"���� ����: 0 ~ {max}";

        building = false;
    }

    // Dropdown�� OnValueChanged�� ����
    public void OnSelectChanged(int index)
    {
        if (building) return;       // �ɼ� ���� �߿� ����
        pendingIndex = index;       // �ϴ� ���� ����
    }

    // '�̵�' ��ư�� ���� �� ���:
    public void OnClickGo()
    {
        GameManager.Instance.GoToStage(dropdown.value);
        BuildOptions();
    }
}
