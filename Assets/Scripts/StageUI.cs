using UnityEngine;
using UnityEngine.UI;

public class StageUI : MonoBehaviour
{
    public void GoToPrev() => GameManager.Instance.GoToStage(GameManager.Instance.stats.stage - 1);
    public void GoToNext() => GameManager.Instance.GoToStage(GameManager.Instance.stats.stage + 1);
    public void GoTo(int index) => GameManager.Instance.GoToStage(index);
}
