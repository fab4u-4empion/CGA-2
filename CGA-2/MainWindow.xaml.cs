using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Objects;
using CGA2.Renderers;
using CGA2.UI;
using CGA2.UI.ObjectSettings;
using CGA2.Utils;
using Microsoft.Win32;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CGA2.Components.Objects.CameraObject;
using static CGA2.Settings;
using static System.Single;

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

        private void UpdateInfo()
        {
            ResolutionTextBlock.Text = $"{Renderer.Result.PixelWidth} × {Renderer.Result.PixelHeight}";
            RenderTimeTextBlock.Text = $"{Timer.ElapsedMilliseconds} ms";

            if (IsLoaded)
            {
                CameraModeInfo.Text = $"{Scene.CameraObjects[SelectedCamera].Mode}";
                CameraFoVInfo.Text = $"{Round(RadiansToDegrees((Scene.CameraObjects[SelectedCamera].Camera as PerspectiveCamera)!.FieldOfView), MidpointRounding.AwayFromZero)}°";
            }

            TonemappingInfo.Text = ToneMapper.Name;

            ShaderInfo.Text = Shader.Name;
        }

        private void Draw()
        {
            Timer.Restart();
            Renderer.Render(Scene);
            Timer.Stop();

            UpdateInfo();
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
                Location = Vector3.Create(0f, 5.5f, 20f),
                TargetRadius = 20f
            };

            Scene.CameraObjects.Add(cameraObject);
            Scene.Cameras.Add(cameraObject.Camera);
            Scene.RootObjects.Add(cameraObject);
            Scene.Nodes.Add(cameraObject);

            Scene.Environment.Color = ToneMapping.ToneMapper.SrgbToLinear(Vector3.Create(0.251f));

            GLTFReader.OpenFile("Assets\\lamps.gltf", Scene);

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

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            PerspectiveCamera camera = (PerspectiveCamera)Scene.CameraObjects[SelectedCamera].Camera;
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                
                camera.FieldOfView -= Pi / 60 * Sign(e.Delta);
                camera.FieldOfView = Clamp(camera.FieldOfView, Pi / 6, Pi / 2);
            }
            else
            {
                Scene.CameraObjects[SelectedCamera].UpdateTragetRadius(-0.3f * Sign(e.Delta));
            }
            Draw();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) 
            {
                case Key.Space:
                    if (!e.IsRepeat)
                        Scene.CameraObjects[SelectedCamera].Mode = (CameraMode)(((int)Scene.CameraObjects[SelectedCamera].Mode + 1) % Enum.GetNames<CameraMode>().Length);
                    Draw();
                    break;

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

                case Key.R:
                    Scene.CameraObjects[SelectedCamera].Rotate(0f, 0, 0.0035f);
                    Draw();
                    break;

                case Key.T:
                    Scene.CameraObjects[SelectedCamera].Rotate(0, 0f, -0.0035f);
                    Draw();
                    break;

                case Key.OemMinus:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        EmissionIntensity -= 10f;
                        EmissionIntensity = Max(EmissionIntensity, 0);
                    }
                    else
                    {
                        Exposure *= 0.84089642f;
                        Exposure = Max(Exposure, 0.03125f);
                    }
                    Draw();
                    break;

                case Key.OemPlus:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        EmissionIntensity += 10f;
                    else
                    {
                        Exposure *= 1.18920712f;
                        Exposure = Min(Exposure, 32);
                    }
                    Draw();
                    break;

                case Key.F2:
                    ObjectSettings OSDialog = new(Scene);
                    OSDialog.ShowDialog();
                    Scene.UpdateScene();
                    Draw();
                    break;

                case Key.F3:
                    ImageSettings imgSettings = new();
                    imgSettings.ShowDialog();
                    Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    Draw();
                    break;

                case Key.F4:
                    EnvironmentSettings envSettings = new(Scene.Environment);
                    envSettings.ShowDialog();
                    Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    Draw();
                    break;

                case Key.F5:
                    UseSkybox = !UseSkybox;
                    Draw();
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
                            Scene.UpdateScene();
                            Draw();
                        }
                    }
                    break;

                case Key.D1:
                    if (!e.IsRepeat)
                    {
                        Scaling = 0.25f;
                        Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    }
                    break;

                case Key.D2:
                    if (!e.IsRepeat)
                    {
                        Scaling = 0.5f;
                        Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    }
                    break;

                case Key.D3:
                    if (!e.IsRepeat)
                    {
                        Scaling = 1;
                        Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    }
                    break;

                case Key.D4:
                    if (!e.IsRepeat)
                    {
                        Scaling = 2;
                        Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    }
                    break;

                case Key.D5:
                    if (!e.IsRepeat)
                    {
                        Scaling = 4;
                        Window_SizeChanged(this, (EventArgs.Empty as SizeChangedEventArgs)!);
                    }
                    break;
            }
        }
        
    }
}