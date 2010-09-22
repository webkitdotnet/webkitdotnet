#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace WebKit::Interop;
using namespace WebKit::JSCore;

namespace WebKit {
namespace JSCore {

ref class JSContext;
ref class JSObject;

public ref class JSValue
{
protected:
    JSValueRef _value;
    JSContext ^ _context;

public:
    ~JSValue();

    virtual String ^ ToString() override;

    // JSValueRef.h functions

    // properties
    property bool IsUndefined { bool get(); };
    property bool IsNull { bool get(); };
    property bool IsBoolean { bool get(); };
    property bool IsNumber { bool get(); };
    property bool IsString { bool get(); };
    property bool IsObject { bool get(); };

    // conversion methods
    String ^ ToJSONString();
    bool ToBoolean();
    double ToNumber();
    JSObject ^ ToObject();

    void Protect();
    void Unprotect();


internal:
    JSValue(JSContext ^ context, JSValueRef value);
};


}} // namespace WebKit::JSCore
