#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::Collections::Generic;

extern JSClassDefinition wrapperClass;

JSValueRef getJSValueRefFromObject(JSContextRef ctx, Object ^ object, JSValueRef * exception);

void wrapper_Finalize(JSObjectRef object);
bool wrapper_HasProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName);
JSValueRef wrapper_GetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception);
bool wrapper_SetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef value, 
    JSValueRef* exception);
void wrapper_GetPropertyNames(JSContextRef ctx, JSObjectRef object, JSPropertyNameAccumulatorRef propertyNames);
JSValueRef wrapper_CallAsFunction (JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, 
    size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception);
JSValueRef wrapper_ConvertToType(JSContextRef ctx, JSObjectRef object, JSType type, JSValueRef* exception);
