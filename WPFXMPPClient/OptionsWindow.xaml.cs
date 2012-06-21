using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = Option.Options;
            
            ComboBoxCallAnswerMode.Items.Add(AnswerType.Normal);
            ComboBoxCallAnswerMode.Items.Add(AnswerType.DND);
            ComboBoxCallAnswerMode.Items.Add(AnswerType.AcceptToConference);
            ComboBoxCallAnswerMode.Items.Add(AnswerType.AcceptToHold);

            ComboBoxSpeakerDevice.ItemsSource = ImageAquisition.NarrowBandMic.GetSpeakerDevices();
            ComboBoxMicrophoneDevice.ItemsSource = ImageAquisition.NarrowBandMic.GetMicrophoneDevices();



        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Option.Save();
            this.Close();
        }
    }
}
