using Avalonia.Input;
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace RelativeLocation;

// TODO 코드 정리 필요. 
public sealed class StartConnector : Connector
{
    protected override Type StyleKeyOverride => typeof(Connector);

    #region Dependency Properties

    // connector 에 넣을까 하다가 그냥 여기다 넣음.
    // TODO 여기서 DirectProperty 안 쓰고, AvaloniaProperty 를 쓴 이유는 외부에서 데이터를 설정해야 하기때문이다.
    // 한번 테스트 해보자. (시간날때.)
    public static readonly StyledProperty<Guid> NodeIdProperty =
        AvaloniaProperty.Register<StartConnector, Guid>(nameof(NodeId));

    public Guid NodeId
    {
        get => GetValue(NodeIdProperty);
        set => SetValue(NodeIdProperty, value);
    }

    #endregion

    #region Constructor

    static StartConnector()
    {
        FillProperty.OverrideDefaultValue<StartConnector>(BrushResources.StartConnectorDefaultFill);
    }

    #endregion

    #region Fields

    // Connector 밖으로 한번이라도 나가면 true 가 된다.
    private bool _outSideOutConnector;

    #endregion

    #region Evnet Handlers

    protected override void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (!args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (sender == null) return;

        Focus();
        args.Pointer.Capture(this);

        if (this.Equals(args.Pointer.Captured))
        {
            Debug.Print("Pointer Pressed");
            this.PreviousConnector = this;
            this.IsPointerPressed = true; // 마우스 눌림 상태 설정
            args.Handled = true; // 이벤트 전파를 막음.
            var parent = this.GetParentVisualByName<Canvas>("PART_TopLayer");
            if (parent == null) return;
            RaiseConnectionStartEvent(this, Anchor);
        }
    }
    
    protected override void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (sender == null || !this.IsPointerPressed || this.PreviousConnector == null) return;

        // PART_TopLayer 는 DAGlynEditor.axaml 에 있다. 이녀석이 없으면 기능을 하지 않는다.
        var parent = this.GetParentVisualByName<Canvas>("PART_TopLayer");
        if (parent == null) return;
        var currentPosition = args.GetPosition(parent);

        // 마우스 이동중 새로운 Connector 에 들어가면 null 이 아님.
        var elementUnderPointer = parent.GetControlUnderPointer<Connector>(currentPosition);

        // 한번이라도 OutConnector 를 나가면.

        if (elementUnderPointer == null && this.IsPointerPressed)
        {
            // 1. 정상적인 Moved
            //TODO 여기서 Captured 와 비교할 필요가 있을까? 일단 해줌. 아래 조건문을 할 필요가 없을 듯 하다.
            if (this.PreviousConnector.Equals(args.Pointer.Captured) && this.Equals(args.Pointer.Captured))
            {
                _outSideOutConnector = true;
                // TODO 여기 버그 있음. 좌표계
                RaiseConnectionDragEvent(this, Anchor, (Vector)currentPosition);
                Debug.Print("정상 Moved");
            }
        }

        // 2` 자신으로 돌아온 경우
        // OutConnector 내에서의 이동도 생각해야한다.
        if (elementUnderPointer != null && elementUnderPointer.Equals(this.PreviousConnector) && this.IsPointerPressed)
        {
            if (_outSideOutConnector && this.PreviousConnector.Equals(args.Pointer.Captured))
            {
                RaiseConnectionDragEvent(this, Anchor, (Vector)currentPosition);
                Debug.Print("그냥 외곽에 있다가 다시 들어간 경우");
            }

            if (!_outSideOutConnector && this.PreviousConnector.Equals(args.Pointer.Captured))
            {
                RaiseConnectionDragEvent(this, Anchor, (Vector)currentPosition);
                Debug.Print("OutConnector 내에서의 마우스 이동.");
            }
        }

        // 2`` 다른 Connector로 들어온 경우
        if (elementUnderPointer != null && !elementUnderPointer.Equals(this.PreviousConnector) && this.IsPointerPressed)
        {
            //if (this.PreviousConnector.Equals(args.Pointer.Captured))
            this.PreviousConnector = elementUnderPointer;
            RaiseConnectionDragEvent(this, Anchor, (Vector)currentPosition);
            Debug.Print("다른 Connector로 들어온 경우");
        }

        // 2``` 다른 Connector 에 들어 갔다가 다시 자신으로 돌아온 경우
        if (elementUnderPointer != null && elementUnderPointer.Equals(this.PreviousConnector) && this.IsPointerPressed)
        {
            if (!this.PreviousConnector.Equals(args.Pointer.Captured))
            {
                RaiseConnectionDragEvent(this, Anchor, (Vector)currentPosition);
                Debug.Print("다른 Connector 에 들어 갔다가 다시 자신으로 돌아온 경우");
            }
        }

        args.Handled = true;
    }
    
    protected override void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (sender == null) return;
        // TODO PreviousConnector 검사 필요한지 생각해보자.
        // 두가지를 생각해야 한다. InConnector 에서 Release 되었는지 아닌지.
        if (this.Equals(args.Pointer.Captured) && this.IsPointerPressed)
        {
            // TODO 이 코드가 여기 꼭 있어야 하는지 생각하자.
            // PART_TopLayer 는 DAGlynEditor.axaml 에 있다. 이녀석이 없으면 기능을 하지 않는다.
            var parent = this.GetParentVisualByName<Canvas>("PART_TopLayer");
            if (parent == null) return;

            var currentPosition = args.GetPosition(parent);
            // 마우스 이동중 새로운 Connector 에 들어가면 null 이 아님.
            var elementUnderPointer = parent.GetControlUnderPointer<Connector>(currentPosition);
            if (elementUnderPointer is EndConnector okConnector)
            {
                okConnector.Focus();
                Debug.Print(" InConnector Pointer Released");
                this.IsPointerPressed = false; // 마우스 눌림 상태 해제
                args.Pointer.Capture(null);
                args.Handled = true;
                PreviousConnector = null;
                _outSideOutConnector = false;
                // TODO InNodeId, OutNodeId 이름 수정 후 그 후에 이것들이 필요한지 생각한다.
                RaiseConnectionCompletedEvent(okConnector, Anchor, NodeId, okConnector.Anchor, okConnector.NodeId);
            }
            else
            {
                Debug.Print("Not InConnectorPointer Released");
                this.IsPointerPressed = false; // 마우스 눌림 상태 해제
                args.Pointer.Capture(null);
                args.Handled = true;
                PreviousConnector = null;
                _outSideOutConnector = false;
                // TODO 일단  이부분도 생각해봐야 한다.
                RaiseConnectionCompletedEvent(null, null, null, null, null);
            }
        }
    }

    #endregion

    #region Methods

    // Raise events
    /// <summary>
    /// OutConnector 에서 Connection 시작할때 PendingConnection 에 필요한 데이터 이벤트로 전달.
    /// </summary>
    /// <param name="connector">전달되는 값은 OutConnector 여야 함.</param>
    /// <param name="anchor">이벤트 발생시텀에서의 위치값. (이후 조정되어야 함.)</param>
    protected override void RaiseConnectionStartEvent(Connector? connector, Point? anchor)
    {
        var args = new PendingConnectionEventArgs(PendingConnectionStartedEvent, connector, anchor);
        RaiseEvent(args);
    }

    /// <summary>
    /// OutConnector 에서 Dragging 할때 PendingConnection 에 필요한 데이터 이벤트로 전달.
    /// </summary>
    /// <param name="connector">전달되는 값은 OutConnector 여야 함.</param>
    /// <param name="anchor">connection 이 시작될때의 위치값이다.</param>
    /// <param name="offset">이동 Vector 값 (이후 조정되어야 함.)</param>
    protected override void RaiseConnectionDragEvent(Connector? connector, Point? anchor, Vector? offset)
    {
        var args = new PendingConnectionEventArgs(PendingConnectionDragEvent, connector, anchor, offset);
        RaiseEvent(args);
    }

    protected override void RaiseConnectionCompletedEvent(Connector? connector, Point? startAnchor, Guid? inNodeId,
        Point? endAnchor, Guid? outNodeId)
    {
        var args = new PendingConnectionEventArgs(PendingConnectionCompletedEvent, connector, startAnchor, inNodeId,
            endAnchor, outNodeId);
        RaiseEvent(args);
    }

    #endregion

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Debug.WriteLine(Anchor.ToString());
    }
}