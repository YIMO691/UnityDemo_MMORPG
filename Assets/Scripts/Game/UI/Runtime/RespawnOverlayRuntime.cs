using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RespawnOverlayRuntime : MonoBehaviour
{
    private static RespawnOverlayRuntime instance;
    public static RespawnOverlayRuntime Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("RespawnOverlayRuntime");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<RespawnOverlayRuntime>();
            }
            return instance;
        }
    }

    private Canvas canvas;
    private Image blackImage;
    private Text countdownText;
    private Button respawnButton;
    private System.Action pendingCallback;

    private Coroutine running;

    private void EnsureUI()
    {
        if (canvas != null) return;

        var canvasGo = new GameObject("RespawnOverlayCanvas");
        canvasGo.transform.SetParent(transform, false);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var imgGo = new GameObject("Black");
        imgGo.transform.SetParent(canvasGo.transform, false);
        blackImage = imgGo.AddComponent<Image>();
        blackImage.color = new Color(0f, 0f, 0f, 0f);
        var imgRt = (RectTransform)imgGo.transform;
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.offsetMin = Vector2.zero;
        imgRt.offsetMax = Vector2.zero;

        var txtGo = new GameObject("Countdown");
        txtGo.transform.SetParent(canvasGo.transform, false);
        countdownText = txtGo.AddComponent<Text>();
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = new Color(1f, 1f, 1f, 0f);
        countdownText.fontSize = 36;
        countdownText.text = "";
        countdownText.font = ResolveDefaultFont();
        var txtRt = (RectTransform)txtGo.transform;
        txtRt.anchorMin = new Vector2(0.5f, 0.5f);
        txtRt.anchorMax = new Vector2(0.5f, 0.5f);
        txtRt.anchoredPosition = Vector2.zero;
        txtRt.sizeDelta = new Vector2(800f, 120f);

        var btnGo = new GameObject("RespawnButton");
        btnGo.transform.SetParent(canvasGo.transform, false);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(1f, 1f, 1f, 0.1f);
        respawnButton = btnGo.AddComponent<Button>();
        var btnRt = (RectTransform)btnGo.transform;
        btnRt.sizeDelta = new Vector2(260f, 60f);
        btnRt.anchorMin = new Vector2(0.5f, 0.3f);
        btnRt.anchorMax = new Vector2(0.5f, 0.3f);
        btnRt.anchoredPosition = Vector2.zero;

        var btnTextGo = new GameObject("Label");
        btnTextGo.transform.SetParent(btnGo.transform, false);
        var btnText = btnTextGo.AddComponent<Text>();
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(1f, 1f, 1f, 0.9f);
        btnText.fontSize = 28;
        btnText.text = "立即复活";
        btnText.font = ResolveDefaultFont();
        var btnTextRt = (RectTransform)btnTextGo.transform;
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        canvasGo.SetActive(false);
    }

    private Font ResolveDefaultFont()
    {
        Font f = null;
        try
        {
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch { }
        if (f == null)
        {
            try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        }
        return f;
    }

    public void Show(float durationSeconds, System.Action onCountdownFinished, string messagePrefix = "自动复活倒计时 ")
    {
        EnsureUI();
        pendingCallback = onCountdownFinished;
        respawnButton.onClick.RemoveAllListeners();
        respawnButton.onClick.AddListener(() =>
        {
            var cb = pendingCallback;
            pendingCallback = null;
            if (running != null)
            {
                StopCoroutine(running);
                running = null;
            }
            cb?.Invoke();
        });
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
        canvas.gameObject.SetActive(true);
        running = StartCoroutine(OverlayRoutine(durationSeconds, onCountdownFinished, messagePrefix));
    }

    public void Hide()
    {
        EnsureUI();
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
        canvas.gameObject.SetActive(false);
    }

    private IEnumerator OverlayRoutine(float duration, System.Action onFinished, string prefix)
    {
        float targetAlpha = 0.85f;
        float fade = 0f;
        while (fade < targetAlpha)
        {
            fade = Mathf.Min(targetAlpha, fade + Time.deltaTime * 2f);
            blackImage.color = new Color(0f, 0f, 0f, fade);
            countdownText.color = new Color(1f, 1f, 1f, Mathf.Clamp01((fade - 0.2f) / 0.65f));
            yield return null;
        }

        float remain = Mathf.Max(0f, duration);
        while (remain > 0f)
        {
            int seconds = Mathf.CeilToInt(remain);
            countdownText.text = prefix + seconds + "s";
            remain -= Time.deltaTime;
            yield return null;
        }

        countdownText.text = "";
        onFinished?.Invoke();
    }
}
