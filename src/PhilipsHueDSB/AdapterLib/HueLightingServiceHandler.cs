using BridgeRT;
using Q42.HueApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private const int OEM_LS_COLOR_TEMPERATURE_MIN = 2700;
        private const int OEM_LS_COLOR_TEMPERATURE_MAX = 9000;

        Q42.HueApi.Light _light;
        Q42.HueApi.HueClient _client;
        public HueLightingServiceHandler(Q42.HueApi.HueClient client, Q42.HueApi.Light light)
        {
            //Doc on supported lights:
            // http://www.developers.meethue.com/documentation/supported-lights
            _light = light;
            var state = light.State;
            int lightType = 0;
            if (light.Type == "Extended color light")
                lightType = 4;
            else if (light.Type == "Color temperature light")
                lightType = 3;
            else if (light.Type == "Color light")
                lightType = 2;
            else if(light.Type == "Dimmable light")
                lightType = 1;
            _client = client;
            
            LampDetails_Color = lightType > 1;
            LampDetails_ColorRenderingIndex = 80;
            LampDetails_Dimmable = true;
            LampDetails_HasEffects = false;
            LampDetails_IncandescentEquivalent = 60;
            LampDetails_LampBaseType = (uint)AdapterLib.LsfEnums.BaseType.BASETYPE_E26;
            LampDetails_LampBeamAngle = 160;
            LampDetails_LampID = light.Id;
            LampDetails_LampType = (uint)AdapterLib.LsfEnums.LampType.LAMPTYPE_A19;
            LampDetails_Make = (uint)AdapterLib.LsfEnums.LampMake.MAKE_OEM1;
            LampDetails_MaxLumens = 620;
            LampDetails_MaxTemperature = OEM_LS_COLOR_TEMPERATURE_MAX;
            LampDetails_MaxVoltage = 120;
            LampDetails_MinTemperature = OEM_LS_COLOR_TEMPERATURE_MIN;
            LampDetails_MinVoltage = 100;
            LampDetails_Model = 1;
            //LampDetails_Model = light.ModelId;
            LampDetails_Type = (uint)AdapterLib.LsfEnums.DeviceType.TYPE_LAMP;
            LampDetails_VariableColorTemp = lightType > 2;
            //LampDetails_Version = light.SoftwareVersion;
            LampDetails_Wattage = 9;
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
                command.Hue = (int)(value / 365d * 65535);
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
