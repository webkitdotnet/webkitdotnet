#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace WebKit::Interop;

namespace WebKit {
namespace JSCore {

ref class JSValue;
ref class JSContext;

public ref class JSObject : public WebKit::JSCore::JSValue
{
public:
    bool HasProperty(String ^ propertyName);
    JSValue ^ GetProperty(String ^ propertyName);
    void SetProperty(String ^ propertyName, int value);

internal:
    JSObject(JSContext ^ context, Object ^ object);
};


}} // namespace WebKit::JSCore
