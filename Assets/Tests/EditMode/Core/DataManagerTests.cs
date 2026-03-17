using NUnit.Framework;

public class DataManagerTests
{
    [SetUp]
    public void SetUp()
    {
        DataManager.Instance.Init();
    }

    [Test]
    public void LoadPlayerDataFromSlot_InvalidSlot_ReturnsFalse()
    {
        bool ok = DataManager.Instance.LoadPlayerDataFromSlot(-1);
        Assert.IsFalse(ok);
    }

    [Test]
    public void GetPlayerDataFromSlot_InvalidSlot_ReturnsNull()
    {
        var data = DataManager.Instance.GetPlayerDataFromSlot(0);
        Assert.IsNull(data);
    }

    [Test]
    public void DeletePlayerDataInSlot_InvalidSlot_NoException()
    {
        Assert.DoesNotThrow(() => DataManager.Instance.DeletePlayerDataInSlot(0));
    }
}
