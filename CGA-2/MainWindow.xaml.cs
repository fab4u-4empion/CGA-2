using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CGA_2
{
    using DPIScale = (double X, double Y);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DPIScale Scale = (1, 1);

        private WindowState LastState;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            Scale = (dpi.DpiScaleX, dpi.DpiScaleY);

            ResolutionTextBlock.Text = $"{(int)(Grid.ActualWidth * Scale.X)} × {(int)(Grid.ActualHeight * Scale.Y)}";
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
            }
        }
    }
}