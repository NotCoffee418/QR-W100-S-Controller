using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Wpf;

namespace QRW100S_Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Set libvlc.dll and libvlccore.dll directory path
            VlcContext.LibVlcDllsPath = CommonStrings.LIBVLC_DLLS_PATH_DEFAULT_VALUE_AMD64;
            //Set the vlc plugins directory path
            VlcContext.LibVlcPluginsPath = CommonStrings.PLUGINS_PATH_DEFAULT_VALUE_AMD64;

            //Set the startup options
            VlcContext.StartupOptions.IgnoreConfig = true;
            VlcContext.StartupOptions.LogOptions.LogInFile = true;
            VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = true;
            VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.Debug;

            //Initialize the VlcContext
            VlcContext.Initialize();

            InitializeComponent();
            myVlcControl.VideoProperties.Scale = 2.0f;
            Closing += MainWindowOnClosing;
        }

        private void StartVideo(bool start = true)
        {
            myVlcControl.Stop();

            if (start == false) return;

            if (myVlcControl.Media != null)
            {
                myVlcControl.Media.ParsedChanged -= MediaOnParsedChanged;
            }

            textBlockOpen.Visibility = Visibility.Collapsed;

            myVlcControl.Media = new LocationMedia("http://admin:admin123@192.168.10.1:8080/?action=stream");
            myVlcControl.Media.ParsedChanged += MediaOnParsedChanged;
            myVlcControl.Play();
        }
        private void StopVideo()
        {
            StartVideo(start: false);
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs e)
        {
            // Close the context. 
            VlcContext.CloseAll();
        }

        private void MediaOnParsedChanged(MediaBase sender, VlcEventArgs<int> e)
        {
            textBlock.Text = string.Format(
                "Duration: {0:00}:{1:00}:{2:00}",
                myVlcControl.Media.Duration.Hours,
                myVlcControl.Media.Duration.Minutes,
                myVlcControl.Media.Duration.Seconds);

        }

        private void buttonRtsp_Click(object sender, RoutedEventArgs e)
        {
            if (buttonRtsp.Content == "Stop watching")
            {
                StopVideo();
                buttonRtsp.Content = "Start watching";
            }
            else
            {
                StartVideo();
                buttonRtsp.Content = "Stop watching";
            }
        }

        Controls controls = new Controls();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (deviceButton.Content == "Disable Device")
            {
                controls.StopControls();
                deviceButton.Content = "Enable Device";
            }
            else
            {
                controls.StartControls();
                deviceButton.Content = "Disable Device";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            controls.Execute(Controls.Cmd.Test);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            controls.Height = Convert.ToInt32(slider1.Value);
        }
    }
}
