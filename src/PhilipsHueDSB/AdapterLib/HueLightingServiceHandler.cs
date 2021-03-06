﻿/*  
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
        private const UInt32 StandbyPowerUsage_Milliwatts = 400;

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
            LampDetails_Wattage = info.Wattage;
        }
        public bool LampDetails_Color { get; }

        public uint LampDetails_ColorRenderingIndex { get; }

        public bool LampDetails_Dimmable { get; }

        public bool LampDetails_HasEffects { get; }

        public uint LampDetails_IncandescentEquivalent { get; }

        public uint LampDetails_LampBaseType { get; }

        public uint LampDetails_LampBeamAngle { get; }

        public string LampDetails_LampID { get; }

        public uint LampDetails_LampType { get; }

        public uint LampDetails_Make { get; }

        public uint LampDetails_MaxLumens { get; }

        public uint LampDetails_MaxTemperature { get; }

        public uint LampDetails_MaxVoltage { get; }

        public uint LampDetails_MinTemperature { get; }

        public uint LampDetails_MinVoltage { get; }

        public uint LampDetails_Model { get; }

        public uint LampDetails_Type { get; }

        public bool LampDetails_VariableColorTemp { get; }

        public uint LampDetails_Version { get { return 1; } }

        public uint LampDetails_Wattage { get; }

        public uint LampParameters_BrightnessLumens
        {
            get
            {
                if (LampState_OnOff == false)
                    return 0;
                return (UInt32)(LampDetails_MaxLumens * (LampState_Brightness / (double)UInt32Max));
            }
        }

        public uint LampParameters_EnergyUsageMilliwatts { get {
                if (!LampState_OnOff)
                    return StandbyPowerUsage_Milliwatts;
                //Assume linear power consumption based on brightness from minimum to maximum usage:
                return (UInt32)((LampDetails_Wattage * 1000 - StandbyPowerUsage_Milliwatts) *
                    (LampState_Brightness / (double)UInt32Max) + StandbyPowerUsage_Milliwatts);
            } }

        public uint LampParameters_Version { get { return 1; } }

        public uint[] LampService_LampFaults { get; private set; }

        public uint LampService_LampServiceVersion { get { return 1; } }

        public uint LampService_Version { get { return 1; } }

        /**< The lamp brightness (in percentage). 0 means 0. uint32_max-1 means 100%. */
        public uint LampState_Brightness
        {
            //1..254
            get { return (uint)(_light.State.Brightness / 254d * UInt32Max); }
            set
            {
                var command = new LightCommand();
                command.Brightness = (byte)(value * 254d / UInt32Max);
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.Brightness = (byte)value;
            }
        }
        // The lamp color temperature (in Kelvin).
        // 0 means 1000K. uint32_max-1 means 20000K.
        private const UInt32 UInt32Max = UInt32.MaxValue - 1;
        public uint LampState_ColorTemp
        {
            get
            {
                if (!_light.State.ColorTemperature.HasValue)
                    return 0;
                var kelvin = _light.State.ColorTemperature.Value / 1000000d; //Convert from Mired to Kelvin
                return (uint)((kelvin - 1000d) / 19000d * UInt32Max); //Convert to 1000K..20000K range on UInt32 scale
            }

            set
            {
                var command = new LightCommand();
                double kelvin = value * 19000d / UInt32Max + 1000;
                int kelvinLimited = (int)Math.Max(LampDetails_MinTemperature, Math.Min(LampDetails_MaxTemperature, kelvin));
                int mired = (int)(1000000d / kelvin);

                command.ColorTemperature = mired;
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.ColorTemperature = (int)mired;
            }
        }

        // The lamp hue (in degree). 0 means 0. uint32_max-1 means 360. */
        public uint LampState_Hue
        {
            //0 .. 65535.
            get
            {
                return _light.State.Hue.HasValue ? (uint)(_light.State.Hue.Value / 65535d * UInt32Max) : 0;
            }

            set
            {
                var command = new LightCommand();
                command.Hue = (int)(value * 65535d / UInt32Max);
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

        // The lamp saturation (in percentage). 0 means 0. uint32_max-1 means 100% */
        public uint LampState_Saturation
        {
            //0=white, 254=fully saturated
            get
            {
                return _light.State.Saturation.HasValue ? (uint)
                    (_light.State.Saturation.Value / 254d * UInt32Max) : 0;
            }

            set
            {
                var command = new LightCommand();
                command.Saturation = (int)(value * 254d / UInt32Max);
                _client.SendCommandAsync(command, new[] { _light.Id });
                _light.State.Saturation = (int)value;
            }
        }

        public uint LampState_Version { get { return 1; } }

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
            return 0;
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
                command.Brightness = (byte)(NewState.Brightness.Value * 254d / UInt32Max);
            if (NewState.Hue.HasValue && LampDetails_Color)
                command.Hue = (int)(NewState.Hue.Value * 65535d / UInt32Max);
            if (NewState.Saturation.HasValue && LampDetails_Color)
                command.Saturation = (int)(NewState.Saturation.Value * 254d / UInt32Max);
            if (NewState.ColorTemp.HasValue && LampDetails_VariableColorTemp)
            {
                double kelvin = NewState.ColorTemp.Value * 19000d / UInt32Max + 1000;
                int kelvinLimited = (int)Math.Max(LampDetails_MinTemperature, Math.Min(LampDetails_MaxTemperature, kelvin));
                int mired = (int)(1000000d / kelvinLimited);
                //Currently hue doesn't like setting color temp if the other parameters are also set.
                //Skipping for now.
                //command.ColorTemperature = mired;
            }
            LampResponseCode = 0;
            System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(Timestamp)).ContinueWith(_ =>
            {
                _client.SendCommandAsync(command, new[] { _light.Id });
                //Update state
                if(command.Hue.HasValue)
                    _light.State.Hue = command.Hue.Value;
                if (command.Brightness.HasValue)
                    _light.State.Brightness = command.Brightness.Value;
                if (command.Saturation.HasValue)
                    _light.State.Saturation= command.Saturation.Value;
                if (command.ColorTemperature.HasValue)
                    _light.State.ColorTemperature = command.ColorTemperature.Value;
                if (command.On.HasValue)
                    _light.State.On = command.On.Value;
            });
            return 0; //TODO
        }
    }
}
