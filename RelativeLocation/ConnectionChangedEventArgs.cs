using Avalonia;
using Avalonia.Interactivity;

namespace RelativeLocation;

// TODD 수정하자. 연결 수정할때 작성해줘야 한다.
public class ConnectionChangedEventArgs : RoutedEventArgs
{
    public ConnectionChangedEventArgs(RoutedEvent routedEvent, Point? inAnchor,
        Point? outAnchor)
        : base(routedEvent)
    {
        InAnchor = inAnchor;
        OutAnchor = outAnchor;
    }
    
    public Point? InAnchor { get; set; }
    public Point? OutAnchor { get; set; }
}