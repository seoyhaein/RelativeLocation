using System;
using Avalonia;
using Avalonia.Interactivity;

namespace RelativeLocation;

// TODO 코드 정리 필요.
// 클래스 설명을 추가 필요
// TODO 아래 구분해줄 필요가 있을지 의문이지만 일단 이렇게 한다.
// Anchor 는 처음 위치
// SourceAnchor, TargetAnchor 는 이동 중 또는 마지막 선분넣을때 사용.
public class PendingConnectionEventArgs : RoutedEventArgs
{
    public PendingConnectionEventArgs(RoutedEvent routedEvent, Connector? sourceConnector)
        : base(routedEvent)
    {
        SourceConnector = sourceConnector;
    }

    public PendingConnectionEventArgs(RoutedEvent routedEvent, Connector? sourceConnector, Point? anchor)
        : base(routedEvent)
    {
        SourceConnector = sourceConnector;
        // Anchor 는 InAnchor 와 같아야 한다.
        Anchor = anchor;
    }

    public PendingConnectionEventArgs(RoutedEvent routedEvent, Connector? sourceConnector, Point? sourceAnchor,
        Point? targetAnchor)
        : base(routedEvent)
    {
        SourceConnector = sourceConnector;
        SourceAnchor = sourceAnchor;
        TargetAnchor = targetAnchor;
    }
    // TODO 일단 이렇게 추가함.
    public PendingConnectionEventArgs(RoutedEvent routedEvent, Connector? sourceConnector, Point? sourceAnchor, Guid? sourceNodeId,
        Point? targetAnchor, Guid? targetNodeId)
        : base(routedEvent)
    {
        SourceConnector = sourceConnector;
        SourceAnchor = sourceAnchor;
        SourceNodeId = sourceNodeId;
        TargetAnchor = targetAnchor;
        TargetNodeId = targetNodeId;
    }
    
    public PendingConnectionEventArgs(RoutedEvent routedEvent, Connector? sourceConnector,Guid? nodeId ,Point? sourceAnchor, Guid? sourceNodeId,
        Point? targetAnchor, Guid? targetNodeId)
        : base(routedEvent)
    {
        SourceConnector = sourceConnector;
        NodeId = nodeId;
        SourceAnchor = sourceAnchor;
        SourceNodeId = sourceNodeId;
        TargetAnchor = targetAnchor;
        TargetNodeId = targetNodeId;
    }

    // TODO 여기서 Connector? sourceConnector 는 필요없을 듯한데 일단 남겨둔다.
    public PendingConnectionEventArgs(RoutedEvent routedEvent, Connector? sourceConnector, Point? anchor, Vector? offset)
        : base(routedEvent)
    {
        SourceConnector = sourceConnector;
        Anchor = anchor;
        Offset = offset;
    }

    // TODO 하나로 할지 두개로 할지 생각해야함.
    // 이름이 좀 애매한데, SourceConnector 는 Source 또는 Target Connector 일 수 있다.
    public Connector? SourceConnector { get; set; }

    // 시작점
    public Point? Anchor { get; set; }

    // 이동 거리
    // TODO 이름 다시 생각하자.
    public Vector? Offset { get; set; }

    // 일단 위의 Anchor 는 살려둠.
    public Point? SourceAnchor { get; set; }
    public Point? TargetAnchor { get; set; }
    
    // 일단 이렇게 추가함.
    public Guid? NodeId { get; set; }
    public Guid? SourceNodeId { get; set; }
    public Guid? TargetNodeId { get; set; }
}