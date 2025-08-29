using UnityEngine;

public class WorldClickFX : MonoBehaviour
{
    public LayerMask hitMask;                // Ground/Enemy ��
    public ParticleSystem worldFxPrefab;     // ����� ��ƼŬ ������
    public AudioClip worldClickSFX;          // ���� Ŭ����
    public float maxDist = 100f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var cam = Camera.main;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, maxDist, hitMask))
            {
                // ��ƼŬ
                if (worldFxPrefab)
                {
                    var fx = Instantiate(worldFxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetimeMultiplier);
                }
                // ����
                if (AudioManager.Instance) AudioManager.Instance.PlayAt(worldClickSFX, hit.point, 1f);
            }
        }
    }
}
