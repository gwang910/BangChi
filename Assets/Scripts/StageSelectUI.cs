using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageSelectUI : MonoBehaviour
{
    public Dropdown dropdown;  // Canvas�� �ִ� uGUI Dropdown
    public Text infoText;      // "���� �� �̵�" ���� �ȳ� ����(�ɼ�)

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
        // ���� ���������� ���ð� ���߱�
        dropdown.value = Mathf.Clamp(GameManager.Instance.stats.stage, 0, max);
        dropdown.RefreshShownValue();

        if (infoText)
            infoText.text = $"���� ����: 0 ~ {max}";
    }

    // Dropdown�� OnValueChanged�� ����
    public void OnSelectChanged(int index)
    {
        // �̸����⸸ �ϰ� �̵��� ��ư���� �Ϸ��� �� �Լ����� �ƹ� �͵� �� �ص� ��
        // ��� �̵��Ϸ��� �Ʒ� ���� ���:
        GameManager.Instance.GoToStage(index);
        // ���� �� ���� ������ �ٽ� ����(�ɼ�)
        BuildOptions();
    }

    // '�̵�' ��ư�� ���� �� ���:
    public void OnClickGo()
    {
        GameManager.Instance.GoToStage(dropdown.value);
        BuildOptions();
    }
}
