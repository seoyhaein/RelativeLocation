using Avalonia;

namespace RelativeLocation;

public interface ILocatable
{
    Point Location { get; }
}