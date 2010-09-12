#include "stdafx.h"

#include "JSContext.h"
#include "JSValue.h"


WebKit::JSCore::JSValue::JSValue(JSContext ^ context, JSValueRef value)
: _context(context), _value(value)
{
}

WebKit::JSCore::JSValue::~JSValue()
{
}

String ^ WebKit::JSCore::JSValue::ToString()
{
    JSStringRef jsStr = JSValueToStringCopy(_context->context(), _value, NULL);
    int len = JSStringGetLength(jsStr);
    JSChar * cStr = (JSChar *) JSStringGetCharactersPtr(jsStr);
    cStr[len] = L'\0';

    // TODO: clean up
    return Marshal::PtrToStringAuto(IntPtr((void *) cStr));
}

bool WebKit::JSCore::JSValue::IsBoolean::get()
{
    return JSValueIsBoolean(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsNull::get()
{
    return JSValueIsNull(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsNumber::get()
{
    return JSValueIsNumber(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsObject::get()
{
    return JSValueIsObject(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsString::get()
{
    return JSValueIsString(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsUndefined::get()
{
    return JSValueIsUndefined(_context->context(), _value);
}

System::String ^ WebKit::JSCore::JSValue::ToJSONString()
{
    return "";
}

bool WebKit::JSCore::JSValue::ToBoolean()
{
    return JSValueToBoolean(_context->context(), _value);
}

double WebKit::JSCore::JSValue::ToNumber()
{
    return JSValueToNumber(_context->context(), _value, NULL);
}

System::Object ^ WebKit::JSCore::JSValue::ToObject()
{
    return nullptr;
}

void WebKit::JSCore::JSValue::Protect()
{
}

void WebKit::JSCore::JSValue::Unprotect()
{
}

