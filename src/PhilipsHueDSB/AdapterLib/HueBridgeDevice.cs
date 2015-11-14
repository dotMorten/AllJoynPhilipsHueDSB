using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BridgeRT;

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
        private object _devicesLock = new object();
        private readonly List<HueBulbDevice> _devices = new List<HueBulbDevice>();
        private readonly HueBridgeDescription _description;
        private System.Threading.Timer CheckForLightsTimer;
        private AdapterMethod _enableJoinMethod;
        private AdapterProperty _isLinkedProperty;
        private Adapter _bridge;
        
        public HueBridgeDevice(Q42.HueApi.HueClient client, HueBridgeDescription desc, Adapter bridge) : base("PhilipsHue",
            desc.Manufacturer, desc.ModelName, "", desc.SerialNumber, $"{desc.FriendlyName}\n{desc.ModelDescription} ({desc.ModelNumber})")
        {
            _client = client;
            _description = desc;
            _bridge = bridge;

            _enableJoinMethod = new AdapterMethod("Link", "Puts the adapter into join mode", 0);
            _enableJoinMethod.InvokeAction = Link;
            _enableJoinMethod.OutputParams.Add(new AdapterValue("Result", "") { Data = "" });
            Methods.Add(_enableJoinMethod);

           
            //var UpdateMethod = new AdapterMethod("Update", "Looks for any removed or added lights", 0);
            //UpdateMethod.InvokeAction = UpdateDeviceList;
            //Methods.Add(UpdateMethod);

            //Check if bridge is already linked and registered
            bool isLinked = false;
            var container = ApplicationData.Current.LocalSettings.CreateContainer("RegisteredHueBridges", ApplicationDataCreateDisposition.Always);
            if(container.Values.ContainsKey(desc.SerialNumber))
            {
                var key = container.Values[desc.SerialNumber] as string;
                if (key != null)
                {
                    (client as Q42.HueApi.LocalHueClient)?.Initialize(key);
                    isLinked = true;
                    UpdateDeviceList();
                }
            }

            _isLinkedProperty = new AdapterProperty("Link", "com.github.dotMorten.PhilipsHueDSB.PhilipsHue");
            _isLinkedProperty.Attributes.Add(new AdapterAttribute("IsLinked", isLinked, E_ACCESS_TYPE.ACCESS_READ) { COVBehavior = SignalBehavior.Always });
            this.Properties.Add(_isLinkedProperty);


            //if (desc.IconUri != null)
            //    Icon = new AdapterIcon(desc.IconUri.OriginalString);
            Icon = new AdapterIcon("ms-appx:///AdapterLib/Icons/PhilipsHueIcon.png");

            // change of value signal
            CreateSignals();
        }

        private void CreateSignals()
        {
            // change of value signal
            AdapterSignal changeOfAttributeValue = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);
            changeOfAttributeValue.AddParam(Constants.COV__PROPERTY_HANDLE);
            changeOfAttributeValue.AddParam(Constants.COV__ATTRIBUTE_HANDLE);
            Signals.Add(changeOfAttributeValue);
        }

        private void Link()
        {
            try
            {
                var c = _client as Q42.HueApi.LocalHueClient;
                var devicename = Windows.Networking.Proximity.PeerFinder.DisplayName;
                var registerTask = c.RegisterAsync("AllJoynDSB", devicename);
                registerTask.Wait();
                var applicationKey = registerTask.Result;
                if (applicationKey != null)
                {
                    var container = ApplicationData.Current.LocalSettings.CreateContainer("RegisteredHueBridges", ApplicationDataCreateDisposition.Always);
                    container.Values[_description.SerialNumber] = applicationKey;
                    c.Initialize(applicationKey);
                    UpdateDeviceList();
                    _isLinkedProperty.Attributes[0].Value.Data = true;
                    _bridge.SignalChangeOfAttributeValue(this, _isLinkedProperty, _isLinkedProperty.Attributes[0]);
                    _enableJoinMethod.OutputParams[0].Data = "Bridge registered successfully";
                    //EnableJoinMethod.SetResult(0);
                }
                else
                {
                    _enableJoinMethod.OutputParams[0].Data = "No key returned";
                    //EnableJoinMethod.SetResult(2);
                }
            }
            catch (System.Exception ex)
            {
                string message;
                if (ex is AggregateException)
                    message = string.Join("\n", ((AggregateException)ex).InnerExceptions.Select(ie => ie.Message).ToArray());
                else
                    message = ex.Message;
                System.Diagnostics.Debug.WriteLine(message);
                _enableJoinMethod.OutputParams[0].Data = message; 
                _enableJoinMethod.SetResult(1);
            }
        }

        private async void UpdateDeviceList()
        {
            try
            {
                var lights = (await _client.GetLightsAsync()).ToList();
                //Report all lost lights
                HueBulbDevice[] devices;
                lock(_devicesLock)
                    devices = _devices.ToArray();
                foreach (var device in devices)
                {
                    if (!lights.Where(l => l.UniqueId == device.Light.UniqueId).Any())
                    {
                        //Light no longer available
                        lock(_devicesLock)
                            _devices.Remove(device);
                        NotifyDeviceRemoval?.Invoke(this, device);
                    }
                }
                var serial = (_client as LocalHueClient2).BridgeSerial;
                //Report all newly found lights
                lock (_devicesLock)
                    devices = _devices.ToArray();
                foreach (var light in lights)
                {
                    if (!devices.Where(l => l.Light.UniqueId == light.UniqueId).Any())
                    {
                        var device = new HueBulbDevice(_client, light, serial);
                        lock (_devicesLock)
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

        public void Run(string runArgument)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<HueBulbDevice> NotifyDeviceArrival;
        public event EventHandler<HueBulbDevice> NotifyDeviceRemoval;
    }
}
