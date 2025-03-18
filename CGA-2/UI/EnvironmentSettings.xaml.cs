using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using static CGA2.ToneMapping.ToneMapper;
using static CGA2.Utils.Vector3ToColorConverter;

namespace CGA2.UI
{
    /// <summary>
    /// Логика взаимодействия для EnvironmentSettings.xaml
    /// </summary>
    public partial class EnvironmentSettings : Window
    {
        Components.Environment Environment { get; set; }

        public EnvironmentSettings(Components.Environment env)
        {
            InitializeComponent();

            if (env.IBLSpecularMap.Count > 0)
                EnvironmentPreview.Source = env.IBLSpecularMap[0].ToLDR().Source;

            AmbientColorBtn.Color = Vector3ToColor(LinearToSrgb(env.Color));

            Environment = env;
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "IBL Configs (*.ibl)|*.ibl"
            };

            if (ofd.ShowDialog() == true)
            {
                Environment.IBLSpecularMap.Clear();

                string directory = Path.GetDirectoryName(ofd.FileName)!;

                JToken jsonData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(ofd.FileName))!;

                Environment.IBLDiffuseMap = new(Path.Combine(directory, (string)jsonData["irradiance"]!));

                foreach (JToken specPath in jsonData["specular"]!)
                    Environment.IBLSpecularMap.Add(new(Path.Combine(directory, (string)specPath!)));

                EnvironmentPreview.Source = Environment.IBLSpecularMap[0].ToLDR().Source;
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.IBLDiffuseMap = null;
            Environment.IBLSpecularMap.Clear();
            EnvironmentPreview.Source = null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Color = SrgbToLinear(ColorToVector3(AmbientColorBtn.Color));
        }
    }
}
