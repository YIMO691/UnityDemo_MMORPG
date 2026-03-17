using NUnit.Framework;

public class UIManagerTests
{
    [Test]
    public void Init_Twice_IsInitedTrue()
    {
        UIManager.Instance.Init();
        UIManager.Instance.Init();
        Assert.IsTrue(GetIsInited());
    }

    [Test]
    public void IsPanelVisible_Unknown_ReturnsFalse()
    {
        UIManager.Instance.Init();
        bool visible = UIManager.Instance.IsPanelVisible("UnknownPanel");
        Assert.IsFalse(visible);
    }

    private bool GetIsInited()
    {
        return typeof(UIManager).GetField("isInited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null
            ? (bool)typeof(UIManager).GetField("isInited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(UIManager.Instance)
            : false;
    }
}
