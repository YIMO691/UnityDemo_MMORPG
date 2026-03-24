using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour, IPoolable
{
    [SerializeField] private TMP_Text txtValue;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float defaultRiseDistance = 1.2f;
    [SerializeField] private float defaultSpeed = 0.8f;
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private bool yAxisOnly = true;
    private float riseDistance;
    private float speed;
    private float moved;
    private Vector3 startPosition;

    public void Play(int value, Vector3 worldPos, float rise = -1f, float spd = -1f)
    {
        if (txtValue != null) txtValue.text = value.ToString();
        transform.position = worldPos;
        riseDistance = rise > 0f ? rise : defaultRiseDistance;
        speed = spd > 0f ? spd : defaultSpeed;
        moved = 0f;
        startPosition = worldPos;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (riseDistance <= 0f) return;
        float delta = speed * Time.deltaTime;
        transform.position += Vector3.up * delta;
        moved += delta;
        if (faceCamera)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                if (yAxisOnly)
                {
                    Vector3 dir = transform.position - cam.transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                }
                else
                {
                    Vector3 toCam = transform.position - cam.transform.position;
                    transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
                }
            }
        }
        if (canvasGroup != null)
        {
            float t = Mathf.Clamp01(1f - moved / riseDistance);
            canvasGroup.alpha = t;
        }
        if (moved >= riseDistance)
        {
            PoolManager.Instance.Recycle(gameObject);
        }
    }

    public void OnSpawnFromPool()
    {
        moved = 0f;
        riseDistance = defaultRiseDistance;
        speed = defaultSpeed;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (txtValue != null) txtValue.text = string.Empty;
    }

    public void OnRecycleToPool()
    {
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (txtValue != null) txtValue.text = string.Empty;
    }
}
