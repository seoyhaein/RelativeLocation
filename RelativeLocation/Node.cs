using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace RelativeLocation;

// TODO 아래 내용 구현해야함.
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
        AvaloniaProperty.Register<Node, Control?>(nameof(ParentControl));

    public Control? ParentControl
    {
        get => GetValue(ParentControlProperty);
        set => SetValue(ParentControlProperty, value);
    }

    // TODO 중요. 아래 내용 잊지말자. 기존 Node(GUID) 와 StartNode(int type), EndNode(int type) 는 다른 ID 쳬계를 가져갈려고 한다. 
    // Id 추가 BaseNode 에 않넣는 이유는 StartNode, EndNode 는 다른 ID 체계로 사용할려고 한다.
    private Guid _id;

    public static readonly DirectProperty<Node, Guid> IdProperty =
        AvaloniaProperty.RegisterDirect<Node, Guid>(
            nameof(Id),
            o => o.Id,
            (o, v) => o.Id = v);

    public Guid Id
    {
        get => _id;
        set => SetAndRaise(IdProperty, ref _id, value);
    }

    #endregion

    #region fields

    // Node 의 움직임을 위해
    private readonly IDisposable _disposable;

    // TODO TranslateTransform 다르게 구현하는 방식을 생각해보자. 시간날때 이렇게 하는거 좀 걸림.
    private TranslateTransform _translateTransform = new TranslateTransform();
    private Point _initialPointerPosition; // 드래그 시작 시 마우스 포인터의 위치
    private Point _initialNodePosition; // 드래그 시작 시 노드의 위치
    private Point _temporaryNewPosition; // 노드의 임시 위치
    private Vector _dragAccumulator; // 드래그 동안의 누적 이동 거리

    // TODO 이름 조정
    private const int GridCellSize = 15; // 그리드 셀 크기, 필요에 따라 조정

    #endregion

    //TODO Node 삭제되는 것도 신경써야 한다.
    #region Constructor

    public Node()
    {
        Focusable = true;
        // 초기 설정에서 TranslateTransform 객체를 RenderTransform으로 설정
        RenderTransform = _translateTransform;
        _disposable = ParentControlProperty.Changed.Subscribe(HandleParentControlChanged);
    }

    public Node(Point location) : this()
    {
        // 생성자에서만 id 설정하도록 하였음.
        //_id = Guid.NewGuid();
        Location = location;
        (SourceAnchor, TargetAnchor) = FindAnchors(location);
    }

    #endregion

    #region Routed Events

    public static readonly RoutedEvent<ConnectionChangedEventArgs> ConnectionChangedEvent =
        RoutedEvent.Register<Node, ConnectionChangedEventArgs>(
            nameof(ConnectionChanged),
            RoutingStrategies.Bubble);

    public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged
    {
        add => AddHandler(ConnectionChangedEvent, value);
        remove => RemoveHandler(ConnectionChangedEvent, value);
    }

    private void RaiseConnectionChangedEvent(Guid? nodeId, Point? location, Point? sourceAnchor, Point? oldSourceAnchor,
        Point? targetAnchor, Point? oldTargetAnchor,
        DAGItemsType dagItemsType)
    {
        var args = new ConnectionChangedEventArgs(ConnectionChangedEvent, nodeId, location, sourceAnchor,
            oldSourceAnchor,
            targetAnchor, oldTargetAnchor,
            dagItemsType);
        RaiseEvent(args);
    }

    #endregion

    #region Evnet Handlers

    protected override void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (ParentControl == null)
            throw new InvalidOperationException("Node cannot move because a DAGlynEditorCanvas parent is not found.");

        if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            //Focus();
            args.Pointer.Capture(this);
            Debug.Print("Dragging Start");
            // 드래그 시작 시의 마우스 포인터 위치와 노드의 현재 위치를 저장.
            _initialPointerPosition = args.GetPosition(ParentControl);
            _initialNodePosition = this.Location; // 현재 노드의 위치를 초기 위치로 설정
            // 여기서 초기화 시켜주는 것이 바람직할 것같다.
            _dragAccumulator = new Vector(); // 드래그 누적 거리 초기화
            IsDragging = true;
            args.Handled = true;
        }
    }

    protected override void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (ParentControl == null)
            throw new InvalidOperationException("Node cannot move because a DAGlynEditorCanvas parent is not found.");

        if (!IsDragging || !this.Equals(args.Pointer.Captured)) return;

        Debug.Print("Dragging...");
        var currentPointerPosition = args.GetPosition(ParentControl);
        // 드래그 시작 위치와 현재 포인터 위치의 차이(delta)를 계산
        var delta = currentPointerPosition - _initialPointerPosition;
        // 노드의 새 위치를 드래그 시작 시 노드 위치 + delta로 계산
        _dragAccumulator += delta;
        // 그리드 크기에 맞추어 효과적인 델타 계산
        var effectiveDelta = new Vector(
            Math.Floor(_dragAccumulator.X / GridCellSize) * GridCellSize,
            Math.Floor(_dragAccumulator.Y / GridCellSize) * GridCellSize);

        if (effectiveDelta != Vector.Zero)
        {
            _translateTransform.X += effectiveDelta.X;
            _translateTransform.Y += effectiveDelta.Y;
            _dragAccumulator -= effectiveDelta; // 적용된 델타만큼 누적 이동 거리 조정
            // 임시 새 위치 계산
            _temporaryNewPosition = new Point(
                _initialNodePosition.X + _translateTransform.X,
                _initialNodePosition.Y + _translateTransform.Y);
        }

        _initialPointerPosition = currentPointerPosition; // 포인터 위치 업데이트
        // TODO oldData 기록할 필요 있을듯
        // 아래와 같이 null check는 해야 하지 않을까??
        Point? oldSourceAnchor = SourceAnchor;
        Point? oldTargetAnchor = TargetAnchor;

        (SourceAnchor, TargetAnchor) = FindAnchors(_temporaryNewPosition);
        // TODO 이렇게 event 에 또 event 를 계속 보내는 것 생각해보자.
        RaiseConnectionChangedEvent(_id, this.Location, SourceAnchor, oldSourceAnchor, TargetAnchor, oldTargetAnchor,
            DAGItemsType.RunnerNode);


        args.Handled = true;
    }

    protected override void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (this.ParentControl == null)
            throw new InvalidOperationException("Node cannot move because a DAGlynEditorCanvas parent is not found.");

        if (sender != null && this.Equals(args.Pointer.Captured) && this.IsDragging)
        {
            Debug.Print("Finish");
            args.Pointer.Capture(null);
            this.IsDragging = false;
            args.Handled = true;
        }
    }

    /*protected override void HandleKeyDown(object? sender, KeyEventArgs args)
    {
        // TODO 현재 IsFocused 이 조건이 필요한지는 살펴봐야 함.
        if (IsFocused)
        {
            var isMatch = EditorGestures.Delete.Matches(args);
            if (isMatch)
            {
                Debug.WriteLine("Match");
                args.Handled = true;
            }
        }

        
    }*/

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
    public override void Dispose(bool disposing)
    {
        _disposable.Dispose();
        base.Dispose(disposing);
    }

    // TODO (Thinking!!) Node 의 width 와 height 는 고정된걸로 처리한다.
    // Node 의 width, height 는 일단 값을 axaml 에 넣어둔다. 향후 이게 정해지면
    // cs 코드에 넣을 예정임. 현재는 Constants 에 넣어 놓기만 해놓았다.
    private (Point sourceAnchor, Point targetAnchor) FindAnchors(Point location)
    {
        var offset = location;

        var sourceAnchorX = offset.X + Constants.NodeWidth;
        var sourceAnchorY = offset.Y + Constants.NodeHeight / 2;

        var targetAnchorX = offset.X;
        var targetAnchorY = offset.Y + Constants.NodeHeight / 2;

        Point sourceAnchor = new Point(sourceAnchorX, sourceAnchorY);
        Point targetAnchor = new Point(targetAnchorX, targetAnchorY);

        return (sourceAnchor, targetAnchor);
    }

    // TODO nullable 정리하자.
    // 이거 버그 있음.
    // FindAnchors 로 해도 될듯하다.
    // 삭제 예정.
    private (Point? updatedInAnchor, Point? updatedOutAnchor) UpdateAnchors(Point? inAnchor, Point? outAnchor,
        Point finalPoint)
    {
        if (!inAnchor.HasValue || !outAnchor.HasValue)
            return (null, null);

        Point updatedInAnchor = new Point(inAnchor.Value.X + finalPoint.X, inAnchor.Value.Y + finalPoint.Y);
        Point updatedOutAnchor = new Point(outAnchor.Value.X + finalPoint.X, outAnchor.Value.Y + finalPoint.Y);

        return (updatedInAnchor, updatedOutAnchor);
    }
}