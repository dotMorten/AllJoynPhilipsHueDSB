//-----------------------------------------------------------------------------
// <auto-generated> 
//   This code was generated by a tool. 
// 
//   Changes to this file may cause incorrect behavior and will be lost if  
//   the code is regenerated.
//
//   Tool: AllJoynCodeGenerator.exe
//
//   This tool is located in the Windows 10 SDK and the Windows 10 AllJoyn 
//   Visual Studio Extension in the Visual Studio Gallery.  
//
//   The generated code should be packaged in a Windows 10 C++/CX Runtime  
//   Component which can be consumed in any UWP-supported language using 
//   APIs that are available in Windows.Devices.AllJoyn.
//
//   Using AllJoynCodeGenerator - Invoke the following command with a valid 
//   Introspection XML file and a writable output directory:
//     AllJoynCodeGenerator -i <INPUT XML FILE> -o <OUTPUT DIRECTORY>
// </auto-generated>
//-----------------------------------------------------------------------------
#include "pch.h"

using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Devices::AllJoyn;
using namespace Windows::Foundation;
using namespace org::alljoyn::Notification;

void NotificationSignals::Initialize(_In_ alljoyn_busobject busObject, _In_ alljoyn_sessionid sessionId)
{
    m_busObject = busObject;
    m_sessionId = sessionId;

    auto interfaceDefinition = alljoyn_busattachment_getinterface(alljoyn_busobject_getbusattachment(busObject), "org.alljoyn.Notification");
    alljoyn_interfacedescription_getmember(interfaceDefinition, "notify", &m_memberNotify);
}

void NotificationSignals::Notify(_In_ uint16 interfaceMemberVersion, _In_ int32 interfaceMemberMsgId, _In_ uint16 interfaceMemberMsgType, _In_ Platform::String^ interfaceMemberDeviceId, _In_ Platform::String^ interfaceMemberDeviceName, _In_ Windows::Foundation::Collections::IVectorView<byte>^ interfaceMemberAppId, _In_ Platform::String^ interfaceMemberAppName, _In_ Windows::Foundation::Collections::IMapView<int32,Platform::Object^>^ interfaceMemberAttributes, _In_ Windows::Foundation::Collections::IMapView<Platform::String^,Platform::String^>^ interfaceMemberCustomAttributes, _In_ Windows::Foundation::Collections::IVectorView<NotificationLangTextItem^>^ interfaceMemberLangText)
{
    if (nullptr == m_busObject)
    {
        return;
    }

    size_t argCount = 10;
    alljoyn_msgarg arguments = alljoyn_msgarg_array_create(argCount);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 0), "q", interfaceMemberVersion);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 1), "i", interfaceMemberMsgId);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 2), "q", interfaceMemberMsgType);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 3), "s", interfaceMemberDeviceId);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 4), "s", interfaceMemberDeviceName);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 5), "ay", interfaceMemberAppId);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 6), "s", interfaceMemberAppName);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 7), "a{iv}", interfaceMemberAttributes);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 8), "a{ss}", interfaceMemberCustomAttributes);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 9), "a(ss)", interfaceMemberLangText);
    
    alljoyn_busobject_signal(
        m_busObject, 
        NULL,  // Generated code only supports broadcast signals.
        m_sessionId,
        m_memberNotify,
        arguments,
        argCount, 
        0, // A signal with a TTL of 0 will be sent to every member of the session, regardless of how long it takes to deliver the message
        ALLJOYN_MESSAGE_FLAG_GLOBAL_BROADCAST, // Broadcast to everyone in the session.
        NULL); // The generated code does not need the generated signal message

    alljoyn_msgarg_destroy(arguments);
}

void NotificationSignals::CallNotifyReceived(_In_ NotificationSignals^ sender, _In_ NotificationNotifyReceivedEventArgs^ args)
{
    NotifyReceived(sender, args);
}
