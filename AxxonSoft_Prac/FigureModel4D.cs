
namespace AxxonSoft_Prac
{
    /// <summary>
    /// Базовый класс для всех фигур, вращаемых в 4D-пространстве.
    /// </summary>
    public abstract class FigureModel4D : IFigureModel
    {
        // Углы вращения в шести 4D-плоскостях
        public double AngleXY { get; set; }
        public double AngleXZ { get; set; }
        public double AngleXW { get; set; }
        public double AngleYZ { get; set; }
        public double AngleYW { get; set; }
        public double AngleZW { get; set; }

        public abstract int VertexCount { get; }
        public abstract double[,] RotatedVertices { get; }
        public abstract double[,] GetInitialVertices();
        public abstract (int, int)[] GetEdges();
        public abstract void RegenerateFromSize();

        public virtual void Reset()
        {
            AngleXY = AngleXZ = AngleXW = AngleYZ = AngleYW = AngleZW = 0.0;
            CopyInitialToRotated();
        }

        /// <summary>
        /// Указывает, что фигура является 3D и не должна участвовать во вращениях, затрагивающих координату W.
        /// Если true, углы XW, YW, ZW игнорируются при обновлении.
        /// </summary>
        public virtual bool Is3DOnly => false;

        /// <summary>
        /// Копирует исходные вершины в массив вращённых.
        /// Должен быть реализован в дочернем классе.
        /// </summary>
        protected abstract void CopyInitialToRotated();


        public virtual void RegenerateVerticesFromCurrentSize()
        {
            // По умолчанию — вызываем RegenerateFromSize
            RegenerateFromSize();
        }
    }
}