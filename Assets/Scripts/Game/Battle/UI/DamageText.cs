using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour, IPoolable
{
    [SerializeField] private TMP_Text txtValue;
    [SerializeField] private CanvasGroup canvasGroup;

    private float lifeTime;
    private float moveSpeed;
    private Vector3 velocity;

    public void Play(int value, Vector3 worldPos, float duration = 0.8f, float speed = 40f)
    {
        if (txtValue != null) txtValue.text = value.ToString();
        transform.position = worldPos;
        lifeTime = duration;
        moveSpeed = speed;
        velocity = Vector3.up * speed;
    }

    private void Update()
    {
        if (lifeTime <= 0f) return;
        lifeTime -= Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(lifeTime);
        if (lifeTime <= 0f) PoolManager.Instance.Recycle(gameObject);
    }

    public void OnSpawnFromPool()
    {
        lifeTime = 0f;
        velocity = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (txtValue != null) txtValue.text = string.Empty;
    }

    public void OnRecycleToPool()
    {
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (txtValue != null) txtValue.text = string.Empty;
    }
}
