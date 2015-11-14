using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AdapterLib
{
    internal class AdapterIcon : BridgeRT.IAdapterIcon
    {
        byte[] _image = null;
        public AdapterIcon(string url)
        {
            if (url.StartsWith("ms-appx:///"))
            {
                var s = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(url)).OpenReadAsync().AsTask();
                s.Wait();
                using (MemoryStream ms = new MemoryStream())
                {
                    s.Result.AsStreamForRead().CopyTo(ms);
                    _image = ms.ToArray();
                }
            }
            else
            {
                Url = url;
            }
        }

        public string MimeType { get; } = "image/png";

        public string Url { get; } = "";

        public byte[] GetImage()
        {
            return _image;
        }
    }
    internal class HueBridgeDevice : AdapterDevice
    {
        private readonly Q42.HueApi.HueClient _client;
        private readonly List<HueBulbDevice> _devices = new List<HueBulbDevice>();
        private readonly HueBridgeDescription _description;
        private System.Threading.Timer CheckForLightsTimer;
        private AdapterMethod EnableJoinMethod;

        public HueBridgeDevice(Q42.HueApi.HueClient client, HueBridgeDescription desc) : base("PhilipsHue",
            desc.Manufacturer, desc.ModelName, "", desc.SerialNumber, $"{desc.FriendlyName}\n{desc.ModelDescription} ({desc.ModelNumber})")
        {
            _client = client;
            _description = desc;

            EnableJoinMethod = new AdapterMethod("Link", "Puts the adapter into join mode", 0);
            EnableJoinMethod.InvokeAction = Link;
            Methods.Add(EnableJoinMethod);

            //var UpdateMethod = new AdapterMethod("Update", "Looks for any removed or added lights", 0);
            //UpdateMethod.InvokeAction = UpdateDeviceList;
            //Methods.Add(UpdateMethod);

            //Check if bridge is already linked and registered
            var container = ApplicationData.Current.LocalSettings.CreateContainer("RegisteredHueBridges", ApplicationDataCreateDisposition.Always);
            if(container.Values.ContainsKey(desc.SerialNumber))
            {
                var key = container.Values[desc.SerialNumber] as string;
                if (key != null)
                {
                    (client as Q42.HueApi.LocalHueClient)?.Initialize(key);
                    UpdateDeviceList();
                }
            }
            //if (desc.IconUri != null)
            //    Icon = new AdapterIcon(desc.IconUri.OriginalString);
                Icon = new AdapterIcon("ms-appx:///AdapterLib/Icons/PhilipsHueIcon.png");
        }

        private async void Link()
        {
            try
            {
                var c = _client as Q42.HueApi.LocalHueClient;
                var devicename = Windows.Networking.Proximity.PeerFinder.DisplayName;
                var applicationKey = await c.RegisterAsync("AllJoynDSB", devicename).ConfigureAwait(false);
                if (applicationKey != null)
                {
                    var container = ApplicationData.Current.LocalSettings.CreateContainer("RegisteredHueBridges", ApplicationDataCreateDisposition.Always);
                    container.Values[_description.SerialNumber] = applicationKey;
                    c.Initialize(applicationKey);
                    UpdateDeviceList();
                    EnableJoinMethod.SetResult(0);
                }
                else
                {
                    EnableJoinMethod.SetResult(2);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                EnableJoinMethod.SetResult(1);
            }
        }

        private async void UpdateDeviceList()
        {
            try
            {
                var lights = (await _client.GetLightsAsync()).ToList();
                //Report all lost lights
                foreach (var device in _devices.ToArray())
                {
                    if (!lights.Where(l => l.UniqueId == device.Light.UniqueId).Any())
                    {
                        //Light no longer available
                        _devices.Remove(device);
                        NotifyDeviceRemoval?.Invoke(this, device);
                    }
                }
                var serial = (_client as LocalHueClient2).BridgeSerial;
                //Report all newly found lights
                foreach (var light in lights)
                {
                    if (!_devices.Where(l => l.Light.UniqueId == light.UniqueId).Any())
                    {
                        var device = new HueBulbDevice(_client, light, serial);
                        _devices.Add(device);
                        NotifyDeviceArrival?.Invoke(this, device);
                    }
                }
            }
            catch { }
            if(CheckForLightsTimer == null)
                CheckForLightsTimer = new System.Threading.Timer((s) => UpdateDeviceList(), null,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds,
                    (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
        }

        public event EventHandler<HueBulbDevice> NotifyDeviceArrival;
        public event EventHandler<HueBulbDevice> NotifyDeviceRemoval;
    }
}
