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

    public class TesseractRotationCalculator
    {
       // public static string temp = "";





        
        private readonly TesseractModel _model;

        // Текущий режим вращения.
        public RotationMode CurrentMode { get; set; } = RotationMode.ManualX;

        // получение модели для вращения 
        public TesseractRotationCalculator(TesseractModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        //  Обновление углов в модели и пересчёт RotatedVertices.
        public void Update()
        {
            
            switch (CurrentMode)
            {
                case RotationMode.ManualX:
                    _model.AngleXY += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    _model.AngleXZ += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    _model.AngleXW += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    break;
                case RotationMode.ManualY:
                    _model.AngleXY += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    _model.AngleYZ += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    _model.AngleYW += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    break;
                case RotationMode.ManualZ:
                    _model.AngleXZ += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    _model.AngleYZ += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    _model.AngleZW += TesseractSettings.BaseRotationSpeed * TesseractSettings.ManualRotationMultiplier;
                    break;
                case RotationMode.Auto:
                    _model.AngleXY += TesseractSettings.BaseRotationSpeed * TesseractSettings.AutoRotationSpeedXY;
                    _model.AngleXZ += TesseractSettings.BaseRotationSpeed * TesseractSettings.AutoRotationSpeedXZ;
                    _model.AngleXW += TesseractSettings.BaseRotationSpeed * TesseractSettings.AutoRotationSpeedXW;
                    _model.AngleYZ += TesseractSettings.BaseRotationSpeed * TesseractSettings.AutoRotationSpeedYZ;
                    _model.AngleYW += TesseractSettings.BaseRotationSpeed * TesseractSettings.AutoRotationSpeedYW;
                    _model.AngleZW += TesseractSettings.BaseRotationSpeed * TesseractSettings.AutoRotationSpeedZW;
                    break;
                case RotationMode.ManualDrag:
                    //skip
                    break;
            }

           
            _model.AngleXY = NormalizeAngle(_model.AngleXY);
            _model.AngleXZ = NormalizeAngle(_model.AngleXZ);
            _model.AngleXW = NormalizeAngle(_model.AngleXW);
            _model.AngleYZ = NormalizeAngle(_model.AngleYZ);
            _model.AngleYW = NormalizeAngle(_model.AngleYW);
            _model.AngleZW = NormalizeAngle(_model.AngleZW);

            

            //temp += ($"Mode: {CurrentMode}, Angles - XY: {_model.AngleXY:F3}, XZ: {_model.AngleXZ:F3}, XW: {_model.AngleXW:F3}, YZ: {_model.AngleYZ:F3}, YW: {_model.AngleYW:F3}, ZW: {_model.AngleZW:F3}");

           
            double[,] rotationMatrix = CalculateRotationMatrix(_model.AngleXY, _model.AngleXZ, _model.AngleXW, _model.AngleYZ, _model.AngleYW, _model.AngleZW);

           
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

            double s = TesseractSettings.ManualDragSensitivity;

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

            double s = TesseractSettings.ManualDragAlternateSensitivity;

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
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            // m = m * R_XY
            
            for (int i = 0; i < 4; i++)
            {
                double col0 = m[i, 0];
                double col1 = m[i, 1];
                m[i, 0] = col0 * cosAngle + col1 * sinAngle;
                m[i, 1] = col0 * (-sinAngle) + col1 * cosAngle;
            }
        }


        private void ApplyRotationXZ(ref double[,] m, double angle)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double col0 = m[i, 0];
                double col2 = m[i, 2];
                m[i, 0] = col0 * cosAngle + col2 * sinAngle;
                m[i, 2] = col0 * (-sinAngle) + col2 * cosAngle;
            }
        }


        private void ApplyRotationXW(ref double[,] m, double angle)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double col0 = m[i, 0];
                double col3 = m[i, 3];
                m[i, 0] = col0 * cosAngle + col3 * sinAngle;
                m[i, 3] = col0 * (-sinAngle) + col3 * cosAngle;
            }
        }


        private void ApplyRotationYZ(ref double[,] m, double angle)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double col1 = m[i, 1];
                double col2 = m[i, 2];
                m[i, 1] = col1 * cosAngle + col2 * sinAngle;
                m[i, 2] = col1 * (-sinAngle) + col2 * cosAngle;
            }
        }


        private void ApplyRotationYW(ref double[,] m, double angle)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double col1 = m[i, 1];
                double col3 = m[i, 3];
                m[i, 1] = col1 * cosAngle + col3 * sinAngle;
                m[i, 3] = col1 * (-sinAngle) + col3 * cosAngle;
            }
        }


        private void ApplyRotationZW(ref double[,] m, double angle)
        {
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            for (int i = 0; i < 4; i++)
            {
                double col2 = m[i, 2];
                double col3 = m[i, 3];
                m[i, 2] = col2 * cosAngle + col3 * sinAngle;
                m[i, 3] = col2 * (-sinAngle) + col3 * cosAngle;
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


        private double[,] RotationXYMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            m[0, 0] = cosAngle;
            m[0, 1] = -sinAngle;
            m[1, 0] = sinAngle; 
            m[1, 1] = cosAngle;
            return m;
        }


        private double[,] RotationXZMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            m[0, 0] = cosAngle; 
            m[0, 2] = -sinAngle;
            m[2, 0] = sinAngle; 
            m[2, 2] = cosAngle;
            return m;
        }


        private double[,] RotationXWMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            m[0, 0] = cosAngle;
            m[0, 3] = -sinAngle;
            m[3, 0] = sinAngle;
            m[3, 3] = cosAngle;
            return m;
        }


        private double[,] RotationYZMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            m[1, 1] = cosAngle;
            m[1, 2] = -sinAngle;
            m[2, 1] = sinAngle;
            m[2, 2] = cosAngle;
            return m;
        }


        private double[,] RotationYWMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            m[1, 1] = cosAngle;
            m[1, 3] = -sinAngle;
            m[3, 1] = sinAngle;
            m[3, 3] = cosAngle;
            return m;
        }


        private double[,] RotationZWMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            m[2, 2] = cosAngle;
            m[2, 3] = -sinAngle;
            m[3, 2] = sinAngle;
            m[3, 3] = cosAngle;
            return m;
        }

        
        private double[,] MultiplyMatrices(double[,] a, double[,] b)
        {
            double[,] result = new double[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return result;
        }


        //Применение общей матрицы вращения к массиву вершин
        private void ApplyRotationMatrix(double[,] rotationMatrix, double[,] initialVertices, double[,] rotatedVertices)
        {
            for (int i = 0; i < TesseractModel.NumberOfVertices; i++)
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

            _model.AngleXY += deltaXY;
            _model.AngleXZ += deltaXZ;
            _model.AngleXW += deltaXW;
            _model.AngleYZ += deltaYZ;
            _model.AngleYW += deltaYW;
            _model.AngleZW += deltaZW;

            // normalisation
            _model.AngleXY = NormalizeAngle(_model.AngleXY);
            _model.AngleXZ = NormalizeAngle(_model.AngleXZ);
            _model.AngleXW = NormalizeAngle(_model.AngleXW);
            _model.AngleYZ = NormalizeAngle(_model.AngleYZ);
            _model.AngleYW = NormalizeAngle(_model.AngleYW);
            _model.AngleZW = NormalizeAngle(_model.AngleZW);

            //recalculating the rotated vertices
            double[,] rotationMatrix = CalculateRotationMatrix(
                _model.AngleXY, _model.AngleXZ, _model.AngleXW,
                _model.AngleYZ, _model.AngleYW, _model.AngleZW);

            ApplyRotationMatrix(rotationMatrix, _model.GetInitialVertices(), _model.RotatedVertices);
        }


        public void Reset()
        {
            _model.Reset();
        }
    }
}