using System;
using Avalonia;
using Avalonia.Interactivity;

namespace RelativeLocation;

// TODO 코드 정리 후 코드 최적화 진행해야한다.
public class ConnectionChangedEventArgs : RoutedEventArgs
{
    public ConnectionChangedEventArgs(RoutedEvent routedEvent, Guid? nodeId, Point? location, Point? startAnchor,
        Point? oldStartAnchor,
        Point? endAnchor, Point? oldEndAnchor, DAGItemsType? dagItemsType)
        : base(routedEvent)
    {
        NodeId = nodeId;
        Location = location;
        StartAnchor = startAnchor;
        OldStartAnchor = oldStartAnchor;
        EndAnchor = endAnchor;
        OldEndAnchor = oldEndAnchor;
        DAGItemType = dagItemsType;
    }

    // 일단 그냥 넣어둠
    public Guid? NodeId { get; set; }
    public Point? Location { get; set; }
    public Point? StartAnchor { get; set; }
    public Point? OldStartAnchor { get; set; }
    public Point? EndAnchor { get; set; }
    public Point? OldEndAnchor { get; set; }
    public DAGItemsType? DAGItemType { get; set; }
}