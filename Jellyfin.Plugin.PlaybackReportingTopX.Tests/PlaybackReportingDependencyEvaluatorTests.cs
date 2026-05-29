using Jellyfin.Plugin.PlaybackReportingTopX;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Tests;

public class PlaybackReportingDependencyEvaluatorTests
{
    [Fact]
    public void Evaluate_ReturnsNotInstalledWhenPluginMissing()
    {
        var result = PlaybackReportingDependencyEvaluator.Evaluate(
            isInstalled: false,
            isPluginActive: false,
            databaseExists: false);

        Assert.Equal(PlaybackReportingDependencyState.NotInstalled, result.State);
        Assert.False(result.IsReady);
    }

    [Theory]
    [InlineData("Disabled")]
    [InlineData("Malfunctioned")]
    [InlineData("NotSupported")]
    public void Evaluate_ReturnsNotActiveWhenPluginIsNotRunning(string statusLabel)
    {
        var result = PlaybackReportingDependencyEvaluator.Evaluate(
            isInstalled: true,
            isPluginActive: false,
            databaseExists: true,
            inactiveStatusLabel: statusLabel);

        Assert.Equal(PlaybackReportingDependencyState.NotActive, result.State);
        Assert.False(result.IsReady);
        Assert.Contains(statusLabel, result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Evaluate_ReturnsDatabaseMissingWhenDatabaseIsAbsent()
    {
        var result = PlaybackReportingDependencyEvaluator.Evaluate(
            isInstalled: true,
            isPluginActive: true,
            databaseExists: false);

        Assert.Equal(PlaybackReportingDependencyState.DatabaseMissing, result.State);
        Assert.False(result.IsReady);
    }

    [Fact]
    public void Evaluate_ReturnsReadyWhenDependencyIsAvailable()
    {
        var result = PlaybackReportingDependencyEvaluator.Evaluate(
            isInstalled: true,
            isPluginActive: true,
            databaseExists: true);

        Assert.Equal(PlaybackReportingDependencyState.Ready, result.State);
        Assert.True(result.IsReady);
    }
}
