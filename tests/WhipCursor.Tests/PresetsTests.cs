namespace WhipCursor.Tests;

public class PresetsTests
{
    [Fact]
    public void HigherSensitivity_MeansLowerThreshold()
    {
        Assert.True(Presets.CrackThresholdDip(CrackSensitivity.High) < Presets.CrackThresholdDip(CrackSensitivity.Medium));
        Assert.True(Presets.CrackThresholdDip(CrackSensitivity.Medium) < Presets.CrackThresholdDip(CrackSensitivity.Low));
    }

    [Fact]
    public void Lengths_MatchSpec()
    {
        Assert.Equal(80f, Presets.LengthDip(WhipLength.Short));
        Assert.Equal(120f, Presets.LengthDip(WhipLength.Medium));
        Assert.Equal(180f, Presets.LengthDip(WhipLength.Long));
    }

    [Fact]
    public void Volume_MuteIsSilent_AndLevelsIncrease()
    {
        Assert.Equal(0f, Presets.Volume(VolumeLevel.Mute));
        Assert.True(Presets.Volume(VolumeLevel.Low) < Presets.Volume(VolumeLevel.Medium));
        Assert.True(Presets.Volume(VolumeLevel.Medium) < Presets.Volume(VolumeLevel.High));
        Assert.True(Presets.Volume(VolumeLevel.High) <= 1f);
    }
}
