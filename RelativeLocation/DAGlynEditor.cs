using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RelativeLocation;

public class DAGlynEditor : SelectingItemsControl, IDisposable
    {
        // TODO Threshold 값은 나중에 따로 빼놓는 방향으로.
        public static double HandleRightClickAfterPanningThreshold { get; set; } = 12d;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        #region Dependency Properties
        
        // TODO 이게 필요한지 살펴보자.
        public static readonly StyledProperty<Point> ViewportLocationProperty =
            AvaloniaProperty.Register<DAGlynEditor, Point>(nameof(ViewportLocation));

        public Point ViewportLocation
        {
            get => GetValue(ViewportLocationProperty);
            set => SetValue(ViewportLocationProperty, value);
        }

        // readonly
        // MouseLocation 만들어줌. 
        // https://docs.avaloniaui.net/docs/next/guides/custom-controls/how-to-create-advanced-custom-controls#datavalidation-support
        public static readonly DirectProperty<DAGlynEditor, Point> MouseLocationProperty =
            AvaloniaProperty.RegisterDirect<DAGlynEditor, Point>(
                nameof(MouseLocation),
                o => o.MouseLocation);

        private Point _mouseLocation;

        public Point MouseLocation
        {
            get => _mouseLocation;
            private set => SetAndRaise(MouseLocationProperty, ref _mouseLocation, value);
        }
        
        public static readonly StyledProperty<bool> DisablePanningProperty =
            AvaloniaProperty.Register<DAGlynEditor, bool>(nameof(DisablePanning), false);

        public bool DisablePanning
        {
            get => GetValue(DisablePanningProperty);
            set => SetValue(DisablePanningProperty, value);
        }
        
        public static readonly DirectProperty<DAGlynEditor, bool> IsPanningProperty =
            AvaloniaProperty.RegisterDirect<DAGlynEditor, bool>(
                nameof(IsPanning),
                o => o.IsPanning);
        
        private bool _isPanning = false;

        public bool IsPanning
        {
            get => _isPanning;
            protected internal set => SetAndRaise(IsPanningProperty, ref _isPanning, value);
        }
        
        public static readonly DirectProperty<DAGlynEditor, bool> IsSelectingProperty =
            AvaloniaProperty.RegisterDirect<DAGlynEditor, bool>(
                nameof(IsSelecting),
                o => o.IsSelecting);

        private bool _isSelecting;

        public bool IsSelecting
        {
            get => _isSelecting;
            internal set => SetAndRaise(IsSelectingProperty, ref _isSelecting, value);
        }
        
        public static readonly StyledProperty<bool> EnableRealtimeSelectionProperty =
            AvaloniaProperty.Register<DAGlynEditor, bool>(nameof(EnableRealtimeSelection), false);

        public bool EnableRealtimeSelection
        {
            get => GetValue(EnableRealtimeSelectionProperty);
            set => SetValue(EnableRealtimeSelectionProperty, value);
        }
        
        public static readonly DirectProperty<DAGlynEditor, Rect> SelectedAreaProperty =
            AvaloniaProperty.RegisterDirect<DAGlynEditor, Rect>(
                nameof(SelectedArea),
                o => o.SelectedArea);

        private Rect _selectedArea;

        public Rect SelectedArea
        {
            get => _selectedArea;
            internal set => SetAndRaise(SelectedAreaProperty, ref _selectedArea, value);
        }
        
        public static readonly DirectProperty<DAGlynEditor, bool?> IsPreviewingSelectionProperty =
            AvaloniaProperty.RegisterDirect<DAGlynEditor, bool?>(
                nameof(IsPreviewingSelection),
                o => o.IsPreviewingSelection);

        private bool? _isPreviewingSelection;

        public bool? IsPreviewingSelection
        {
            get => _isPreviewingSelection;
            internal set => SetAndRaise(IsPreviewingSelectionProperty, ref _isPreviewingSelection, value);
        }
        
        #endregion

        #region Constructors

        public DAGlynEditor()
        {
            InitializeSubscriptions();
        }
        #endregion
        
        #region Event Handlers

        private void InitializeSubscriptions()
        {
            Observable.FromEventPattern<PointerPressedEventArgs>(
                    h => this.PointerPressed += h,
                    h => this.PointerPressed -= h)
                .Subscribe(args => HandlePointerPressed(args.EventArgs))
                .DisposeWith(_disposables);

            Observable.FromEventPattern<PointerEventArgs>(
                    h => this.PointerMoved += h,
                    h => this.PointerMoved -= h)
                .Subscribe(args => HandlePointerMoved(args.EventArgs))
                .DisposeWith(_disposables);

            Observable.FromEventPattern<PointerReleasedEventArgs>(
                    h => this.PointerReleased += h,
                    h => this.PointerReleased -= h)
                .Subscribe(args => HandlePointerReleased(args.EventArgs))
                .DisposeWith(_disposables);
        }

        // 이후 더 자세히 살펴보자 by seoy
        private void HandlePointerPressed(PointerPressedEventArgs args)
        {
            if (args.Pointer.Captured != null)
                args.Pointer.Capture(null);

            Focus();
            args.Pointer.Capture(this);

            UpdateMouseLocation(args);
            args.Handled = true;
        }

        private void HandlePointerMoved(PointerEventArgs args)
        {
            UpdateMouseLocation(args);
            args.Handled = true;
        }

        private void HandlePointerReleased(PointerReleasedEventArgs args)
        {
            UpdateMouseLocation(args);
            args.Pointer.Capture(null);
            args.Handled = true;
        }

        #endregion
        
        #region Methods

        private void UpdateMouseLocation(PointerEventArgs args)
        {
            MouseLocation = args.GetPosition(this);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
        }

        // 컨테이너 생성
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return base.CreateContainerForItemOverride(item,index,recycleKey);
        }

        // 컨테이너 설정
        // 컨테이너를 바로 Node 로 설정해주는 것도 가능할 것으로 판단된다.
        // 일단 주석으로 남겨 놓았는데, 움직임을 구현하기 위해서는 남겨놓는다.
        protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
        {
            return;
        }

        #endregion
        
    }