using CGA2.Components.Objects;
using System.Globalization;
using System.Windows.Controls;
using static System.Single;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для SceneObjectSettings.xaml
    /// </summary>
    public partial class SceneObjectSettings : UserControl
    {
        private SceneObject SceneObject { get; set; }

        public event EventHandler? OnSave;
            
        public SceneObjectSettings(SceneObject sceneObject)
        {
            InitializeComponent();

            ObjectNameTextBox.Text = sceneObject.Name;

            ObjectYawTextBox.Text = RadiansToDegrees(sceneObject.Yaw).ToString("0.###", CultureInfo.InvariantCulture);
            ObjectPitchTextBox.Text = RadiansToDegrees(sceneObject.Pitch).ToString("0.###", CultureInfo.InvariantCulture);
            ObjectRollTextBox.Text = RadiansToDegrees(sceneObject.Roll).ToString("0.###", CultureInfo.InvariantCulture);

            ObjectXTextBox.Text = sceneObject.Location.X.ToString("0.###", CultureInfo.InvariantCulture);
            ObjectYTextBox.Text = sceneObject.Location.Y.ToString("0.###", CultureInfo.InvariantCulture);
            ObjectZTextBox.Text = sceneObject.Location.Z.ToString("0.###", CultureInfo.InvariantCulture);

            ObjectScaleXTextBox.Text = sceneObject.Scale.X.ToString("0.###", CultureInfo.InvariantCulture);
            ObjectScaleYTextBox.Text = sceneObject.Scale.Y.ToString("0.###", CultureInfo.InvariantCulture);
            ObjectScaleZTextBox.Text = sceneObject.Scale.Z.ToString("0.###", CultureInfo.InvariantCulture);

            SceneObject = sceneObject;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SceneObject.Name = ObjectNameTextBox.Text;

            SceneObject.Yaw = DegreesToRadians(Parse(ObjectYawTextBox.Text, CultureInfo.InvariantCulture));

            float pitch = DegreesToRadians(Parse(ObjectPitchTextBox.Text, CultureInfo.InvariantCulture));
            float roll = DegreesToRadians(Parse(ObjectRollTextBox.Text, CultureInfo.InvariantCulture));

            SceneObject.Pitch = SceneObject is CameraObject ? Clamp(pitch, -Pi / 2f, Pi / 2f) : pitch;
            SceneObject.Roll = SceneObject is CameraObject ? Clamp(roll, -Pi / 2f, Pi / 2f) : roll;

            SceneObject.Location = new(
                Parse(ObjectXTextBox.Text, CultureInfo.InvariantCulture),
                Parse(ObjectYTextBox.Text, CultureInfo.InvariantCulture),
                Parse(ObjectZTextBox.Text, CultureInfo.InvariantCulture)
            );

            SceneObject.Scale = new(
                Parse(ObjectScaleXTextBox.Text, CultureInfo.InvariantCulture),
                Parse(ObjectScaleYTextBox.Text, CultureInfo.InvariantCulture),
                Parse(ObjectScaleZTextBox.Text, CultureInfo.InvariantCulture)
            );

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
