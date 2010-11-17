#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace WebKit::Interop;
using namespace WebKit::JSCore;

namespace WebKit {
namespace JSCore {

ref class JSValue;
ref class JSContext;

public ref class JSObject : public WebKit::JSCore::JSValue
{
public:
    bool HasProperty(String ^ propertyName);
    JSValue ^ GetProperty(String ^ propertyName);
    void SetProperty(String ^ propertyName, bool value);
    void SetProperty(String ^ propertyName, double value);
    void SetProperty(String ^ propertyName, System::Object ^ value);
    void SetProperty(String ^ propertyName, System::String ^ value);
    JSValue ^ CallFunction(String ^ methodName, ... array<Object ^> ^ variableArgs);
internal:
    JSObject(JSContext ^ context, JSObjectRef object);

private:
    void SetProperty(String ^ propertyName, JSValueRef value);
};


}} // namespace WebKit::JSCore
