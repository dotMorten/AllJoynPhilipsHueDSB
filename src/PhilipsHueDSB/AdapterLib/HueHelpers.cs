using Q42.HueApi;
using Q42.HueApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Data.Xml.Dom;

namespace AdapterLib
{
    internal class HueBridgeDescription
    {
        XmlDocument _description;
        object xmlns;
        internal HueBridgeDescription(XmlDocument description, string ip)
        {
            Ip = ip;
            _description = description;

            //_description.SelectSingleNodeNS("x:root/x:device", "xmlns:x='" + (string)xmlns + "'")
            xmlns = $"xmlns:x='{description.DocumentElement.NamespaceUri}'";
        }
        public string Ip { get; private set; }
        public string FriendlyName
        {
            get { return _description.SelectSingleNodeNS("x:root/x:device/x:friendlyName", xmlns)?.InnerText; }
        }
        public string SerialNumber
        {
            get { return _description.SelectSingleNodeNS("x:root/x:device/x:serialNumber", xmlns)?.InnerText; }
        }
        public string ModelNumber
        {
            get { return _description.SelectSingleNodeNS("x:root/x:device/x:modelNumber", xmlns)?.InnerText; }
        }
        public string ModelName
        {
            get { return _description.SelectSingleNodeNS("x:root/x:device/x:modelName", xmlns)?.InnerText; }
        }
        public string ModelDescription
        {
            get { return _description.SelectSingleNodeNS("x:root/x:device/x:modelDescription", xmlns)?.InnerText; }
        }
        public string Manufacturer
        {
            get { return _description.SelectSingleNodeNS("x:root/x:device/x:manufacturer", xmlns)?.InnerText; }
        }
        public Uri IconUri
        {
            get
            {
                string uri = _description.SelectNodesNS("x:root/x:device/x:iconList/x:icon/x:url", xmlns).LastOrDefault()?.InnerText;
                if (uri != null)
                    return new Uri($"http://{Ip}/{uri}");
                return null;
            }
        }
    }
    class HueHelpers
    {
        //const string 
        public static async Task<IEnumerable<HueBridgeDescription>> FindHueBridges()
        {
            IBridgeLocator locator = new Q42.HueApi.WinRT.SSDPBridgeLocator();
            IEnumerable<string> bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(10));
            List<HueBridgeDescription> clients = new List<HueBridgeDescription>();
            foreach (var ip in bridgeIPs)
            {
                XmlDocument descriptionDoc = await XmlDocument.LoadFromUriAsync(new Uri($"http://{ip}/description.xml"));
                clients.Add(new HueBridgeDescription(descriptionDoc, ip));
            }
            return clients;
        }

        private HueClient CreateClient(string ip, string applicationKey)
        {
            LocalHueClient client = new LocalHueClient(ip);
            client.Initialize(applicationKey);
            return client;
        }
        private async Task<HueClient> RegisterClientAsync(string ip)
        {
            LocalHueClient client = new LocalHueClient(ip);
            var applicationKey = await client.RegisterAsync(GetApplicationName(), "minwinpc").ConfigureAwait(false);
            client.Initialize(applicationKey);
            return client;
        }

        private static string GetApplicationName()
        {
            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId packageId = package.Id;
            return package.Id.Name;
        }
    }
}
