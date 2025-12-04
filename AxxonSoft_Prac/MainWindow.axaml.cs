using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using Avalonia.Input;

namespace AxxonSoft_Prac
{
    public partial class MainWindow : Window
    {
        // --- Логические компоненты ---
        private TesseractModel _tesseractModel;
        private TesseractRotationCalculator _rotationCalculator;
        private TesseractRenderer _renderer;

        // --- bool elements ---
        private bool _isInManualDragMode = false;
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private bool _isShiftPressed = false;

        // --- UI компоненты ---
        private Canvas _drawCanvas;
        private RadioButton _radioX;
        private RadioButton _radioY;
        private RadioButton _radioZ;
        private RadioButton _radioAuto;
        private RadioButton _radioManual;
        private Button _btnStart;
        private Button _btnStop;
        private Button _btnReset;
        private Slider _speedSlider;
        private Slider _sizeSlider;
        private Slider _projectionDistSlider;
        private Slider _projectionScaleSlider;
        private Slider _vertexSizeSlider;
        private Button _changeEdgeColorButton;
        private Button _changeVertexColorButton;

        // Текстовые блоки для значений
        private TextBlock _speedValue;
        private TextBlock _sizeValue;
        private TextBlock _projectionDistValue;
        private TextBlock _projectionScaleValue;
        private TextBlock _vertexSizeValue;
        
       

       
        private bool _isAnimating = false;

        
        private int _currentEdgeColorIndex = 0;
        private int _currentVertexColorIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            bool uiLoadedSuccessfully = FindUIElements();
            if (uiLoadedSuccessfully)
            {
                InitializeLogic();
                SetupEventHandlers();
                SetSliderValuesFromSettings();
                StartAnimationLoop();
            }
            else
            {
                Logger.Warn("UI initialization failed. Core logic will not be started.");
            }


            //  Handels of keys
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
        }

        private bool FindUIElements()
        {
            var missing = new List<string>();

            T Require<T>(string name) where T : Control
            {
                var c = this.FindControl<T>(name);
                if (c is null)
                {
                    missing.Add(name);
                }
                return c;
            }

            _drawCanvas = Require<Canvas>("DrawCanvas");
            _radioX = Require<RadioButton>("RadioX");
            _radioY = Require<RadioButton>("RadioY");
            _radioZ = Require<RadioButton>("RadioZ");
            _radioAuto = Require<RadioButton>("RadioAuto");
            _radioManual = Require<RadioButton>("RadioManual");
            _btnStart = Require<Button>("BtnStart");
            _btnStop = Require<Button>("BtnStop");
            _btnReset = Require<Button>("BtnReset");
            _speedSlider = Require<Slider>("SpeedSlider");
            _sizeSlider = Require<Slider>("SizeSlider");
            _projectionDistSlider = Require<Slider>("ProjectionDistSlider");
            _projectionScaleSlider = Require<Slider>("ProjectionScaleSlider");
            _vertexSizeSlider = Require<Slider>("VertexSizeSlider");
            _changeEdgeColorButton = Require<Button>("ChangeEdgeColorButton");
            _changeVertexColorButton = Require<Button>("ChangeVertexColorButton");

            _speedValue = Require<TextBlock>("SpeedValue");
            _sizeValue = Require<TextBlock>("SizeValue");
            _projectionDistValue = Require<TextBlock>("ProjectionDistValue");
            _projectionScaleValue = Require<TextBlock>("ProjectionScaleValue");
            _vertexSizeValue = Require<TextBlock>("VertexSizeValue");

            if (missing.Count > 0)
            {
                Logger.Error("Failed to find UI controls: " + string.Join(", ", missing));
                return false;
            }

            return true;
        }


        private void InitializeLogic()
        {
            _tesseractModel = new TesseractModel();
            _rotationCalculator = new TesseractRotationCalculator(_tesseractModel);
            _renderer = new TesseractRenderer(_drawCanvas, _tesseractModel);
        }

        private void SetupEventHandlers()
        {
            //  Handlers of Window_bounds
            _drawCanvas.PropertyChanged += DrawCanvas_PropertyChanged;

            _btnStart.Click += BtnStart_Click;
            _btnStop.Click += BtnStop_Click;
            _btnReset.Click += BtnReset_Click;

            _radioX.Click += RadioX_Click;
            _radioY.Click += RadioY_Click;
            _radioZ.Click += RadioZ_Click;
            _radioAuto.Click += RadioAuto_Click;
            _radioManual.Click += RadioManual_Click;

            _changeEdgeColorButton.Click += ChangeEdgeColorButton_Click;
            _changeVertexColorButton.Click += ChangeVertexColorButton_Click;

            // Обработчики для слайдеров
            _speedSlider.ValueChanged += SpeedSlider_ValueChanged;
            _sizeSlider.ValueChanged += SizeSlider_ValueChanged;
            _projectionDistSlider.ValueChanged += ProjectionDistSlider_ValueChanged;
            _projectionScaleSlider.ValueChanged += ProjectionScaleSlider_ValueChanged;
            _vertexSizeSlider.ValueChanged += VertexSizeSlider_ValueChanged;

            // Handlers of mouse
            _drawCanvas.PointerPressed += DrawCanvas_PointerPressed;
            _drawCanvas.PointerMoved += DrawCanvas_PointerMoved;
            _drawCanvas.PointerReleased += DrawCanvas_PointerReleased;
           
        }

        private void SetSliderValuesFromSettings()
        {
            _speedSlider.Value = TesseractSettings.BaseRotationSpeed;
            _sizeSlider.Value = TesseractSettings.TesseractBaseSize;
            _projectionDistSlider.Value = TesseractSettings.ProjectionDistance;
            _projectionScaleSlider.Value = TesseractSettings.ProjectionScale;
            _vertexSizeSlider.Value = TesseractSettings.VertexSize;

            
            _speedValue.Text = TesseractSettings.BaseRotationSpeed.ToString("F3");
            _sizeValue.Text = TesseractSettings.TesseractBaseSize.ToString("F0");
            _projectionDistValue.Text = TesseractSettings.ProjectionDistance.ToString("F0");
            _projectionScaleValue.Text = TesseractSettings.ProjectionScale.ToString("F0");
            _vertexSizeValue.Text = TesseractSettings.VertexSize.ToString("F0");
        }


        private void UpdateUIForManualDragMode()
        {
            _btnStart.IsEnabled = !_isInManualDragMode;
            _btnStop.IsEnabled = !_isInManualDragMode;
            _speedSlider.IsEnabled = !_isInManualDragMode;
            _btnReset.IsEnabled = true;

            // Установка курсора без тернарного оператора
            if (_isInManualDragMode)
            {
                _drawCanvas.Cursor = new Cursor(StandardCursorType.SizeAll);
            }
            else
            {
                _drawCanvas.Cursor = null;
            }
        }


        private void StartAnimationLoop()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            RequestNextFrame();
        }


        private void StopAnimationLoop()
        {
            _isAnimating = false;
        }


        private void RequestNextFrame()
        {
            if (!_isAnimating) return;

            var topLevel = TopLevel.GetTopLevel(this);
            topLevel?.RequestAnimationFrame(timestamp =>
            {
                _rotationCalculator.Update();
                _renderer.Update();
                RequestNextFrame();
            });
        }


        private void BtnStart_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            StartAnimationLoop();
        }


        private void BtnStop_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            StopAnimationLoop();
        }


        private void BtnReset_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _rotationCalculator.Reset();
            _renderer.Update();
        }


        private void RadioX_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioX.IsChecked == true)
            {
                _rotationCalculator.CurrentMode = RotationMode.ManualX;
                _isInManualDragMode = false;
                UpdateUIForManualDragMode();
            }    
               
        }


        private void RadioY_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioY.IsChecked == true)
            {
                _rotationCalculator.CurrentMode = RotationMode.ManualY;
                _isInManualDragMode = false;
                UpdateUIForManualDragMode();
            }
               
        }


        private void RadioZ_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioZ.IsChecked == true)
            {
                _rotationCalculator.CurrentMode = RotationMode.ManualZ;
                _isInManualDragMode = false;
                UpdateUIForManualDragMode();
            }
               
        }


        private void RadioAuto_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioAuto.IsChecked == true)
            {
                _rotationCalculator.CurrentMode = RotationMode.Auto;
                _isInManualDragMode = false;
                UpdateUIForManualDragMode();
            }
                 
        }


        private void RadioManual_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioManual.IsChecked == true)
            {
                _rotationCalculator.CurrentMode = RotationMode.ManualDrag;
                _isInManualDragMode = true;
                StopAnimationLoop();
                UpdateUIForManualDragMode();
            }
        }


        private void ChangeEdgeColorButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
           
            _currentEdgeColorIndex = (_currentEdgeColorIndex + 1) % TesseractSettings.EdgeColorPalette.Length;
            TesseractSettings.EdgeColor = TesseractSettings.EdgeColorPalette[_currentEdgeColorIndex];
            
           _renderer.UpdateColors();
        }


        private void ChangeVertexColorButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
           
            _currentVertexColorIndex = (_currentVertexColorIndex + 1) % TesseractSettings.VertexColorPalette.Length;
            TesseractSettings.VertexColor = TesseractSettings.VertexColorPalette[_currentVertexColorIndex];

            _renderer.UpdateColors();
        }


        private void SpeedSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < TesseractSettings.MinRotationSpeed)
                newValue = TesseractSettings.MinRotationSpeed;
            else if (newValue > TesseractSettings.MaxRotationSpeed)
                newValue = TesseractSettings.MaxRotationSpeed;

            TesseractSettings.BaseRotationSpeed = newValue;
            if (_speedValue != null)
                _speedValue.Text = newValue.ToString("F3");
        }


        private void SizeSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;

            if (newValue < TesseractSettings.MinTesseractSize)
                newValue = TesseractSettings.MinTesseractSize;
            else if (newValue > TesseractSettings.MaxTesseractSize)
                newValue = TesseractSettings.MaxTesseractSize;

            TesseractSettings.TesseractBaseSize = newValue;
            if (_sizeValue != null)
                _sizeValue.Text = newValue.ToString("F0");

            _tesseractModel.RegenerateVerticesFromCurrentSize();
            _rotationCalculator.ApplyCurrentRotation();
            _renderer.Update();
        }


        private void ProjectionDistSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < TesseractSettings.MinProjectionDistance)
                newValue = TesseractSettings.MinProjectionDistance;
            else if (newValue > TesseractSettings.MaxProjectionDistance)
                newValue = TesseractSettings.MaxProjectionDistance;

            TesseractSettings.ProjectionDistance = newValue;
            if (_projectionDistValue != null)
                _projectionDistValue.Text = newValue.ToString("F0");

            _renderer.Update();
        }


        private void ProjectionScaleSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < TesseractSettings.MinProjectionScale)
                newValue = TesseractSettings.MinProjectionScale;
            else if (newValue > TesseractSettings.MaxProjectionScale)
                newValue = TesseractSettings.MaxProjectionScale;

            TesseractSettings.ProjectionScale = newValue;
            if (_projectionScaleValue != null)
                _projectionScaleValue.Text = newValue.ToString("F0");

            _renderer.Update();
        }


        private void VertexSizeSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < TesseractSettings.MinVertexSize)
                newValue = TesseractSettings.MinVertexSize;
            else if (newValue > TesseractSettings.MaxVertexSize)
                newValue = TesseractSettings.MaxVertexSize;

            TesseractSettings.VertexSize = newValue;
            if (_vertexSizeValue != null)
                _vertexSizeValue.Text = newValue.ToString("F0");

            _renderer.Update();
        }


        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = true;
            }
        }


        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = false;
            }
        }


        private void DrawCanvas_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!_isInManualDragMode) return;
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(_drawCanvas);
                e.Pointer.Capture(_drawCanvas); 
            }
        }

        private void DrawCanvas_PointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isInManualDragMode || !_isDragging) return;

            var currentPos = e.GetPosition(_drawCanvas);
            double deltaX = currentPos.X - _lastMousePosition.X;
            double deltaY = currentPos.Y - _lastMousePosition.Y;

            if (_isShiftPressed)
            {
                _rotationCalculator.HandleMouseDragAlternate(deltaX, deltaY);
            }
            else
            {
                _rotationCalculator.HandleMouseDragPrimary(deltaX, deltaY);
            }

            _renderer.Update();
            _lastMousePosition = currentPos;
        }

        private void DrawCanvas_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                e.Pointer.Capture(null); 
            }
        }


        private void DrawCanvas_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Canvas.BoundsProperty)
            {
                _renderer.Update();
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            StopAnimationLoop();
            base.OnClosed(e);
        }
    }
}