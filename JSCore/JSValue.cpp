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

