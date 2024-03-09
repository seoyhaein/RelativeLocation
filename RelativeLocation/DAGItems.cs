using System;
using System.ComponentModel;
using Avalonia;

namespace RelativeLocation;

public enum DAGItemsType
{
    // Connection 도 일단 1개 이상일지 생각해야함.
    // Node 역시 3개 이상일지 생각해야함.
    StartNode,
    EndNode,
    RunnerNode,
    Connection
}

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
    
    private Point? _start;
    public Point? Start
    {
        get => _start;
        set
        {
            if (_start != value)
            {
                _start = value;
                OnPropertyChanged(nameof(Start));
            }
        }
    }
    
    //public Point? End { get; set; }
    
    private Point? _end;
    public Point? End
    {
        get => _end;
        set
        {
            if (_end != value)
            {
                _end = value;
                OnPropertyChanged(nameof(End));
            }
        }
    }

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

    // 이렇게 할지, AvaloniaList 를 따로 만들지 생각해보자.
    public DAGItems(Point? location, DAGItemsType dagItemsType)
    {
        Location = location;
        DAGItemType = dagItemsType;
    }

    // connection 을 위해서
    public DAGItems(Point? start, Point? end)
    {
        Start = start;
        End = end;
        DAGItemType = DAGItemsType.Connection;
    }
    
    public DAGItems(Point? start, Guid? inNodeId, Point? end, Guid? outNodeId)
    {
        Start = start;
        InNodeId = inNodeId;
        OutNodeId = outNodeId;
        End = end;
        DAGItemType = DAGItemsType.Connection;
    }

    #endregion
    
    // TODO 관련해서 좀 찾아보자
    // INotifyPropertyChanged 구현 
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}