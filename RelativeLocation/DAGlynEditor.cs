using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
        // PendingConnection
        AddHandler(Connector.PendingConnectionStartedEvent, _connectionStartedHandler);
        AddHandler(Connector.PendingConnectionDragEvent, _connectionDragHandler);
        AddHandler(Connector.PendingConnectionCompletedEvent, _connectionCompleteHandler);
        // Connection Changed
        AddHandler(Node.ConnectionChangedEvent, _connectionChangedHandler);

        // 이벤트 핸들러 해제
        _disposables.Add(Disposable.Create(() =>
        {
            // PendingConnection
            RemoveHandler(Connector.PendingConnectionStartedEvent, _connectionStartedHandler);
            RemoveHandler(Connector.PendingConnectionDragEvent, _connectionDragHandler);
            RemoveHandler(Connector.PendingConnectionCompletedEvent, _connectionCompleteHandler);
            // Connection Changed
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
        // TODO equal 쓸까??
        if (IsPanning && args.Pointer.Captured == this)
        {
            args.Pointer.Capture(null);
            IsPanning = false;
            args.Handled = true;
        }
    }

    private void HandleConnectionStarted(object? sender, PendingConnectionEventArgs args)
    {
        if (args.Source is SourceConnector)
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
        // TODO 버그 있음. 살펴보기. 
        if (IsVisiblePendingConnection)
        {
            if (args.Offset.HasValue)
                TargetAnchor = new Point(args.Offset.Value.X, args.Offset.Value.Y);
            args.Handled = true;
        }
    }

    private void HandleConnectionComplete(object? sender, PendingConnectionEventArgs args)
    {
        if (args.SourceConnector == null || args.SourceAnchor == null || args.TargetAnchor == null)
        {
            args.Handled = true;
            IsVisiblePendingConnection = false;
            return;
        }

        // 선추가하는 구문.
        dag.AddDAGConnectionItem(args.SourceAnchor, args.SourceNodeId, args.TargetAnchor, args.TargetNodeId);
        args.Handled = true;
        IsVisiblePendingConnection = false;
    }

    private void HandleConnectionChanged(object? sender, ConnectionChangedEventArgs args)
    {
        foreach (var item in dag.DAGItemsSource)
        {
            // node 도 변경되지만 connection 도 변경됨.
            // dag 데이터 변경은 node 나 connection 에서는 하지 않음. 명심.
            if (item.NodeItem != null)
            {
                if (item.NodeItem.NodeId == args.NodeId)
                {
                    // 노드 업데이트
                    item.NodeItem.Location = args.Location;
                    item.NodeItem.SourceAnchor = args.SourceAnchor;
                    item.NodeItem.TargetAnchor = args.TargetAnchor;
                }
            }

            var connectionItem = item.ConnectionItem;
            if (connectionItem?.ConnectionInstance != null)
            {
                if (args.SourceAnchor.HasValue && connectionItem.SourceAnchor == args.OldSourceAnchor)
                {
                    connectionItem.ConnectionInstance.UpdateStart(args.SourceAnchor.Value);
                    connectionItem.SourceAnchor = args.SourceAnchor.Value;
                }

                if (args.TargetAnchor.HasValue && connectionItem.TargetAnchor == args.OldTargetAnchor)
                {
                    connectionItem.ConnectionInstance.UpdateEnd(args.TargetAnchor.Value);
                    connectionItem.TargetAnchor = args.TargetAnchor.Value;
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

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is DAGItems dagItems)
        {
            if (dagItems.NodeItem != null)
            {
                if (dagItems.NodeItem.Location.HasValue)
                {
                    // 여기서 실제로 SourceAnchor, TargetAnchor 가 생성된다.
                    var node = new Node(dagItems.NodeItem.Location.Value);
                    dagItems.NodeItem.SourceAnchor = node.SourceAnchor;
                    dagItems.NodeItem.TargetAnchor = node.TargetAnchor;
                    dagItems.NodeItem.NodeInstance = node;

                    return node;
                }
            }

            if (dagItems.ConnectionItem != null)
            {
                if (dagItems.ConnectionItem.SourceAnchor.HasValue && dagItems.ConnectionItem.TargetAnchor.HasValue)
                {
                    var connection = new Connection(dagItems.ConnectionItem.SourceAnchor.Value,
                        dagItems.ConnectionItem.TargetAnchor.Value);

                    dagItems.ConnectionItem.ConnectionInstance = connection;

                    return connection;
                }
            }
        }

        var emptyControl = new ContentControl { IsVisible = false };
        return emptyControl;
    }
}