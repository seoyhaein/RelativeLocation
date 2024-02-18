using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Collections;

namespace RelativeLocation;

public class DAGlynEditor : SelectingItemsControl, IDisposable
{
    #region Dependency Properties

    public static readonly StyledProperty<Point> ViewportLocationProperty =
        AvaloniaProperty.Register<DAGlynEditor, Point>(
            nameof(ViewportLocation), new Point(0, 0));

    public Point ViewportLocation
    {
        get => GetValue(ViewportLocationProperty);
        set => SetValue(ViewportLocationProperty, value);
    }

    // readonly
    // MouseLocation 만들어줌. 
    // https://docs.avaloniaui.net/docs/next/guides/custom-controls/how-to-create-advanced-custom-controls#datavalidation-support
    public static readonly DirectProperty<DAGlynEditor, Point> MouseLocationProperty =
        AvaloniaProperty.RegisterDirect<DAGlynEditor, Point>(
            nameof(MouseLocation),
            o => o.MouseLocation);

    private Point _mouseLocation;

    public Point MouseLocation
    {
        get => _mouseLocation;
        private set => SetAndRaise(MouseLocationProperty, ref _mouseLocation, value);
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

    private bool _isPanning = false;

    public bool IsPanning
    {
        get => _isPanning;
        protected internal set => SetAndRaise(IsPanningProperty, ref _isPanning, value);
    }

    // 향후 Node, Connection 들이 저장될 collection 임.
    public static readonly AvaloniaProperty<AvaloniaList<object>> DAGItemsProperty =
        AvaloniaProperty.Register<DAGlynEditor, AvaloniaList<object>>(
            nameof(DAGItems));

    public AvaloniaList<object> DAGItems
    {
        get => GetValue(DAGItemsProperty) as AvaloniaList<object> ?? new AvaloniaList<object>();
        set => SetValue(DAGItemsProperty, value);
    }

    #endregion

    #region Constructors

    public DAGlynEditor()
    {
        /*
         * PendingConnection 인스턴스를 여기서 만들어 준다.
         */
        //_pendingConnection = new PendingConnection();
        //_pendingConnection.IsVisible = false; // 초기 가시성 설정
        InitPendingConnection();

        ItemsSource = MyItems;
        InitializeSubscriptions();

        // TODO 이거 확인하자. 꼭~~
        this.Unloaded += (sender, e) => this.Dispose();
    }

    #endregion

    #region Fields

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private EventHandler<PendingConnectionEventArgs>? _connectionStartedHandler;

    // Panning 관련 포인터 위치 값 
    private Point _previousPointerPosition;
    private Point _currentPointerPosition;
    //private bool _isPanning;

    private PendingConnection? _pendingConnection;

    #endregion

    #region Event Handlers

    private void InitializeSubscriptions()
    {
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

        // 모든 Connector 들의 이벤트를 하나로? 모아서 처리한다.
        // 이벤트 처리 방식은 동일함으로 이러한 방식을 채택했다.
        _connectionStartedHandler = HandleConnectionStarted;
        AddHandler(Connector.PendingConnectionStartedEvent, _connectionStartedHandler);
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

    private void HandleConnectionStarted(object? sender, PendingConnectionEventArgs e)
    {
        /*if (!e.Canceled && (ConnectionStartedCommand?.CanExecute(e.SourceConnector) ?? false))
        {
            ConnectionStartedCommand.Execute(e.SourceConnector);
        }*/

        Debug.WriteLine("Ok!!!");
    }

    #endregion

    #region Methods

    private void UpdateMouseLocation(PointerEventArgs args)
    {
        MouseLocation = args.GetPosition(this);
    }

    private void InitPendingConnection()
    {
        _pendingConnection = new PendingConnection();
        _pendingConnection.IsVisible = false; // 초기 가시성 설정
    }

    // TODO Unload 와 관련 및 GC 관련 해서 생각해보자.
    public void Dispose()
    {
        _disposables.Dispose();
        if (_pendingConnection != null)
        {
            // _pendingConnection에 대한 정리 코드
            _pendingConnection.Dispose();
            _pendingConnection = null;
        }

        RemoveHandler(Connector.PendingConnectionStartedEvent, _connectionStartedHandler);
    }

    #endregion

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }

    // 테스트로 일단 이렇게 제작한다. 테스트 후 이후 내용 변경함.
    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var testConnector = item as TestConnector;
        if (testConnector != null)
        {
            switch (testConnector.ConType)
            {
                case ConnectorType.OutConnector:
                    return new OutConnector();
                case ConnectorType.InConnector:
                    return new InConnector();
                default:
                    break;
            }
        }
        
        return new Node();
    }

    // 컨테이너 설정
    // 컨테이너를 바로 Node 로 설정해주는 것도 가능할 것으로 판단된다.
    // 일단 주석으로 남겨 놓았는데, 움직임을 구현하기 위해서는 남겨놓는다.
    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is Node myContainer)
            myContainer.Location = new Point(10, 10);

        if (container is InConnector inConnector && item is TestConnector inCon)
            inConnector.Location = inCon.Location;

        if (container is OutConnector outConnector && item is TestConnector outCon)
            outConnector.Location = outCon.Location;
    }

    #region Tests

    public ObservableCollection<Node> MyItems { get; set; } = new ObservableCollection<Node>
    {
        new Node()
    };

    // TODO 이걸로 속성하나 만들어야 할듯하다.
    // 그리고 여기에 Node 들을 추가하는 메서드들을 만들자.
    public AvaloniaList<Node> NodeItems { get; set; } = new AvaloniaList<Node>();

    #endregion
}