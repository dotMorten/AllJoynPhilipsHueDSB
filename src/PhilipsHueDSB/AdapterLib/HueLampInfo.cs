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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{
    internal class HueLampInfo
    {
        Q42.HueApi.Light _light;
        public HueLampInfo(Q42.HueApi.Light light)
        {
            _light = light;
        }

        public LsfEnums.LampType LampType { get { return GetLampType(_light); } }
        
        public bool IsDimmable
        {
            get
            {
                return true; //All hue bulbs are dimmable
            }
        }

        public bool SupportsColor
        {
            get
            {
                return (_light.Type == "Extended color light" ||
                        _light.Type == "Color temperature light" ||
                        _light.Type == "Color light");
            }
        }

        public bool SupportsTemperature
        {
            get
            {
                return (_light.Type == "Extended color light" ||
                        _light.Type == "Color temperature light");
            }
        }
        public uint ColorRenderingIndex
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                    case "LCT002": // Hue Spot BR30
                    case "LCT003": // Hue Spot GU10
                    case "LWB004": // Hue A19 Lux
                        return 80;
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "Go":
                    case "LLM001": // Color Light Module
                    default:
                        return 80; //Default fallback
                }
            }
        }

        public uint MinTemperature
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                    case "LCT002": // Hue Spot BR30
                    case "LCT003": // Hue Spot GU10
                        return 2000;
                    case "LWB004": // Hue A19 Lux
                        return 2700;
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "Go":
                    case "LLM001": // Color Light Module
                    default:
                        return 2000; //Default fallback
                }
            }
        }
        public uint MaxTemperature
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                    case "LCT002": // Hue Spot BR30
                    case "LCT003": // Hue Spot GU10
                        return 6500;
                    case "LWB004": // Hue A19 Lux
                        return 2700;
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "Go":
                    case "LLM001": // Color Light Module
                    default:
                        return 6500; //Default fallback
                }
            }
        }
        public uint Wattage
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                    case "LWB004": // Hue A19 Lux
                        return 9;
                    case "LCT002": // Hue Spot BR30
                        return 8;
                    case "LCT003": // Hue Spot GU10
                        return 7; //really 7, but for whatever reason wattage is uint
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "Go":
                    case "LLM001": // Color Light Module
                    default:
                        return 9; //Default fallback
                }
            }
        }

        public uint LampBeamAngle
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                        return 160;
                    case "LCT002": // Hue Spot BR30
                        return 160;
                    case "LCT003": // Hue Spot GU10
                        return 38;
                    case "LWB004": // Hue A19 Lux
                        return 150;
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "Go":
                    case "LLM001": // Color Light Module
                    default:
                        return 160; //Default fallback
                }
            }
        }
        public uint MaxLumens
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                        return 600;
                    case "LCT002": // Hue Spot BR30
                        return 630;
                    case "LCT003": // Hue Spot GU10
                        return 250;
                    case "LWB004": // Hue A19 Lux
                        return 750;
                    case "Go":
                        return 300;
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "LLM001": // Color Light Module
                    default:
                        return 600; //Default fallback
                }
            }
        }
        

        public uint IncandescentEquivalent
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                        return 60;
                    case "LCT002": // Hue Spot BR30
                        return 70;
                    case "LCT003": // Hue Spot GU10
                        return 38;
                    case "Go":
                        return 20;
                    case "LWB004": // Hue A19 Lux
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "LLM001": // Color Light Module
                    default:
                        return 60; //Default fallback
                }
            }
        }

        public LsfEnums.BaseType BaseType
        {
            get
            {
                switch (_light.ModelId)
                {
                    case "LCT001": // Hue bulb A19
                    case "LWB004": // Hue A19 Lux
                        return LsfEnums.BaseType.BASETYPE_E27;
                    case "LCT002": // Hue Spot BR30
                        return LsfEnums.BaseType.BASETYPE_E26;
                    case "LCT003": // Hue Spot GU10
                    case "LLC006": // Living Colors Gen3 Iris
                    case "LLC010": // Hue Living Colors Iris
                    case "LLC013": // Disney Living Colors
                    case "LLC007": // Living Colors Gen3 Bloom, Aura
                    case "LLC011": // Hue Living Colors Bloom'
                    case "LLC012": // Hue Living Colors Bloom
                    case "LST001": // Hue LightStrips
                    case "Beyond":
                    case "Go":
                    case "LLM001": // Color Light Module
                    default:
                        return LsfEnums.BaseType.BASETYPE_E27; //Default fallback
                }
            }
        }
        public string IconUri
        {
            get
            {
                switch (_light.ModelId)
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
                switch (_light.Type)
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

        private static LsfEnums.LampType GetLampType(Q42.HueApi.Light light)
        {
            switch (light.ModelId)
            {
                case "LCT001": // Hue bulb A19
                case "LWB004": // Hue A19 Lux
                    return LsfEnums.LampType.LAMPTYPE_A19;
                case "LCT002": // Hue Spot BR30
                    return LsfEnums.LampType.LAMPTYPE_BR30;
                case "LCT003": // Hue Spot GU10
                case "LLC006": // Living Colors Gen3 Iris
                case "LLC010": // Hue Living Colors Iris
                case "LLC013": // Disney Living Colors
                case "LLC007": // Living Colors Gen3 Bloom, Aura
                case "LLC011": // Hue Living Colors Bloom'
                case "LLC012": // Hue Living Colors Bloom
                case "LST001": // Hue LightStrips
                case "Beyond":
                case "Go":
                case "LLM001": // Color Light Module
                default:
                    return LsfEnums.LampType.LAMPTYPE_A19; //Default fallback
            }
        }
    }
}
