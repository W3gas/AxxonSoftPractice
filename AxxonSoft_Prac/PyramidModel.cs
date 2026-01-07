using System;

namespace AxxonSoft_Prac
{
    public class PyramidModel : FigureModel4D
    {
        public const int NumberOfVertices = 5;
        public const int NumberOfEdges = 8;

        private readonly double[,] _initialVertices;
        private (int, int)[] _edges;

        public override double[,] RotatedVertices { get; }

        public override int VertexCount => NumberOfVertices;

        public override bool Is3DOnly => true;

        public PyramidModel()
        {
            _initialVertices = new double[NumberOfVertices, 4];
            RotatedVertices = new double[NumberOfVertices, 4];
            _edges = new (int, int)[NumberOfEdges];
            InitializeVertices();
            InitializeEdges();
            CopyInitialToRotated();
        }

        private void InitializeVertices()
        {
            double s = FigureSettings.FigureBaseSize;
            double height = s * 1.5; // высота пирамиды

            // Основание (квадрат в плоскости Z = -s)
            _initialVertices[0, 0] = -s;  // X
            _initialVertices[0, 1] = -s;  // Y
            _initialVertices[0, 2] = -s;  // Z
            _initialVertices[0, 3] = 0;   // W

            _initialVertices[1, 0] = s;
            _initialVertices[1, 1] = -s;
            _initialVertices[1, 2] = -s;
            _initialVertices[1, 3] = 0;

            _initialVertices[2, 0] = s;
            _initialVertices[2, 1] = s;
            _initialVertices[2, 2] = -s;
            _initialVertices[2, 3] = 0;

            _initialVertices[3, 0] = -s;
            _initialVertices[3, 1] = s;
            _initialVertices[3, 2] = -s;
            _initialVertices[3, 3] = 0;

            // Вершина пирамиды
            _initialVertices[4, 0] = 0;
            _initialVertices[4, 1] = 0;
            _initialVertices[4, 2] = height;
            _initialVertices[4, 3] = 0;
        }

        private void InitializeEdges()
        {
            _edges = new (int, int)[]
            {
                // Основание (квадрат)
                (0, 1), (1, 2), (2, 3), (3, 0),
                // Рёбра к вершине
                (0, 4), (1, 4), (2, 4), (3, 4)
            };
        }

        protected override void CopyInitialToRotated()
        {
            for (int i = 0; i < NumberOfVertices; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    RotatedVertices[i, j] = _initialVertices[i, j];
                }
            }
        }

        public override double[,] GetInitialVertices()
        {
            return _initialVertices;
        }

        public override (int, int)[] GetEdges()
        {
            var copy = new (int, int)[_edges.Length];
            Array.Copy(_edges, copy, _edges.Length);
            return copy;
        }

        public override void RegenerateVerticesFromCurrentSize()
        {
            InitializeVertices();
            CopyInitialToRotated();
        }

        public override void Reset()
        {
            AngleXY = AngleXZ = AngleXW = AngleYZ = AngleYW = AngleZW = 0.0;
            CopyInitialToRotated();
        }


        public override void RegenerateFromSize()
        {
            RegenerateVerticesFromCurrentSize();
        }
    }
}