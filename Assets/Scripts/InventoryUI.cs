using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryUI : MonoBehaviour
{
    public Transform listRoot;     // ���� ���� ���ξ�
    public GameObject linePrefab;  // �ؽ�Ʈ 2��¥�� ������(�̸�/ȿ��)
    public Text summaryText;       // ���� �ɷ�ġ ���

    void OnEnable() { Refresh(); }

    public void Refresh()
    {
        foreach (Transform c in listRoot) Destroy(c.gameObject);
        var inv = Inventory.Instance;
        foreach (var it in inv.purchased)
        {
            var go = Instantiate(linePrefab, listRoot);
            var texts = go.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.name.Contains("Name")) t.text = it.displayName;
                else if (t.name.Contains("Effect")) t.text = $"+{it.amount} {it.stat}";
            }
        }

        var s = GameManager.Instance.stats;
        if (summaryText)
            summaryText.text = $"HP {s.hp}/{s.maxHp}  MP {s.mp}/{s.maxMp}  ATK {s.atk}  DEF {s.def}  DEX {s.dex}";
    }
}
