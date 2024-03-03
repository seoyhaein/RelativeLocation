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
public class DAGItems
{
    #region Fields

    // 일단 넣어 둠.
    // 인스턴스를 구분하기 좋게 하기위해서 Id 를 둠. 향후 삭제 및 재활용도를 높이기 위해서.
    public int Id { get; set; }

    public Point? Location { get; set; }

    // TODO 이름은 추후 수정
    public Point? Start { get; set; }
    public Point? End { get; set; }

    public DAGItemsType DAGItemType { get; set; }

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

    #endregion
}