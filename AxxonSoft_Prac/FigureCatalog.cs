using System;
using System.Collections.Generic;

namespace AxxonSoft_Prac
{
    public enum FigureType
    {
        Tesseract,
        Pyramid
    }

    public static class FigureCatalog
    {
        private static readonly Dictionary<FigureType, string> _displayNames = new()
        {
            { FigureType.Tesseract, "Tesseract" },
            { FigureType.Pyramid, "Pyramid" }
        };

        private static readonly Dictionary<FigureType, Func<FigureModel4D>> _factories = new()
        {
            { FigureType.Tesseract, () => new TesseractModel() },
            { FigureType.Pyramid,   () => new PyramidModel() }
        };

        public static IReadOnlyDictionary<FigureType, string> DisplayNames => _displayNames;

        /// <summary>
        /// Создаёт фигуру по типу. В случае ошибки — логирует и возвращает фигуру по умолчанию.
        /// </summary>
        public static FigureModel4D Create(FigureType type)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                try
                {
                    return factory();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to instantiate figure of type {type}. Exception: {ex.Message}", ex);
                }
            }

            // Если тип неизвестен или фабрика выбросила исключение — безопасно возвращаем фигуру по умолчанию
            Logger.Warn($"Falling back to default figure due to unknown or failed type: {type}");
            return Create(GetDefault());
        }

        public static FigureType GetDefault() => FigureType.Tesseract;
    }
}