using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Objects;
using CGA2.Renderers;
using CGA2.Utils;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CGA2
{
    using DPIScale = (double X, double Y);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DPIScale Scale = (1, 1);

        private WindowState LastState;

        private Stopwatch Timer = new();

        private Scene Scene = new();
        private Renderer Renderer = new Rasterizer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            Scale = (dpi.DpiScaleX, dpi.DpiScaleY);

            Renderer.ResizeBuffers(Grid.ActualWidth * Scale.X, Grid.ActualHeight * Scale.Y);
            Canvas.Source = Renderer.Result.Source;

            Timer.Restart();
            Renderer.Render(Scene);
            Timer.Stop();

            ResolutionTextBlock.Text = $"{Renderer.Result.PixelWidth} × {Renderer.Result.PixelHeight}";
            RenderTimeTextBlock.Text = $"{Timer.ElapsedMilliseconds} ms";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.Source = Renderer.Result.Source;

            PerspectiveCamera camera = new();

            CameraObject cameraObject = new() 
            { 
                Camera = camera,
                Location = new(0, 5f, 30f),
            };

            Scene.CameraObjects.Add(cameraObject);
            Scene.Cameras.Add(camera);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) 
            {
                case Key.F11:
                    if (!e.IsRepeat)
                    {
                        if (WindowStyle != WindowStyle.None)
                        {
                            LastState = WindowState;
                            WindowStyle = WindowStyle.None;
                            ResizeMode = ResizeMode.NoResize;
                            WindowState = WindowState.Normal;
                            WindowState = WindowState.Maximized;
                        }
                        else
                        {
                            WindowStyle = WindowStyle.SingleBorderWindow;
                            ResizeMode = ResizeMode.CanResize;
                            WindowState = LastState;
                        }
                    }
                    break;

                case Key.O:
                    if (!e.IsRepeat)
                    {
                        OpenFileDialog dlg = new()
                        {
                            Filter = "glTF (*.gltf)|*.gltf"
                        };

                        if (dlg.ShowDialog() == true)
                        {
                            GLTFReader.OpenFile(dlg.FileName, Scene);
                        }
                    }
                    break;
            }
        }
    }
}