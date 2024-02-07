using Avalonia;
using Avalonia.Input;
using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RelativeLocation;

/*
 * InConnector 의 경우는 line 이 들어가는 Connector 임, 즉 Target 임.
 * 
 */

public class InConnector : Connector
{
    protected override Type StyleKeyOverride => typeof(Connector);
    
    private void HandleStarted(object? sender, PendingConnectionEventArgs e)
    {
        // sender 따라 처리   
    }

    private void HandleDrag(object? sender, PendingConnectionEventArgs e)
    {
    }

    private void HandleCompleted(object? sender, PendingConnectionEventArgs e)
    {
    }
}