using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;

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
public class DAGItems : INotifyPropertyChanged
{
    #region Fields

    // 일단 넣어 둠.
    // 인스턴스를 구분하기 좋게 하기위해서 Id 를 둠. 향후 삭제 및 재활용도를 높이기 위해서.

    // TODO NodeId 넣을지 고민
    // 이건 노드 생성에서 삭제 및 수정 할때 필요한 nodeId 이다. 그런데 이게 필요한지 모르겠다.
    // public Guid? NodeId {get; set;}
    // 아래 In, OutNodeId 는 선에 연결할때 커넥터에서 얻어오는 해당 커텍터의 NodeId 이다.
    //public Guid? InNodeId { get; set; }

    private Guid? _nodeId;

    public Guid? NodeId
    {
        get => _nodeId;
        set
        {
            if (_nodeId != value)
            {
                _nodeId = value;
                OnPropertyChanged(nameof(NodeId));
            }
        }
    }

    private Guid? _inNodeId;

    public Guid? InNodeId
    {
        get => _inNodeId;
        set
        {
            if (_inNodeId != value)
            {
                _inNodeId = value;
                OnPropertyChanged(nameof(InNodeId));
            }
        }
    }

    //public Guid? OutNodeId { get; set; }

    private Guid? _outNodeId;

    public Guid? OutNodeId
    {
        get => _outNodeId;
        set
        {
            if (_outNodeId != value)
            {
                _outNodeId = value;
                OnPropertyChanged(nameof(OutNodeId));
            }
        }
    }
    
    // 추가, 실험적
    private Guid? _connectionId;

    public Guid? ConnectionId
    {
        get => _connectionId;
        set
        {
            if (_connectionId != value)
            {
                _connectionId = value;
                OnPropertyChanged(nameof(ConnectionId));
            }
        }
    }
    
    private Connection? _vertex;

    public Connection? Vertex
    {
        get => _vertex;
        set
        {
            if (_vertex != value)
            {
                _vertex = value;
                OnPropertyChanged(nameof(Vertex));
            }
        }
    }
    
    

    //TODO ConnectionId 도 필요할까?

    // TODO 여기에 좌표 이동시에 발생하는 이전값과 최신값을 저장하는 속성이 있어야 할듯하다.
    // OldLocation, OldStart, OldEnd 등등..
    // 일단 저장은 나중에 생각하고, 노드 이동시 반영하도록 한다.
    //public Point? Location { get; set; }

    private Point? _location;

    public Point? Location
    {
        get => _location;
        set
        {
            if (_location != value)
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }
    }

    // TODO 이름은 추후 수정
    //public Point? Start { get; set; }

    private Point? _startAnchor;

    public Point? StartAnchor
    {
        get => _startAnchor;
        set
        {
            if (_startAnchor != value)
            {
                _startAnchor = value;
                OnPropertyChanged(nameof(StartAnchor));
            }
        }
    }

    //public Point? End { get; set; }

    private Point? _endAnchor;

    public Point? EndAnchor
    {
        get => _endAnchor;
        set
        {
            if (_endAnchor != value)
            {
                _endAnchor = value;
                OnPropertyChanged(nameof(EndAnchor));
            }
        }
    }

    // 일단 넣어둠.
    /*private Point? _inAnchor;

    public Point? InAnchor
    {
        get => _inAnchor;
        set
        {
            if (_inAnchor != value)
            {
                _inAnchor = value;
                OnPropertyChanged(nameof(InAnchor));
            }
        }
    }

    private Point? _outAnchor;

    public Point? OutAnchor
    {
        get => _outAnchor;
        set
        {
            if (_outAnchor != value)
            {
                _outAnchor = value;
                OnPropertyChanged(nameof(OutAnchor));
            }
        }
    }*/

    //public DAGItemsType DAGItemType { get; set; }

    private DAGItemsType _dAGItemType;

    public DAGItemsType DAGItemType
    {
        get => _dAGItemType;
        set
        {
            if (_dAGItemType != value)
            {
                _dAGItemType = value;
                OnPropertyChanged(nameof(DAGItemType));
            }
        }
    }

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

    // TODO 관련해서 좀 찾아보자
    // INotifyPropertyChanged 구현 
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        Debug.WriteLine($"PropertyChanged: {propertyName}"); // 디버깅 목적의 로그
    }
}
// 좀더 생각하자.
// 일단 DAG 는 추후 생각하고(사실 생각할것이 많음) UI 구현 먼저 진행하고 완벽해질때, 클래스 맞춰나가자.
//public class 