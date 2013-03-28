#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace WebKit::Interop;
using namespace WebKit::JSCore;

namespace WebKit {
namespace JSCore {


public delegate void ActionDelegate(array<Object^>^ args);

public delegate Object^ EventDelegate(array<Object^>^ args);

public delegate Object^ JavaScriptFunction(JSContext ^context, ... array<Object ^> ^ variableArgs);


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
    void SetProperty(String ^ propertyName, EventDelegate ^ func);
    void SetProperty(String ^ propertyName, ActionDelegate ^ func);
    JSValue ^ CallAsFunction(array<Object ^> ^ variableArgs);
    JSValue ^ CallFunction(String ^ methodName, ... array<Object ^> ^ variableArgs);

    Dictionary<Object^, Object^>^ ToDictionary();
    Dictionary<Object^, Object^>^ ToDictionary(bool recursive);

internal:
    JSObject(JSContext ^ context, JSObjectRef object);

private:
    void SetProperty(String ^ propertyName, JSValueRef value);
};


}} // namespace WebKit::JSCore
