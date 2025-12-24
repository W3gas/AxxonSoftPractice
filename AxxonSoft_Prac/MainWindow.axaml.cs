using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace AxxonSoft_Prac
{
    public partial class MainWindow : Window
    {
        // --- Logick elements ---
        private FigureModel4D _currentFigure;
        private FigureRotationCalculator _rotationCalculator;
        private FigureRenderer _renderer;

        // --- Bool elements ---
        private bool _isInManualDragMode = false;
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private bool _isShiftPressed = false;
        private bool _isAnimating = false;

        // --- UI elements ---
        private Canvas _drawCanvas;
        private RadioButton _radioX;
        private RadioButton _radioY;
        private RadioButton _radioZ;
        private RadioButton _radioAuto;
        private RadioButton _radioManual;
        private Button _btnTogglePlayPause;
        private Button _btnReset;
        private Slider _speedSlider;
        private Slider _sizeSlider;
        private Slider _projectionDistSlider;
        private Slider _projectionScaleSlider;
        private Slider _vertexSizeSlider;
        private ComboBox _edgeColorSelector;
        private ComboBox _vertexColorSelector;
        private ComboBox _figureSelector;

        // Text blocks
        private TextBlock _speedValue;
        private TextBlock _sizeValue;
        private TextBlock _projectionDistValue;
        private TextBlock _projectionScaleValue;
        private TextBlock _vertexSizeValue;
        

        public MainWindow()
        {
            InitializeComponent();

            bool uiLoadedSuccessfully = FindUIElements();
            if (uiLoadedSuccessfully)
            {
                LoadFigureSelector();
                LoadColorSelectors();
                InitializeLogic();
                SetupEventHandlers();
                SetSliderValuesFromSettings();
                StartAnimationLoop();
            }
            else
            {
                Logger.Warn("UI initialization failed. Core logic will not be started.");
            }


            //  Handlers of keys
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
            _btnTogglePlayPause = Require<Button>("BtnTogglePlayPause");
            _btnReset = Require<Button>("BtnReset");
            _speedSlider = Require<Slider>("SpeedSlider");
            _sizeSlider = Require<Slider>("SizeSlider");
            _projectionDistSlider = Require<Slider>("ProjectionDistSlider");
            _projectionScaleSlider = Require<Slider>("ProjectionScaleSlider");
            _vertexSizeSlider = Require<Slider>("VertexSizeSlider");
            _edgeColorSelector = Require<ComboBox>("EdgeColorSelector");
            _vertexColorSelector = Require<ComboBox>("VertexColorSelector");
            _figureSelector = Require<ComboBox>("FigureSelector");

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
            InitializeFigure(FigureCatalog.GetDefault()); // default figure
        }

        
        private void InitializeFigure(FigureType figureType)
        {
            // Clear Canvas
            _drawCanvas.Children.Clear();

            // Create model
            _currentFigure = FigureCatalog.Create(figureType);

            // Create calculator and renderer
            _rotationCalculator = new FigureRotationCalculator(_currentFigure);
            _renderer = new FigureRenderer(_drawCanvas, _currentFigure);

            // apply settings
            _renderer.UpdateColors();
            _renderer.Update();

            // apply rotation mode
            if (_radioX.IsChecked == true) _rotationCalculator.CurrentMode = RotationMode.ManualX;
            else if (_radioY.IsChecked == true) _rotationCalculator.CurrentMode = RotationMode.ManualY;
            else if (_radioZ.IsChecked == true) _rotationCalculator.CurrentMode = RotationMode.ManualZ;
            else if (_radioAuto.IsChecked == true) _rotationCalculator.CurrentMode = RotationMode.Auto;
            else if (_radioManual.IsChecked == true) _rotationCalculator.CurrentMode = RotationMode.ManualDrag;
        }


        private void SetupEventHandlers()
        {
            //  Handlers of window_bounds
            _drawCanvas.PropertyChanged += DrawCanvas_PropertyChanged;

            _btnTogglePlayPause.Click += BtnTogglePlayPause_Click;
            _btnReset.Click += BtnReset_Click;

            _radioX.Click += RadioX_Click;
            _radioY.Click += RadioY_Click;
            _radioZ.Click += RadioZ_Click;
            _radioAuto.Click += RadioAuto_Click;
            _radioManual.Click += RadioManual_Click;

            _edgeColorSelector.SelectionChanged += EdgeColorSelector_SelectionChanged;
            _vertexColorSelector.SelectionChanged += VertexColorSelector_SelectionChanged;
            _figureSelector.SelectionChanged += FigureSelector_SelectionChanged;

            // Handlers of sliders
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
            _speedSlider.Value = FigureSettings.BaseRotationSpeed;
            _sizeSlider.Value = FigureSettings.TesseractBaseSize;
            _projectionDistSlider.Value = FigureSettings.ProjectionDistance;
            _projectionScaleSlider.Value = FigureSettings.ProjectionScale;
            _vertexSizeSlider.Value = FigureSettings.VertexSize;

            
            _speedValue.Text = FigureSettings.BaseRotationSpeed.ToString("F3");
            _sizeValue.Text = FigureSettings.TesseractBaseSize.ToString("F0");
            _projectionDistValue.Text = FigureSettings.ProjectionDistance.ToString("F0");
            _projectionScaleValue.Text = FigureSettings.ProjectionScale.ToString("F0");
            _vertexSizeValue.Text = FigureSettings.VertexSize.ToString("F0");
        }


        private void UpdateUIForManualDragMode()
        {
            _btnTogglePlayPause.IsEnabled = !_isInManualDragMode;

            if (_isInManualDragMode)
            {
                StopAnimationLoop();
                _btnTogglePlayPause.Content = "Play";
                _drawCanvas.Cursor = new Cursor(StandardCursorType.SizeAll);
            }
            else
            {
                if (_isAnimating)
                {
                    _btnTogglePlayPause.Content = "Pause";
                }
                else
                {
                    _btnTogglePlayPause.Content = "Play";
                }
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


        private void BtnTogglePlayPause_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_isAnimating)
            {
                StopAnimationLoop();
                _btnTogglePlayPause.Content = "Play";
            }
            else
            {
                StartAnimationLoop();
                _btnTogglePlayPause.Content = "Pause";
            }
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


        private void LoadColorSelectors()
        {
            _edgeColorSelector.Items.Clear();
            _vertexColorSelector.Items.Clear();

            foreach (var color in FigureSettings.EdgeColorPalette)
            {
                var border = new Border
                {
                    Width = 20,
                    Height = 16,
                    Background = new SolidColorBrush(color),
                    Margin = new Avalonia.Thickness(2),
                    CornerRadius = new Avalonia.CornerRadius(2)
                };
                _edgeColorSelector.Items.Add(new ComboBoxItem { Content = border });
            }

            foreach (var color in FigureSettings.VertexColorPalette)
            {
                var border = new Border
                {
                    Width = 20,
                    Height = 16,
                    Background = new SolidColorBrush(color),
                    Margin = new Avalonia.Thickness(2),
                    CornerRadius = new Avalonia.CornerRadius(2)
                };
                _vertexColorSelector.Items.Add(new ComboBoxItem { Content = border });
            }

            _edgeColorSelector.SelectedIndex = 0;
            _vertexColorSelector.SelectedIndex = 0;
        }


        private void EdgeColorSelector_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (_edgeColorSelector.SelectedIndex >= 0)
            {
                FigureSettings.EdgeColor = FigureSettings.EdgeColorPalette[_edgeColorSelector.SelectedIndex];
                _renderer?.UpdateColors();
            }
        }

        
        private void VertexColorSelector_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (_vertexColorSelector.SelectedIndex >= 0)
            {
                FigureSettings.VertexColor = FigureSettings.VertexColorPalette[_vertexColorSelector.SelectedIndex];
                _renderer?.UpdateColors();
            }
        }


        private void SpeedSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < FigureSettings.MinRotationSpeed)
                newValue = FigureSettings.MinRotationSpeed;
            else if (newValue > FigureSettings.MaxRotationSpeed)
                newValue = FigureSettings.MaxRotationSpeed;

            FigureSettings.BaseRotationSpeed = newValue;
            if (_speedValue != null)
                _speedValue.Text = newValue.ToString("F3");
        }


        private void SizeSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;

            if (newValue < FigureSettings.MinTesseractSize)
                newValue = FigureSettings.MinTesseractSize;
            else if (newValue > FigureSettings.MaxTesseractSize)
                newValue = FigureSettings.MaxTesseractSize;

            FigureSettings.TesseractBaseSize = newValue;
            if (_sizeValue != null)
                _sizeValue.Text = newValue.ToString("F0");

            _currentFigure.RegenerateVerticesFromCurrentSize();
            _rotationCalculator.ApplyCurrentRotation();
            _renderer.Update();
        }


        private void ProjectionDistSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < FigureSettings.MinProjectionDistance)
                newValue = FigureSettings.MinProjectionDistance;
            else if (newValue > FigureSettings.MaxProjectionDistance)
                newValue = FigureSettings.MaxProjectionDistance;

            FigureSettings.ProjectionDistance = newValue;
            if (_projectionDistValue != null)
                _projectionDistValue.Text = newValue.ToString("F0");

            _renderer.Update();
        }


        private void ProjectionScaleSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < FigureSettings.MinProjectionScale)
                newValue = FigureSettings.MinProjectionScale;
            else if (newValue > FigureSettings.MaxProjectionScale)
                newValue = FigureSettings.MaxProjectionScale;

            FigureSettings.ProjectionScale = newValue;
            if (_projectionScaleValue != null)
                _projectionScaleValue.Text = newValue.ToString("F0");

            _renderer.Update();
        }


        private void VertexSizeSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            double newValue = e.NewValue;
            if (newValue < FigureSettings.MinVertexSize)
                newValue = FigureSettings.MinVertexSize;
            else if (newValue > FigureSettings.MaxVertexSize)
                newValue = FigureSettings.MaxVertexSize;

            FigureSettings.VertexSize = newValue;
            if (_vertexSizeValue != null)
                _vertexSizeValue.Text = newValue.ToString("F0");

            _renderer.Update();
        }


        private void LoadFigureSelector()
        {
            _figureSelector.Items.Clear();
            foreach (var name in FigureCatalog.DisplayNames.Values)
            {
                _figureSelector.Items.Add(name);
            }
            _figureSelector.SelectedIndex = 0;
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


        private void FigureSelector_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (_figureSelector.SelectedIndex < 0) return;

            var figureTypes = new List<FigureType>(FigureCatalog.DisplayNames.Keys);
            FigureType selectedType = figureTypes[_figureSelector.SelectedIndex];

            InitializeFigure(selectedType);
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