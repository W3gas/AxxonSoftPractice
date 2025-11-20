using System;

namespace AxxonSoft_Prac
{
   

    // Перечисление для определения режима вращения.
    public enum RotationMode
    {
        ManualX,
        ManualY,
        ManualZ,
        Auto
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

        
        private double NormalizeAngle(double angle)
        {
            return Math.IEEERemainder(angle, 2 * Math.PI);
        }

        // Порядок умножения: R_ZW * R_YW * R_YZ * R_XW * R_XZ * R_XY
        private double[,] CalculateRotationMatrix(double angleXY, double angleXZ, double angleXW, double angleYZ, double angleYW, double angleZW)
        {

            double[,] matrix = IdentityMatrix4D();


            // R_XY
            matrix = MultiplyMatrices(matrix, RotationXYMatrix(angleXY));
            // R_XZ
            matrix = MultiplyMatrices(matrix, RotationXZMatrix(angleXZ));
            // R_XW
            matrix = MultiplyMatrices(matrix, RotationXWMatrix(angleXW));
            // R_YZ
            matrix = MultiplyMatrices(matrix, RotationYZMatrix(angleYZ));
            // R_YW
            matrix = MultiplyMatrices(matrix, RotationYWMatrix(angleYW));
            // R_ZW
            matrix = MultiplyMatrices(matrix, RotationZWMatrix(angleZW));

            return matrix;
        }



        private double[,] IdentityMatrix4D()
        {
            double[,] m = new double[4, 4];
            m[0, 0] = 1; m[1, 1] = 1; m[2, 2] = 1; m[3, 3] = 1;
            return m;
        }

        private double[,] RotationXYMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[0, 0] = c; m[0, 1] = -s;
            m[1, 0] = s; m[1, 1] = c;
            return m;
        }

        private double[,] RotationXZMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[0, 0] = c; m[0, 2] = -s;
            m[2, 0] = s; m[2, 2] = c;
            return m;
        }

        private double[,] RotationXWMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[0, 0] = c; m[0, 3] = -s;
            m[3, 0] = s; m[3, 3] = c;
            return m;
        }

        private double[,] RotationYZMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[1, 1] = c; m[1, 2] = -s;
            m[2, 1] = s; m[2, 2] = c;
            return m;
        }

        private double[,] RotationYWMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[1, 1] = c; m[1, 3] = -s;
            m[3, 1] = s; m[3, 3] = c;
            return m;
        }

        private double[,] RotationZWMatrix(double angle)
        {
            double[,] m = IdentityMatrix4D();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            m[2, 2] = c; m[2, 3] = -s;
            m[3, 2] = s; m[3, 3] = c;
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




        public void Reset()
        {
            _model.Reset();
        }
    }
}