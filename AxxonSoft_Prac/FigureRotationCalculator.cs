using System;

namespace AxxonSoft_Prac
{
   

    // Перечисление для определения режима вращения.
    public enum RotationMode
    {
        ManualX,
        ManualY,
        ManualZ,
        Auto,
        ManualDrag
    }

    public class FigureRotationCalculator
    {
       // public static string temp = "";





        
        private readonly FigureModel4D _model;

        // Текущий режим вращения.
        public RotationMode CurrentMode { get; set; } = RotationMode.ManualX;

        // получение модели для вращения 
        public FigureRotationCalculator(FigureModel4D model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        //  Обновление углов в модели и пересчёт RotatedVertices.
        public void Update()
        {
            bool is3D = _model.Is3DOnly;

            switch (CurrentMode)
            {
                case RotationMode.ManualX:
                    _model.AngleXY += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    _model.AngleXZ += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    if (!is3D) _model.AngleXW += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    break;
                case RotationMode.ManualY:
                    _model.AngleXY += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    _model.AngleYZ += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    if (!is3D) _model.AngleYW += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    break;
                case RotationMode.ManualZ:
                    _model.AngleXZ += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    _model.AngleYZ += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    if (!is3D) _model.AngleZW += FigureSettings.BaseRotationSpeed * FigureSettings.ManualRotationMultiplier;
                    break;
                case RotationMode.Auto:
                    _model.AngleXY += FigureSettings.BaseRotationSpeed * FigureSettings.AutoRotationSpeedXY;
                    _model.AngleXZ += FigureSettings.BaseRotationSpeed * FigureSettings.AutoRotationSpeedXZ;
                    _model.AngleYZ += FigureSettings.BaseRotationSpeed * FigureSettings.AutoRotationSpeedYZ;
                    if (!is3D)
                    {
                        _model.AngleXW += FigureSettings.BaseRotationSpeed * FigureSettings.AutoRotationSpeedXW;
                        _model.AngleYW += FigureSettings.BaseRotationSpeed * FigureSettings.AutoRotationSpeedYW;
                        _model.AngleZW += FigureSettings.BaseRotationSpeed * FigureSettings.AutoRotationSpeedZW;
                    }
                    break;
                case RotationMode.ManualDrag:
                    // Обработка происходит в ApplyManualRotationDelta — см. ниже
                    break;
            }

            // Нормализация — для всех углов, но если фигура 3D, W-углы остаются 0
            _model.AngleXY = NormalizeAngle(_model.AngleXY);
            _model.AngleXZ = NormalizeAngle(_model.AngleXZ);
            _model.AngleYZ = NormalizeAngle(_model.AngleYZ);
            if (!is3D)
            {
                _model.AngleXW = NormalizeAngle(_model.AngleXW);
                _model.AngleYW = NormalizeAngle(_model.AngleYW);
                _model.AngleZW = NormalizeAngle(_model.AngleZW);
            }

            double[,] rotationMatrix = CalculateRotationMatrix(
                _model.AngleXY, _model.AngleXZ, is3D ? 0.0 : _model.AngleXW,
                _model.AngleYZ, is3D ? 0.0 : _model.AngleYW, is3D ? 0.0 : _model.AngleZW);

            ApplyRotationMatrix(rotationMatrix, _model.GetInitialVertices(), _model.RotatedVertices);
        }


        public void ApplyCurrentRotation()
        {
            //  normalisation
            _model.AngleXY = NormalizeAngle(_model.AngleXY);
            _model.AngleXZ = NormalizeAngle(_model.AngleXZ);
            _model.AngleXW = NormalizeAngle(_model.AngleXW);
            _model.AngleYZ = NormalizeAngle(_model.AngleYZ);
            _model.AngleYW = NormalizeAngle(_model.AngleYW);
            _model.AngleZW = NormalizeAngle(_model.AngleZW);

            //  calculating
            double[,] rotationMatrix = CalculateRotationMatrix(
                _model.AngleXY, _model.AngleXZ, _model.AngleXW,
                _model.AngleYZ, _model.AngleYW, _model.AngleZW);

            
            ApplyRotationMatrix(rotationMatrix, _model.GetInitialVertices(), _model.RotatedVertices);
        }


        // Handle rotation without SHIFT
        public void HandleMouseDragPrimary(double deltaX, double deltaY)
        {
            if (CurrentMode != RotationMode.ManualDrag)
                return;

            double s = FigureSettings.ManualDragSensitivity;

            // asimut
            double deltaXY = deltaX * s;

            // elewation
            double deltaYZ = -deltaY * s;

            ApplyManualRotationDelta(
                deltaXY: deltaXY,
                deltaYZ: deltaYZ
            
            );
        }


        // Handle rotation with SHIFT
        public void HandleMouseDragAlternate(double deltaX, double deltaY)
        {
            if (CurrentMode != RotationMode.ManualDrag)
                return;

            double s = FigureSettings.ManualDragAlternateSensitivity;

            // Основные 4D-плоскости
            double deltaXW = deltaX * s;       // горизонт → XW

            // Вертикаль → сразу две плоскости с разным фазовым сдвигом
            double deltaYW = -deltaY * s;      // YW (инверсия, как в 3D)
            double deltaZW = deltaY * s; // ZW — с ослаблением и без инверсии

            ApplyManualRotationDelta(
                deltaXW: deltaXW,
                deltaYW: deltaYW,
                deltaZW: deltaZW
            );
        }


        private double NormalizeAngle(double angle)
        {
            return Math.IEEERemainder(angle, 2 * Math.PI);
        }


        // Порядок умножения: R_ZW * R_YW * R_YZ * R_XW * R_XZ * R_XY
        private double[,] CalculateRotationMatrix( double angleXY, double angleXZ, double angleXW,
                                                   double angleYZ, double angleYW, double angleZW)
        {
            //  identity matrix
            double[,] m = IdentityMatrix4D();

            //  Turn right: m = m * R
            //  Order: R_XY → R_XZ → R_XW → R_YZ → R_YW → R_ZW

            ApplyRotationXY(ref m, angleXY);
            ApplyRotationXZ(ref m, angleXZ);
            ApplyRotationXW(ref m, angleXW);
            ApplyRotationYZ(ref m, angleYZ);
            ApplyRotationYW(ref m, angleYW);
            ApplyRotationZW(ref m, angleZW);

            return m;
        }


        private void ApplyRotationXY(ref double[,] m, double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            // Применяем: m = m * R_XY
            // R_XY затрагивает столбцы 0 (X) и 1 (Y)
            for (int i = 0; i < 4; i++)
            {
                double x = m[i, 0];
                double y = m[i, 1];
                m[i, 0] = x * cosA - y * sinA;
                m[i, 1] = x * sinA + y * cosA;
            }
        }

        private void ApplyRotationXZ(ref double[,] m, double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double x = m[i, 0];
                double z = m[i, 2];
                m[i, 0] = x * cosA - z * sinA;
                m[i, 2] = x * sinA + z * cosA;
            }
        }

        private void ApplyRotationXW(ref double[,] m, double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double x = m[i, 0];
                double w = m[i, 3];
                m[i, 0] = x * cosA - w * sinA;
                m[i, 3] = x * sinA + w * cosA;
            }
        }

        private void ApplyRotationYZ(ref double[,] m, double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double y = m[i, 1];
                double z = m[i, 2];
                m[i, 1] = y * cosA - z * sinA;
                m[i, 2] = y * sinA + z * cosA;
            }
        }

        private void ApplyRotationYW(ref double[,] m, double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double y = m[i, 1];
                double w = m[i, 3];
                m[i, 1] = y * cosA - w * sinA;
                m[i, 3] = y * sinA + w * cosA;
            }
        }

        private void ApplyRotationZW(ref double[,] m, double angle)
        {
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double z = m[i, 2];
                double w = m[i, 3];
                m[i, 2] = z * cosA - w * sinA;
                m[i, 3] = z * sinA + w * cosA;
            }
        }


        private double[,] IdentityMatrix4D()
        {
            double[,] m = new double[4, 4];
            m[0, 0] = 1;
            m[1, 1] = 1;
            m[2, 2] = 1;
            m[3, 3] = 1;
            return m;
        }


        

        //Применение общей матрицы вращения к массиву вершин
        private void ApplyRotationMatrix(double[,] rotationMatrix, double[,] initialVertices, double[,] rotatedVertices)
        {
            for (int i = 0; i < _model.VertexCount; i++)
            {
                double x = initialVertices[i, 0];
                double y = initialVertices[i, 1];
                double z = initialVertices[i, 2];
                double w = initialVertices[i, 3];

               
                rotatedVertices[i, 0] = rotationMatrix[0, 0] * x + rotationMatrix[0, 1] * y + rotationMatrix[0, 2] * z + rotationMatrix[0, 3] * w;
                rotatedVertices[i, 1] = rotationMatrix[1, 0] * x + rotationMatrix[1, 1] * y + rotationMatrix[1, 2] * z + rotationMatrix[1, 3] * w;
                rotatedVertices[i, 2] = rotationMatrix[2, 0] * x + rotationMatrix[2, 1] * y + rotationMatrix[2, 2] * z + rotationMatrix[2, 3] * w;
                rotatedVertices[i, 3] = rotationMatrix[3, 0] * x + rotationMatrix[3, 1] * y + rotationMatrix[3, 2] * z + rotationMatrix[3, 3] * w;
            }
        }


        public void ApplyManualRotationDelta(
    double deltaXY = 0,
    double deltaXZ = 0,
    double deltaXW = 0,
    double deltaYZ = 0,
    double deltaYW = 0,
    double deltaZW = 0)
        {
            if (CurrentMode != RotationMode.ManualDrag)
                return;

            bool is3D = _model.Is3DOnly;

            _model.AngleXY += deltaXY;
            _model.AngleXZ += deltaXZ;
            _model.AngleYZ += deltaYZ;

            if (!is3D)
            {
                _model.AngleXW += deltaXW;
                _model.AngleYW += deltaYW;
                _model.AngleZW += deltaZW;
            }

            _model.AngleXY = NormalizeAngle(_model.AngleXY);
            _model.AngleXZ = NormalizeAngle(_model.AngleXZ);
            _model.AngleYZ = NormalizeAngle(_model.AngleYZ);

            if (!is3D)
            {
                _model.AngleXW = NormalizeAngle(_model.AngleXW);
                _model.AngleYW = NormalizeAngle(_model.AngleYW);
                _model.AngleZW = NormalizeAngle(_model.AngleZW);
            }

            double[,] rotationMatrix = CalculateRotationMatrix(
                _model.AngleXY, _model.AngleXZ, is3D ? 0.0 : _model.AngleXW,
                _model.AngleYZ, is3D ? 0.0 : _model.AngleYW, is3D ? 0.0 : _model.AngleZW);

            ApplyRotationMatrix(rotationMatrix, _model.GetInitialVertices(), _model.RotatedVertices);
        }


        public void Reset()
        {
            _model.Reset();
        }
    }
}