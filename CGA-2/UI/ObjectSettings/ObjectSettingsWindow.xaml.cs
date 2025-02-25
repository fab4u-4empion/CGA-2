using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Objects;
using SharpVectors.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private static TreeViewItem CreateTreeViewItemContent(Component component)
        {
            TreeViewItem item = new()
            {
                Tag = component,
                Cursor = Cursors.Hand
            };

            SvgViewbox svg = new()
            {
                Width = 16,
                Height = 16
            };

            if (component is SceneObject)
                svg.Source = new("\\UI\\Images\\empty_orange.svg", UriKind.Relative);

            if (component is MeshObject)
                svg.Source = new("\\UI\\Images\\cube_orange.svg", UriKind.Relative);

            if (component is Mesh)
                svg.Source = new("\\UI\\Images\\cube_green.svg", UriKind.Relative);

            if (component is CameraObject)
                svg.Source = new("\\UI\\Images\\camera_orange.svg", UriKind.Relative);

            if (component is Camera)
                svg.Source = new("\\UI\\Images\\camera_green.svg", UriKind.Relative);

            StackPanel content = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new(0, 1, 0, 1),
            };

            content.Children.Add(svg);
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
            }
        }
    }
}
