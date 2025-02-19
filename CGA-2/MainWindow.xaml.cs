using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Objects;
using CGA2.Renderers;
using CGA2.UI.ObjectSettings;
using CGA2.Utils;
using Microsoft.Win32;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CGA2.Settings;

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

        private Point MousePosition;

        private Stopwatch Timer = new();

        private Scene Scene = new();
        private Renderer Renderer = new Rasterizer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Draw()
        {
            Timer.Restart();
            Renderer.Render(Scene);
            Timer.Stop();

            ResolutionTextBlock.Text = $"{Renderer.Result.PixelWidth} × {Renderer.Result.PixelHeight}";
            RenderTimeTextBlock.Text = $"{Timer.ElapsedMilliseconds} ms";
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            Scale = (dpi.DpiScaleX, dpi.DpiScaleY);

            Renderer.ResizeBuffers(Grid.ActualWidth * Scale.X, Grid.ActualHeight * Scale.Y);
            Canvas.Source = Renderer.Result.Source;

            Draw();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.Source = Renderer.Result.Source;

            PerspectiveCamera camera = new();

            CameraObject cameraObject = new() 
            { 
                Camera = camera,
                Location = new(0, 5f, 20f),
            };

            Scene.CameraObjects.Add(cameraObject);
            Scene.Cameras.Add(cameraObject.Camera);
            Scene.RootObjects.Add(cameraObject);
            Scene.Nodes.Add(cameraObject);

            Scene.Environment.Color = ToneMapping.ToneMapper.SrgbToLinear(new(0.251f));

            Draw();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MousePosition = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point current_position = e.GetPosition(this);
                Scene.CameraObjects[SelectedCamera].Rotate((float)(current_position.X - MousePosition.X) * -0.0035f, (float)(current_position.Y - MousePosition.Y) * -0.0035f, 0f);
                Draw();
            }
            MousePosition = e.GetPosition(this);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) 
            {
                case Key.W:
                    Scene.CameraObjects[SelectedCamera].Move(-0.2f, 0f, 0f);
                    Draw();
                    break;

                case Key.S:
                    Scene.CameraObjects[SelectedCamera].Move(0.2f, 0f, 0f);
                    Draw();
                    break;

                case Key.A:
                    Scene.CameraObjects[SelectedCamera].Move(0f, -0.2f, 0f);
                    Draw();
                    break;

                case Key.D:
                    Scene.CameraObjects[SelectedCamera].Move(0, 0.2f, 0f);
                    Draw();
                    break;

                case Key.Q:
                    Scene.CameraObjects[SelectedCamera].Move(0f, 0, -0.2f);
                    Draw();
                    break;

                case Key.E:
                    Scene.CameraObjects[SelectedCamera].Move(0, 0f, 0.2f);
                    Draw();
                    break;

                case Key.F2:
                    ObjectSettings OSDialog = new(Scene);
                    OSDialog.ShowDialog();
                    break;

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
                            Scene.RootObjects[1].Location = new(0, -2f, 0);
                            //Scene.MeshObjects[1].Roll = float.Pi;
                            Draw();
                        }
                    }
                    break;
            }
        }
    }
}