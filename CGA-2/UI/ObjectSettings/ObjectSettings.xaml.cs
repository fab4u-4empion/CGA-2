using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Objects;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для ObjectSettings.xaml
    /// </summary>
    public partial class ObjectSettings : Window
    {
        public ObjectSettings(Scene scene)
        {
            InitializeComponent();

            foreach (SceneObject sceneObject in scene.RootObjects)
            {
                TreeViewItem treeViewItem = CreateTreeViewItem(sceneObject);

                ObjectsTreeView.Items.Add(treeViewItem);
            }
        }

        private static TreeViewItem CreateTreeViewItemContent(Component component)
        {
            TreeViewItem item = new()
            {
                Tag = component
            };

            Image img = new()
            {
                Width = 16,
                Height = 16
            };

            if (component is SceneObject)
                img.Source = new BitmapImage(new Uri("\\UI\\Images\\empty_orange.png", UriKind.Relative));

            if (component is MeshObject)
                img.Source = new BitmapImage(new Uri("\\UI\\Images\\cube_orange.png", UriKind.Relative));

            if (component is Mesh)
                img.Source = new BitmapImage(new Uri("\\UI\\Images\\cube_green.png", UriKind.Relative));

            if (component is CameraObject)
                img.Source = new BitmapImage(new Uri("\\UI\\Images\\camera_orange.png", UriKind.Relative));

            if (component is Camera)
                img.Source = new BitmapImage(new Uri("\\UI\\Images\\camera_green.png", UriKind.Relative));

            StackPanel content = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new(0, 1, 0, 1),
            };

            content.Children.Add(img);
            content.Children.Add(new TextBlock()
            {
                Text = component.Name,
                Margin = new(5, 0, 0, 0),
                FontSize = 14
            });

            item.Header = content;

            return item;
        }

        private static TreeViewItem CreateTreeViewItem(SceneObject sceneObject)
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

            return item;
        }
    }
}
