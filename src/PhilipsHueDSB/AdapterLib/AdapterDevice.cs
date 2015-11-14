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
using BridgeRT;

namespace AdapterLib
{
    //
    // AdapterValue.
    // Description:
    // The class that implements IAdapterValue from BridgeRT.
    //
    class AdapterValue : IAdapterValue
    {
        // public properties
        public string Name { get; }
        public object Data { get; set; }

        internal AdapterValue(string ObjectName, object DefaultData)
        {
            this.Name = ObjectName;
            this.Data = DefaultData;
        }

        internal AdapterValue(AdapterValue Other)
        {
            this.Name = Other.Name;
            this.Data = Other.Data;
        }
    }

    //
    // AdapterProperty.
    // Description:
    // The class that implements IAdapterProperty from BridgeRT.
    //
    class AdapterProperty : IAdapterProperty
    {
        // public properties
        public string Name { get; }
        public string InterfaceHint { get; }
        public IList<IAdapterAttribute> Attributes { get; }

        internal AdapterProperty(string ObjectName, string IfHint)
        {
            this.Name = ObjectName;
            this.InterfaceHint = IfHint;

            try
            {
                this.Attributes = new List<IAdapterAttribute>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterProperty(AdapterProperty Other)
        {
            this.Name = Other.Name;
            this.InterfaceHint = Other.InterfaceHint;

            try
            {
                this.Attributes = new List<IAdapterAttribute>(Other.Attributes);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
    }

    //
    // AdapterAttribute.
    // Description:
    // The class that implements IAdapterAttribute from BridgeRT.
    //
    class AdapterAttribute : IAdapterAttribute
    {
        // public properties
        public IAdapterValue Value { get; }

        public E_ACCESS_TYPE Access { get; set; }
        public IDictionary<string, string> Annotations { get; }
        public SignalBehavior COVBehavior { get; set; }

        internal AdapterAttribute(string ObjectName, object DefaultData, E_ACCESS_TYPE access = E_ACCESS_TYPE.ACCESS_READ)
        {
            try
            {
                this.Value = new AdapterValue(ObjectName, DefaultData);
                this.Annotations = new Dictionary<string, string>();
                this.Access = access;
                this.COVBehavior = SignalBehavior.Never;
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterAttribute(AdapterAttribute Other)
        {
            this.Value = Other.Value;
            this.Annotations = Other.Annotations;
            this.Access = Other.Access;
            this.COVBehavior = Other.COVBehavior;
        }
    }

    //
    // AdapterMethod.
    // Description:
    // The class that implements IAdapterMethod from BridgeRT.
    //
    class AdapterMethod : IAdapterMethod
    {
        // public properties
        public string Name { get; }

        public string Description { get; }

        public IList<IAdapterValue> InputParams { get; set; }

        public IList<IAdapterValue> OutputParams { get; }

        public int HResult { get; private set; }

        internal AdapterMethod(
            string ObjectName,
            string Description,
            int ReturnValue)
        {
            this.Name = ObjectName;
            this.Description = Description;
            this.HResult = ReturnValue;

            try
            {
                this.InputParams = new List<IAdapterValue>();
                this.OutputParams = new List<IAdapterValue>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterMethod(AdapterMethod Other)
        {
            this.Name = Other.Name;
            this.Description = Other.Description;
            this.HResult = Other.HResult;

            try
            {
                this.InputParams = new List<IAdapterValue>(Other.InputParams);
                this.OutputParams = new List<IAdapterValue>(Other.OutputParams);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal void SetResult(int ReturnValue)
        {
            this.HResult = ReturnValue;
        }
        internal Action InvokeAction { get; set; }

        internal protected void Invoke()
        {
            if(InvokeAction != null)
            {
                InvokeAction();
            }
        }
    }
    
    //
    // AdapterDevice.
    // Description:
    // The class that implements IAdapterDevice from BridgeRT.
    //
    class AdapterDevice : IAdapterDevice,
                            IAdapterDeviceLightingService,
                            IAdapterDeviceControlPanel
    {
        // Object Name
        public string Name { get; }

        // Device information
        public string Vendor { get; }

        public string Model { get; }

        public string Version { get; }

        public string FirmwareVersion { get; }

        public string SerialNumber { get; }

        public string Description { get; }

        // Device properties
        public IList<IAdapterProperty> Properties { get; }

        // Device methods
        public IList<IAdapterMethod> Methods { get; }

        // Device signals
        public IList<IAdapterSignal> Signals { get; }

        // Control Panel Handler
        public IControlPanelHandler ControlPanelHandler
        {
            get
            {
                return null;
            }
        }

        // Lighting Service Handler
        public ILSFHandler LightingServiceHandler
        {
            get; protected set;
        }

        // Icon
        public IAdapterIcon Icon
        {
            get; protected set;
        }


        internal AdapterDevice(
            string Name,
            string VendorName,
            string Model,
            string Version,
            string SerialNumber,
            string Description)
        {
            this.Name = Name;
            this.Vendor = VendorName;
            this.Model = Model;
            this.Version = Version;
            this.FirmwareVersion = Version;
            this.SerialNumber = SerialNumber;
            this.Description = Description;

            try
            {
                this.Properties = new List<IAdapterProperty>();
                this.Methods = new List<IAdapterMethod>();
                this.Signals = new List<IAdapterSignal>();
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal AdapterDevice(AdapterDevice Other)
        {
            this.Name = Other.Name;
            this.Vendor = Other.Vendor;
            this.Model = Other.Model;
            this.Version = Other.Version;
            this.FirmwareVersion = Other.FirmwareVersion;
            this.SerialNumber = Other.SerialNumber;
            this.Description = Other.Description;

            try
            {
                this.Properties = new List<IAdapterProperty>(Other.Properties);
                this.Methods = new List<IAdapterMethod>(Other.Methods);
                this.Signals = new List<IAdapterSignal>(Other.Signals);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }

        internal void AddChangeOfValueSignal(
            IAdapterProperty Property,
            IAdapterValue Attribute)
        {
            try
            {
                AdapterSignal covSignal = new AdapterSignal(Constants.CHANGE_OF_VALUE_SIGNAL);

                // Property Handle
                AdapterValue propertyHandle = new AdapterValue(
                                                    Constants.COV__PROPERTY_HANDLE,
                                                    Property);

                // Attribute Handle
                AdapterValue attrHandle = new AdapterValue(
                                                    Constants.COV__ATTRIBUTE_HANDLE,
                                                    Attribute);

                covSignal.Params.Add(propertyHandle);
                covSignal.Params.Add(attrHandle);

                this.Signals.Add(covSignal);
            }
            catch (OutOfMemoryException ex)
            {
                throw;
            }
        }
    }
}
