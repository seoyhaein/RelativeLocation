using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace RelativeLocation;

/*
 * 새롭게 Reload 할 경우 위치에 대한 변경을 해줘야 한다.
 * Location 이 최종적으로 변할때 이것을 기록하고 UI 변경사항을 남겨야 한다.
 * InValid~~ Transform 은 시각적인 위치만 변경시킨다. 다시 reload 할 경우 원래대로 돌아온다.
 */

/// <summary>
/// 속성 변경에 따른 메서드로 처리하는게 나을지 고민해야함. DAGlynEditorCanvas 와 비교해봐야 함.
/// 중복되는 기능들은 통합할 필요 있음.
/// </summary>

public class Node : BaseNode
{
    #region Dependency Properties
    
    public static readonly StyledProperty<Control?> ParentControlProperty =
        AvaloniaProperty.Register<BaseNode, Control?>(nameof(ParentControl));

    public Control? ParentControl
    {
        get => GetValue(ParentControlProperty);
        set => SetValue(ParentControlProperty, value);
    }
    #endregion
    
    #region fields
    
    // Node 의 움직임을 위해
    private TranslateTransform _translateTransform = new TranslateTransform();
    private readonly IDisposable _disposable;
    
    #endregion
    
    //TODO Node 삭제되는 것도 신경써야 한다.
    #region Constructor

    public Node()
    {
        // 초기 설정에서 TranslateTransform 객체를 RenderTransform으로 설정
        this.RenderTransform = _translateTransform;
        _disposable = ParentControlProperty.Changed.Subscribe(HandleParentControlChanged);
    }

    public Node(Point location) : this()
    {
        this.Location = location;
        (InAnchor, OutAnchor) = FindAnchors(location);
    }
    
    #endregion

    #region Evnet Handlers

    protected override void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if(this.ParentControl == null)
            throw new InvalidOperationException("Node cannot move because a DAGlynEditorCanvas parent is not found.");
        
        // 불필요한 조건 검사 제거
        if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender != null)
        {
            Focus();
            args.Pointer.Capture(this);
            Debug.Print("Dragging Start");

            InitialPointerPosition = args.GetPosition(ParentControl);
            PreviousPointerPosition = InitialPointerPosition;
            this.IsDragging = true;
            args.Handled = true;
        }
    }

    protected override void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if(this.ParentControl == null)
            throw new InvalidOperationException("Node cannot move because a DAGlynEditorCanvas parent is not found.");
        
        if (sender == null || !this.IsDragging || !this.Equals(args.Pointer.Captured)) return;

        Debug.Print("Dragging...");
        CurrentPointerPosition = args.GetPosition(ParentControl);
        var delta = CurrentPointerPosition - PreviousPointerPosition;

        // 드래그 임계값 검사
        // TODO 현재 이동 위치에 대한 최종값을 저장하는 변수를 설정한다.
        // 이거 정리 필요.
        if (((Vector)delta).SquaredLength  > Constants.AppliedThreshold)
        {
            // TODO location + delta 값을 저장하고 있지 않고 있다. 다른값을 저장하고 있다.
            // 이거 버그 생각해야함.
            NodeMove(this.Location + delta); // SetLocation 메소드를 통한 위치 업데이트
            //TODO 일단 이렇게 넣어두었음. 새로운 node 위치임.
            FinalPosition = Location + delta;
            PreviousPointerPosition = CurrentPointerPosition;
            args.Handled = true;
        }
    }

    protected override void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if(this.ParentControl == null)
            throw new InvalidOperationException("Node cannot move because a DAGlynEditorCanvas parent is not found.");
        
        if (sender != null && this.Equals(args.Pointer.Captured) && this.IsDragging)
        {
            // Anchors update
            (InAnchor, OutAnchor) = UpdateAnchors(InAnchor, OutAnchor, FinalPosition);
            // TODO InAnchor, OutAnchor 는 null 일 수 없다. 이거 정리하자. 일단 이렇게 둠.
            if (InAnchor == null || OutAnchor == null)
            {
                throw new InvalidOperationException("InAnchor and OutAnchor must be updated successfully and cannot be null after moving the node.");
            }
            
            Debug.Print("Finish");
            args.Pointer.Capture(null);
            this.IsDragging = false;
            args.Handled = true;
        }
    }
    
    // TODO 향후 삭제 예정.
    protected override void HandleLoaded(object? sender, RoutedEventArgs args)
    {
        // 테스트 메서드
        // SetAnchor();
    }

    private void HandleParentControlChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is DAGlynEditorCanvas editorCanvas)
            this.ParentControl = editorCanvas;
        else
            ParentControl = this.GetParentVisualOfType<DAGlynEditorCanvas>();
    }

    #endregion
    
    #region Methods

    private void NodeMove(Point point)
    {
        Location = point;
        _translateTransform.X = point.X;
        _translateTransform.Y = point.Y;
        
        /*//Anchors update
        (InAnchor, OutAnchor)= UpdateAnchors(InAnchor, OutAnchor, point);
        if (InAnchor == null || OutAnchor == null)
            throw new InvalidOperationException("InAnchor and OutAnchor should not be null");*/
    }

    #endregion
    
    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.ParentControl = this.GetParentVisualOfType<DAGlynEditorCanvas>();
     
    }
   
    public bool CanNodeMove()
    {
        var parentControl = this.GetParentVisualOfType<DAGlynEditorCanvas>();
        if (parentControl != null)
        {
            this.ParentControl = parentControl;
            return true;
        }
        else
        {
            this.ParentControl = null;
            return false;
        }
    }

    public void SetLocation(Point location)
    {
        this.Location = location;
    }
    
    // TODO 살펴보자.
    protected override void Dispose(bool disposing)
    {
        _disposable.Dispose();
        base.Dispose(disposing);
    }
    
    // TODO (Thinking!!) Node 의 width 와 height 는 고정된걸로 처리한다.
    // Node 의 width, height 는 일단 값을 axaml 에 넣어둔다. 향후 이게 정해지면
    // cs 코드에 넣을 예정임. 현재는 Constants 에 넣어 놓기만 해놓았다.
    private (Point inAnchor, Point outAnchor) FindAnchors(Point location)
    {
        var offset = location;

        var inAnchorX = offset.X;
        var inAnchorY = offset.Y + Constants.NodeHeight / 2;
        
        var outAnchorX = offset.X + Constants.NodeWidth;
        var outAnchorY = offset.Y + Constants.NodeHeight / 2;

        Point inAnchor = new Point(inAnchorX, inAnchorY);
        Point outAnchor = new Point(outAnchorX, outAnchorY);
    
        return (inAnchor, outAnchor);
    }
    
    // TODO nullable 정리하자.
    private (Point? updatedInAnchor, Point? updatedOutAnchor) UpdateAnchors(Point? inAnchor, Point? outAnchor, Point finalPoint)
    {
        if (!inAnchor.HasValue || !outAnchor.HasValue)
            // inAnchor 또는 outAnchor 중 하나라도 null이면, null 튜플을 반환
            return (null, null);

        Point updatedInAnchor = new Point(inAnchor.Value.X + finalPoint.X, inAnchor.Value.Y + finalPoint.Y);
        Point updatedOutAnchor = new Point(outAnchor.Value.X + finalPoint.X, outAnchor.Value.Y + finalPoint.Y);

        return (updatedInAnchor, updatedOutAnchor);
    }
}