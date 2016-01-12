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
            LampDetails_HasEffects = true;
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
                if (!_light.State.ColorTemperature.HasValue)
                    return 0;
                var kelvin = 1000000d / (_light.State.ColorTemperature);// - 654.2222222222222) / -0.07711111111111111111111111111111;
                return (uint)kelvin;
            }

            set
            {
                var command = new LightCommand();
                int mired = (int)(1000000d / value);

                command.ColorTemperature = mired;
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.ColorTemperature = (int)mired;
            }
        }

        public uint LampState_Hue
        {
            //0 .. 65535.
            get
            {
                return _light.State.Hue.HasValue ? (uint)(_light.State.Hue.Value / 65535d * UInt32.MaxValue) : 0;
            }

            set
            {
                var command = new LightCommand();
                command.Hue = (int)(value * 65535d / UInt32.MaxValue);
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
            ApplyPulseEffectAsync(FromState, ToState, Period, Duration, NumPulses, Timestamp);
            return 1; //TODO
        }

        /// <summary>
        /// Change the state of the lamp at the specified time, between the specified OnOff, Brightness, Hue,
        /// Saturation, and ColorTemp values. Pulse for the specified number of times, at the specified duration
        /// </summary>
        /// <param name="FromState">Current state of the lamp to transition from</param>
        /// <param name="ToState">New state of the lamp to transition to</param>
        /// <param name="Period">Time period(in ms) to transition over to new state</param>
        /// <param name="Duration">Time period(in ms) to remain in new state</param>
        /// <param name="NumPulses">Number of pulses</param>
        /// <param name="Timestamp">Timestamp (in ms) of when to start the pulses</param>
        private async void ApplyPulseEffectAsync(BridgeRT.State FromState, BridgeRT.State ToState, uint Period, uint Duration, uint NumPulses, ulong Timestamp)
        {
            uint response;
            await System.Threading.Tasks.Task.Delay((int)Timestamp).ConfigureAwait(false);
            TransitionLampState(0, FromState, 0, out response);
            for (int i = 0; i < NumPulses; i++)
            {
                TransitionLampState(0, ToState, Period, out response);
                await System.Threading.Tasks.Task.Delay((int)(Period + Duration)).ConfigureAwait(false);
                TransitionLampState(0, FromState, Period, out response);
                await System.Threading.Tasks.Task.Delay((int)(Period + Duration)).ConfigureAwait(false);
            }
        }

        public uint TransitionLampState(ulong Timestamp, BridgeRT.State NewState, uint TransitionPeriod, out uint LampResponseCode)
        {
            var command = new LightCommand();
            command.TransitionTime = TimeSpan.FromMilliseconds(TransitionPeriod);
            command.On = NewState.IsOn;
            if (NewState.Brightness.HasValue && LampDetails_Dimmable)
                command.Brightness = (byte)(NewState.Brightness.Value * 254d / (UInt32.MaxValue - 1));
            if (NewState.Hue.HasValue && LampDetails_Color)
                command.Hue = (int)(NewState.Hue.Value * 65535d / UInt32.MaxValue);
            if (NewState.Saturation.HasValue && LampDetails_Color)
                command.Saturation = (int)(NewState.Saturation.Value * 254d / (UInt32.MaxValue - 1));
            //Currently hue doesn't like setting color temp if the other parameters are also set.
            //Skipping for now.
            //if (NewState.ColorTemp.HasValue && LampDetails_VariableColorTemp)
            //{
            //    int kelvin = (int)Math.Max(LampDetails_MinTemperature, Math.Min(LampDetails_MaxTemperature, NewState.ColorTemp.Value));
            //    UInt16 mired = (UInt16)(kelvin * -0.07711111111111111111111111111111 + 654.2222222222222);
            //    command.ColorTemperature = mired;
            //}
            LampResponseCode = 0;
            System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(Timestamp)).ContinueWith(_ =>
            {
                _client.SendCommandAsync(command, new[] { _light.Id });
            });
            return 0; //TODO
        }
    }
}
