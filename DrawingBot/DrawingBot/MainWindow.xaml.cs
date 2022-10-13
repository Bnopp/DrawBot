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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
//using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;

namespace DrawingBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSelecting = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (isSelecting) 
                isSelecting = false; 
            else
            {
                isSelecting = true;
                SelectCanvas();
            }
                
        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void SelectCanvas()
        {
            CanvasSelectWindow canvasSelect = new CanvasSelectWindow();
            canvasSelect.ShowDialog();

            var image = new Image();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(tbObjectToDraw.Text, UriKind.Absolute);
            bmp.EndInit();
            image.Source = bmp;

            Showimage display = new Showimage(image, canvasSelect.Canavs.X, canvasSelect.Canavs.Y);
            display.Show();


            MessageBox.Show($"{canvasSelect.Canavs.Width} {canvasSelect.Canavs.Height}");
        }

    }
}
