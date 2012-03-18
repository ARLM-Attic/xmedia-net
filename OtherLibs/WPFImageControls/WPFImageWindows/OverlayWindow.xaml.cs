using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFImageWindows
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rectangle.Width = CaptureRectangle.Width;
            rectangle.Height = CaptureRectangle.Height;
            Canvas.SetLeft(rectangle, CaptureRectangle.X);
            Canvas.SetTop(rectangle, CaptureRectangle.Y);
        }

        public Rect CaptureRectangle = new Rect(0, 0, 100, 100);
        Point StartingPoint;
        private bool m_bDrawingCaptureArea = true;

        protected bool DrawingCaptureArea
        {
            get { return m_bDrawingCaptureArea; }
        }

        void SaveScreenAndClose()
        {
            /// Hide our rectangles/etc before closing
            /// 

            CaptureRectangle.X = StartingPoint.X;
            CaptureRectangle.Y = StartingPoint.Y;
            CaptureRectangle.Width = rectangle.Width;
            CaptureRectangle.Height = rectangle.Height;

            this.Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
                SaveScreenAndClose();
            }
            else if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (DrawingCaptureArea == true)
            {
                StartingPoint = e.GetPosition(this as IInputElement);
                this.CaptureMouse();

                rectangle.Width = 0;
                rectangle.Height = 0;
                Canvas.SetLeft(rectangle, StartingPoint.X);
                Canvas.SetTop(rectangle, StartingPoint.Y);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((DrawingCaptureArea == true) && (e.MouseDevice.Captured == this))
            {
                Point CurrentPoint = e.GetPosition(this as IInputElement);

                double fX = Math.Min(CurrentPoint.X, StartingPoint.X);
                double fY = Math.Min(CurrentPoint.Y, StartingPoint.Y);
                Canvas.SetLeft(rectangle, fX);
                Canvas.SetTop(rectangle, fY);

                rectangle.Width = Math.Abs(CurrentPoint.X - StartingPoint.X);
                rectangle.Height = Math.Abs(CurrentPoint.Y - StartingPoint.Y);

            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if ((DrawingCaptureArea == true) && (e.MouseDevice.Captured == this))
            {
                Point CurrentPoint = e.GetPosition(this as IInputElement);
                double fX = Math.Min(CurrentPoint.X, StartingPoint.X);
                double fY = Math.Min(CurrentPoint.Y, StartingPoint.Y);
                Canvas.SetLeft(rectangle, fX);
                Canvas.SetTop(rectangle, fY);

                rectangle.Width = Math.Abs(CurrentPoint.X - StartingPoint.X);
                rectangle.Height = Math.Abs(CurrentPoint.Y - StartingPoint.Y);

                ReleaseMouseCapture();
            }
            base.OnMouseUp(e);
        }

        public void ShowArea()
        {

            //this.Left = rect.left;
            //this.Top = rect.top;
            //this.Width = rect.right - rect.left;
            //this.Height = rect.bottom - rect.top;
            rectangle.Stroke = Brushes.Red;
            System.Windows.Media.Animation.Storyboard board = Resources["FadeBorder"] as System.Windows.Media.Animation.Storyboard;
            BeginStoryboard(board);
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
