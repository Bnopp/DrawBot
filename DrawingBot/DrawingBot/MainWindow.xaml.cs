using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using GlobalLowLevelHooks;
using System.Drawing;
using Point = System.Drawing.Point;
using System.Windows.Controls;
using Color = System.Drawing.Color;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.IO;
using System.Xml.Linq;

namespace DrawingBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        private Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);


        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private bool isSelecting = false;
        private MouseHook mh = new MouseHook();
        private KeyboardHook kbdh = new KeyboardHook();
        private int clickCount = 0;
        private bool isCanvas = true;
        private Rectangle drawCanvas;
        private Rectangle colorsArea;
        //private IDictionary<Point, Color> colorPoints = new Dictionary<Point, Color>();
        private List<Point> colorPoints = new List<Point>();
        private List<Color> colorValues = new List<Color>();
        private Point frstCorner;
        private Point scndCorner;
        private int genAmount = 0;
        private int resolution = 2;

        public void DoMouseClick(int X, int Y)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, Convert.ToUInt32(X), Convert.ToUInt32(Y), 0, 0);
        }

        public MainWindow()
        {
            InitializeComponent();

            mh.Install();
            mh.LeftButtonDown += new MouseHook.MouseHookCallback(mouseHook_LeftButtonDown);
            mh.MouseMove += new MouseHook.MouseHookCallback(mouseHook_Move);
            kbdh.Install();
            kbdh.KeyDown += new KeyboardHook.KeyboardHookCallback(keyBoardHookClose);
            
        }

        private void mouseHook_Move(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            //tbObjectToDraw.Text = ($"x:{mouseStruct.pt.x} y:{mouseStruct.pt.y}");
        }

        private void keyBoardHookClose(KeyboardHook.VKeys key)
        {
            if (key.ToString() == "ESCAPE")
                System.Windows.Application.Current.Shutdown();
        }

        private void mouseHook_LeftButtonDown(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            Debug.WriteLine($"clicked {clickCount}");
            if (isSelecting)
            {
                if (clickCount <= 1)
                {
                    if (clickCount == 0)
                        frstCorner = new Point(mouseStruct.pt.x, mouseStruct.pt.y);
                    else
                    {
                        scndCorner = new Point(mouseStruct.pt.x, mouseStruct.pt.y);
                        if (isCanvas)
                        {
                            drawCanvas = GetArea();
                            SaveAreas(drawCanvas, "drawArea");
                        }
                        else
                        {
                            colorsArea = GetArea();
                            colorPoints = GetColorCoords(colorsArea);
                            SaveAreas(colorsArea, "colorsArea");
                        }
                        isSelecting = false;
                        if (isCanvas)
                            btnSelectCanvas.Content = "Drawing Area";
                        else
                            btnSelectColors.Content = "Colors Area";
                    }
                    clickCount++;
                }
            }
        }

        //private IDictionary<Point, Color> GetColorCoords(Rectangle area)
        private List<Point> GetColorCoords(Rectangle area)
        {
            List <Point> points = new List<Point>();

            for (int i = 0; i < 6; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    Point pos = new Point(area.X + n * (area.Width / 2), area.Y + i * (area.Height / 5));
                    points.Add(pos);
                    colorValues.Add(GetColorAt(pos));
                }
            }

            return points;
        }

        private void btnSelectCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                (sender as Button).Content = "Drawing Area";
            }
            else
            {
                isSelecting = true;
                isCanvas = true;
                clickCount = 0;
                (sender as Button).Content = "Selecting...";
            }
        }

        private void btnSelectColors_Click(object sender, RoutedEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                (sender as Button).Content = "Colors Area";
            }
            else
            {
                isSelecting = true;
                isCanvas = false;
                clickCount = 0;
                (sender as Button).Content = "Selecting...";
            }
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            //ColorCheck();
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    Draw();
                });
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            if (genAmount > 0)
                genAmount = 0;
            else
                genAmount++;
            MessageBox.Show("finished drawing the masterpiece");
        }

        private void Draw()
        {
            BitmapImage bmpImg = new BitmapImage();
            bmpImg.BeginInit();
            bmpImg.UriSource = new Uri(tbObjectToDraw.Text, UriKind.Absolute);
            bmpImg.DecodePixelHeight = drawCanvas.Height;
            bmpImg.DecodePixelWidth = drawCanvas.Width;
            bmpImg.EndInit();
            Bitmap bmp = BitmapImage2Bitmap(bmpImg);

            if (genAmount > 0)
            {
                bmp.Save(@$"D://normal.png");
                bmp.Save(@$"D://resized.png");
                Color last = Color.Transparent;
                for (int x = 0; x + resolution < bmp.Width; x += resolution)
                {
                    for (int y = 0; y + resolution < bmp.Height; y += resolution)
                    {
                        var pixel = bmp.GetPixel(x, y);

                        //bmp.SetPixel(x, y, colorValues[FindNearestColor(colorValues, pixel)]);

                        int index = colorValues.FindIndex(a => a == colorValues[FindNearestColor(colorValues, pixel)]);
                        if (colorValues[index] != last)
                        {
                            SetCursorPos(colorPoints[index].X, colorPoints[index].Y);
                            DoMouseClick(colorPoints[index].X, colorPoints[index].Y);
                        }
                        last = colorValues[index];
                        Thread.Sleep(5);
                        SetCursorPos(drawCanvas.X + x, drawCanvas.Y + y);
                        DoMouseClick(drawCanvas.X + x, drawCanvas.Y + y);

                    }
                }
                genAmount = 0;
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private void ColorCheck()
        {
            foreach (Point p in colorPoints)
            {
                SetCursorPos(p.X, p.Y);

                Thread.Sleep(10);

                Debug.WriteLine($"Position: ({p.X},{p.Y})");
            }
        }

        private Rectangle GetArea()
        {
            int width;
            int height;
            int x;
            int y;

            if (scndCorner.X >= frstCorner.X)
            {
                width = scndCorner.X - frstCorner.X;
                x = frstCorner.X;
            }
            else
            {
                width = frstCorner.X - scndCorner.X;
                x = scndCorner.X;
            }

            if (scndCorner.Y >= frstCorner.Y)
            {
                height = scndCorner.Y - frstCorner.Y;
                y = frstCorner.Y;
            }
            else
            {
                height = frstCorner.Y - scndCorner.Y;
                y = scndCorner.Y;
            }

            return new Rectangle(x, y, width, height);
        }

        private void SaveAreas(Rectangle rect, string name)
        {
            using (Bitmap bitmap = new Bitmap(rect.Width, rect.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(rect.Left, rect.Top), Point.Empty, rect.Size);
                }
                // select the save location of the captured screenshot
                bitmap.Save(@$"D://{name}.png", ImageFormat.Png);

                // show a message to let the user know that a screenshot has been captured
                Debug.WriteLine("Screenshot taken! Press `OK` to continue...");
            }
        }

        public Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        public static int FindNearestColor(List<Color> map, Color current)
        {
            int shortestDistance;
            int index;

            index = -1;
            shortestDistance = int.MaxValue;

            for (int i = 0; i < map.Count; i++)
            {
                Color match;
                int distance;

                match = map[i];
                distance = GetDistance(current, match);

                if (distance < shortestDistance)
                {
                    index = i;
                    shortestDistance = distance;
                }
            }

            return index;
        }

        public static int GetDistance(Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;

            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }

        /*private void SelectCanvas()
        {
            CanvasSelectWindow canvasSelect = new CanvasSelectWindow();
            canvasSelect.ShowDialog();

            var image = new Image();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(tbObjectToDraw.Text, UriKind.Absolute);
            bmp.EndInit();
            image.Source = bmp;
            image.Stretch = Stretch.Fill;

            Showimage display = new Showimage(image, canvasSelect.Canavs.X, canvasSelect.Canavs.Y, canvasSelect.Canavs.Width, canvasSelect.Canavs.Height);
            display.Show();
        }*/
    }
}
