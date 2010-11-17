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
#ifdef DOTNET4
    : System::Dynamic::DynamicObject
#endif
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

#ifdef DOTNET4
    virtual bool TryConvert(System::Dynamic::ConvertBinder^ binder, [OutAttribute] Object ^% result) override;
    virtual bool TryGetMember(System::Dynamic::GetMemberBinder^ binder, [OutAttribute] Object ^% result) override;
    virtual bool TrySetMember(System::Dynamic::SetMemberBinder^ binder, Object ^ result) override;
    virtual bool TryInvokeMember(System::Dynamic::InvokeMemberBinder^ binder, array<Object ^> ^ args, [OutAttribute] Object ^% result) override;
#endif

internal:
    JSValue(JSContext ^ context, JSValueRef value);
};


}} // namespace WebKit::JSCore
