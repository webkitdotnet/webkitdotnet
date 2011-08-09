#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;

extern JSClassDefinition wrapperClass;
delegate void callbackFunction(Object^ object);

JSValueRef getJSValueRefFromObject(JSContextRef ctx, Object ^ object, JSValueRef * exception);

void wrapper_Finalize(JSObjectRef object);
bool wrapper_HasProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName);
JSValueRef wrapper_GetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception);
bool wrapper_SetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef value, 
    JSValueRef* exception);
JSValueRef wrapper_CallAsFunction (JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, 
    size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception);
