using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace RelativeLocation;

// 일단 테스트로 여기서 연결을 시켜보자.
public partial class MainWindow : Window
{
    /*private DAGItems node1;
    private DAGItems node2;
    private DAGItems node3;*/
    //private DAG dag = new DAG();
    public MainWindow()
    {
        InitializeComponent();
        //dag = new DAG();
        //DataContext = dag;
        // for test
        /*_pendingConnection = new PendingConnection();
        _pendingConnection.IsVisible = true; // 초기 가시성 설정
        _pendingConnection.SourceAnchor = new Point(10, 10);
        _pendingConnection.TargetAnchor = new Point(200, 200);
        
        MainCanvas.Children.Add(_pendingConnection);*/
        
        //DataContext = this;
        // 이벤트 적용되지 않음.
        // EditorTester.Dispose();
        
       //MakeItems();

        //ConItems = new AvaloniaList<DAGItems>();
        
        //ConItems.Add(node1);
        //ConItems.Add(node2);
        //ConItems.Add(node3);
        
    }

    /*private void MakeItems()
    {
        node1 = new DAGItems(new Point(10, 10), DAGItemsType.RunnerNode);
        node2 = new DAGItems(new Point(20, 20), DAGItemsType.RunnerNode);
        node3 = new DAGItems(new Point(30, 30), DAGItemsType.RunnerNode);
    }*/

    //public AvaloniaList<DAGItems> ConItems { get; set; }

    

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        //dag.AddDAGItem(new Point(600, 600), Guid.NewGuid(), DAGItemsType.RunnerNode);
    }
}

public enum ConnectorType
{
    InConnector,
    OutConnector
}

public class TestConnector
{
    public Point Location { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    //public IImmutableSolidColorBrush Br { get; set; }

    public ConnectorType ConType { get; set; }


}