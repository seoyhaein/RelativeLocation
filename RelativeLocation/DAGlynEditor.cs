using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;

namespace RelativeLocation;

public class DAGlynEditor : SelectingItemsControl, IDisposable
{
    #region Dependency Properties

    public static readonly StyledProperty<Point> ViewportLocationProperty =
        AvaloniaProperty.Register<DAGlynEditor, Point>(
            nameof(ViewportLocation), Constants.ZeroPoint);

    public Point ViewportLocation
    {
        get => GetValue(ViewportLocationProperty);
        set => SetValue(ViewportLocationProperty, value);
    }

    public static readonly StyledProperty<bool> DisablePanningProperty =
        AvaloniaProperty.Register<DAGlynEditor, bool>(nameof(DisablePanning));

    public bool DisablePanning
    {
        get => GetValue(DisablePanningProperty);
        set => SetValue(DisablePanningProperty, value);
    }

    public static readonly DirectProperty<DAGlynEditor, bool> IsSelectingProperty =
        AvaloniaProperty.RegisterDirect<DAGlynEditor, bool>(
            nameof(IsSelecting),
            o => o.IsSelecting);

    private bool _isSelecting;

    public bool IsSelecting
    {
        get => _isSelecting;
        internal set => SetAndRaise(IsSelectingProperty, ref _isSelecting, value);
    }

    public static readonly StyledProperty<bool> EnableRealtimeSelectionProperty =
        AvaloniaProperty.Register<DAGlynEditor, bool>(
            nameof(EnableRealtimeSelection));

    public bool EnableRealtimeSelection
    {
        get => GetValue(EnableRealtimeSelectionProperty);
        set => SetValue(EnableRealtimeSelectionProperty, value);
    }

    public static readonly DirectProperty<DAGlynEditor, Rect> SelectedAreaProperty =
        AvaloniaProperty.RegisterDirect<DAGlynEditor, Rect>(
            nameof(SelectedArea),
            o => o.SelectedArea);

    private Rect _selectedArea;

    public Rect SelectedArea
    {
        get => _selectedArea;
        internal set => SetAndRaise(SelectedAreaProperty, ref _selectedArea, value);
    }

    public static readonly DirectProperty<DAGlynEditor, bool?> IsPreviewingSelectionProperty =
        AvaloniaProperty.RegisterDirect<DAGlynEditor, bool?>(
            nameof(IsPreviewingSelection),
            o => o.IsPreviewingSelection);

    private bool? _isPreviewingSelection;

    public bool? IsPreviewingSelection
    {
        get => _isPreviewingSelection;
        internal set => SetAndRaise(IsPreviewingSelectionProperty, ref _isPreviewingSelection, value);
    }

    public static readonly DirectProperty<DAGlynEditor, bool> IsPanningProperty =
        AvaloniaProperty.RegisterDirect<DAGlynEditor, bool>(
            nameof(IsPanning),
            o => o.IsPanning);

    private bool _isPanning;

    public bool IsPanning
    {
        get => _isPanning;
        protected internal set => SetAndRaise(IsPanningProperty, ref _isPanning, value);
    }

    // 추가
    public static readonly StyledProperty<DataTemplate?> PendingConnectionTemplateProperty =
        AvaloniaProperty.Register<DAGlynEditor, DataTemplate?>(
            nameof(PendingConnectionTemplate));

    public DataTemplate? PendingConnectionTemplate
    {
        get => GetValue(PendingConnectionTemplateProperty);
        set => SetValue(PendingConnectionTemplateProperty, value);
    }

    public static readonly StyledProperty<object?> PendingConnectionProperty =
        AvaloniaProperty.Register<DAGlynEditor, object?>(
            nameof(PendingConnection));

    public object? PendingConnection
    {
        get => GetValue(PendingConnectionProperty);
        set => SetValue(PendingConnectionProperty, value);
    }

    public static readonly StyledProperty<Point?> SourceAnchorProperty =
        AvaloniaProperty.Register<DAGlynEditor, Point?>(nameof(SourceAnchor));

    public static readonly StyledProperty<Point?> TargetAnchorProperty =
        AvaloniaProperty.Register<DAGlynEditor, Point?>(nameof(TargetAnchor));

    public Point? SourceAnchor
    {
        get => GetValue(SourceAnchorProperty);
        set => SetValue(SourceAnchorProperty, value);
    }

    public Point? TargetAnchor
    {
        get => GetValue(TargetAnchorProperty);
        set => SetValue(TargetAnchorProperty, value);
    }

    // PendingConnection visible 설정에 사용
    public static readonly StyledProperty<bool> IsVisiblePendingConnectionProperty =
        AvaloniaProperty.Register<DAGlynEditor, bool>(
            nameof(IsVisiblePendingConnection));

    public bool IsVisiblePendingConnection
    {
        get => GetValue(IsVisiblePendingConnectionProperty);
        set => SetValue(IsVisiblePendingConnectionProperty, value);
    }

    #endregion

    #region Fields

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    // 이건 connector 에서 올라오는 event
    private EventHandler<PendingConnectionEventArgs>? _connectionStartedHandler;
    private EventHandler<PendingConnectionEventArgs>? _connectionDragHandler;
    private EventHandler<PendingConnectionEventArgs>? _connectionCompleteHandler;

    // 이건 node 에서 올라오는 event
    private EventHandler<ConnectionChangedEventArgs>? _connectionChangedHandler;

    private DAG dag = new DAG();
    private bool _isLoaded = true;
    private Canvas? topLayer;
    private DAGlynEditorCanvas? editorCanvas;

    // Panning 관련 포인터 위치 값 
    private Point _previousPointerPosition;
    private Point _currentPointerPosition;
    //private bool _isPanning;
    // 일단 이렇게 하고 이거 나중에 static constructor 에 넣자. 
    //private PendingConnection? _pendingConnection = new PendingConnection();

    #endregion

    #region Constructors

    public DAGlynEditor()
    {
        //dag.AddDAGItem(new Point(10,10), Guid.NewGuid(), new Point(100,100), Guid.NewGuid());
        //dag.AddDAGItem(new Point(400, 100), Guid.NewGuid(), DAGItemsType.RunnerNode);
        DataContext = dag;
        InitializeSubscriptions();
        this.Unloaded += (_, _) => this.Dispose();
    }

    #endregion

    #region Event Handlers

    private void InitializeSubscriptions()
    {
        _connectionStartedHandler = HandleConnectionStarted;
        _connectionDragHandler = HandleConnectionDrag;
        _connectionCompleteHandler = HandleConnectionComplete;
        _connectionChangedHandler = HandleConnectionChanged;

        Observable.FromEventPattern<PointerPressedEventArgs>(
                h => this.PointerPressed += h,
                h => this.PointerPressed -= h)
            .Subscribe(args => HandlePointerPressed(args.Sender, args.EventArgs))
            .DisposeWith(_disposables);

        Observable.FromEventPattern<PointerEventArgs>(
                h => this.PointerMoved += h,
                h => this.PointerMoved -= h)
            .Subscribe(args => HandlePointerMoved(args.Sender, args.EventArgs))
            .DisposeWith(_disposables);

        Observable.FromEventPattern<PointerReleasedEventArgs>(
                h => this.PointerReleased += h,
                h => this.PointerReleased -= h)
            .Subscribe(args => HandlePointerReleased(args.Sender, args.EventArgs))
            .DisposeWith(_disposables);

        Observable.FromEventPattern<RoutedEventArgs>(
                h => this.Loaded += h,
                h => this.Loaded -= h)
            .Subscribe(args => HandleLoaded(args.Sender, args.EventArgs))
            .DisposeWith(_disposables);

        // 이벤트 핸들러 등록
        AddHandler(Connector.PendingConnectionStartedEvent, _connectionStartedHandler);
        AddHandler(Connector.PendingConnectionDragEvent, _connectionDragHandler);
        AddHandler(Connector.PendingConnectionCompletedEvent, _connectionCompleteHandler);

        AddHandler(Node.ConnectionChangedEvent, _connectionChangedHandler);

        // 이벤트 핸들러 해제
        _disposables.Add(Disposable.Create(() =>
        {
            RemoveHandler(Connector.PendingConnectionStartedEvent, _connectionStartedHandler);
            RemoveHandler(Connector.PendingConnectionDragEvent, _connectionDragHandler);
            RemoveHandler(Connector.PendingConnectionCompletedEvent, _connectionCompleteHandler);

            AddHandler(Node.ConnectionChangedEvent, _connectionChangedHandler);
        }));
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed && !DisablePanning)
        {
            args.Pointer.Capture(this);
            _previousPointerPosition = args.GetPosition(this);
            IsPanning = true;
            args.Handled = true;
        }
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (IsPanning)
        {
            _currentPointerPosition = args.GetPosition(this);
            ViewportLocation -=
                (_currentPointerPosition - _previousPointerPosition) / 1; // Adjust division based on actual zoom level
            _previousPointerPosition = _currentPointerPosition;
            args.Handled = true;
        }
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (IsPanning && args.Pointer.Captured == this)
        {
            args.Pointer.Capture(null);
            IsPanning = false;
            args.Handled = true;
        }
    }

    private void HandleConnectionStarted(object? sender, PendingConnectionEventArgs args)
    {
        if (args.Source is StartConnector)
        {
            IsVisiblePendingConnection = true;

            if (args.Anchor.HasValue)
            {
                SourceAnchor = args.Anchor.Value;
                if (args.Offset.HasValue)
                    TargetAnchor = new Point(SourceAnchor.Value.X + args.Offset.Value.X,
                        SourceAnchor.Value.Y + args.Offset.Value.Y);
                else TargetAnchor = SourceAnchor;
            }
            else
            {
                SourceAnchor = null;
                TargetAnchor = null;
            }

            args.Handled = true;
        }

        Debug.WriteLine("Ok!!!");
    }
    // TODO 중요 여기 반드시 살펴보기
    private void HandleConnectionDrag(object? sender, PendingConnectionEventArgs args)
    {
        // TODO 버그 있음. 
        if (IsVisiblePendingConnection)
        {
            if (args.Offset.HasValue)
            {
                TargetAnchor = new Point(args.Offset.Value.X, args.Offset.Value.Y);
            }

            args.Handled = true;

            // TODO 아래 코드는 좀더 생각하자. 해당 코드 정검 후 지우기
            //TargetAnchor = new Point(e.Anchor.X + e.OffsetX, e.Anchor.Y + e.OffsetY);

            /*if (Editor != null && (EnablePreview || EnableSnapping))
            {
                // Look for a potential connector
                FrameworkElement? connector = GetPotentialConnector(Editor, AllowOnlyConnectors);

                // Update the connector's anchor and snap to it if snapping is enabled
                if (EnableSnapping && connector is Connector target)
                {
                    target.UpdateAnchor();
                    TargetAnchor = target.Anchor;
                }

                // If it's not the same connector
                if (connector != _previousConnector)
                {
                    if (_previousConnector != null)
                    {
                        SetIsOverElement(_previousConnector, false);
                    }

                    // And we have a connector
                    if (connector != null)
                    {
                        SetIsOverElement(connector, true);

                        // Update the preview target if enabled
                        if (EnablePreview)
                        {
                            PreviewTarget = connector.DataContext;
                        }
                    }

                    _previousConnector = connector;
                }
            }*/
        }
    }

    private void HandleConnectionComplete(object? sender, PendingConnectionEventArgs args)
    {
        if (args.SourceConnector == null || args.StartAnchor == null || args.EndAnchor == null)
        {
            args.Handled = true;
            IsVisiblePendingConnection = false;
            return;
        }

        // TODO (중요) 여기서 일단은 그냥 가는데, AddDAGItem 메서드들에 대한 정리는 반드시 필요. 최적화 필요.
        // 선추가하는 구문.
        dag.AddDAGItem(args.StartAnchor, args.InNodeId, args.EndAnchor, args.OutNodeId);
        args.Handled = true;
        IsVisiblePendingConnection = false;
    }
    
    private void HandleConnectionChanged(object? sender, ConnectionChangedEventArgs args)
    {
        foreach (var item in dag.DAGItemsSource)
        {
            if (item.StartAnchor == args.OldStartAnchor || item.EndAnchor == args.OldEndAnchor)
            {
                if (item.Vertex != null)
                {
                    // TODO UpdateRending 관련 메서드 수정 필요.
                    if (args.StartAnchor.HasValue && args.EndAnchor.HasValue)
                    {
                        // TODO 아래 코드에 버그 있음.
                        //item.Vertex.UpdateRending(args.StartAnchor.Value, args.EndAnchor.Value);
                        item.Vertex.UpdateEnd(args.EndAnchor.Value);
                        Debug.WriteLine("OutAnchor:");
                        Debug.WriteLine(args.StartAnchor.Value);
                        Debug.WriteLine("InAnchor:");
                        Debug.WriteLine(args.EndAnchor.Value);

                        // update
                        item.StartAnchor = args.StartAnchor.Value;
                        item.EndAnchor = args.EndAnchor.Value;

                        // check 디버깅 위해서 이후 삭제
                        if (item.Vertex.Source == item.StartAnchor)
                            Debug.WriteLine("start 동일");
                        if (item.Vertex.Target == item.EndAnchor)
                            Debug.WriteLine("end 동일");
                    }
                }
            }
        }

        args.Handled = true;
    }

    // TODO 이건 일단 어떻할지 생각해본다.
    // 일단 디버깅 용으로 넣어둔다. 추후 삭제 검토.
    private void HandleLoaded(object? sender, RoutedEventArgs args)
    {
        if (_isLoaded)
        {
            editorCanvas = this.GetChildControlByName<DAGlynEditorCanvas>("PART_ItemsHost");
            if (editorCanvas == null)
                throw new InvalidOperationException("PART_ItemsHost cannot be found in the template.");

            // 두 컨트롤이 모두 찾아진 후 로직 수행
            if (topLayer != null && editorCanvas != null)
            {
                bool isMatch = topLayer.IsCoordinateSystemMatch(editorCanvas);
                if (!isMatch)
                {
                    Extension.WriteErrorsToFile(
                        "The coordinate systems do not match, causing rendering issues in the application.");
                }
            }

            // 한번만 실행되게 만드는 flag
            _isLoaded = false;
        }
    }

    #endregion

    #region Methods

    // TODO Unload 와 관련 및 GC 관련 해서 생각해보자.
    public void Dispose()
    {
        _disposables.Dispose();
    }

    #endregion

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        topLayer = e.NameScope.Find<Canvas>("PART_TopLayer");
        if (topLayer == null)
            throw new InvalidOperationException("PART_TopLayer cannot be found in the template.");
    }

    // TODO 아래 코드에서 따로 메서드를 빼놓는 것을 생각하자.
    // Connection 에는 id 관련해서는 넣지 않았다.
    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        // 중요! Node 생성자에서 자동으로 Guid 를 만들어 준다.
        if (item is DAGItems dagItems)
        {
            // TODO (중요) 일단 nodeid 를 생성해주는 것을 여기서 해주자. 노드를 생성
            // 해주는 부분이 없어서 땜빵코드로 일단 넣어두었다.
            if (dagItems.DAGItemType == DAGItemsType.RunnerNode)
                if (dagItems.Location.HasValue)
                {
                    // 아래 땜빵 코드
                    // TODO 좀더 친절히 할 필요는 있을것 같다. InAnchor, OutAnchor 최조 만들어줄때.
                    // Node 의 인스턴스도 넣어줄 필요가 있을듯하다. 일단 여기서는 생략.
                    var node = new Node(dagItems.Location.Value, dagItems.NodeId);
                    // 이거 디버깅 해보자.
                    // 아 땜빵 코드 고칠거 많다.
                    if (!dagItems.StartAnchor.HasValue)
                    {
                        dagItems.StartAnchor = node.StartAnchor;
                    }

                    if (!dagItems.EndAnchor.HasValue)
                    {
                        dagItems.EndAnchor = node.EndAnchor;
                    }


                    return node;
                }

            //TODO 확인해야함. 중요! Node 생성된후에 추가될 수 있다.
            if (dagItems.DAGItemType == DAGItemsType.Connection)
                if (dagItems.StartAnchor.HasValue && dagItems.EndAnchor.HasValue)
                {
                    // TODO 실험적 코드, 아직은 인스턴스를 구별할 수 있는 키도 없다.
                    var connection = new Connection(dagItems.StartAnchor.Value, dagItems.EndAnchor.Value);
                    // TODO 실제로 DAGItems 에서 반영되는지 확인해야 함.
                    dagItems.Vertex = connection;
                    return connection;
                }
        }

        var emptyControl = new ContentControl { IsVisible = false };
        return emptyControl;
    }

    // 컨테이너 설정
    // 컨테이너를 바로 Node 로 설정해주는 것도 가능할 것으로 판단된다.
    // 일단 주석으로 남겨 놓았는데, 움직임을 구현하기 위해서는 남겨놓는다.
    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        /*if (container is Node myContainer)
            myContainer.Location = new Point(10, 10);

        if (container is InConnector inConnector && item is TestConnector inCon)
            inConnector.Location = inCon.Location;

        if (container is OutConnector outConnector && item is TestConnector outCon)
            outConnector.Location = outCon.Location;*/
    }
}