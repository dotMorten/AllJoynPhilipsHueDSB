using org.alljoyn.Icon;
using org.allseen.LSF.LampDetails;
using org.allseen.LSF.LampParameters;
using org.allseen.LSF.LampService;
using org.allseen.LSF.LampState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;

namespace SampleApp
{
    public class Light
    {
        public LampDetailsConsumer Details { get; set; }
        public LampStateConsumer State { get; set; }
        public LampParametersConsumer Parameters { get; set; }
        public LampServiceConsumer Service { get; set; }
        public org.alljoyn.Icon.IconConsumer Icon { get; set; }
        public AllJoynServiceInfo Info { get; set; }
        public AllJoynAboutDataView About { get; set; }
        public async Task Initialize(AllJoynBusAttachment bus)
        {
            AllJoynAboutDataView about = await AllJoynAboutDataView.GetDataBySessionPortAsync(Info.UniqueName, bus, Info.SessionPort);
            About = about;
        }
    }
    public class HueBridge
    {
        public com.dotMorten.PhilipsHueDSB.PhilipsHue.PhilipsHueConsumer Hue
        {
            get; set;
        }
        public com.dotMorten.PhilipsHueDSB.PhilipsHue.MainInterface.MainInterfaceConsumer MainInterface
        {
            get; set;
        }
        public AllJoynAboutDataView About { get; set; }
        public AllJoynServiceInfo Info { get; internal set; }
        public async Task Initialize(AllJoynBusAttachment bus)
        {
            AllJoynAboutDataView about = await AllJoynAboutDataView.GetDataBySessionPortAsync(Info.UniqueName, bus, Info.SessionPort);
            About = about;
        }
        public Windows.UI.Xaml.Media.ImageSource Icon { get; set; }
    }
    public class DeviceManager
    {
        object _lightsLock = new object();
        private Dictionary<string, Light> _lights = new Dictionary<string, Light>();
        private Dictionary<string, HueBridge> _bridges = new Dictionary<string, HueBridge>();
        static DeviceManager _Instance;
        public static DeviceManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DeviceManager();
                return _Instance;
            }
        }

        public event EventHandler<Light> LightAdded;
        public event EventHandler<HueBridge> BridgeAdded;

        private DeviceManager()
        {
            StartWatcher();
        }
        AllJoynBusAttachment lsfBusAttachment;
        AllJoynBusAttachment hueBusAttachment;
        private void StartWatcher()
        {
            
            lsfBusAttachment = new AllJoynBusAttachment();
            lsfBusAttachment.AboutData.DefaultAppName = "Hue Client Sample App";
            lsfBusAttachment.AboutData.DefaultDescription = "Sample app for working with AllJoyn Lights and the Hue Bridge DSB";
            lsfBusAttachment.AboutData.AppNames.Add("da-DK", "Hue Klient eksempel app");
            lsfBusAttachment.AboutData.DefaultManufacturer = "Morten Nielsen";
            lsfBusAttachment.AboutData.SoftwareVersion = "1.0";
            lsfBusAttachment.AboutData.SupportUrl = new Uri("https://github.com/dotMorten/AllJoynPhilipsHueDSB");
            
            var stateWatcher = new LampStateWatcher(lsfBusAttachment);
            stateWatcher.Added += LampStateWatcher_Added;
            
            var detailsWatcher = new LampDetailsWatcher(lsfBusAttachment);
            detailsWatcher.Added += LampDetailsWatcher_Added;

            var parametersWatcher = new LampParametersWatcher(lsfBusAttachment);
            parametersWatcher.Added += LampParametersWatcher_Added;

            var serviceWatcher = new LampServiceWatcher(lsfBusAttachment);
            serviceWatcher.Added += LampServiceWatcher_Added;

            IconWatcher iconWatcher = new IconWatcher(lsfBusAttachment);
            iconWatcher.Added += IconWatcher_Added;

            //hueBusAttachment = lsfBusAttachment; // new AllJoynBusAttachment();
            //var hue1Watcher = new com.dotMorten.PhilipsHueDSB.PhilipsHue.PhilipsHueWatcher(hueBusAttachment);
            //hue1Watcher.Added += Hue1Watcher_Added;
            //var hue2Watcher = new com.dotMorten.PhilipsHueDSB.PhilipsHue.MainInterface.MainInterfaceWatcher(hueBusAttachment);
            //hue2Watcher.Added += Hue2Watcher_Added;

            lsfBusAttachment.Connect();
            stateWatcher.Start();
            detailsWatcher.Start();
            parametersWatcher.Start();
            serviceWatcher.Start();
            iconWatcher.Start();
            //hue1Watcher.Start();
            //hue2Watcher.Start();
            // 

            producer = new org.alljoyn.Notification.NotificationProducer(lsfBusAttachment);
            producer.Start();
            producer.SessionMemberAdded += Producer_SessionMemberAdded;


            var signals = producer.Signals;
        }

        private void Producer_SessionMemberAdded(org.alljoyn.Notification.NotificationProducer sender, AllJoynSessionMemberAddedEventArgs args)
        {
            
        }

        org.alljoyn.Notification.NotificationProducer producer;
        private async void Hue1Watcher_Added(com.dotMorten.PhilipsHueDSB.PhilipsHue.PhilipsHueWatcher sender, AllJoynServiceInfo args)
        {
            var result = await com.dotMorten.PhilipsHueDSB.PhilipsHue.PhilipsHueConsumer.JoinSessionAsync(args, sender);
            Debug.WriteLine($"Hue link : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                 AddBridge(args);
                _bridges[args.UniqueName].Hue = result.Consumer;
                await CheckBridge(_bridges[args.UniqueName]);
            }
            else
            {
                Debug.WriteLine($"Hue link Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }

        private async void Hue2Watcher_Added(com.dotMorten.PhilipsHueDSB.PhilipsHue.MainInterface.MainInterfaceWatcher sender, AllJoynServiceInfo args)
        {
            var result = await com.dotMorten.PhilipsHueDSB.PhilipsHue.MainInterface.MainInterfaceConsumer.JoinSessionAsync(args, sender);
            Debug.WriteLine($"Hue MainInterface : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                AddBridge(args);
                _bridges[args.UniqueName].MainInterface = result.Consumer;
                await CheckBridge(_bridges[args.UniqueName]);
            }
            else
            {
                Debug.WriteLine($"Hue MainInterface Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }

        private string FromAllJoynStatus(int id)
        {
            if(id == AllJoynStatus.AuthenticationFailed)
                return "Authentication Failed";
            if (id == AllJoynStatus.AuthenticationRejectedByUser)
                return "AuthenticationRejectedByUser";
            if (id == AllJoynStatus.ConnectionRefused)
                return "ConnectionRefused";
            if (id == AllJoynStatus.Fail)
                return "Fail";
            if (id == AllJoynStatus.InsufficientSecurity)
                return "InsufficientSecurity";
            if (id == AllJoynStatus.InvalidArgument1)
                return "InvalidArgument1";
            if (id == AllJoynStatus.InvalidArgument2)
                return "InvalidArgument2";
            if (id == AllJoynStatus.InvalidArgument3)
                return "InvalidArgument3";
            if (id == AllJoynStatus.InvalidArgument4)
                return "InvalidArgument4";
            if (id == AllJoynStatus.InvalidArgument5)
                return "InvalidArgument5";
            if (id == AllJoynStatus.InvalidArgument6)
                return "InvalidArgument6";
            if (id == AllJoynStatus.InvalidArgument7)
                return "InvalidArgument7";
            if (id == AllJoynStatus.InvalidArgument8)
                return "InvalidArgument8";
            if (id == AllJoynStatus.Ok)
                return "OK";
            if (id == AllJoynStatus.OperationTimedOut)
                return "OperationTimedOut";
            if (id == AllJoynStatus.OtherEndClosed)
                return "OtherEndClosed";
            if (id == AllJoynStatus.SslConnectFailed)
                return "SslConnectFailed";
            if (id == AllJoynStatus.SslIdentityVerificationFailed)
                return "SslIdentityVerificationFailed";
            if (id == AllJoynStatus.SslConnectFailed)
                return "SslConnectFailed";
            if (id == AllJoynStatus.SslConnectFailed)
                return "SslConnectFailed";
            return "Unknown AllJoyn Status";
        }

        private async void IconWatcher_Added(IconWatcher sender, AllJoynServiceInfo args)
        {
            var result = await IconConsumer.JoinSessionAsync(args, sender);
            Debug.WriteLine($"Icon : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                AddLight(args);
                _lights[args.UniqueName].Icon = result.Consumer;
            }
            else
            {
                Debug.WriteLine($"Icon Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }

        private async void LampDetailsWatcher_Added(LampDetailsWatcher sender, AllJoynServiceInfo args)
        {
            var result = await LampDetailsConsumer.JoinSessionAsync(args, sender);
            Debug.WriteLine($"LampDetails : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                AddLight(args);
                _lights[args.UniqueName].Details = result.Consumer;
                await CheckLight(_lights[args.UniqueName]);
            }
            else
            {
                Debug.WriteLine($"LampDetails Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }

        private async void LampParametersWatcher_Added(LampParametersWatcher sender, AllJoynServiceInfo args)
        {
            var result = await LampParametersConsumer.JoinSessionAsync(args, sender);
            Debug.WriteLine($"LampParameters : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                AddLight(args);
                _lights[args.UniqueName].Parameters = result.Consumer;
                await CheckLight(_lights[args.UniqueName]);
            }
            else
            {
                Debug.WriteLine($"LampParameters Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }

        private async void LampStateWatcher_Added(LampStateWatcher sender, AllJoynServiceInfo args)
        {
            var result = await LampStateConsumer.JoinSessionAsync(args, sender);

            Debug.WriteLine($"LampState : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                AddLight(args);
                _lights[args.UniqueName].State = result.Consumer;
                await CheckLight(_lights[args.UniqueName]);
            }
            else
            {
                Debug.WriteLine($"LampState Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }
        private async void LampServiceWatcher_Added(LampServiceWatcher sender, AllJoynServiceInfo args)
        {
            var result = await LampServiceConsumer.JoinSessionAsync(args, sender);
            Debug.WriteLine($"LampService : {args.UniqueName}");
            if (result.Status == AllJoynStatus.Ok)
            {
                AddLight(args);
                _lights[args.UniqueName].Service = result.Consumer;
                await CheckLight(_lights[args.UniqueName]);
            }
            else
            {
                Debug.WriteLine($"LampService Join Failed: {args.UniqueName} {FromAllJoynStatus(result.Status)}");
            }
        }

        private async Task CheckLight(Light light)
        {
            if(light.Details != null && light.Parameters != null &&
                light.Service != null && light.State != null)
            {
                await light.Initialize(lsfBusAttachment);
                LightAdded?.Invoke(this, light);
            }
        }
        private void AddLight(AllJoynServiceInfo info)
        {
            Light light = null;
            lock (_lightsLock)
            {
                if (!_lights.ContainsKey(info.UniqueName))
                {
                    light = new Light() { Info = info };
                    _lights.Add(info.UniqueName, light);
                }
            }
        }
        private void AddBridge(AllJoynServiceInfo info)
        {
            HueBridge bridge = null;
            lock (_lightsLock)
            {
                if (!_bridges.ContainsKey(info.UniqueName))
                {
                    bridge = new HueBridge() { Info = info };
                    _bridges.Add(info.UniqueName, bridge);
                }
            }
        }
        private async Task CheckBridge(HueBridge bridge)
        {
            if (bridge.Hue != null && bridge.MainInterface!= null)
            {
                await bridge.Initialize(lsfBusAttachment);
                BridgeAdded?.Invoke(this, bridge);
            }
        }
    }
}
