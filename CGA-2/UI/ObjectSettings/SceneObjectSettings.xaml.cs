using CGA2.Components.Objects;
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

            ObjectYawTextBox.Text = RadiansToDegrees(sceneObject.Yaw).ToString();
            ObjectPitchTextBox.Text = RadiansToDegrees(sceneObject.Pitch).ToString();
            ObjectRollTextBox.Text = RadiansToDegrees(sceneObject.Roll).ToString();

            SceneObject = sceneObject;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SceneObject.Name = ObjectNameTextBox.Text;

            SceneObject.Yaw = DegreesToRadians(Parse(ObjectYawTextBox.Text));

            float pitch = DegreesToRadians(Parse(ObjectPitchTextBox.Text));
            float roll = DegreesToRadians(Parse(ObjectRollTextBox.Text));

            SceneObject.Pitch = SceneObject is CameraObject ? Clamp(pitch, -Pi / 2f, Pi / 2f) : pitch;
            SceneObject.Roll = SceneObject is CameraObject ? Clamp(roll, -Pi / 2f, Pi / 2f) : roll;

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
