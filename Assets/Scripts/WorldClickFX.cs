using UnityEngine;

public class WorldClickFX : MonoBehaviour
{
    public LayerMask hitMask;                // Ground/Enemy 등
    public ParticleSystem worldFxPrefab;     // 월드용 파티클 프리팹
    public AudioClip worldClickSFX;          // 월드 클릭음
    public float maxDist = 100f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var cam = Camera.main;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, maxDist, hitMask))
            {
                // 파티클
                if (worldFxPrefab)
                {
                    var fx = Instantiate(worldFxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetimeMultiplier);
                }
                // 사운드
                if (AudioManager.Instance) AudioManager.Instance.PlayAt(worldClickSFX, hit.point, 1f);
            }
        }
    }
}
