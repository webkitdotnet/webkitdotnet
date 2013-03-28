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
    JSValueRef jsVal = getJSValueRefFromObject(_context->context(), value, NULL);
    SetProperty(propertyName, jsVal);
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

void JSObject::SetProperty(String ^ propertyName, EventDelegate ^ func)
{
    JSValueRef jsVal = getJSValueRefFromObject(_context->context(), func, NULL);
    SetProperty(propertyName, jsVal);
}

void JSObject::SetProperty(String ^ propertyName, ActionDelegate ^ func)
{
    JSValueRef jsVal = getJSValueRefFromObject(_context->context(), func, NULL);
    SetProperty(propertyName, jsVal);
}

JSValue ^ JSObject::CallAsFunction(array<Object ^> ^ variableArgs)
{
    JSContextRef ctx = _context->context();

    JSValueRef * args = new JSValueRef[variableArgs->Length];
    for(int i = 0; i < variableArgs->Length; i++)
    {
        args[i] = getJSValueRefFromObject(ctx, variableArgs[i], NULL);
    }

    JSValueRef ret = JSObjectCallAsFunction(ctx, (JSObjectRef)_value, NULL, variableArgs->Length, args, NULL);

    for(int i = 0; i < variableArgs->Length; i++)
    {
        JSValueUnprotect(ctx, args[i]);
    }
    delete[] args;
    return gcnew JSValue(_context, ret);
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

Dictionary<Object^, Object^>^ JSObject::ToDictionary()
{
    return ToDictionary(false);
}

Dictionary<Object^, Object^>^ JSObject::ToDictionary(bool recursive)
{
    JSObjectRef o = (JSObjectRef)_value;
    JSContextRef ctx = _context->context();

    JSPropertyNameArrayRef properties = JSObjectCopyPropertyNames(ctx, o);
    size_t count = JSPropertyNameArrayGetCount(properties);
        
    Dictionary<Object^, Object^>^ resultsDict = gcnew Dictionary<Object^, Object^>();

    for (size_t i = 0; i < count; i++) {
        JSStringRef jsNameRef = JSPropertyNameArrayGetNameAtIndex(properties, i);
            
        String^ name = JSCoreMarshal::JSStringToString(jsNameRef);
        JSValueRef propertyValue = JSObjectGetProperty(ctx, o, jsNameRef, NULL);
        
        Object^ value = nullptr;
        
        value = getObjectFromJSValueRef(ctx, nullptr, propertyValue, NULL);
        if (value->GetType() == JSObject::typeid && recursive)
        {
            value = ((JSObject^)value)->ToDictionary(recursive);
        }

        resultsDict->Add((Object^)name, value);
    }

    JSPropertyNameArrayRelease(properties);

    return resultsDict;
}