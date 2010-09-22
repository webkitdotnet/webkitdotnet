#include "stdafx.h"

#include "JSCoreObjectWrapper.h"


JSClassDefinition wrapperClass = 
{
    0,                     /* int version; */
    kJSClassAttributeNone, /* JSClassAttributes attributes; */

    "wrapper",             /* const char* className; */
    NULL,                  /* JSClassRef parentClass; */
        
    NULL,                  /* const JSStaticValue* staticValues; */
    NULL,                  /* const JSStaticFunction* staticFunctions; */
    
    NULL,                  /* JSObjectInitializeCallback initialize; */
    NULL,                  /* JSObjectFinalizeCallback finalize; */
    wrapper_HasProperty,   /* JSObjectHasPropertyCallback hasProperty; */
    wrapper_GetProperty,   /* JSObjectGetPropertyCallback getProperty; */
    wrapper_SetProperty    /* JSObjectSetPropertyCallback setProperty; */
                           /* JSObjectDeletePropertyCallback deleteProperty; */
                           /* JSObjectGetPropertyNamesCallback getPropertyNames; */
                           /* JSObjectCallAsFunctionCallback callAsFunction; */
                           /* JSObjectCallAsConstructorCallback callAsConstructor; */
                           /* JSObjectHasInstanceCallback hasInstance; */
                           /* JSObjectConvertToTypeCallback convertToType; */
};

bool wrapper_HasProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName)
{    
    void * ptr = JSObjectGetPrivate(object);
    GCHandle handle = GCHandle::FromIntPtr(IntPtr(ptr));
    Object ^ obj = handle.Target;

    // TODO: reflection magicks

    return false;
}

JSValueRef wrapper_GetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception)
{
    void * ptr = JSObjectGetPrivate(object);
    GCHandle handle = GCHandle::FromIntPtr(IntPtr(ptr));
    Object ^ obj = handle.Target;

    // TODO: reflection magicks

    return NULL;
}

bool wrapper_SetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef value, JSValueRef* exception)
{
    return false;
}
