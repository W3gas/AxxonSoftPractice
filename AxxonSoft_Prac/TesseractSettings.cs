using Avalonia.Media;
using System;

namespace AxxonSoft_Prac
{
    
    public static class TesseractSettings
    {
        

        // Базовая скорость вращения
        public static double BaseRotationSpeed { get; set; } = 0.01;

        // Множители скорости для разных режимов вращения
        public static double ManualRotationMultiplier { get; set; } = 2.0;

        // Множители для автоматического вращения 
        public static double AutoRotationSpeedXY { get; set; } = 1.0;
        public static double AutoRotationSpeedXZ { get; set; } = 1.2;
        public static double AutoRotationSpeedXW { get; set; } = 0.8;
        public static double AutoRotationSpeedYZ { get; set; } = 1.1;
        public static double AutoRotationSpeedYW { get; set; } = 0.9;
        public static double AutoRotationSpeedZW { get; set; } = 0.7;

        // Параметры проекции
        public static double ProjectionDistance { get; set; } = 400.0;
        public static double ProjectionScale { get; set; } = 300.0;

        // Цвета
        public static Color EdgeColor { get; set; } = Colors.Cyan;
        public static Color VertexColor { get; set; } = Colors.White;

        // Размер точки
        public static double VertexSize { get; set; } = 6.0;

        

        // Базовый размер тессеракта (половина длины стороны в 4D)
        public static double TesseractBaseSize { get; set; } = 100.0;

        // Палитры цветов для кнопок
        public static Color[] EdgeColorPalette { get; } = { Colors.Cyan, Colors.Red, Colors.Green, Colors.Blue, Colors.Magenta, Colors.Yellow };
        public static Color[] VertexColorPalette { get; } = { Colors.White, Colors.Orange, Colors.Lime, Colors.Aqua, Colors.Pink, Colors.Gold };

        // Минимальные/максимальные значения для слайдеров
        public const double MinRotationSpeed = 0.001;
        public const double MaxRotationSpeed = 0.05;
        public const double MinTesseractSize = 50.0;
        public const double MaxTesseractSize = 150.0;
        public const double MinProjectionDistance = 200.0;
        public const double MaxProjectionDistance = 800.0;
        public const double MinProjectionScale = 150.0;
        public const double MaxProjectionScale = 400.0;
        public const double MinVertexSize = 2.0;
        public const double MaxVertexSize = 12.0;
    }
}