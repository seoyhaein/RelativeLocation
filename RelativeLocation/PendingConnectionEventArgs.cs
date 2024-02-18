using System;
using Avalonia;
using Avalonia.Interactivity;

namespace RelativeLocation;

// 클래스 설명을 추가 필요
public class PendingConnectionEventArgs : RoutedEventArgs
{
    // 기본 생성자에 대한 설명을 추가 필요
    /*public PendingConnectionEventArgs()
    {
    }*/
    
    public PendingConnectionEventArgs(RoutedEvent routedEvent, object? sender) 
        : base(routedEvent, sender ?? throw new ArgumentNullException(nameof(sender)))
    {
        Sender = sender;
    }
    public PendingConnectionEventArgs(RoutedEvent routedEvent, object? sender, Vector? offset)
        : base(routedEvent, sender ?? throw new ArgumentNullException(nameof(sender)))
    {
        Sender = sender;
        Offset = offset;
    }
    
    // DataContext를 설정하는 생성자에 대한 설명 추가 필요
    public PendingConnectionEventArgs(object? dataContext)
    {
        SourceConnectorDataContext = dataContext;
    }
        
    

    // 새롭게 만들어 줌.
    // TODO 일단 이걸로 대체되면 아래 값들은 삭제한다.
    // TODO 아래 코드는 이벤트 발생할때 마다 new 해준다. 필요없지 않나 생각이 든다.
    public Point StartLocation { get; set; } = new Point(0,0); // 기본값 설정
    public Vector MovingLocation { get; set; }

    // 각 속성에 대한 설명을 추가
    public Point Anchor { get; set; } = new Point(0,0); // 기본값 설정

    public object? SourceConnectorDataContext { get; }

    public object? TargetConnectorDataContext { get; set; }

    public double OffsetX { get; set; }

    public double OffsetY { get; set; }

    public bool Canceled { get; set; }

    public object? Sender { get; set; }
    public Vector? Offset { get; set; }
}