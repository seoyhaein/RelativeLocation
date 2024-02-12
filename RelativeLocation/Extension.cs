using Avalonia;
using System;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace RelativeLocation;

public static class Extension
{
    #region Static Methods
    
    // Subscribe 를 static 에서 사용하기 위해서
    public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> action)
    {
        return observable.Subscribe(new AnonymousObserver<T>(action));
    }
    public static T? GetParentControlOfType<T>(this Control child) where T : Control
    {
        var current = child;

        while (current != null)
        {
            if (current is T target)
            {
                return target;
            }
            current = current.GetVisualParent() as Control;
        }

        return default;
    }
    
    public static T? GetParentVisualOfType<T>(this Visual child) where T : Visual
    {
        var current = child;

        while (current != null)
        {
            if (current is T target)
            {
                return target;
            }
            current = current.GetVisualParent();
        }

        return default;
    }
    
    public static T? GetElementUnderMouse<T>(this Visual container, Point pointerPosition) where T : Visual
    {
        foreach (var visual in container.GetVisualDescendants())
        {
            if (visual.Bounds.Contains(pointerPosition) && visual is T foundElement)
            {
                return foundElement;
            }
        }

        return null;
    }
    
    #endregion
}