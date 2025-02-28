using CGA2.ToneMapping;
using System.Windows;
using System.Windows.Controls;
using static CGA2.Settings;

namespace CGA2.UI
{
    /// <summary>
    /// Логика взаимодействия для ImageSettings.xaml
    /// </summary>
    public partial class ImageSettings : Window
    {
        public ImageSettings()
        {
            InitializeComponent();

            ACESComboItem.Tag = new AcesFilmicToneMapper();
            AgXComboItem.Tag = new AgXToneMapper();
            LinearComboItem.Tag = new LinearToneMapper();
            ReinhardComboItem.Tag = new ReinhardToneMapper();
            PBRNeutralComboItem.Tag = new PBRNeutralToneMapper();

            if (Settings.ToneMapper is AcesFilmicToneMapper)
                ACESComboItem.IsSelected = true;

            if (Settings.ToneMapper is AgXToneMapper)
                AgXComboItem.IsSelected = true;

            if (Settings.ToneMapper is LinearToneMapper)
                LinearComboItem.IsSelected = true;

            if (Settings.ToneMapper is ReinhardToneMapper)
                ReinhardComboItem.IsSelected = true;

            if (Settings.ToneMapper is PBRNeutralToneMapper)
                PBRNeutralComboItem.IsSelected = true;

            switch (Scaling)
            {
                case 0.25f:
                    Radio025.IsChecked = true;
                    break;

                case 0.5f:
                    Radio05.IsChecked = true;
                    break;

                case 1f:
                    Radio1.IsChecked = true;
                    break;

                case 2f:
                    Radio2.IsChecked = true;
                    break;

                case 4f:
                    Radio4.IsChecked = true;
                    break;
            }
        }

        private void RadioButton_025_Checked(object sender, RoutedEventArgs e)
        {
            Scaling = 0.25f;
        }

        private void RadioButton_05_Checked(object sender, RoutedEventArgs e)
        {
            Scaling = 0.5f;
        }

        private void RadioButton_1_Checked(object sender, RoutedEventArgs e)
        {
            Scaling = 1f;
        }

        private void RadioButton_2_Checked(object sender, RoutedEventArgs e)
        {
            Scaling = 2f;
        }

        private void RadioButton_4_Checked(object sender, RoutedEventArgs e)
        {
            Scaling = 4f;
        }

        private void TonemapperComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Settings.ToneMapper = ((TonemapperComboBox.SelectedItem! as ComboBoxItem)!.Tag as ToneMapper)!;
        }
    }
}
