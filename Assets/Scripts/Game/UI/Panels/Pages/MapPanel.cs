using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapPanel : BasePanel
{
    public override UILayer Layer => UILayer.Normal;

    [SerializeField] private TMP_Text txtMapName;
    [SerializeField] private Image imgMap;

    protected override void OnShow()
    {
        base.OnShow();
        Refresh();
    }

    private void Refresh()
    {
        var config = MapService.Instance.GetCurrentMapConfig();

        if (config == null)
        {
            if (txtMapName != null) txtMapName.text = "未知地图";
            if (imgMap != null) imgMap.sprite = null;
            return;
        }

        if (txtMapName != null) txtMapName.text = config.displayName;

        if (imgMap != null && !string.IsNullOrEmpty(config.mapImage))
        {
            Sprite sp = ResourceManager.Instance.Load<Sprite>("Map/" + config.mapImage);
            imgMap.sprite = sp;
        }
    }
}
