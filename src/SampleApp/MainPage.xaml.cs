using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleApp
{
    public class LightItem
    {
        public ImageSource Source { get; set; }
        public string Name { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<LightItem> coll = new ObservableCollection<LightItem>();
        ObservableCollection<HueBridge> coll2 = new ObservableCollection<HueBridge>();
        public MainPage()
        {
            this.InitializeComponent();
            lightsList.ItemsSource = coll;
            bridgeList.ItemsSource = coll2;
            var i = DeviceManager.Instance;
            i.LightAdded += I_LightAdded;
            i.BridgeAdded += I_BridgeAdded;
        }

        private async void I_BridgeAdded(object sender, HueBridge e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                coll2.Add(e);
            });
        }

        private async void I_LightAdded(object sender, Light e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
               // e.LampDetails.
                var state = e.State;
                LightItem item = new LightItem();
                item.Name = e.About.DeviceName; // $"Light {coll.Count + 1}";
                if(e.Icon != null)
                {
                    var url = await e.Icon.GetUrlAsync();
                    if (!string.IsNullOrEmpty(url.OutputArg))
                    {
                        item.Source = new BitmapImage(new Uri(url.OutputArg));
                    }
                    else
                    {
                        var iconContent = await e.Icon.GetContentAsync();
                        if (iconContent.OutputArg != null && iconContent.OutputArg.Any())
                        {
                            BitmapImage bmi = new BitmapImage();
                            using (MemoryStream ms = new MemoryStream(iconContent.OutputArg.ToArray()))
                            {
                                bmi.SetSource(ms.AsRandomAccessStream());
                                item.Source = bmi;
                            }                            
                        }
                    }

                }
                coll.Add(item);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //org.alljoyn.Notification.NotificationProducer producer = new org.alljoyn.Notification.NotificationProducer();
        }
    }
}
