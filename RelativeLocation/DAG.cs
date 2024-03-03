using Avalonia;
using Avalonia.Collections;

namespace RelativeLocation;

// 일단 여기서 DAG 관련 처리를 하자.
// static 으로 만들어 줄지 고민.
public class DAG
{
    // 일단 이렇게 처리한다. 테스트 용으로 이렇게 만듬.
    public AvaloniaList<DAGItems> DAGItemsSource { get; set; } = new AvaloniaList<DAGItems>()
    {
        new DAGItems(new Point(10, 10), DAGItemsType.RunnerNode),
        //new DAGItems(new Point(20, 20), DAGItemsType.RunnerNode),
        new DAGItems(new Point(300, 200), DAGItemsType.RunnerNode),
        //new DAGItems(new Point(15, 15), new Point(30,30)),
        //new DAGItems(new Point(25, 25), new Point(50,50)),
    };

    // DAGItemsSource에 새로운 DAGItems 인스턴스를 추가하고 성공 여부를 반환
    // DAGItemsType 타입 검사는 하지 않는다. 내부적으로만 사용할 예정임으로.
    //TODO 접근한정자의 경우도 수정할 필요가 있다. 일단 public 으로 해놓았다.
    public bool AddDAGItem(Point? location, DAGItemsType dagItemsType)
    {
        if (location is null) return false;
        var newItem = new DAGItems(location, dagItemsType);
        DAGItemsSource.Add(newItem);
        return true;
    }

    public bool AddDAGItem(Point? start, Point? end)
    {
        if (start is null || end is null) return false;

        var newItem = new DAGItems(start, end);
        DAGItemsSource.Add(newItem);
        return true; // 성공적으로 추가되었으면 true 반환
    }
}