namespace WhipCursor.Core;

/// <summary>Numeric values behind the settings choices. "Dip" values are multiplied by the monitor scale.</summary>
public static class Presets
{
    public const float GravityDip = 900f;
    public const float TeleportDistanceDip = 800f;

    public static float LengthDip(WhipLength length) => length switch
    {
        WhipLength.Short => 80f,
        WhipLength.Long => 180f,
        _ => 120f,
    };

    /// <summary>Tip speed (DIP/s) that cracks. Tuned in Task 13.</summary>
    public static float CrackThresholdDip(CrackSensitivity sensitivity) => sensitivity switch
    {
        CrackSensitivity.Low => 7000f,
        CrackSensitivity.High => 3500f,
        _ => 5000f,
    };

    public static float Volume(VolumeLevel level) => level switch
    {
        VolumeLevel.Mute => 0f,
        VolumeLevel.Low => 0.25f,
        VolumeLevel.High => 0.85f,
        _ => 0.5f,
    };
}
