using NexaTweaks.Core;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

public class ShellCommandTweakTests
{
    private static ShellCommandTweak MakeTweak(string applyCommand, string revertCommand = "cmd /c exit 0") => new()
    {
        Id = "test.shell",
        Name = "test",
        Description = "test",
        Category = TweakCategory.Windows,
        Risk = RiskLevel.Safe,
        DefaultEnabled = false,
        CaptureState = () => null,
        ApplyCommand = () => applyCommand,
        RevertCommand = _ => revertCommand,
    };

    [Fact]
    public void Apply_WhenCommandExitsZero_Succeeds()
    {
        var tweak = MakeTweak("cmd /c exit 0");

        var entry = tweak.Apply();

        Assert.NotNull(entry);
    }

    [Fact]
    public void Apply_WhenCommandExitsNonZero_ThrowsInsteadOfSilentlySucceeding()
    {
        var tweak = MakeTweak("cmd /c exit 1");

        var ex = Assert.Throws<InvalidOperationException>(() => tweak.Apply());
        Assert.Contains("1", ex.Message);
    }

    [Fact]
    public void Revert_WhenCommandExitsNonZero_ThrowsInsteadOfSilentlySucceeding()
    {
        var tweak = MakeTweak("cmd /c exit 0", revertCommand: "cmd /c exit 1");
        var entry = tweak.Apply();

        Assert.Throws<InvalidOperationException>(() => tweak.Revert(entry));
    }

    [Fact]
    public void ExecCapture_ReturnsStandardOutput()
    {
        var output = ShellCommandTweak.ExecCapture("cmd /c echo hola");

        Assert.Contains("hola", output);
    }
}
