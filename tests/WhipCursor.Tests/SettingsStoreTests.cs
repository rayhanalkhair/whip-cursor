namespace WhipCursor.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "WhipCursorTests", Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_dir, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    private void WriteRaw(string json)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, json);
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        Assert.Equal(new AppSettings(), new SettingsStore(FilePath).Load());
    }

    [Fact]
    public void Save_CreatesDirectory_AndRoundTrips()
    {
        var store = new SettingsStore(FilePath);
        var settings = new AppSettings
        {
            Enabled = false,
            Volume = VolumeLevel.High,
            Length = WhipLength.Long,
            Sensitivity = CrackSensitivity.Low,
            SparksEnabled = false,
            StartWithWindows = true,
        };
        store.Save(settings);
        Assert.Equal(settings, store.Load());
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{\"Volume\":\"Deafening\"}")]
    public void Load_CorruptFile_ReturnsDefaults(string json)
    {
        WriteRaw(json);
        Assert.Equal(new AppSettings(), new SettingsStore(FilePath).Load());
    }

    [Fact]
    public void Load_PartialFile_FillsMissingWithDefaults()
    {
        WriteRaw("{\"Volume\":\"High\"}");
        Assert.Equal(new AppSettings { Volume = VolumeLevel.High }, new SettingsStore(FilePath).Load());
    }

    [Fact]
    public void Load_OutOfRangeEnumNumber_FallsBackToDefaultForThatField()
    {
        WriteRaw("{\"Volume\":99,\"Length\":\"Long\"}");
        Assert.Equal(new AppSettings { Length = WhipLength.Long }, new SettingsStore(FilePath).Load());
    }
}
