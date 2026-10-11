namespace WhipCursor.Core;

public enum VolumeLevel { Mute, Low, Medium, High }

public enum WhipLength { Short, Medium, Long }

public enum CrackSensitivity { Low, Medium, High }

public sealed record AppSettings
{
    public bool Enabled { get; init; } = true;
    public VolumeLevel Volume { get; init; } = VolumeLevel.Medium;
    public WhipLength Length { get; init; } = WhipLength.Medium;
    public CrackSensitivity Sensitivity { get; init; } = CrackSensitivity.Medium;
    public bool SparksEnabled { get; init; } = true;
    public bool StartWithWindows { get; init; }
}
