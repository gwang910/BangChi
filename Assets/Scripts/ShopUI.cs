using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Catalog")]
    public StatUpgradeSO[] items;

    [Header("UI Refs")]
    public Transform listRoot;     // 아이템 버튼들이 놓일 부모
    public GameObject itemPrefab;  // 프리팹: 버튼 + 텍스트 2~3개 (이름/코스트)

    public Text goldText;

    void OnEnable() { Refresh(); }
    public void Refresh()
    {
        if (goldText) goldText.text = $"Gold: {GameManager.Instance.gold}";

        // 매우 단순하게 매번 재생성(아이템 수 적다면 ok)
        foreach (Transform c in listRoot) Destroy(c.gameObject);
        foreach (var it in items)
        {
            var go = Instantiate(itemPrefab, listRoot); // 알아서 세로 배치됨
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
            Debug.Log("골드 부족");
    }
}