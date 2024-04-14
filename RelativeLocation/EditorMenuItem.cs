using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace RelativeLocation;

public class EditorMenuItem : MenuItem 
{
    protected override Type StyleKeyOverride => typeof(MenuItem);

    static EditorMenuItem()
    {
        BackgroundProperty.OverrideDefaultValue<EditorMenuItem>(Brushes.Brown);
    }
}