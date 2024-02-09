using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace RelativeLocation;

public sealed class DAGlynEditorCanvas : Canvas, IDisposable
{
    #region Dependency Properties

    public static readonly StyledProperty<Point> ViewportLocationProperty =
        AvaloniaProperty.Register<DAGlynEditorCanvas, Point>(nameof(ViewportLocation));

    public Point ViewportLocation
    {
        get => GetValue(ViewportLocationProperty);
        set => SetValue(ViewportLocationProperty, value);
    }

    #endregion

    #region Fields

    private readonly CompositeDisposable _disposables = new CompositeDisposable();


    // Warning Readonly 주면 안됨.
    private TranslateTransform _translateTransform = new TranslateTransform();

    /// <summary>
    /// Panning 관련 포인터 위치 값 
    /// </summary>
    private Point _initialPointerPosition;
    private Point _previousPointerPosition;
    private Point _currentPointerPosition;
    private bool _isPanning;

    #endregion

    #region Constructor

    public DAGlynEditorCanvas()
    {
        InitializeSubscriptions();
        this.RenderTransform = _translateTransform;
        //TODO 아래처럼 직접할지 생각해봐야 함.
        //this.RenderTransform = new TranslateTransform();
        // 일단 기억을 위해서 주석으로 남겨놓음. 
        // GetPropertyChangedObservable 방식은 Rx 방식으로 접근한 것임. 좀더 복잡하고, 다양한 기능제공(notion 참고 및 향후 기술 자료에서 언급)
        // this.GetPropertyChangedObservable(ViewportLocationProperty).Subscribe(OnViewportLocationChanged);
        // 동일한 기능을 제공하지만, 단순히 속성의 변경만을 감지함. 코드는 간단함.
        ViewportLocationProperty.Changed.Subscribe(OnViewportLocationChanged);
    }

    #endregion


    private void OnViewportLocationChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Point pointValue)
        {
            if (RenderTransform is TranslateTransform translateTransform)
            {
                translateTransform.X = -pointValue.X;
                translateTransform.Y = -pointValue.Y;
            }
        }
    }

    //TODO 사이즈에 대한 것은 디버깅해서 살펴보자.
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            // ILocatable 인터페이스를 구현하는지 확인
            if (child is ILocatable locatableChild)
            {
                Point location = locatableChild.Location;
                child.Arrange(new Rect(location, child.DesiredSize));
            }
            else
            {
                // ILocatable을 구현하지 않는 경우, 기본 위치나 다른 로직을 사용하여 Arrange를 수행
                // 기본 위치를 (0, 0)으로 설정
                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
            }
        }

        // TODO finalSize 한번 디버깅해서 봐야 한다.
        return finalSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        foreach (var child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        return default;
    }

    #region Event Handlers

    /// <summary>
    /// Canvas 의 패닝(Panning) 관련 이벤트, 마우스 오른쪽 버튼 사용하여 화면 이동.
    /// </summary>
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

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        // 마우스 오른쪽 버튼 클릭
        if (!args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            return;

        if (sender == null) return;

        if (args.Pointer.Captured != null)
            args.Pointer.Capture(null);

        Focus();
        args.Pointer.Capture(this);
        _initialPointerPosition = args.GetPosition(this);
        _previousPointerPosition = _initialPointerPosition;
        _currentPointerPosition = _initialPointerPosition;
        _isPanning = true;
        args.Handled = true;
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (!_isPanning && args.Pointer.Captured == null && !args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            return;

        if (sender == null) return;

        _currentPointerPosition = args.GetPosition(this);
        // 1 은 나중에 ViewportZoom 으로 대체할 예정임.
        this.ViewportLocation -= (_currentPointerPosition - _previousPointerPosition) / 1;
        _previousPointerPosition = _currentPointerPosition;

        if (((Vector)(_currentPointerPosition - _initialPointerPosition)).SquaredLength > Constants.AppliedThreshold)
        {
            SetValue(ViewportLocationProperty, _currentPointerPosition);
        }

        args.Handled = true;
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        args.Pointer.Capture(null);
        _isPanning = false;
        args.Handled = true;
    }

    #endregion

    #region Methods

    public void Dispose()
    {
        // 관리되는 자원 해제
        _disposables.Dispose();
    }

    #endregion
}