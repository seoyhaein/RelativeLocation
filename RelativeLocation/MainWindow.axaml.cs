using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;

namespace RelativeLocation;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    public AvaloniaList<TestConnector> ConItems { get; set; } = new AvaloniaList<TestConnector>
    {
        new TestConnector
        {
            Location = new Point(0, 0),
            /*W = 100,
            H = 100,
            Br = Brushes.Red,*/
            ConType = ConnectorType.OutConnector
        },
        new TestConnector
        {
            Location = new Point(200, 200),
            /*W = 150,
            H = 150,
            Br = Brushes.Blue,*/
            ConType = ConnectorType.InConnector
        }
    };
}

public enum ConnectorType
{
    InConnector,
    OutConnector
}

public class TestConnector
{
    public Point Location { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    public IImmutableSolidColorBrush Br { get; set; }

    public ConnectorType ConType { get; set; }


}