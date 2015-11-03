//
// Copyright (c) 2015, Microsoft Corporation
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR
// IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//

#pragma once

namespace BridgeRT
{
    static const int WAIT_TIMEOUT_FOR_ADAPTER_OPERATION = 20000;  // ms

    class DeviceMain;

    class AllJoynHelper
    {
    public:
        AllJoynHelper();

        static QStatus SetMsgArg(_In_ IAdapterValue ^adapterValue, _Inout_ alljoyn_msgarg msgArg);
        static QStatus SetMsgArgFromAdapterObject(_In_ IAdapterValue ^adapterValue, _Inout_ alljoyn_msgarg msgArg, _In_ DeviceMain *deviceMain);
        static QStatus GetAdapterValue(_Inout_ IAdapterValue ^adapterValue, _In_ alljoyn_msgarg msgArg);
        static QStatus GetAdapterObject(_Inout_ IAdapterValue ^adapterValue, _In_ alljoyn_msgarg msgArg, _In_ DeviceMain *deviceMain);
        static QStatus GetSignature(_In_ Windows::Foundation::PropertyType propertyType, _Out_ std::string &signature);

        static void BuildBusObjectName(_In_ Platform::String ^inString, _Inout_ std::string &builtName);
        static void BuildPropertyOrMethodOrSignalName(_In_ Platform::String ^inString, _Inout_ std::string &builtName);
        static void EncodeStringForInterfaceName(_In_ Platform::String ^inString, _Out_ std::string &encodeString);
        static void EncodeStringForServiceName(_In_ Platform::String ^inString, _Out_ std::string &encodeString);
        static void EncodeStringForRootServiceName(_In_ Platform::String ^inString, _Out_ std::string &encodeString);
        static void EncodeStringForAppName(_In_ Platform::String ^inString, _Out_ std::string &encodeString);
    };
}

