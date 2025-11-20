using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;

namespace AxxonSoft_Prac
{
    public class TesseractRenderer
    {
       
        private readonly Canvas _canvas;
        private readonly TesseractModel _model;
        private Line[] _lines;
        private Ellipse[] _points;

        // Принимаем Canvas
        public TesseractRenderer(Canvas canvas, TesseractModel model)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            InitializeVisualElements();
        }

        private void InitializeVisualElements()
        {
            var edges = _model.GetEdges();
            var numberOfEdges = edges.Length;
            _lines = new Line[numberOfEdges];
            for (int i = 0; i < numberOfEdges; i++)
            {
                _lines[i] = new Line
                {
                    Stroke = new SolidColorBrush(TesseractSettings.EdgeColor),
                    StrokeThickness = 1.5,
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(_lines[i]);
            }

            var numberOfVertices = TesseractModel.NumberOfVertices;
            _points = new Ellipse[numberOfVertices];
            for (int i = 0; i < numberOfVertices; i++)
            {
                _points[i] = new Ellipse
                {
                    Width = TesseractSettings.VertexSize,
                    Height = TesseractSettings.VertexSize,
                    Fill = new SolidColorBrush(TesseractSettings.VertexColor),
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(_points[i]);
            }
        }

        public void Update()
        {
            double[,] rotated = _model.RotatedVertices;

            double centerX = _canvas.Bounds.Width / 2;
            double centerY = _canvas.Bounds.Height / 2; 
            double[,] projected = new double[TesseractModel.NumberOfVertices, 2];

            for (int i = 0; i < TesseractModel.NumberOfVertices; i++)
            {
                double x = rotated[i, 0];
                double y = rotated[i, 1];
                double z = rotated[i, 2];
                double w = rotated[i, 3];

                double distance = TesseractSettings.ProjectionDistance;
                double factor1 = distance / (distance + w);
                double x3d = x * factor1;
                double y3d = y * factor1;
                double z3d = z * factor1;

                double scale = TesseractSettings.ProjectionScale / (TesseractSettings.ProjectionScale + z3d);
                double x2d = x3d * scale + centerX;
                double y2d = y3d * scale + centerY;

                projected[i, 0] = x2d;
                projected[i, 1] = y2d;
            }

            var edges = _model.GetEdges();
            for (int i = 0; i < edges.Length; i++)
            {
                var (from, to) = edges[i];
                _lines[i].StartPoint = new Avalonia.Point(projected[from, 0], projected[from, 1]);
                _lines[i].EndPoint = new Avalonia.Point(projected[to, 0], projected[to, 1]);
            }

            for (int i = 0; i < TesseractModel.NumberOfVertices; i++)
            {
                Canvas.SetLeft(_points[i], projected[i, 0] - TesseractSettings.VertexSize / 2);
                Canvas.SetTop(_points[i], projected[i, 1] - TesseractSettings.VertexSize / 2);
            }
        }
    }
}