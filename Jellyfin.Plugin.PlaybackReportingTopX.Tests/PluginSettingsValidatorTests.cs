using Jellyfin.Plugin.PlaybackReportingTopX;

namespace Jellyfin.Plugin.PlaybackReportingTopX.Tests;

public class PluginSettingsValidatorTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    [InlineData(500, 100)]
    public void NormalizeTopCount_ClampsToSafeRange(int input, int expected)
    {
        Assert.Equal(expected, PluginSettingsValidator.NormalizeTopCount(input));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(30, 30)]
    [InlineData(3650, 3650)]
    [InlineData(99999, 3650)]
    public void NormalizeDaysToLookBack_ClampsToSafeRange(int input, int expected)
    {
        Assert.Equal(expected, PluginSettingsValidator.NormalizeDaysToLookBack(input));
    }

    [Fact]
    public void NormalizeCollectionName_UsesFallbackForBlankValues()
    {
        Assert.Equal("Fallback", PluginSettingsValidator.NormalizeCollectionName("   ", "Fallback"));
        Assert.Equal("Fallback", PluginSettingsValidator.NormalizeCollectionName(null, "Fallback"));
    }

    [Fact]
    public void NormalizeCollectionName_TrimsAndTruncates()
    {
        var longName = new string('A', 300);

        var normalized = PluginSettingsValidator.NormalizeCollectionName($"  {longName}  ", "Fallback");

        Assert.Equal(PluginSettingsValidator.MaxCollectionNameLength, normalized.Length);
        Assert.StartsWith("A", normalized, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(10, 2000)]
    [InlineData(100, 10000)]
    [InlineData(200, 10000)]
    public void GetEpisodeScanLimit_CapsScanVolume(int topCount, int expected)
    {
        Assert.Equal(expected, PluginSettingsValidator.GetEpisodeScanLimit(topCount));
    }
}
