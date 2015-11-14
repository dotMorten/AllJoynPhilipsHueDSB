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

using BridgeRT;
using Q42.HueApi;
using System;

// This class implements the AllJoyn Lighting Service Framework for the Hue Bulbs

namespace AdapterLib
{
    internal class HueLightingServiceHandler : ILSFHandler
    {
        // Defines standard ranges for Lamp light state.
        private const double OEM_LS_HUE_MIN = 0;
        private const double OEM_LS_HUE_MAX = 360;
        private const double OEM_LS_BRIGHTNESS_MIN = 0;
        private const double OEM_LS_BRIGHTNESS_MAX = 1.0;
        private const double OEM_LS_SATURATION_MIN = 0;
        private const double OEM_LS_SATURATION_MAX = 1.0;

        private readonly Q42.HueApi.Light _light;
        private readonly Q42.HueApi.HueClient _client;

        public HueLightingServiceHandler(Q42.HueApi.HueClient client, Q42.HueApi.Light light)
        {
            //Doc on supported lights:
            // http://www.developers.meethue.com/documentation/supported-lights
            _light = light;
            var info = new HueLampInfo(light);
            var state = light.State;
            _client = client;
            
            LampDetails_Color = info.SupportsColor;
            LampDetails_ColorRenderingIndex = info.ColorRenderingIndex;
            LampDetails_Dimmable = info.IsDimmable;
            LampDetails_HasEffects = false;
            LampDetails_IncandescentEquivalent = info.IncandescentEquivalent;
            LampDetails_LampBaseType = (uint)info.BaseType;
            LampDetails_LampBeamAngle = info.LampBeamAngle;
            LampDetails_LampID = light.Id;
            LampDetails_LampType = (uint) info.LampType;
            LampDetails_Make = (uint)AdapterLib.LsfEnums.LampMake.MAKE_OEM1;
            LampDetails_MaxLumens = info.MaxLumens;
            LampDetails_MaxTemperature = info.MaxTemperature;
            LampDetails_MaxVoltage = 120;
            LampDetails_MinTemperature = info.MinTemperature;
            LampDetails_MinVoltage = 100;
            LampDetails_Model = 1;
            LampDetails_Type = (uint)AdapterLib.LsfEnums.DeviceType.TYPE_LAMP;
            LampDetails_VariableColorTemp = info.SupportsTemperature;
            LampDetails_Version = 1;
            LampDetails_Wattage = info.Wattage;
        }
        public bool LampDetails_Color
        {
            get; private set;
        }

        public uint LampDetails_ColorRenderingIndex { get; private set; }

        public bool LampDetails_Dimmable { get; private set; }

        public bool LampDetails_HasEffects { get; private set; }

        public uint LampDetails_IncandescentEquivalent { get; private set; }

        public uint LampDetails_LampBaseType { get; private set; }

        public uint LampDetails_LampBeamAngle { get; private set; }

        public string LampDetails_LampID { get; private set; }

        public uint LampDetails_LampType { get; private set; }

        public uint LampDetails_Make { get; private set; }

        public uint LampDetails_MaxLumens { get; private set; }

        public uint LampDetails_MaxTemperature { get; private set; }

        public uint LampDetails_MaxVoltage { get; private set; }

        public uint LampDetails_MinTemperature { get; private set; }

        public uint LampDetails_MinVoltage { get; private set; }

        public uint LampDetails_Model { get; private set; }

        public uint LampDetails_Type { get; private set; }

        public bool LampDetails_VariableColorTemp { get; private set; }

        public uint LampDetails_Version { get; private set; }

        public uint LampDetails_Wattage { get; private set; }

        public uint LampParameters_BrightnessLumens { get; private set; }

        public uint LampParameters_EnergyUsageMilliwatts { get; private set; }

        public uint LampParameters_Version { get; private set; }

        public uint[] LampService_LampFaults { get; private set; }

        public uint LampService_LampServiceVersion { get; private set; }

        public uint LampService_Version { get; private set; }

        public uint LampState_Brightness
        {
            //1..254
            get { return (uint)(_light.State.Brightness / 254d * (UInt32.MaxValue - 1)); }
            set
            {
                var command = new LightCommand();
                command.Brightness = (byte)(value * 254d / (UInt32.MaxValue - 1));
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.Brightness = (byte)value;
            }
        }

        public uint LampState_ColorTemp
        {
            get
            {
                return _light.State.ColorTemperature.HasValue ? (uint)_light.State.ColorTemperature.Value : 0;
            }

            set
            {
                var command = new LightCommand();
                command.ColorTemperature = (int)value;
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.ColorTemperature = (int)value;
            }
        }

        public uint LampState_Hue
        {
            //0 .. 65535.
            get
            {
                return _light.State.Hue.HasValue ? (uint)(_light.State.Hue.Value / 65535d * 360) : 0;
            }

            set
            {
                var command = new LightCommand();
                command.Hue = (int)(value / 360d * 65535);
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.Hue = (int)value;
            }
        }

        private AdapterSignal _LampStateChanged = new AdapterSignal(Constants.LAMP_STATE_CHANGED_SIGNAL_NAME);

        public IAdapterSignal LampState_LampStateChanged
        {
            get
            {
                return _LampStateChanged;
            }
        }

        public bool LampState_OnOff
        {
            get
            {
                return _light.State.On;
            }

            set
            {
                var command = new LightCommand();
                command.On = value;
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.On = value;
            }
        }

        public uint LampState_Saturation
        {
            //0=white, 254=fully saturated
            get
            {
                return _light.State.Saturation.HasValue ? (uint)
                    (_light.State.Saturation.Value / 254d * (UInt32.MaxValue - 1))
                    : 0;
            }

            set
            {
                var command = new LightCommand();
                command.Saturation = (int)(value * 254d / (UInt32.MaxValue - 1));
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.Saturation = (int)value;
            }
        }

        public uint LampState_Version
        {
            get; private set;
        }

        public uint ClearLampFault(uint InLampFaultCode, out uint LampResponseCode, out uint OutLampFaultCode)
        {
            InLampFaultCode = 0;
            LampResponseCode = 0;
            OutLampFaultCode = 0;
            return 0; //TODO
        }

        public uint LampState_ApplyPulseEffect(BridgeRT.State FromState, BridgeRT.State ToState, uint Period, uint Duration, uint NumPulses, ulong Timestamp, out uint LampResponseCode)
        {
            LampResponseCode = 0;
            return 0; //TODO
        }

        public uint TransitionLampState(ulong Timestamp, BridgeRT.State NewState, uint TransitionPeriod, out uint LampResponseCode)
        {
            LampResponseCode = 0;
            return 0; //TODO
        }
    }
}
