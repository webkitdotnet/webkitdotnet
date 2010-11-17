#include "stdafx.h"

#include "JSCoreMarshal.h"
#include "JSContext.h"
#include "JSValue.h"
#include "JSObject.h"


JSValue::JSValue(JSContext ^ context, JSValueRef value)
: _context(context), _value(value)
{
    // TODO: is this necessary (probably...)?
    JSValueProtect(_context->context(), _value);
}

JSValue::~JSValue()
{
    // TODO: would probably be nicer to implement IDisposable instead
    // erm......
    if (_context != nullptr && _context->context() != NULL)
        JSValueUnprotect(_context->context(), _value);
}

String ^ JSValue::ToString()
{
    JSStringRef jsStr = JSValueToStringCopy(_context->context(), _value, NULL);
    String ^ str = JSCoreMarshal::JSStringToString(jsStr);
    JSStringRelease(jsStr);
    return str;
}

bool JSValue::IsBoolean::get()
{
    return JSValueIsBoolean(_context->context(), _value);
}

bool JSValue::IsNull::get()
{
    return JSValueIsNull(_context->context(), _value);
}

bool JSValue::IsNumber::get()
{
    return JSValueIsNumber(_context->context(), _value);
}

bool JSValue::IsObject::get()
{
    return JSValueIsObject(_context->context(), _value);
}

bool JSValue::IsString::get()
{
    return JSValueIsString(_context->context(), _value);
}

bool JSValue::IsUndefined::get()
{
    return JSValueIsUndefined(_context->context(), _value);
}

String ^ JSValue::ToJSONString()
{
    return "";
}

bool JSValue::ToBoolean()
{
    return JSValueToBoolean(_context->context(), _value);
}

double JSValue::ToNumber()
{
    return JSValueToNumber(_context->context(), _value, NULL);
}

JSObject ^ JSValue::ToObject()
{
    return gcnew JSObject(_context, (JSObjectRef)_value);
}

void JSValue::Protect()
{
}

void JSValue::Unprotect()
{
}

#ifdef DOTNET4
bool JSValue::TryConvert(System::Dynamic::ConvertBinder^ binder, [OutAttribute] Object ^% result)
{
    if(binder->Type == double::typeid)
    {
        if(IsNumber)
        {
            result = ToNumber();
            return true;
        }
        return false;
    }
    if(binder->Type == bool::typeid)
    {
        if(IsBoolean)
        {
            result = ToBoolean();
            return true;
        }
        return false;
    }
    if(binder->Type == String::typeid)
    {
        if(IsString)
        {
            result = ToString();
            return true;
        }
        return false;
    }
    return false;
}

bool JSValue::TryGetMember(System::Dynamic::GetMemberBinder ^ binder, Object ^% result)
{
    if(!IsObject) return false;
    JSObject ^ obj = ToObject();
    if(!obj->HasProperty(binder->Name)) return false;

    JSValue ^ value = obj->GetProperty(binder->Name);
    if(value->IsNumber)
    {
        result = value->ToNumber();
        return true;
    }
    if(value->IsBoolean)
    {
        result = value->ToBoolean();
        return true;
    }
    if(value->IsString)
    {
        result = value->ToString();
        return true;
    }
    if(value->IsObject)
    {
        result = value->ToObject();
        return true;
    }
    return false;
}

bool JSValue::TrySetMember(System::Dynamic::SetMemberBinder ^ binder, Object ^ value)
{
    if(!IsObject) return false;

    JSObject ^ obj = ToObject();

    Type ^ type = value->GetType();
    if(type == double::typeid)
    {
        obj->SetProperty(binder->Name, (double)value);
        return true;
    }
    if(type == bool::typeid)
    {
        obj->SetProperty(binder->Name, (bool)value);
        return true;
    }
    if(type == String::typeid)
    {
        obj->SetProperty(binder->Name, (String ^)value);
        return true;
    }
    obj->SetProperty(binder->Name, value);
    return true;
}

bool JSValue::TryInvokeMember(System::Dynamic::InvokeMemberBinder ^ binder, array<Object ^> ^ args, Object ^% result)
{
    if(!IsObject) return false;
    JSObject ^ obj = ToObject();
    result = obj->CallFunction(binder->Name, args);
    return true;
}
#endif

