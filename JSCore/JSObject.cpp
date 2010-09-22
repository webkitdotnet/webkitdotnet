#include "stdafx.h"

#include "JSValue.h"
#include "JSObject.h"
#include "JSContext.h"
#include "JSCoreMarshal.h"
#include "JSCoreObjectWrapper.h"


JSObject::JSObject(JSContext ^ context, JSObjectRef object)
: JSValue(context, (JSValueRef)object)
{
}

bool JSObject::HasProperty(String ^ propertyName)
{
    JSStringRef str = JSCoreMarshal::StringToJSString(propertyName);
    bool val = JSObjectHasProperty(_context->context(), (JSObjectRef)_value, str);
    JSStringRelease(str);
    return val;
}

JSValue ^ JSObject::GetProperty(String ^ propertyName)
{
    JSStringRef str = JSCoreMarshal::StringToJSString(propertyName);

    JSValueRef val = JSObjectGetProperty(_context->context(), (JSObjectRef)_value, str, NULL);

    JSStringRelease(str);
    return gcnew JSValue(_context, val);
}

void JSObject::SetProperty(String ^ propertyName, bool value)
{
    SetProperty(propertyName, JSValueMakeBoolean(_context->context(), value));
}

void JSObject::SetProperty(String ^ propertyName, double value)
{
    SetProperty(propertyName, JSValueMakeNumber(_context->context(), value));
}

void JSObject::SetProperty(String ^ propertyName, System::Object ^ value)
{
    JSClassRef wrap = JSClassCreate(&wrapperClass);
    
    GCHandle handle = GCHandle::Alloc(value, GCHandleType::Normal);
    void * ptr = GCHandle::ToIntPtr(handle).ToPointer();

    JSObjectRef jsObj = JSObjectMake(_context->context(), wrap, ptr);
    SetProperty(propertyName, (JSValueRef)jsObj);
}

void JSObject::SetProperty(String ^ propertyName, System::String ^ value)
{
    JSStringRef jsStr = JSCoreMarshal::StringToJSString(value);
    SetProperty(propertyName, JSValueMakeString(_context->context(), jsStr));
    JSStringRelease(jsStr);
}

void JSObject::SetProperty(String ^ propertyName, JSValueRef value)
{
    JSStringRef jsStr = JSCoreMarshal::StringToJSString(propertyName);
    JSObjectSetProperty(_context->context(), (JSObjectRef)_value, jsStr, value, NULL, NULL);
    JSStringRelease(jsStr);
}