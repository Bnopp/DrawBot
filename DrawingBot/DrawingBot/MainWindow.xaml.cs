using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using GlobalLowLevelHooks;
using System.Drawing;
using Point = System.Drawing.Point;
using System.Windows.Controls;
using Color = System.Drawing.Color;
using System.Drawing.Imaging;

namespace DrawingBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSelecting = false;
        private MouseHook mh = new MouseHook();
        private KeyboardHook kbdh = new KeyboardHook();
        private int clickCount = 0;
        private bool isCanvas = true;
        private Rectangle drawCanvas;
        private Rectangle colorsArea;
        private IDictionary<Point, Color> drawPoints = new Dictionary<Point, Color>();
        private Point frstCorner;
        private Point scndCorner;

        public MainWindow()
        {
            InitializeComponent();

            mh.Install();
            mh.LeftButtonDown += new MouseHook.MouseHookCallback(mouseHook_LeftButtonDown);
            mh.MouseMove += new MouseHook.MouseHookCallback(mouseHook_Move);
        }

        private void mouseHook_Move(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            //Debug.WriteLine($"x:{mouseStruct.pt.x} y:{mouseStruct.pt.y}");
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
                            drawPoints = GetColorCoords(colorsArea);
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

        private IDictionary<Point, Color> GetColorCoords(Rectangle area)
        {
            IDictionary < Point, Color > colors = new Dictionary<Point, Color >();

            for (int i = 0; i < 5; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    colors.Add(new Point(area.X + n * (area.Width / 2), (area.Y + i * (area.Height / 5))), Color.Red);
                }
            }

            return colors;
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

        private void SelectCanvas()
        {
            /*CanvasSelectWindow canvasSelect = new CanvasSelectWindow();
            canvasSelect.ShowDialog();

            var image = new Image();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(tbObjectToDraw.Text, UriKind.Absolute);
            bmp.EndInit();
            image.Source = bmp;
            image.Stretch = Stretch.Fill;

            Showimage display = new Showimage(image, canvasSelect.Canavs.X, canvasSelect.Canavs.Y, canvasSelect.Canavs.Width, canvasSelect.Canavs.Height);
            display.Show();*/
        }
    }
}
