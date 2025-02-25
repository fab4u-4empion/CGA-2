using CGA2.Components;
using System.Windows;
using System.Windows.Controls;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для MeshSettings.xaml
    /// </summary>
    public partial class MeshSettings : UserControl
    {
        private Mesh Mesh { get; set; }

        public event EventHandler? OnSave;

        public MeshSettings(Mesh mesh)
        {
            InitializeComponent();

            MeshNameTextBox.Text = mesh.Name;

            Mesh = mesh;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Mesh.Name = MeshNameTextBox.Text;

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
