using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{
    internal class HueBulbDevice : AdapterDevice
    {
        public Q42.HueApi.Light _light;
        public HueBulbDevice(Q42.HueApi.HueClient client, Q42.HueApi.Light light) : base(light.Name,
            light.ManufacturerName, light.ModelId, light.SoftwareVersion, light.UniqueId, light.Type)
        {
            _light = light;
            
            base.LightingServiceHandler = new HueLightingServiceHandler(client, light);
        }
        public Q42.HueApi.Light Light
        {
            get { return _light; }
        }
    }
}
