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
        new DAGItems(new Point(20, 20), DAGItemsType.RunnerNode),
        new DAGItems(new Point(30, 30), DAGItemsType.RunnerNode),
        new DAGItems(new Point(15, 15), new Point(30,30)),
        new DAGItems(new Point(25, 25), new Point(50,50)),
    };

}