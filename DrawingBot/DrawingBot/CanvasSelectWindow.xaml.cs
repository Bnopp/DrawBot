using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;
//using System.Windows.Shapes;
using Point = System.Drawing.Point;
using System.Diagnostics;

namespace DrawingBot
{
    /// <summary>
    /// Logique d'interaction pour CanvasSelectWindow.xaml
    /// </summary>
    public partial class CanvasSelectWindow : Window
    {
        private Rectangle _canvas;

        public Rectangle Canavs { get { return _canvas; } }

        public CanvasSelectWindow()
        {
            InitializeComponent();

            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Topmost = false;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Left = 0;
            this.Top = 0;
            this.AllowsTransparency = true;
            this.Opacity = 0.1;
            this.Background = new SolidColorBrush(Colors.Red);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Task.Run(() => GetCanvas(e));
        }

        private void GetCanvas(MouseButtonEventArgs e)
        {
            Rectangle canvas;
            Point tL = GetCursorPosition();
            while (e.LeftButton == MouseButtonState.Pressed)
            {
                Debug.WriteLine("mouse is down " + DateTime.Now);
            }
            Point bR = GetCursorPosition();
            canvas = new Rectangle(tL.X, tL.Y, bR.X, bR.Y);

            _canvas = canvas;

            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }
    }
}
