using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Collections;

namespace RelativeLocation;

// TODO 인스턴스를 하나 만들어서 Connection 과 Node 를 분리하자.
public enum DAGItemsType
{
    // Connection 도 일단 1개 이상일지 생각해야함.
    // Node 역시 3개 이상일지 생각해야함.
    StartNode,
    EndNode,
    RunnerNode,
    Connection
}

// TODO (최우선 처리) DAGItems 가 좀 섞여 있다라는 느낌이 있음. 이것은 일단 구현후에 코드정리할때 어떻게 처리할지 고민하자.

// DAGlynEditor 에서 AvaloniaList 속성이 있고 여기에 대응되는 녀석이다.
// 향후 AvaloniaList<DAGItems> 해야함.
// internal 로 하는게 나을듯 한데 이것도 생각해보자.

// Connection 과 Node 등을 다시 한번 상세히 살펴보자.
// 일단 이녀석들을 하나의 Canvas 안에 넣을 예정임으로, 관련해서 살펴봐야 한다.
public class DAGItems
{
    #region Fields
    
    public Guid? NodeId { get; set; }
    public Guid? InNodeId { get; set; }
    public Guid? OutNodeId { get; set; }
    public Guid? ConnectionId { get; set; }
    public Connection? Vertex { get; set; }
    public Point? Location { get; set; }
    public Point? StartAnchor { get; set; }
    public Point? EndAnchor { get; set; }
    public DAGItemsType DAGItemType { get; set; }
    
    #endregion

    #region Constructor

    // TODO 추후 constructor 수정해줘야 함. 일단 지금은 이렇게 처리함.    
    // 이렇게 할지, AvaloniaList 를 따로 만들지 생각해보자.
    public DAGItems(Point? location, DAGItemsType dagItemsType)
    {
        Location = location;
        DAGItemType = dagItemsType;
    }

    // TODO 땜빵코드    
    public DAGItems(Point? location, Guid? nodeId, DAGItemsType dagItemsType)
    {
        Location = location;
        NodeId = nodeId;
        DAGItemType = dagItemsType;
    }

    // TODO 땜빵코드    
    public DAGItems(Point? location, Guid? nodeId, Point? startAnchor, Point? endAnchor, DAGItemsType dagItemsType)
    {
        Location = location;
        NodeId = nodeId;
        DAGItemType = dagItemsType;
        StartAnchor = startAnchor;
        EndAnchor = endAnchor;
    }
    
    // connection 을 위해서
    public DAGItems(Point? startAnchor, Point? endAnchor)
    {
        StartAnchor = startAnchor;
        EndAnchor = endAnchor;
        DAGItemType = DAGItemsType.Connection;
    }

    // TODO 중복된 코드가 들어가 있다.
    /*
     * NodeId = inNodeId;
     * InNodeId = inNodeId;
     */
    public DAGItems(Point? startAnchor, Guid? inNodeId, Point? endAnchor, Guid? outNodeId)
    {
        // TODO 여기서 Connection 의 정보를 만들어줌.
        ConnectionId = Guid.NewGuid();
        StartAnchor = startAnchor;
        NodeId = inNodeId;
        InNodeId = inNodeId;
        OutNodeId = outNodeId;
        EndAnchor = endAnchor;
        DAGItemType = DAGItemsType.Connection;
    }

    #endregion
}
// 아래와 같이 변경할 예정임. 
public class DAGNode
{
    public Guid? NodeId { get; set; }
    public Node? NodeInstance { get; set; }
    public Point? Location { get; set; }
    // node 의 anchor 를 나타냄.
    public Point? SourceAnchor { get; set; }
    public Point? TargetAnchor { get; set; }
    
    // TODO 이름은 추후 생각하자. Source, Target 으로 고치다. 현재는 start, end 로 되어 있음.
    // 이 녀석을 통해서 connection 을 검색할 수 있어야 한다.
    public AvaloniaList<DAGConnection> SourceConnections { get; set; } = new AvaloniaList<DAGConnection>();
    public AvaloniaList<DAGConnection> TargetConnections { get; set; } = new AvaloniaList<DAGConnection>();
    public DAGItemsType DAGItemType { get; set; }
}

public class DAGConnection
{
    public Guid? ConnectionId { get; set; }
    public Connection? ConnectionInstance { get; set; }
    public Guid? SourceNodeId { get; set; }
    public Node? SourceNodeInstance { get; set; }
    public Guid? TargetNodeId { get; set; }
    public Node? TargetNodeInstance { get; set; }
    public DAGItemsType DAGItemType { get; set; }
    // connection 의 source, target 을 나타냄.
    public Point? SourceAnchor { get; set; }
    public Point? TargetAnchor { get; set; }
   
} 