using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace RelativeLocation;

public class EditorContextMenu : ContextMenu
{
    protected override Type StyleKeyOverride => typeof(ContextMenu);
    
    // 테스트 코드
    #region Fields
    // 0, 1, 2, 3    
    public int PanningOk { get; set; }

    #endregion

    #region Events

    public static readonly RoutedEvent<EditorContextMenuEventArgs> EditorContextMenuChangedEvent =
        RoutedEvent.Register<EditorContextMenu, EditorContextMenuEventArgs>(
            nameof(EditorContextMenuChanged),
            RoutingStrategies.Bubble);

    public event EventHandler<EditorContextMenuEventArgs> EditorContextMenuChanged
    {
        add => AddHandler(EditorContextMenuChangedEvent, value);
        remove => RemoveHandler(EditorContextMenuChangedEvent, value);
    }

    #endregion

    #region Constructors

    static EditorContextMenu()
    {
    }

    public EditorContextMenu()
    {
        Initialize();
    }

    // 이렇게 사용했을때는 editor 는 null 이 아님.
    public EditorContextMenu(DAGlynEditor editor) : this()
    {
        var menuItem = SetupContextMenu(editor);
        this.Items.Add(menuItem);
        // 모든 세팅이 끝남.
        PanningOk = 3;
    }

    #endregion
    
    #region Methods

    private MenuItem SetupContextMenu(DAGlynEditor editor)
    {
        DAGlynEditor dagEditor = editor;
        // '파일(_F)' 메뉴 아이템 생성
        var fileMenuItem = new MenuItem
        {
            Header = "바보(_F)"
        };

        // '새로 만들기' 메뉴 아이템 생성
        var newMenuItem = new MenuItem
        {
            Header = "_새로 멍충이 만들기",
            InputGesture = new KeyGesture(Key.N, KeyModifiers.Control),
            HotKey = new KeyGesture(Key.N, KeyModifiers.Control),
        };
        newMenuItem.Click += (sender, e) => editor.AddNode();


        // '열기(_O)' 메뉴 아이템 생성
        var openMenuItem = new MenuItem
        {
            Header = "열기(_O)",
        };

        // 메뉴 아이템들을 '파일(_F)' 메뉴에 추가
        fileMenuItem.Items.Add(newMenuItem);
        fileMenuItem.Items.Add(new Separator());
        fileMenuItem.Items.Add(openMenuItem);

        // '파일(_F)' 메뉴 아이템을 ContextMenu에 추가
        //contextMenu.Items.Add(fileMenuItem);

        return fileMenuItem;
    }

    private void RaiseEditorContextMenuChangedEvent(bool isOpened, bool isClosed)
    {
        var args = new EditorContextMenuEventArgs(EditorContextMenuChangedEvent, isOpened, isClosed);
        RaiseEvent(args);
    }

    private void Initialize()
    {
        this.Opened += (sender, e) =>
        {
            // 불가능
            PanningOk = 2;
            RaiseEditorContextMenuChangedEvent(true, false);
        };
        this.Closed += (sender, e) =>
        {
            // 가능
            PanningOk = 1;
            RaiseEditorContextMenuChangedEvent(false, true);
        };
    }

    #endregion
}