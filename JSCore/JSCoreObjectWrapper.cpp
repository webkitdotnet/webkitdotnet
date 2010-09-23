#include "stdafx.h"

#include "JSCoreObjectWrapper.h"
#include "JSCoreMarshal.h"


JSClassDefinition wrapperClass = 
{
    0,                       /* int version; */
    kJSClassAttributeNone,   /* JSClassAttributes attributes; */

    "wrapper",               /* const char* className; */
    NULL,                    /* JSClassRef parentClass; */
        
    NULL,                    /* const JSStaticValue* staticValues; */
    NULL,                    /* const JSStaticFunction* staticFunctions; */
    
    NULL,                    /* JSObjectInitializeCallback initialize; */
    NULL,                    /* JSObjectFinalizeCallback finalize; */
    wrapper_HasProperty,     /* JSObjectHasPropertyCallback hasProperty; */
    wrapper_GetProperty,     /* JSObjectGetPropertyCallback getProperty; */
    wrapper_SetProperty,     /* JSObjectSetPropertyCallback setProperty; */
    NULL,                    /* JSObjectDeletePropertyCallback deleteProperty; */
    NULL,                    /* JSObjectGetPropertyNamesCallback getPropertyNames; */
    wrapper_CallAsFunction   /* JSObjectCallAsFunctionCallback callAsFunction; */
                             /* JSObjectCallAsConstructorCallback callAsConstructor; */
                             /* JSObjectHasInstanceCallback hasInstance; */
                             /* JSObjectConvertToTypeCallback convertToType; */
};

Object ^ getObjectFromJSObjectRef(JSObjectRef object)
{
    void * ptr = JSObjectGetPrivate(object);
    GCHandle handle = GCHandle::FromIntPtr(IntPtr(ptr));
    return handle.Target;
}

bool wrapper_HasProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName)
{    
    Object ^ obj = getObjectFromJSObjectRef(object);

    String ^ propName = JSCoreMarshal::JSStringToString(propertyName);
    return obj->GetType()->GetProperty(propName) != nullptr;
}

JSValueRef wrapper_GetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception)
{
    Object ^ obj = getObjectFromJSObjectRef(object);

    String ^ propName = JSCoreMarshal::JSStringToString(propertyName);
    PropertyInfo ^ prop = obj->GetType()->GetProperty(propName);
    if (prop != nullptr)
    {
        // for the moment, just return as string
        if (prop->CanRead)
        {
            String ^ val = prop->GetValue(obj, nullptr)->ToString();
            JSStringRef jsStr = JSCoreMarshal::StringToJSString(val);
            return JSValueMakeString(ctx, jsStr);
        }
        else
        {
            // exception?
        }
    }

    return NULL;
}

bool wrapper_SetProperty(JSContextRef ctx, JSObjectRef object, 
                         JSStringRef propertyName, JSValueRef value, JSValueRef* exception)
{
    Object ^ obj = getObjectFromJSObjectRef(object);

    String ^ propName = JSCoreMarshal::JSStringToString(propertyName);
    PropertyInfo ^ prop = obj->GetType()->GetProperty(propName);
    if (prop != nullptr)
    {
        if (prop->CanWrite)
        {
            Type ^ type = prop->PropertyType;
            Object ^ val;
            JSValueRef exception = NULL;

            if (type == double::typeid)
            {
                val = JSValueToNumber(ctx, value, &exception);
            }
            else if (type == bool::typeid)
            {
                val = JSValueToBoolean(ctx, value);
            }
            else if (type == String::typeid)
            {
                JSStringRef temp = JSValueToStringCopy(ctx, value, &exception);
                if (!exception)
                {
                    val = JSCoreMarshal::JSStringToString(temp);
                    JSStringRelease(temp);
                }
            }

            if (!exception)
            {
                prop->SetValue(obj, val, nullptr);
                return true;
            }
        }
        else
        {
            // exception?
        }
    }

    return false;
}

JSValueRef wrapper_CallAsFunction (JSContextRef ctx, JSObjectRef function, JSObjectRef thisObject, 
                                   size_t argumentCount, const JSValueRef arguments[], JSValueRef* exception)
{
    return NULL;
}