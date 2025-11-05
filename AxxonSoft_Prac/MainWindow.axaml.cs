using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace AxxonSoft_Prac
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private double angleXY = 0, angleXZ = 0, angleXW = 0;
        private double angleYZ = 0, angleYW = 0, angleZW = 0;

        private double[,] vertices4D = new double[16, 4];

        private Canvas _drawCanvas;
        private RadioButton _radioX;
        private RadioButton _radioY;
        private RadioButton _radioZ;
        private RadioButton _radioAuto;

        public MainWindow()
        {
            InitializeComponent();

            _drawCanvas = this.FindControl<Canvas>("DrawCanvas");
            _radioX = this.FindControl<RadioButton>("RadioX");
            _radioY = this.FindControl<RadioButton>("RadioY");
            _radioZ = this.FindControl<RadioButton>("RadioZ");
            _radioAuto = this.FindControl<RadioButton>("RadioAuto");

            InitializeTesseract();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void InitializeTesseract()
        {
            int index = 0;
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        for (int l = 0; l < 2; l++)
                        {
                            vertices4D[index, 0] = (i * 2 - 1) * 100;
                            vertices4D[index, 1] = (j * 2 - 1) * 100;
                            vertices4D[index, 2] = (k * 2 - 1) * 100;
                            vertices4D[index, 3] = (l * 2 - 1) * 100;
                            index++;
                        }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_radioX.IsChecked == true)
            {
                angleXY += 0.02;
                angleXZ += 0.02;
                angleXW += 0.02;
            }
            else if (_radioY.IsChecked == true)
            {
                angleXY += 0.02;
                angleYZ += 0.02;
                angleYW += 0.02;
            }
            else if (_radioZ.IsChecked == true)
            {
                angleXZ += 0.02;
                angleYZ += 0.02;
                angleZW += 0.02;
            }
            else if (_radioAuto.IsChecked == true)
            {
                angleXY += 0.01;
                angleXZ += 0.012;
                angleXW += 0.008;
                angleYZ += 0.011;
                angleYW += 0.009;
                angleZW += 0.007;
            }

            _drawCanvas.InvalidateVisual();
            DrawTesseract();
        }

        private void DrawTesseract()
        {
            _drawCanvas.Children.Clear();

            double centerX = _drawCanvas.Width / 2;
            double centerY = _drawCanvas.Height / 2;

            double[,] rotated = new double[16, 4];
            for (int i = 0; i < 16; i++)
            {
                double x = vertices4D[i, 0];
                double y = vertices4D[i, 1];
                double z = vertices4D[i, 2];
                double w = vertices4D[i, 3];

                double x1 = x * Math.Cos(angleXY) - y * Math.Sin(angleXY);
                double y1 = x * Math.Sin(angleXY) + y * Math.Cos(angleXY);
                double z1 = z;
                double w1 = w;

                double x2 = x1 * Math.Cos(angleXZ) - z1 * Math.Sin(angleXZ);
                double z2 = x1 * Math.Sin(angleXZ) + z1 * Math.Cos(angleXZ);
                double y2 = y1;
                double w2 = w1;

                double x3 = x2 * Math.Cos(angleXW) - w2 * Math.Sin(angleXW);
                double w3 = x2 * Math.Sin(angleXW) + w2 * Math.Cos(angleXW);
                double y3 = y2;
                double z3 = z2;

                double y4 = y3 * Math.Cos(angleYZ) - z3 * Math.Sin(angleYZ);
                double z4 = y3 * Math.Sin(angleYZ) + z3 * Math.Cos(angleYZ);
                double x4 = x3;
                double w4 = w3;

                double y5 = y4 * Math.Cos(angleYW) - w4 * Math.Sin(angleYW);
                double w5 = y4 * Math.Sin(angleYW) + w4 * Math.Cos(angleYW);
                double x5 = x4;
                double z5 = z4;

                double z6 = z5 * Math.Cos(angleZW) - w5 * Math.Sin(angleZW);
                double w6 = z5 * Math.Sin(angleZW) + w5 * Math.Cos(angleZW);

                rotated[i, 0] = x5;
                rotated[i, 1] = y5;
                rotated[i, 2] = z6;
                rotated[i, 3] = w6;
            }

            double[,] projected = new double[16, 2];
            for (int i = 0; i < 16; i++)
            {
                double distance = 400;
                double w = rotated[i, 3];
                double factor = distance / (distance + w);

                double x3d = rotated[i, 0] * factor;
                double y3d = rotated[i, 1] * factor;
                double z3d = rotated[i, 2] * factor;

                double scale = 300 / (300 + z3d);
                projected[i, 0] = x3d * scale + centerX;
                projected[i, 1] = y3d * scale + centerY;
            }

            var edges = new (int, int)[]
            {
                (0,1), (0,2), (0,4), (0,8), (1,3), (1,5), (1,9), (2,3), (2,6), (2,10),
                (3,7), (3,11), (4,5), (4,6), (4,12), (5,7), (5,13), (6,7), (6,14),
                (7,15), (8,9), (8,10), (8,12), (9,11), (9,13), (10,11), (10,14),
                (11,15), (12,13), (12,14), (13,15), (14,15)
            };

            foreach (var (from, to) in edges)
            {
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Avalonia.Point(projected[from, 0], projected[from, 1]),
                    EndPoint = new Avalonia.Point(projected[to, 0], projected[to, 1]),
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 1.5
                };
                _drawCanvas.Children.Add(line);
            }

            for (int i = 0; i < 16; i++)
            {
                var ellipse = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.White
                };
                Canvas.SetLeft(ellipse, projected[i, 0] - 3);
                Canvas.SetTop(ellipse, projected[i, 1] - 3);
                _drawCanvas.Children.Add(ellipse);
            }
        }

        private void BtnStart_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            timer.Start();
        }

        private void BtnStop_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void BtnReset_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            angleXY = angleXZ = angleXW = angleYZ = angleYW = angleZW = 0;
            DrawTesseract();
        }
    }
}