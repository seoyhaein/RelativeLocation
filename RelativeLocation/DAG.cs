using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Collections;

namespace RelativeLocation;

public class DAG : INotifyPropertyChanged
{
    private AvaloniaList<DAGItems> _dagItemsSource = new AvaloniaList<DAGItems>();

    public AvaloniaList<DAGItems> DAGItemsSource
    {
        get => _dagItemsSource;
        set
        {
            if (_dagItemsSource != value)
            {
                _dagItemsSource = value;
                //OnPropertyChanged(nameof(DAGItemsSource));
            }
        }
    }
    // TODO 이거 내용 추가적으로 없애주어야함. 테스트로 넣어둠.
    public DAG()
    {
        AddDAGItem(new Point(10, 10), Guid.NewGuid(), DAGItemsType.RunnerNode);
        AddDAGItem(new Point(300, 300), Guid.NewGuid(), DAGItemsType.RunnerNode);
        //AddDAGItem(new Point(400, 100), Guid.NewGuid(), DAGItemsType.RunnerNode);
        
        /*DAGItemsSource.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (DAGItems item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (DAGItems item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
        };*/
    }
    
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // DAGItems 인스턴스의 속성 변경 시 필요한 UI 업데이트 로직을 실행
        // 예: 특정 속성 변경에 따른 처리, 전체 리스트를 다시 바인딩하는 경우 등
        OnPropertyChanged(nameof(DAGItemsSource));
    }
    
    // 일단 이렇게 처리한다. 테스트 용으로 이렇게 만듬.
    /*public AvaloniaList<DAGItems> DAGItemsSource { get; set; } = new AvaloniaList<DAGItems>()
    {
        new DAGItems(new Point(10, 10), Guid.NewGuid(), DAGItemsType.RunnerNode),
        new DAGItems(new Point(300, 300), Guid.NewGuid(), DAGItemsType.RunnerNode),
        new DAGItems(new Point(400, 100), Guid.NewGuid(), DAGItemsType.RunnerNode),
        //new DAGItems(new Point(15, 15), new Point(30,30)),
        //new DAGItems(new Point(25, 25), new Point(50,50)),
    };*/

    // DAGItemsSource에 새로운 DAGItems 인스턴스를 추가하고 성공 여부를 반환
    // DAGItemsType 타입 검사는 하지 않는다. 내부적으로만 사용할 예정임으로.
    //TODO 접근한정자의 경우도 수정할 필요가 있다. 일단 public 으로 해놓았다.
    public bool AddDAGItem(Point? location, DAGItemsType dagItemsType)
    {
        if (location is null) return false;
        var newItem = new DAGItems(location, dagItemsType);
        _dagItemsSource.Add(newItem);
        return true;
    }

    //TODO 땜빵코드
    public bool AddDAGItem(Point? location, Guid? nodeId, DAGItemsType dagItemsType)
    {
        if (location is null) return false;

        var newItem = new DAGItems(location, nodeId, dagItemsType);
        _dagItemsSource.Add(newItem);
        return true;
    }

    public bool AddDAGItem(Point? start, Point? end)
    {
        if (start is null || end is null) return false;

        var newItem = new DAGItems(start, end);
        _dagItemsSource.Add(newItem);
        return true; // 성공적으로 추가되었으면 true 반환
    }

    public bool AddDAGItem(Point? start, Guid? inNodeId, Point? end, Guid? outNodeId)
    {
        if (start is null || end is null) return false;

        var newItem = new DAGItems(start, inNodeId, end, outNodeId);
        _dagItemsSource.Add(newItem);
        return true; // 성공적으로 추가되었으면 true 반환
    }

    // 특정 NodeId를 가진 DAGItems 객체의 속성을 변경하는 메서드
    public bool UpdateDAGItem(Guid? nodeId, Point? newLocation, Point? newStartAnchor, Point? newEndAnchor,
        DAGItemsType? newDAGItemType)
    {
        // DAGItemsSource에서 해당 NodeId를 가진 DAGItems 객체를 찾음
        var dagItemToUpdate = _dagItemsSource.FirstOrDefault(dagItem => dagItem.NodeId == nodeId);

        if (dagItemToUpdate != null)
        {
            // 찾은 DAGItems 객체의 속성을 새 값으로 업데이트
            if (newLocation != null) dagItemToUpdate.Location = newLocation;
            if (newStartAnchor != null) dagItemToUpdate.StartAnchor = newStartAnchor;
            if (newEndAnchor != null) dagItemToUpdate.EndAnchor = newEndAnchor;
            if (newDAGItemType != null) dagItemToUpdate.DAGItemType = newDAGItemType.Value;
            
            /*dagItemToUpdate.InAnchor = newStart;
            dagItemToUpdate.OutAnchor = newEnd;*/

            return true; // 성공적으로 업데이트되었으면 true 반환
        }

        return false; // 해당 NodeId를 가진 DAGItems 객체를 찾지 못했으면 false 반환
    }
    
    // INotifyPropertyChanged 구현 
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}