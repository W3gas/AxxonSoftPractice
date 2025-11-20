using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace AxxonSoft_Prac
{
    public partial class MainWindow : Window
    {
        // --- Логические компоненты ---
        private TesseractModel _tesseractModel;
        private TesseractRotationCalculator _rotationCalculator;
        private TesseractRenderer _renderer;

        // --- UI компоненты ---
        private Canvas _drawCanvas;
        private RadioButton _radioX;
        private RadioButton _radioY;
        private RadioButton _radioZ;
        private RadioButton _radioAuto;
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
            FindUIElements();
            InitializeLogic();
            SetupEventHandlers();
            SetSliderValuesFromSettings();
            StartAnimationLoop();
        }

        private void FindUIElements()
        {
            _drawCanvas = this.FindControl<Canvas>("DrawCanvas");
            _radioX = this.FindControl<RadioButton>("RadioX");
            _radioY = this.FindControl<RadioButton>("RadioY");
            _radioZ = this.FindControl<RadioButton>("RadioZ");
            _radioAuto = this.FindControl<RadioButton>("RadioAuto");
            _btnStart = this.FindControl<Button>("BtnStart");
            _btnStop = this.FindControl<Button>("BtnStop");
            _btnReset = this.FindControl<Button>("BtnReset");
            _speedSlider = this.FindControl<Slider>("SpeedSlider");
            _sizeSlider = this.FindControl<Slider>("SizeSlider");
            _projectionDistSlider = this.FindControl<Slider>("ProjectionDistSlider");
            _projectionScaleSlider = this.FindControl<Slider>("ProjectionScaleSlider");
            _vertexSizeSlider = this.FindControl<Slider>("VertexSizeSlider");
            _changeEdgeColorButton = this.FindControl<Button>("ChangeEdgeColorButton");
            _changeVertexColorButton = this.FindControl<Button>("ChangeVertexColorButton");

            // Текстовые блоки для значений
            _speedValue = this.FindControl<TextBlock>("SpeedValue");
            _sizeValue = this.FindControl<TextBlock>("SizeValue");
            _projectionDistValue = this.FindControl<TextBlock>("ProjectionDistValue");
            _projectionScaleValue = this.FindControl<TextBlock>("ProjectionScaleValue");
            _vertexSizeValue = this.FindControl<TextBlock>("VertexSizeValue");

            
            
        }

        private void InitializeLogic()
        {
            _tesseractModel = new TesseractModel();
            _rotationCalculator = new TesseractRotationCalculator(_tesseractModel);
            _renderer = new TesseractRenderer(_drawCanvas, _tesseractModel);
        }

        private void SetupEventHandlers()
        {
            _btnStart.Click += BtnStart_Click;
            _btnStop.Click += BtnStop_Click;
            _btnReset.Click += BtnReset_Click;

            _radioX.Click += RadioX_Click;
            _radioY.Click += RadioY_Click;
            _radioZ.Click += RadioZ_Click;
            _radioAuto.Click += RadioAuto_Click;

            _changeEdgeColorButton.Click += ChangeEdgeColorButton_Click;
            _changeVertexColorButton.Click += ChangeVertexColorButton_Click;

            // Обработчики для слайдеров
            _speedSlider.ValueChanged += SpeedSlider_ValueChanged;
            _sizeSlider.ValueChanged += SizeSlider_ValueChanged;
            _projectionDistSlider.ValueChanged += ProjectionDistSlider_ValueChanged;
            _projectionScaleSlider.ValueChanged += ProjectionScaleSlider_ValueChanged;
            _vertexSizeSlider.ValueChanged += VertexSizeSlider_ValueChanged;

           
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
        }

        private void RadioX_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioX.IsChecked == true)
                _rotationCalculator.CurrentMode = RotationMode.ManualX;
        }

        private void RadioY_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioY.IsChecked == true)
                _rotationCalculator.CurrentMode = RotationMode.ManualY;
        }

        private void RadioZ_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioZ.IsChecked == true)
                _rotationCalculator.CurrentMode = RotationMode.ManualZ;
        }

        private void RadioAuto_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_radioAuto.IsChecked == true)
                _rotationCalculator.CurrentMode = RotationMode.Auto;
        }

        private void ChangeEdgeColorButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
           
            _currentEdgeColorIndex = (_currentEdgeColorIndex + 1) % TesseractSettings.EdgeColorPalette.Length;
            TesseractSettings.EdgeColor = TesseractSettings.EdgeColorPalette[_currentEdgeColorIndex];
            
            _drawCanvas.Children.Clear();
            _renderer = new TesseractRenderer(_drawCanvas, _tesseractModel);
        }

        private void ChangeVertexColorButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
           
            _currentVertexColorIndex = (_currentVertexColorIndex + 1) % TesseractSettings.VertexColorPalette.Length;
            TesseractSettings.VertexColor = TesseractSettings.VertexColorPalette[_currentVertexColorIndex];
            
            _drawCanvas.Children.Clear();
            _renderer = new TesseractRenderer(_drawCanvas, _tesseractModel);
        }




        private void SpeedSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            TesseractSettings.BaseRotationSpeed = e.NewValue;
            if (_speedValue != null)
                _speedValue.Text = e.NewValue.ToString("F3");
        }

        private void SizeSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            TesseractSettings.TesseractBaseSize = e.NewValue;
            if (_sizeValue != null)
                _sizeValue.Text = e.NewValue.ToString("F0");
            
            _tesseractModel = new TesseractModel(); 
           
            _rotationCalculator = new TesseractRotationCalculator(_tesseractModel);
            _drawCanvas.Children.Clear();
            _renderer = new TesseractRenderer(_drawCanvas, _tesseractModel);
          
            _rotationCalculator.Reset();
        }

        private void ProjectionDistSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            TesseractSettings.ProjectionDistance = e.NewValue;
            if (_projectionDistValue != null)
                _projectionDistValue.Text = e.NewValue.ToString("F0");
        }

        private void ProjectionScaleSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            TesseractSettings.ProjectionScale = e.NewValue;
            if (_projectionScaleValue != null)
                _projectionScaleValue.Text = e.NewValue.ToString("F0");
        }

        private void VertexSizeSlider_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            TesseractSettings.VertexSize = e.NewValue;
            if (_vertexSizeValue != null)
                _vertexSizeValue.Text = e.NewValue.ToString("F0");
            
            _drawCanvas.Children.Clear();
            _renderer = new TesseractRenderer(_drawCanvas, _tesseractModel); 
        }

        
        
        protected override void OnClosed(EventArgs e)
        {
            StopAnimationLoop();
            base.OnClosed(e);
        }
    }
}