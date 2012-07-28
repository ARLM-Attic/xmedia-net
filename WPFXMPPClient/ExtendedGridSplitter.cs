using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WPFXMPPClient
{
    /// <summary>
    /// Specifies different collapse modes of a ExtendedGridSplitter.
    /// </summary>
    public enum GridSplitterCollapseMode
    {
        /// <summary>
        /// The ExtendedGridSplitter cannot be collapsed or expanded.
        /// </summary>
        None = 0,
        /// <summary>
        /// The column (or row) to the right (or below) the
        /// splitter's column, will be collapsed.
        /// </summary>
        Next = 1,
        /// <summary>
        /// The column (or row) to the left (or above) the
        /// splitter's column, will be collapsed.
        /// </summary>
        Previous = 2
    }

    /// <summary>
    /// An updated version of the standard ExtendedGridSplitter control that includes a centered handle
    /// which allows complete collapsing and expanding of the appropriate grid column or row.
    /// </summary>
    [TemplatePart(Name = ExtendedGridSplitter.ELEMENT_HORIZONTAL_HANDLE_NAME, Type = typeof(ToggleButton))]
    [TemplatePart(Name = ExtendedGridSplitter.ELEMENT_VERTICAL_HANDLE_NAME, Type = typeof(ToggleButton))]
    [TemplatePart(Name = ExtendedGridSplitter.ELEMENT_HORIZONTAL_TEMPLATE_NAME, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ExtendedGridSplitter.ELEMENT_VERTICAL_TEMPLATE_NAME, Type = typeof(FrameworkElement))]

    public class ExtendedGridSplitter : System.Windows.Controls.GridSplitter
    {

        #region TemplateParts

        private const string ELEMENT_HORIZONTAL_HANDLE_NAME = "HorizontalGridSplitterHandle";
        private const string ELEMENT_VERTICAL_HANDLE_NAME = "VerticalGridSplitterHandle";
        private const string ELEMENT_HORIZONTAL_TEMPLATE_NAME = "HorizontalTemplate";
        private const string ELEMENT_VERTICAL_TEMPLATE_NAME = "VerticalTemplate";
        private const string ELEMENT_GRIDSPLITTER_BACKGROUND = "GridSplitterBackground";

        private ToggleButton _elementHorizontalGridSplitterButton;
        private ToggleButton _elementVerticalGridSplitterButton;
        private Rectangle _elementGridSplitterBackground;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets a value that indicates the CollapseMode.
        /// </summary>
        public GridSplitterCollapseMode CollapseMode
        {
            get { return (GridSplitterCollapseMode)GetValue(CollapseModeProperty); }
            set { SetValue(CollapseModeProperty, value); }
        }
        /// <summary>
        /// Identifies the CollapseMode dependency property
        /// </summary>
        public static readonly DependencyProperty CollapseModeProperty =
            DependencyProperty.Register(
                "CollapseMode",
                typeof(GridSplitterCollapseMode),
                typeof(ExtendedGridSplitter),
                new PropertyMetadata(GridSplitterCollapseMode.None, new PropertyChangedCallback(OnCollapseModePropertyChanged))
                );

        /// <summary>
        /// Gets or sets the style that customizes the appearance of the horizontal handle 
        /// that is used to expand and collapse the ExtendedGridSplitter.
        /// </summary>
        public Style HorizontalHandleStyle
        {
            get { return (Style)GetValue(HorizontalHandleStyleProperty); }
            set { SetValue(HorizontalHandleStyleProperty, value); }
        }
        /// <summary>
        /// Identifies the HorizontalHandleStyle dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalHandleStyleProperty =
            DependencyProperty.Register(
                "HorizontalHandleStyle",
                typeof(Style),
                typeof(ExtendedGridSplitter),
                null
                );

        /// <summary>
        /// Gets or sets the style that customizes the appearance of the horizontal handle 
        /// that is used to expand and collapse the ExtendedGridSplitter.
        /// </summary>
        public Orientation SplitterOrientation
        {
            get { return (Orientation)GetValue(SplitterOrientationProperty); }
            set
            {
                SetValue(SplitterOrientationProperty, value);
                //if (value == System.Windows.Controls.Orientation.Horizontal)
                //{
                //    SetValue(HeightProperty, 50.0);
                //    SetValue(WidthProperty, 50.0);
                //}
                //if (value == System.Windows.Controls.Orientation.Vertical)
                //{
                //    SetValue(WidthProperty, 0.0);
                //    SetValue(HeightProperty, 50.0);
                //}
            }
        }
        /// <summary>
        /// Identifies the HorizontalHandleStyle dependency property
        /// </summary>
        public static readonly DependencyProperty SplitterOrientationProperty =
            DependencyProperty.Register(
                "SplitterOrientation",
                typeof(Orientation),
                typeof(ExtendedGridSplitter),
                null
                );

        private double m_Dimension = 10.0;

        public double Dimension
        {
            get
            {
                if (DimensionProperty == null)
                    SetValue(DimensionProperty, m_Dimension);
                return (double)GetValue(DimensionProperty);
            }
            set
            {
                SetValue(DimensionProperty, value);
            }
        }
        /// <summary>
        /// Identifies the Dimension dependency property
        /// </summary>
        public static readonly DependencyProperty DimensionProperty =
            DependencyProperty.Register(
                "Dimension",
                typeof(double),
                typeof(ExtendedGridSplitter),
                null
                );

        ///<summary>
        /// Gets or sets the style that customizes the appearance of the vertical handle 
        /// that is used to expand and collapse the ExtendedGridSplitter.
        /// </summary>
        public Style VerticalHandleStyle
        {
            get { return (Style)GetValue(VerticalHandleStyleProperty); }
            set { SetValue(VerticalHandleStyleProperty, value); }
        }
        /// <summary>
        /// Identifies the VerticalHandleStyle dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalHandleStyleProperty =
            DependencyProperty.Register(
                "VerticalHandleStyle",
                typeof(Style),
                typeof(ExtendedGridSplitter),
                null
                );

        // <summary>
        /// Gets or sets a value that indicates if the collapse and
        /// expanding actions should be animated.
        /// </summary>
        public bool IsAnimated
        {
            get { return (bool)GetValue(IsAnimatedProperty); }
            set { SetValue(IsAnimatedProperty, value); }
        }
        /// <summary>
        /// Identifies the VerticalHandleStyle dependency property
        /// </summary>
        public static readonly DependencyProperty IsAnimatedProperty =
            DependencyProperty.Register(
                "IsAnimated",
                typeof(bool),
                typeof(ExtendedGridSplitter),
                null
                );


        /// <summary>
        /// Gets or sets a value that indicates if the target column is 
        /// currently collapsed.
        /// </summary>
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        /// <summary>
        /// Identifies the IsCollapsed dependency property
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register(
                "IsCollapsed",
                typeof(bool),
                typeof(ExtendedGridSplitter),
                new PropertyMetadata(new PropertyChangedCallback(OnIsCollapsedPropertyChanged))
                );

        /// <summary>
        /// The IsCollapsed property porperty changed handler.
        /// </summary>
        /// <param name="d">ExtendedGridSplitter that changed IsCollapsed.</param>
        /// <param name="e">An instance of DependencyPropertyChangedEventArgs.</param>
        private static void OnIsCollapsedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedGridSplitter s = d as ExtendedGridSplitter;

            bool value = (bool)e.NewValue;
            s.OnIsCollapsedChanged(value);
        }

        /// <summary>
        /// The CollapseModeProperty property changed handler.
        /// </summary>
        /// <param name="d">ExtendedGridSplitter that changed IsCollapsed.</param>
        /// <param name="e">An instance of DependencyPropertyChangedEventArgs.</param>
        private static void OnCollapseModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtendedGridSplitter s = d as ExtendedGridSplitter;

            GridSplitterCollapseMode value = (GridSplitterCollapseMode)e.NewValue;
            s.OnCollapseModeChanged(value);
        }

        #endregion

        #region Local Members

        private GridCollapseDirection _gridCollapseDirection = GridCollapseDirection.Auto;
        private GridLength _savedGridLength;
        private double _savedActualValue;
        private double _animationTimeMillis = 200;

        #endregion

        #region Control Instantiation

        /// <summary>
        /// Initializes a new instance of the ExtendedGridSplitter class,
        /// which inherits from System.Windows.Controls.ExtendedGridSplitter.
        /// </summary>
        public ExtendedGridSplitter()
            : base()
        {
            // Set default values
            DefaultStyleKey = typeof(ExtendedGridSplitter);

            CollapseMode = GridSplitterCollapseMode.None;
            IsAnimated = true;
            this.LayoutUpdated += delegate { _gridCollapseDirection = GetCollapseDirection(); };

            // All ExtendedGridSplitter visual states are handled by the parent GridSplitter class.
        }

        /// <summary>
        /// This method is called when the tempalte should be applied to the control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _elementHorizontalGridSplitterButton = GetTemplateChild(ELEMENT_HORIZONTAL_HANDLE_NAME) as ToggleButton;
            _elementVerticalGridSplitterButton = GetTemplateChild(ELEMENT_VERTICAL_HANDLE_NAME) as ToggleButton;
            _elementGridSplitterBackground = GetTemplateChild(ELEMENT_GRIDSPLITTER_BACKGROUND) as Rectangle;

            // Wire up the Checked and Unchecked events of the HorizontalGridSplitterHandle.
            if (_elementHorizontalGridSplitterButton != null)
            {
                _elementHorizontalGridSplitterButton.Checked += new RoutedEventHandler(GridSplitterButton_Checked);
                _elementHorizontalGridSplitterButton.Unchecked += new RoutedEventHandler(GridSplitterButton_Unchecked);
            }

            // Wire up the Checked and Unchecked events of the VerticalGridSplitterHandle.
            if (_elementVerticalGridSplitterButton != null)
            {
                _elementVerticalGridSplitterButton.Checked += new RoutedEventHandler(GridSplitterButton_Checked);
                _elementVerticalGridSplitterButton.Unchecked += new RoutedEventHandler(GridSplitterButton_Unchecked);
            }

            // Set default direction since we don't have all the components layed out yet.
            _gridCollapseDirection = GridCollapseDirection.Auto;

            // Directely call these events so design-time view updates appropriately
            OnCollapseModeChanged(CollapseMode);
            OnIsCollapsedChanged(IsCollapsed);
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Handles the property change event of the IsCollapsed property.
        /// </summary>
        /// <param name="isCollapsed">The new value for the IsCollapsed property.</param>
        protected virtual void OnIsCollapsedChanged(bool isCollapsed)
        {
            // Determine if we are dealing with a vertical or horizontal splitter.
            if (_gridCollapseDirection == GridCollapseDirection.Rows)
            {
                if (_elementHorizontalGridSplitterButton != null)
                {
                    // Sets the target ToggleButton's IsChecked property equal
                    // to the provided isCollapsed property.
                    _elementHorizontalGridSplitterButton.IsChecked = isCollapsed;
                }
            }
            else
            {
                if (_elementVerticalGridSplitterButton != null)
                {
                    // Sets the target ToggleButton's IsChecked property equal
                    // to the provided isCollapsed property.
                    _elementVerticalGridSplitterButton.IsChecked = isCollapsed;
                }
            }
        }

        /// <summary>
        /// Handles the property change event of the CollapseMode property.
        /// </summary>
        /// <param name="collapseMode">The new value for the CollapseMode property.</param>
        protected virtual void OnCollapseModeChanged(GridSplitterCollapseMode collapseMode)
        {
            // Hide the handles if the CollapseMode is set to None.
            if (collapseMode == GridSplitterCollapseMode.None)
            {
                if (_elementHorizontalGridSplitterButton != null)
                {
                    _elementHorizontalGridSplitterButton.Visibility = Visibility.Collapsed;
                }
                if (_elementVerticalGridSplitterButton != null)
                {
                    _elementVerticalGridSplitterButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Ensure the handles are Visible.
                if (_elementHorizontalGridSplitterButton != null)
                {
                    _elementHorizontalGridSplitterButton.Visibility = Visibility.Visible;
                }
                if (_elementVerticalGridSplitterButton != null)
                {
                    _elementVerticalGridSplitterButton.Visibility = Visibility.Visible;
                }

                //TODO:  Add in error handling if current template does not include an existing ScaleTransform.

                // Rotate the direction that the handle is facing depending on the CollapseMode.
                if (collapseMode == GridSplitterCollapseMode.Previous)
                {
                    if (_elementHorizontalGridSplitterButton != null)
                    {
                        _elementHorizontalGridSplitterButton.RenderTransform.SetValue(ScaleTransform.ScaleYProperty, -1.0);
                    }
                    if (_elementVerticalGridSplitterButton != null)
                    {
                        _elementVerticalGridSplitterButton.RenderTransform.SetValue(ScaleTransform.ScaleXProperty, -1.0);
                    }
                }
                else if (collapseMode == GridSplitterCollapseMode.Next)
                {
                    if (_elementHorizontalGridSplitterButton != null)
                    {
                        _elementHorizontalGridSplitterButton.RenderTransform.SetValue(ScaleTransform.ScaleYProperty, 1.0);
                    }
                    if (_elementVerticalGridSplitterButton != null)
                    {
                        _elementVerticalGridSplitterButton.RenderTransform.SetValue(ScaleTransform.ScaleXProperty, 1.0);
                    }
                }

            }

        }

        #endregion

        #region Collapse and Expand Methods

        /// <summary>
        /// Collapses the target ColumnDefinition or RowDefinition.
        /// </summary>
        private void Collapse()
        {
            Grid parentGrid = base.Parent as Grid;
            int splitterIndex = int.MinValue;

            if (_gridCollapseDirection == GridCollapseDirection.Rows)
            {
                // Get the index of the row containing the splitter
                splitterIndex = (int)base.GetValue(Grid.RowProperty);

                // Determing the curent CollapseMode
                if (this.CollapseMode == GridSplitterCollapseMode.Next)
                {
                    // Save the next rows Height information
                    _savedGridLength = parentGrid.RowDefinitions[splitterIndex + 1].Height;
                    _savedActualValue = parentGrid.RowDefinitions[splitterIndex + 1].ActualHeight;

                    // Collapse the next row
                    if (IsAnimated)
                        AnimateCollapse(parentGrid.RowDefinitions[splitterIndex + 1]);
                    else
                        parentGrid.RowDefinitions[splitterIndex + 1].SetValue(RowDefinition.HeightProperty, new GridLength(0));
                }
                else
                {
                    // Save the previous row's Height information
                    _savedGridLength = parentGrid.RowDefinitions[splitterIndex - 1].Height;
                    _savedActualValue = parentGrid.RowDefinitions[splitterIndex - 1].ActualHeight;

                    // Collapse the previous row
                    if (IsAnimated)
                        AnimateCollapse(parentGrid.RowDefinitions[splitterIndex - 1]);
                    else
                        parentGrid.RowDefinitions[splitterIndex - 1].SetValue(RowDefinition.HeightProperty, new GridLength(0));
                }
            }
            else
            {
                // Get the index of the column containing the splitter
                splitterIndex = (int)base.GetValue(Grid.ColumnProperty);

                // Determing the curent CollapseMode
                if (this.CollapseMode == GridSplitterCollapseMode.Next)
                {
                    // Save the next column's Width information
                    _savedGridLength = parentGrid.ColumnDefinitions[splitterIndex + 1].Width;
                    _savedActualValue = parentGrid.ColumnDefinitions[splitterIndex + 1].ActualWidth;

                    // Collapse the next column
                    if (IsAnimated)
                        AnimateCollapse(parentGrid.ColumnDefinitions[splitterIndex + 1]);
                    else
                        parentGrid.ColumnDefinitions[splitterIndex + 1].SetValue(ColumnDefinition.WidthProperty, new GridLength(0));
                }
                else
                {
                    // Save the previous column's Width information
                    _savedGridLength = parentGrid.ColumnDefinitions[splitterIndex - 1].Width;
                    _savedActualValue = parentGrid.ColumnDefinitions[splitterIndex - 1].ActualWidth;

                    // Collapse the previous column
                    if (IsAnimated)
                        AnimateCollapse(parentGrid.ColumnDefinitions[splitterIndex - 1]);
                    else
                        parentGrid.ColumnDefinitions[splitterIndex - 1].SetValue(ColumnDefinition.WidthProperty, new GridLength(0));
                }
            }

        }

        /// <summary>
        /// Expands the target ColumnDefinition or RowDefinition.
        /// </summary>
        private void Expand()
        {
            Grid parentGrid = base.Parent as Grid;
            int splitterIndex = int.MinValue;

            if (_gridCollapseDirection == GridCollapseDirection.Rows)
            {
                // Get the index of the row containing the splitter
                splitterIndex = (int)this.GetValue(Grid.RowProperty);

                // Determine the curent CollapseMode
                if (this.CollapseMode == GridSplitterCollapseMode.Next)
                {
                    // Expand the next row
                    if (IsAnimated)
                        AnimateExpand(parentGrid.RowDefinitions[splitterIndex + 1]);
                    else
                        parentGrid.RowDefinitions[splitterIndex + 1].SetValue(RowDefinition.HeightProperty, _savedGridLength);
                }
                else
                {
                    // Expand the previous row
                    if (IsAnimated)
                        AnimateExpand(parentGrid.RowDefinitions[splitterIndex - 1]);
                    else
                        parentGrid.RowDefinitions[splitterIndex - 1].SetValue(RowDefinition.HeightProperty, _savedGridLength);
                }
            }
            else
            {
                // Get the index of the column containing the splitter
                splitterIndex = (int)this.GetValue(Grid.ColumnProperty);

                // Determine the curent CollapseMode
                if (this.CollapseMode == GridSplitterCollapseMode.Next)
                {
                    // Expand the next column
                    if (IsAnimated)
                        AnimateExpand(parentGrid.ColumnDefinitions[splitterIndex + 1]);
                    else
                        parentGrid.ColumnDefinitions[splitterIndex + 1].SetValue(ColumnDefinition.WidthProperty, _savedGridLength);
                }
                else
                {
                    // Expand the previous column
                    if (IsAnimated)
                        AnimateExpand(parentGrid.ColumnDefinitions[splitterIndex - 1]);
                    else
                        parentGrid.ColumnDefinitions[splitterIndex - 1].SetValue(ColumnDefinition.WidthProperty, _savedGridLength);
                }
            }
        }

        /// <summary>
        /// Determine the collapse direction based on the horizontal and vertical alignments
        /// </summary>
        private GridCollapseDirection GetCollapseDirection()
        {
            if (base.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                // Width = Dimension;
                // Height = 0.0;
                return GridCollapseDirection.Columns;
            }
            if ((base.VerticalAlignment == VerticalAlignment.Stretch) && (base.ActualWidth <= base.ActualHeight))
            {

                // Width = Dimension;
                // Height = 0.0;

                return GridCollapseDirection.Columns;
            }

            //Width = 0.0;
            //Height = Dimension;
            return GridCollapseDirection.Rows;
        }

        #endregion

        #region Event Handlers and Throwers

        // Define Collapsed and Expanded evenets
        public event EventHandler<EventArgs> Collapsed;
        public event EventHandler<EventArgs> Expanded;

        /// <summary>
        /// Raises the Collapsed event.
        /// </summary>
        /// <param name="e">Contains event arguments.</param>
        protected virtual void OnCollapsed(EventArgs e)
        {
            EventHandler<EventArgs> handler = this.Collapsed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises the Expanded event.
        /// </summary>
        /// <param name="e">Contains event arguments.</param>
        protected virtual void OnExpanded(EventArgs e)
        {
            EventHandler<EventArgs> handler = this.Expanded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Handles the Checked event of either the Vertical or Horizontal
        /// GridSplitterHandle ToggleButton.
        /// </summary>
        /// <param name="sender">An instance of the ToggleButton that fired the event.</param>
        /// <param name="e">Contains event arguments for the routed event that fired.</param>
        private void GridSplitterButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsCollapsed != true)
            {
                // In our case, Checked = Collapsed.  Which means we want everything
                // ready to be expanded.
                Collapse();

                IsCollapsed = true;

                // Deactivate the background so the splitter can not be dragged.
                _elementGridSplitterBackground.IsHitTestVisible = false;
                //_elementGridSplitterBackground.Opacity = 0.5;

                // Raise the Collapsed event.
                OnCollapsed(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the Unchecked event of either the Vertical or Horizontal
        /// GridSplitterHandle ToggleButton.
        /// </summary>
        /// <param name="sender">An instance of the ToggleButton that fired the event.</param>
        /// <param name="e">Contains event arguments for the routed event that fired.</param>
        private void GridSplitterButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsCollapsed != false)
            {
                // In our case, Unchecked = Expanded.  Which means we want everything
                // ready to be collapsed.
                Expand();

                IsCollapsed = false;

                // Activate the background so the splitter can be dragged again.
                _elementGridSplitterBackground.IsHitTestVisible = true;
                //_elementGridSplitterBackground.Opacity = 1;

                // Raise the Expanded event.
                OnExpanded(EventArgs.Empty);
            }
        }

        #endregion

        #region Collapse and Expand Animations

        #region Property for animating rows

        private RowDefinition AnimatingRow;

        private static readonly DependencyProperty RowHeightAnimationProperty =
            DependencyProperty.Register(
                "RowHeightAnimation",
                typeof(double),
                typeof(ExtendedGridSplitter),
                new PropertyMetadata(new PropertyChangedCallback(RowHeightAnimationChanged))
                );
        private double RowHeightAnimation
        {
            get { return (double)GetValue(RowHeightAnimationProperty); }
            set { SetValue(RowHeightAnimationProperty, value); }
        }

        private static void RowHeightAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ExtendedGridSplitter).AnimatingRow.Height = new GridLength((double)e.NewValue);
        }

        #endregion

        #region Property for animating columns

        private ColumnDefinition AnimatingColumn;

        private static readonly DependencyProperty ColWidthAnimationProperty =
            DependencyProperty.Register(
                "ColWidthAnimation",
                typeof(double),
                typeof(ExtendedGridSplitter),
                new PropertyMetadata(new PropertyChangedCallback(ColWidthAnimationChanged))
                );
        private double ColWidthAnimation
        {
            get { return (double)GetValue(ColWidthAnimationProperty); }
            set { SetValue(ColWidthAnimationProperty, value); }
        }

        private static void ColWidthAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ExtendedGridSplitter).AnimatingColumn.Width = new GridLength((double)e.NewValue);
        }

        #endregion

        /// <summary>
        /// Uses DoubleAnimation and a StoryBoard to animated the collapsing
        /// of the specificed ColumnDefinition or RowDefinition.
        /// </summary>
        /// <param name="definition">The RowDefinition or ColumnDefintition that will be collapsed.</param>
        private void AnimateCollapse(object definition)
        {
            double currentValue;

            // Setup the animation and StoryBoard
            DoubleAnimation gridLengthAnimation = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromMilliseconds(_animationTimeMillis)) };
            Storyboard sb = new Storyboard();

            // Add the animation to the StoryBoard
            sb.Children.Add(gridLengthAnimation);

            if (_gridCollapseDirection == GridCollapseDirection.Rows)
            {
                // Specify the target RowDefinition and property (Height) that will be altered by the animation.
                this.AnimatingRow = (RowDefinition)definition;
                Storyboard.SetTarget(gridLengthAnimation, this);
                Storyboard.SetTargetProperty(gridLengthAnimation, new PropertyPath("RowHeightAnimation"));

                currentValue = AnimatingRow.ActualHeight;
            }
            else
            {
                // Specify the target ColumnDefinition and property (Width) that will be altered by the animation.
                this.AnimatingColumn = (ColumnDefinition)definition;
                Storyboard.SetTarget(gridLengthAnimation, this);
                Storyboard.SetTargetProperty(gridLengthAnimation, new PropertyPath("ColWidthAnimation"));

                currentValue = AnimatingColumn.ActualWidth;
            }

            gridLengthAnimation.From = currentValue;
            gridLengthAnimation.To = 0;

            // Start the StoryBoard.
            sb.Begin();
        }

        /// <summary>
        /// Uses DoubleAnimation and a StoryBoard to animate the expansion
        /// of the specificed ColumnDefinition or RowDefinition.
        /// </summary>
        /// <param name="definition">The RowDefinition or ColumnDefintition that will be expanded.</param>
        private void AnimateExpand(object definition)
        {
            double currentValue;

            // Setup the animation and StoryBoard
            DoubleAnimation gridLengthAnimation = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromMilliseconds(_animationTimeMillis)) };
            Storyboard sb = new Storyboard();

            // Add the animation to the StoryBoard
            sb.Children.Add(gridLengthAnimation);

            if (_gridCollapseDirection == GridCollapseDirection.Rows)
            {
                // Specify the target RowDefinition and property (Height) that will be altered by the animation.
                this.AnimatingRow = (RowDefinition)definition;
                Storyboard.SetTarget(gridLengthAnimation, this);
                Storyboard.SetTargetProperty(gridLengthAnimation, new PropertyPath("RowHeightAnimation"));

                currentValue = AnimatingRow.ActualHeight;
            }
            else
            {
                // Specify the target ColumnDefinition and property (Width) that will be altered by the animation.
                this.AnimatingColumn = (ColumnDefinition)definition;
                Storyboard.SetTarget(gridLengthAnimation, this);
                Storyboard.SetTargetProperty(gridLengthAnimation, new PropertyPath("ColWidthAnimation"));

                currentValue = AnimatingColumn.ActualWidth;
            }
            gridLengthAnimation.From = currentValue;
            gridLengthAnimation.To = _savedActualValue;

            // Start the StoryBoard.
            sb.Begin();
        }

        #endregion

        /// <summary>
        /// An enumeration that specifies the direction the ExtendedGridSplitter will
        /// be collapased (Rows or Columns).
        /// </summary>
        internal enum GridCollapseDirection
        {
            Auto,
            Columns,
            Rows
        }
    }
}
