﻿using CGA2.Shaders;
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

            PBRComboItem.Tag = new PBRShader();
            PhongComboItem.Tag = new PhongShader();

            if (Settings.Shader is PBRShader)
                PBRComboItem.IsSelected = true;

            if (Settings.Shader is PhongShader)
                PhongComboItem.IsSelected = true;

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

            switch (MaxAnisotropy)
            {
                case 1:
                    RadioAniso1.IsChecked = true;
                    break;

                case 2:
                    RadioAniso2.IsChecked = true;
                    break;

                case 4:
                    RadioAniso4.IsChecked = true;
                    break;

                case 8:
                    RadioAniso8.IsChecked = true;
                    break;

                case 16:
                    RadioAniso16.IsChecked = true;
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

        private void RadioButton_Aniso_1_Checked(object sender, RoutedEventArgs e)
        {
            MaxAnisotropy = 1;
        }

        private void RadioButton_Aniso_2_Checked(object sender, RoutedEventArgs e)
        {
            MaxAnisotropy = 2;
        }

        private void RadioButton_Aniso_4_Checked(object sender, RoutedEventArgs e)
        {
            MaxAnisotropy = 4;
        }

        private void RadioButton_Aniso_8_Checked(object sender, RoutedEventArgs e)
        {
            MaxAnisotropy = 8;
        }

        private void RadioButton_Aniso_16_Checked(object sender, RoutedEventArgs e)
        {
            MaxAnisotropy = 16;
        }

        private void TonemapperComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Settings.ToneMapper = ((TonemapperComboBox.SelectedItem! as ComboBoxItem)!.Tag as ToneMapper)!;
        }

        private void ShaderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Shader = ((ShaderComboBox.SelectedItem! as ComboBoxItem)!.Tag as Shader)!;
        }
    }
}
