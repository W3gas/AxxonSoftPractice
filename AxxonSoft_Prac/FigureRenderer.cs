using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;

namespace AxxonSoft_Prac
{
    public class FigureRenderer
    {
       
        private readonly Canvas _canvas;
        private readonly IFigureModel _model;
        private Line[] _edgeLines;
        private Ellipse[] _vertexEllipses;

        // Принимаем Canvas
        public FigureRenderer(Canvas canvas, IFigureModel model)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            InitializeVisualElements();
        }

        private void InitializeVisualElements()
        {
            var edges = _model.GetEdges();
            var numberOfEdges = edges.Length;
            _edgeLines = new Line[numberOfEdges];
            for (int i = 0; i < numberOfEdges; i++)
            {
                _edgeLines[i] = new Line
                {
                    Stroke = new SolidColorBrush(FigureSettings.EdgeColor),
                    StrokeThickness = FigureSettings.StrokeThickness,
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(_edgeLines[i]);
            }

            var numberOfVertices = _model.VertexCount;
            _vertexEllipses = new Ellipse[numberOfVertices];
            for (int i = 0; i < numberOfVertices; i++)
            {
                _vertexEllipses[i] = new Ellipse
                {
                    Width = FigureSettings.VertexSize,
                    Height = FigureSettings.VertexSize,
                    Fill = new SolidColorBrush(FigureSettings.VertexColor),
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(_vertexEllipses[i]);
            }
        }

        public void Update()
        {
            // Skip update if canvas has no valid size yet
            if (_canvas.Bounds.Width <= 0 || _canvas.Bounds.Height <= 0)
            {
                return;
            }

            try
            {
                double centerX = _canvas.Bounds.Width / 2;
                double centerY = _canvas.Bounds.Height / 2;

                double[,] rotatedVertices = _model.RotatedVertices;
                double[,] projectedVertices = new double[_model.VertexCount, 2];

                for (int i = 0; i < _model.VertexCount; i++)
                {
                    double x = rotatedVertices[i, 0];
                    double y = rotatedVertices[i, 1];
                    double z = rotatedVertices[i, 2];
                    double w = rotatedVertices[i, 3];

                    double distance = FigureSettings.ProjectionDistance;
                    // Avoid division by zero in 4D projection
                    if (Math.Abs(distance + w) < 1e-9)
                    {
                        projectedVertices[i, 0] = centerX;
                        projectedVertices[i, 1] = centerY;
                    }
                    else
                    {
                        double factor1 = distance / (distance + w);
                        double x3d = x * factor1;
                        double y3d = y * factor1;
                        double z3d = z * factor1;

                        double scale = FigureSettings.ProjectionScale;
                        // Avoid division by zero in 3D→2D projection
                        if (Math.Abs(scale + z3d) < 1e-9)
                        {
                            projectedVertices[i, 0] = centerX;
                            projectedVertices[i, 1] = centerY;
                        }
                        else
                        {
                            double factor2 = scale / (scale + z3d);
                            double x2d = x3d * factor2 + centerX;
                            double y2d = y3d * factor2 + centerY;
                            projectedVertices[i, 0] = x2d;
                            projectedVertices[i, 1] = y2d;
                        }
                    }
                }

                var edges = _model.GetEdges();
                for (int i = 0; i < edges.Length; i++)
                {
                    var (from, to) = edges[i];
                    _edgeLines[i].StartPoint = new Avalonia.Point(projectedVertices[from, 0], projectedVertices[from, 1]);
                    _edgeLines[i].EndPoint = new Avalonia.Point(projectedVertices[to, 0], projectedVertices[to, 1]);
                }

                for (int i = 0; i < _model.VertexCount; i++)
                {
                    Canvas.SetLeft(_vertexEllipses[i], projectedVertices[i, 0] - FigureSettings.VertexSize / 2);
                    Canvas.SetTop(_vertexEllipses[i], projectedVertices[i, 1] - FigureSettings.VertexSize / 2);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Exception occurred in TesseractRenderer.Update", ex);
            }
        }


        public void UpdateColors()
        {
            var edgeBrush = new SolidColorBrush(FigureSettings.EdgeColor);
            foreach (var line in _edgeLines)
            {
                line.Stroke = edgeBrush;
            }

            var vertexBrush = new SolidColorBrush(FigureSettings.VertexColor);
            foreach (var point in _vertexEllipses)
            {
                point.Fill = vertexBrush;
            }
        }
    }
}