using Avalonia.Interactivity;

namespace RelativeLocation;

// contextmenu 가 open/close 되었는지 메세지로 전달함. invoke 는 쓰지 않음.
// raiseEvent 로 처리함.
public class EditorContextMenuEventArgs : RoutedEventArgs
{
    public EditorContextMenuEventArgs(RoutedEvent routedEvent, bool isOpened, bool isClosed)
        : base(routedEvent)
    {
        IsOpened = isOpened;
        IsClosed = isClosed;
    }
    
    public bool IsOpened { get; set; }
    public bool IsClosed { get; set; }
}