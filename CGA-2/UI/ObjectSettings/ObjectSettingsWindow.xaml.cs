using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Lights;
using CGA2.Components.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для ObjectSettings.xaml
    /// </summary>
    public partial class ObjectSettings : Window
    {
        private Scene Scene { get; set; }
        public ObjectSettings(Scene scene)
        {
            InitializeComponent();

            Scene = scene;

            UpdateTreeView();
        }

        private TreeViewItem CreateTreeViewItemContent(Component component)
        {
            TreeViewItem item = new()
            {
                Tag = component,
                Cursor = Cursors.Hand
            };

            Path icon = new()
            {
                Data = Resources["EmptyIcon"] as Geometry,
                Fill = Brushes.Orange,
                Stretch = Stretch.Uniform,
                Width = 16,
                Height = 16
            };

            if (component is MeshObject || component is Mesh)
                icon.Data = Resources["MeshIcon"] as Geometry;

            if (component is CameraObject || component is Camera)
                icon.Data = Resources["CameraIcon"] as Geometry;

            if (component is LightObject || component is Light)
                icon.Data = Resources["LightIcon"] as Geometry;

            if (component is Camera || component is Mesh || component is Light)
                icon.Fill = Brushes.ForestGreen;

            StackPanel content = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new(0, 1, 0, 1),
            };

            content.Children.Add(icon);
            content.Children.Add(new TextBlock()
            {
                Text = component.Name,
                Margin = new(5, 0, 0, 0),
                FontSize = 14
            });

            item.Header = content;

            return item;
        }

        private TreeViewItem CreateTreeViewItem(SceneObject sceneObject)
        {
            TreeViewItem item = CreateTreeViewItemContent(sceneObject);

            foreach (SceneObject childSceneObject in sceneObject.Children)
            {
                TreeViewItem childItem = CreateTreeViewItem(childSceneObject);
                item.Items.Add(childItem);
            }

            if (sceneObject is MeshObject meshObject)
                item.Items.Add(CreateTreeViewItemContent(meshObject.Mesh));

            if (sceneObject is CameraObject cameraObject)
                item.Items.Add(CreateTreeViewItemContent(cameraObject.Camera));

            if (sceneObject is LightObject lightObject)
                item.Items.Add(CreateTreeViewItemContent(lightObject.Light));

            return item;
        }

        private void UpdateTreeView()
        {
            ObjectsTreeView.Items.Clear();

            foreach (SceneObject sceneObject in Scene.RootObjects)
            {
                TreeViewItem treeViewItem = CreateTreeViewItem(sceneObject);

                ObjectsTreeView.Items.Add(treeViewItem);
            }
        }

        private void UpdateTreeViewItem(object? sender, EventArgs args)
        {
            if (ObjectsTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                ((selectedItem.Header as StackPanel)!.Children[1] as TextBlock)!.Text = (selectedItem.Tag as Component)!.Name;
            }
        }

        private void ObjectsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((sender as TreeView)!.SelectedItem is TreeViewItem selectedItem)
            {
                Component component = (Component)selectedItem!.Tag;

                ObjectSettingsStackPanel.Children.Clear();

                if (component is SceneObject)
                {
                    SceneObjectSettings sceneObjectSettings = new((SceneObject)component!);
                    sceneObjectSettings.OnSave += UpdateTreeViewItem;
                    ObjectSettingsStackPanel.Children.Add(sceneObjectSettings);
                }

                if (component is PerspectiveCamera)
                {
                    PerspectiveCameraSettings perspectiveCameraSettings = new((PerspectiveCamera)component!);
                    perspectiveCameraSettings.OnSave += UpdateTreeViewItem;
                    ObjectSettingsStackPanel.Children.Add(perspectiveCameraSettings);
                }

                if (component is Mesh)
                {
                    MeshSettings meshSettings = new((Mesh)component!);
                    meshSettings.OnSave += UpdateTreeViewItem;
                    ObjectSettingsStackPanel.Children.Add(meshSettings);
                }
            }
        }
    }
}
