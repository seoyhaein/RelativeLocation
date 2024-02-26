using Avalonia;

namespace RelativeLocation;

public static class Constants
{
    public const double AppliedThreshold = 12d * 12d;
    public static readonly Point ZeroPoint = new Point(0, 0);
    public static readonly Vector ZeroVector = new(0d, 0d);
    public static readonly Size DefaultArrowSize = new Size(7, 6);
}