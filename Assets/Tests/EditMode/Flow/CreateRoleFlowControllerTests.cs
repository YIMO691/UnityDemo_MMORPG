using NUnit.Framework;

public class CreateRoleFlowControllerTests
{
    [SetUp]
    public void SetUp()
    {
        EventBus.Clear();
        CreateRoleFlowController.Instance.Init();
        Game.Runtime.GameRuntime.CurrentPlayerData = null;
        Game.Runtime.GameRuntime.CurrentSlotId = 0;
    }

    [Test]
    public void PublishNullRequest_DoesNotSetGameRuntime()
    {
        EventBus.Publish(new CreateRoleRequestEvent(default));
        Assert.IsNull(Game.Runtime.GameRuntime.CurrentPlayerData);
        Assert.AreEqual(0, Game.Runtime.GameRuntime.CurrentSlotId);
    }
}
