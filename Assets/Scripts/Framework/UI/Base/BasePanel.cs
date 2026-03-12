using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel : MonoBehaviour
{
    protected CanvasGroup canvasGroup;

    [Header("淡入淡出速度")]
    [SerializeField] private float alphaSpeed = 10f;

    public bool IsVisible { get; private set; }
    public bool IsInitialized { get; private set; }

    private UnityAction hideCallBack;

    public virtual UILayer Layer => UILayer.Normal;

    // 添加三个属性，分别控制是否使用遮罩、点击遮罩是否关闭面板、面板的排序层级
    public virtual bool UseMask => Layer == UILayer.Popup;
    public virtual bool CloseByMask => false;
    public virtual int SortOrder => 0;


    protected enum PanelState
    {
        None,
        Showing,
        Shown,
        Hiding,
        Hidden
    }

    protected PanelState State { get; private set; } = PanelState.None;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        State = PanelState.Hidden;
        IsVisible = false;
    }

    protected virtual void Update()
    {
        UpdateFade();
    }

    /// <summary>
    /// 只在首次创建时初始化一次
    /// </summary>
    public void Create()
    {
        if (IsInitialized) return;

        IsInitialized = true;
        OnCreate();
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    public virtual void ShowMe()
    {
        Create();

        gameObject.SetActive(true);

        IsVisible = true;
        State = PanelState.Showing;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        OnShow();
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public virtual void HideMe(UnityAction callBack = null)
    {
        if (!gameObject.activeSelf)
        {
            callBack?.Invoke();
            return;
        }

        hideCallBack = callBack;

        IsVisible = false;
        State = PanelState.Hiding;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        OnHide();
    }

    // 方便后期页面之间的跳转，直接隐藏不播放动画
    public virtual void HideImmediately()
    {
        hideCallBack = null;

        IsVisible = false;
        State = PanelState.Hidden;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        OnHide();
        OnHideComplete();

        gameObject.SetActive(false);
    }


    /// <summary>
    /// 外部主动请求刷新
    /// </summary>
    public virtual void Refresh()
    {
        OnRefresh();
    }

    /// <summary>
    /// 销毁前调用
    /// </summary>
    public virtual void DestroyPanel()
    {
        OnDestroyPanel();
        Destroy(gameObject);
    }

    private void UpdateFade()
    {
        if (State == PanelState.Showing)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1f)
            {
                canvasGroup.alpha = 1f;
                State = PanelState.Shown;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                OnShowComplete();
            }
        }
        else if (State == PanelState.Hiding)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0f)
            {
                canvasGroup.alpha = 0f;
                State = PanelState.Hidden;

                OnHideComplete();

                hideCallBack?.Invoke();
                hideCallBack = null;

                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 首次创建时调用一次：缓存组件、注册按钮事件等
    /// </summary>
    protected abstract void OnCreate();

    /// <summary>
    /// 每次显示时调用：刷新显示、读取数据、更新UI
    /// </summary>
    protected virtual void OnShow() { }

    /// <summary>
    /// 每次隐藏时调用：停止监听、关闭子界面、保存状态等
    /// </summary>
    protected virtual void OnHide() { }

    /// <summary>
    /// 需要手动刷新时调用
    /// </summary>
    protected virtual void OnRefresh() { }

    /// <summary>
    /// 显示动画完成后调用
    /// </summary>
    protected virtual void OnShowComplete() { }

    /// <summary>
    /// 隐藏动画完成后调用
    /// </summary>
    protected virtual void OnHideComplete() { }

    /// <summary>
    /// 销毁前调用
    /// </summary>
    protected virtual void OnDestroyPanel() { }
}
