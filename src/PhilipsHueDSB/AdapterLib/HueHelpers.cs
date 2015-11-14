/*  
* AllJoyn Device Service Bridge for Philips Hue
*  
* Copyright (c) Morten Nielsen
* All rights reserved.  
*  
* MIT License  
*  
* Permission is hereby granted, free of charge, to any person obtaining a copy of this  
* software and associated documentation files (the "Software"), to deal in the Software  
* without restriction, including without limitation the rights to use, copy, modify, merge,  
* publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons  
* to whom the Software is furnished to do so, subject to the following conditions:  
*  
* The above copyright notice and this permission notice shall be included in all copies or  
* substantial portions of the Software.  
*  
* THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,  
* INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR  
* PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE  
* FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR  
* OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  
* DEALINGS IN THE SOFTWARE.  
*/

using Q42.HueApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace AdapterLib
{
    //Hue Client that adds the serial number for the bridge that the bulb belongs to
    internal class LocalHueClient2 : Q42.HueApi.LocalHueClient
    {
        public LocalHueClient2(string ip, string bridgeSerial) : base(ip)
        {
            BridgeSerial = bridgeSerial;
        }
        public LocalHueClient2(string ip, string appKey, string bridgeSerial) : base(ip, appKey)
        {
            BridgeSerial = bridgeSerial;
        }
        public string BridgeSerial { get; }
    }
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
    }
}
