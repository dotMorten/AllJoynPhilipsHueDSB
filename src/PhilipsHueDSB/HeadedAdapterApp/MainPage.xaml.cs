using AdapterLib;
using BridgeRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeadedAdapterApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            CheckBridgeStatus();
        }

        private async void CheckBridgeStatus()
        {
            status.Text = "Starting up bridge...";
            try
            {
                await ((App)App.Current).startupTask;
                status.Text = "Bridge Successfully Initialized";
            }
            catch(System.Exception ex)
            {
                status.Text = "Bridge failed to initialize:\n" + ex.Message;
            }
        }
    }
}
