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

    JSClassRelease(wrap);

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

JSValue ^ JSObject::CallFunction(String ^ methodName, ... array<Object ^> ^ variableArgs)
{
    JSStringRef str = JSCoreMarshal::StringToJSString(methodName);

    JSObjectRef val = (JSObjectRef)JSObjectGetProperty(_context->context(), (JSObjectRef)_value, str, NULL);

    bool x = JSValueIsObject(_context->context(), val);
    JSStringRelease(str);

    JSValueRef * args = new JSValueRef[variableArgs->Length];
    for(int i = 0; i < variableArgs->Length; i++)
    {
        args[i] = getJSValueRefFromObject(_context->context(), variableArgs[i], NULL);
    }

    JSValueRef ret = JSObjectCallAsFunction(_context->context(), val, (JSObjectRef)_value, variableArgs->Length, args, NULL);

    for(int i = 0; i < variableArgs->Length; i++)
    {
        JSValueUnprotect(_context->context(), args[i]);
    }
    delete[] args;
    return gcnew JSValue(_context, ret);
}