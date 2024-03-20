using System;
using Avalonia;
using Avalonia.Interactivity;

namespace RelativeLocation;

public class ConnectionChangedEventArgs : RoutedEventArgs
{
    public ConnectionChangedEventArgs(RoutedEvent routedEvent, Guid? nodeId, Point? location, Point? sourceAnchor,
        Point? oldSourceAnchor,
        Point? targetAnchor, Point? oldTargetAnchor, DAGItemsType? dagItemsType)
        : base(routedEvent)
    {
        NodeId = nodeId;
        Location = location;
        SourceAnchor = sourceAnchor;
        OldSourceAnchor = oldSourceAnchor;
        TargetAnchor = targetAnchor;
        OldTargetAnchor = oldTargetAnchor;
        DAGItemType = dagItemsType;
    }
    
    public Guid? NodeId { get; set; }
    public Point? Location { get; set; }
    public Point? SourceAnchor { get; set; }
    public Point? OldSourceAnchor { get; set; }
    public Point? TargetAnchor { get; set; }
    public Point? OldTargetAnchor { get; set; }
    public DAGItemsType? DAGItemType { get; set; }
}