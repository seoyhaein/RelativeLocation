using System;
using Avalonia;
using Avalonia.Collections;

namespace RelativeLocation;

public class DAG 
{
    private AvaloniaList<DAGItems> _dagItemsSource = new AvaloniaList<DAGItems>();

    public AvaloniaList<DAGItems> DAGItemsSource
    {
        get => _dagItemsSource;
        set =>  _dagItemsSource = value;
    }
    // TODO 이거 내용 추가적으로 없애주어야함. 테스트로 넣어둠.
    public DAG()
    {
        // 테스트 위해 넣어둠.
        AddDAGNodeItem(new Point(10, 10));
        AddDAGNodeItem(new Point(300, 300));
    }
    
    public bool AddDAGConnectionItem(Point? source, Guid? sourceNodeId, Point? target, Guid? targetNodeId)
    {
        if (source is null || target is null) return false;

        var newItem = new DAGItems();
        newItem.CreateDAGConnection(source, sourceNodeId, target, targetNodeId);
        _dagItemsSource.Add(newItem);
        return true; 
    }
   
    public bool AddDAGNodeItem(Point? location)
    {
        if (!location.HasValue) return false;

        var newItem = new DAGItems();
        newItem.CreateDAGNode(location);
        _dagItemsSource.Add(newItem);
        return true; 
    }
    
}