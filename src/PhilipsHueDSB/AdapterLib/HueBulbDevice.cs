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

namespace AdapterLib
{
    internal class HueBulbDevice : AdapterDevice
    {
        public HueBulbDevice(Q42.HueApi.HueClient client, Q42.HueApi.Light light, string bridgeSerialNumber) : base(light.Name,
            light.ManufacturerName, light.ModelId, light.SoftwareVersion, light.UniqueId, light.Type)
        {
            Light = light;
            BridgeSerialNumber = bridgeSerialNumber;
            base.LightingServiceHandler = new HueLightingServiceHandler(client, light);
            Icon = new AdapterIcon(GetIconUri(light));
        }

        public Q42.HueApi.Light Light { get; }

        public string BridgeSerialNumber { get; }

        private static string GetIconUri(Q42.HueApi.Light light)
        {
            switch (light.ModelId)
            {
                case "LCT001": // Hue bulb A19
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueBulb_Color.png";
                case "LCT002": // Hue Spot BR30
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueSpot_BR30.png";
                case "LCT003": // Hue Spot GU10
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueSpot_GU10.png";
                case "LWB004": // Hue A19 Lux
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueBulb.png";
                case "LLC006": // Living Colors Gen3 Iris
                case "LLC010": // Hue Living Colors Iris
                case "LLC013": // Disney Living Colors
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueIris.png";
                case "LLC007": // Living Colors Gen3 Bloom, Aura
                case "LLC011": // Hue Living Colors Bloom'
                case "LLC012": // Hue Living Colors Bloom
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueBloom.png";
                case "LST001": // Hue LightStrips
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueLightstrip.png";
                case "Beyond":
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueBeyond.png";
                case "Go":
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueGo.png";

                case "LLM001": // Color Light Module
                default:
                    break;
            }
            
            switch (light.Type)
            {
                case "Extended color light":
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueBulb_Color.png";
                case "Color temperature light":
                case "Color light":
                case "Dimmable light":
                default:
                    return "ms-appx:///AdapterLib/Icons/PhilipsHueBulb.png";
            }
        }
    }
}
