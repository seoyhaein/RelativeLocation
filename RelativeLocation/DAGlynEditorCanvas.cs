using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace RelativeLocation;

/*
 *
 * Panning 후 화면 이동후 이벤트 처리가 발생하지 않는 현상이 발생한다.
 * 당연한 이야기 이겠지만, 이동후 빈공간에서는 canvas 영역이 아님으로 이벤트가 발생하지 않는 것이다.
 * 이문제를 어떻게 해결할지 고민해보자.
 * nodify 에서는 이것을 어떻게 해결했는지 살펴보자.
 *
 * panning 과련 이벤트 처리를 Editor 에 하는 건 어떨까?
 * 공백 문제는 해결이 되는데, 숨겨진 자식들의 화면에 보여지는 문제는 어떻게 해결할 것인가????
 */
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
    /*private Point _initialPointerPosition;
    private Point _previousPointerPosition;
    private Point _currentPointerPosition;
    private bool _isPanning;*/

    #endregion

    #region Constructor

    public DAGlynEditorCanvas()
    {
        //InitializeSubscriptions();
        //this.RenderTransform = _translateTransform;
        //TODO 아래처럼 직접할지 생각해봐야 함.
        //this.RenderTransform = new TranslateTransform();
        // 일단 기억을 위해서 주석으로 남겨놓음. 
        // GetPropertyChangedObservable 방식은 Rx 방식으로 접근한 것임. 좀더 복잡하고, 다양한 기능제공(notion 참고 및 향후 기술 자료에서 언급)
        // this.GetPropertyChangedObservable(ViewportLocationProperty).Subscribe(OnViewportLocationChanged);
        // 동일한 기능을 제공하지만, 단순히 속성의 변경만을 감지함. 코드는 간단함.
        //ViewportLocationProperty.Changed.Subscribe(OnViewportLocationChanged);
        
        RenderTransform = new TranslateTransform();
        this.GetPropertyChangedObservable(ViewportLocationProperty).Subscribe(OnViewportLocationChanged);
    }

    #endregion


    /*private void OnViewportLocationChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Point pointValue)
        {
            if (RenderTransform is TranslateTransform translateTransform)
            {
                translateTransform.X = -pointValue.X;
                translateTransform.Y = -pointValue.Y;
            }
        }
    }*/

    //TODO 사이즈에 대한 것은 디버깅해서 살펴보자.
    /// <inheritdoc />
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
    /// <inheritdoc />
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
    /*private void InitializeSubscriptions()
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
    }*/

    /*private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        // 마우스 오른쪽 버튼 클릭 시 패닝 시작
        if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            args.Pointer.Capture(this);
            _initialPointerPosition = args.GetPosition(this);
            _previousPointerPosition = _initialPointerPosition;
            _isPanning = true;
            args.Handled = true;
        }
    }

    // TODO 이거 신경 쓰자. if (!_isPanning) 이거 반전해서 사용하는 거 검토.
    private void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (!_isPanning || args.Pointer.Captured != this)
            return;

        _currentPointerPosition = args.GetPosition(this);
        var delta = _currentPointerPosition - _previousPointerPosition;
        if (((Vector)delta).SquaredLength > Constants.AppliedThreshold) // 임계값 확인
        {
            ViewportLocation -= delta;
            _previousPointerPosition = _currentPointerPosition;

            _translateTransform.X = -ViewportLocation.X;
            _translateTransform.Y = -ViewportLocation.Y;
            args.Handled = true;
        }
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (_isPanning && args.Pointer.Captured == this)
        {
            // 포인터 캡처 해제
            args.Pointer.Capture(null);
            _isPanning = false;
            args.Handled = true;
        }
    }*/

    #endregion

    #region Methods
    
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

    public void Dispose()
    {
        // 관리되는 자원 해제
        _disposables.Dispose();
    }

    #endregion
}