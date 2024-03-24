using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RelativeLocation;

public partial class EditorContextMenu : UserControl
{
    public EditorContextMenu()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}