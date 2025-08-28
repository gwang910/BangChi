using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Catalog")]
    public StatUpgradeSO[] items;

    [Header("UI Refs")]
    public Transform listRoot;     // ������ ��ư���� ���� �θ�
    public GameObject itemPrefab;  // ������: ��ư + �ؽ�Ʈ 2~3�� (�̸�/�ڽ�Ʈ)

    public Text goldText;

    void OnEnable() { Refresh(); }
    public void Refresh()
    {
        if (goldText) goldText.text = $"Gold: {GameManager.Instance.gold}";

        // �ſ� �ܼ��ϰ� �Ź� �����(������ �� ���ٸ� ok)
        foreach (Transform c in listRoot) Destroy(c.gameObject);
        foreach (var it in items)
        {
            var go = Instantiate(itemPrefab, listRoot); // �˾Ƽ� ���� ��ġ��
            var texts = go.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.name.Contains("Name")) t.text = it.displayName;
                else if (t.name.Contains("Cost")) t.text = $"{it.cost}";
                else if (t.name.Contains("Desc")) t.text = it.description;
            }

            var btn = go.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => { TryBuy(it); });
        }
    }

    void TryBuy(StatUpgradeSO item)
    {
        if (item == null) { Debug.LogError("[ShopUI] item null"); return; }
        if (Inventory.Instance == null) { Debug.LogError("[ShopUI] Inventory.Instance null"); return; }

        if (Inventory.Instance.Buy(item))
            Refresh();
        else
            Debug.Log("��� ����");
    }
}