using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RelativeLocation;

public partial class EditorContextMenu1 : UserControl
{
    #region Fields

    public AvaloniaList<string> ContextMenuSource { get; set; } = new AvaloniaList<string>();

    #endregion
    
    
    #region Constructors

    public EditorContextMenu1()
    {
        InitializeComponent();
        DataContext = this;
        ContextMenuSource.Add("item1");
    }

    #endregion
    
    #region Methods

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    #endregion
}