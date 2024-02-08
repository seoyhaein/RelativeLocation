using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace RelativeLocation;

public class DAGlynEditorCanvas : Canvas
{
    #region Dependency Properties
    
    /// <summary>
    /// ViewportLocation 이라는 것은 현재 자신의 현재 위치를 말한다.
    /// TODO 초기값 설정은 MousePosition 으로 해야 하는 것 아닌가???
    /// </summary>
    public static readonly StyledProperty<Point> ViewportLocationProperty =
        AvaloniaProperty.Register<DAGlynEditorCanvas, Point>(nameof(ViewportLocation), default(Point));

    public Point ViewportLocation
    {
        get => GetValue(ViewportLocationProperty);
        set => SetValue(ViewportLocationProperty, value);
    }

    #endregion

    public DAGlynEditorCanvas()
    {
        RenderTransform = new TranslateTransform();
        this.GetPropertyChangedObservable(ViewportLocationProperty).Subscribe(OnViewportLocationChanged);
    }

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
        var children = Children;

        foreach (var child in children)
        {
            var locationProperty = child.GetType().GetProperty("Location");
            if (locationProperty != null)
            {
                var locationValue = locationProperty.GetValue(child);
                if (locationValue is Point location)
                {
                    child.Arrange(new Rect(location, child.DesiredSize));
                }
            }
        }

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
}