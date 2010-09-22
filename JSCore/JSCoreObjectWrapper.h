#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

extern JSClassDefinition wrapperClass;

bool wrapper_HasProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName);
JSValueRef wrapper_GetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception);
bool wrapper_SetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef value, JSValueRef* exception);
