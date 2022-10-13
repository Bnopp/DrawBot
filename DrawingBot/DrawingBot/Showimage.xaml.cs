﻿using System;
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
using System.Windows.Shapes;

namespace DrawingBot
{
    /// <summary>
    /// Logique d'interaction pour Showimage.xaml
    /// </summary>
    public partial class Showimage : Window
    {
        public Showimage(Image img, int X, int Y)
        {
            InitializeComponent();

            this.Width = img.Width;
            this.Height = img.Height;
            this.Top = Y;
            this.Left = X;
            this.AddChild(img);
        }
    }
}
