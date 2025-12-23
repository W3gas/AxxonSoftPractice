using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxxonSoft_Prac
{
    /// <summary>
    /// Представляет любую фигуру, пригодную для визуализации и вращения.
    /// Все координаты вершин — 4D (x, y, z, w).
    /// </summary>
    public interface IFigureModel
    {
        /// <summary>
        /// Количество вершин фигуры.
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Текущие вращённые 4D-координаты вершин. Размерность: [VertexCount, 4].
        /// </summary>
        double[,] RotatedVertices { get; }

        /// <summary>
        /// Возвращает копию исходных (невращённых) 4D-вершин.
        /// </summary>
        double[,] GetInitialVertices();

        /// <summary>
        /// Возвращает копию массива рёбер как пары индексов вершин.
        /// </summary>
        (int, int)[] GetEdges();

        /// <summary>
        /// Сбрасывает углы вращения и возвращает фигуру в исходное положение.
        /// </summary>
        void Reset();

        /// <summary>
        /// Пересоздаёт исходные вершины с учётом текущего размера фигуры.
        /// </summary>
        void RegenerateFromSize();
    }
}
