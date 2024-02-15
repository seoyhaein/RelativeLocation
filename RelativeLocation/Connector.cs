using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace RelativeLocation;

public class Connector : TemplatedControl, IDisposable
{
    #region Constructor

    public Connector()
    {
        InitializeSubscriptions();

        // TODO axaml 에서 생성한 경우 Dispose 할 수 없는데 이렇게 하면 될까?
        this.Unloaded += (sender, e) => this.Dispose();
    }

    /*static Connector()
    {
        FocusableProperty.OverrideDefaultValue<Connector>(true);
    }*/

    #endregion

    #region Routed Events

    public static readonly RoutedEvent<PendingConnectionEventArgs> PendingConnectionStartedEvent =
        RoutedEvent.Register<Connector, PendingConnectionEventArgs>(nameof(PendingConnectionStarted),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PendingConnectionEventArgs> PendingConnectionCompletedEvent =
        RoutedEvent.Register<Connector, PendingConnectionEventArgs>(nameof(PendingConnectionCompleted),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PendingConnectionEventArgs> PendingConnectionDragEvent =
        RoutedEvent.Register<Connector, PendingConnectionEventArgs>(nameof(PendingConnectionDrag),
            RoutingStrategies.Bubble);

    public event PendingConnectionEventHandler PendingConnectionStarted
    {
        add => AddHandler(PendingConnectionStartedEvent, value);
        remove => RemoveHandler(PendingConnectionStartedEvent, value);
    }

    public event PendingConnectionEventHandler PendingConnectionCompleted
    {
        add => AddHandler(PendingConnectionCompletedEvent, value);
        remove => RemoveHandler(PendingConnectionCompletedEvent, value);
    }

    public event PendingConnectionEventHandler PendingConnectionDrag
    {
        add => AddHandler(PendingConnectionDragEvent, value);
        remove => RemoveHandler(PendingConnectionDragEvent, value);
    }

    #endregion

    #region Fields & Dependency Properties

    // TODO Anchor 가 필요한지 일단 모르겠지만 넣어둠.
    public static readonly StyledProperty<Point> AnchorProperty =
        AvaloniaProperty.Register<Connector, Point>(nameof(Anchor));

    public Point Anchor
    {
        get => GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }
    
    // 추가
    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<Connector, IBrush?>(nameof(Fill));
    
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    protected bool IsPointerPressed;
    protected Connector? PreviousConnector;

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
    }

    protected virtual void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
    }

    protected virtual void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
    }

    protected virtual void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
    }

    // DataContext 는 살펴보자.
    /*private void StartedRaiseEvent(object? sender)
    {
        var args = new PendingConnectionEventArgs(PendingConnectionStartedEvent, this, DataContext)
        {
            Anchor = Anchor,
            Sender = sender,
        };

        RaiseEvent(args);
    }

    // 빈공란으로 향후 남겨두자.
    private void DragRaiseEvent(object? sender ,Vector? offset)
    {
        if (offset == null) return;

        var args = new PendingConnectionEventArgs(PendingConnectionDragEvent, this, DataContext)
        {
            OffsetX = offset.Value.X,
            OffsetY = offset.Value.Y,
            Sender = sender,
        };

        RaiseEvent(args);
    }

    private void CompletedRaiseEvent(object? sender)
    {
        // PendingConnectionEventArgs(DataContext) 관련해서 살펴봐야 함.
        var args = new PendingConnectionEventArgs(PendingConnectionCompletedEvent, this, DataContext)
        {
            Sender = sender,
            //Anchor = Anchor,
        };
        RaiseEvent(args);
    }*/

    #endregion

    #region Methods

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // 종료자 호출 억제
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 관리되는 자원 해제
            _disposables.Dispose();
        }
        // 관리되지 않는 자원 해제 코드가 필요한 경우 여기에 추가
    }

    // 종료자
    ~Connector()
    {
        Dispose(false);
    }

    #endregion
}