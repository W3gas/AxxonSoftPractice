using System;

namespace AxxonSoft_Prac
{
    public class TesseractModel : FigureModel4D
    {
        public const int NumberOfVertices = 16;
        public const int NumberOfEdges = 32;

        private readonly double[,] _initialVertices;
        private (int, int)[] _edges;

        public override double[,] RotatedVertices { get; }

        public override int VertexCount => NumberOfVertices;

        public TesseractModel()
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
            int index = 0;
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        for (int l = 0; l < 2; l++)
                        {
                            _initialVertices[index, 0] = (i * 2 - 1) * FigureSettings.TesseractBaseSize;
                            _initialVertices[index, 1] = (j * 2 - 1) * FigureSettings.TesseractBaseSize;
                            _initialVertices[index, 2] = (k * 2 - 1) * FigureSettings.TesseractBaseSize;
                            _initialVertices[index, 3] = (l * 2 - 1) * FigureSettings.TesseractBaseSize;
                            index++;
                        }
        }

        private void InitializeEdges()
        {
            _edges = new (int, int)[]
            {
                (0,1), (0,2), (0,4), (0,8), (1,3), (1,5), (1,9), (2,3), (2,6), (2,10),
                (3,7), (3,11), (4,5), (4,6), (4,12), (5,7), (5,13), (6,7), (6,14),
                (7,15), (8,9), (8,10), (8,12), (9,11), (9,13), (10,11), (10,14),
                (11,15), (12,13), (12,14), (13,15), (14,15)
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

        

        public override void RegenerateFromSize()
        {
            RegenerateVerticesFromCurrentSize();
        }

        public override void Reset()
        {
            base.Reset(); 
        }


        public override void RegenerateVerticesFromCurrentSize()
        {
            InitializeVertices();
            CopyInitialToRotated();
        }
    }
}