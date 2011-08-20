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
    wrapper_Finalize,        /* JSObjectFinalizeCallback finalize; */
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

ref class CallbackClass
{
public:
	CallbackClass(JSContextRef ctx, JSObjectRef jsFunction) : ctx_(ctx), jsFunction_(jsFunction)
	{
	}   
	
	void callbackFunction(Object^ object)
	{
		if (object == NULL) {
			JSObjectCallAsFunction(ctx_, jsFunction_, NULL, 0, NULL, NULL);	   
		} else {
			JSValueRef arg = getJSValueRefFromObject(ctx_, object, NULL);
			JSValueRef arguments[] = {arg};
			JSObjectCallAsFunction(ctx_, jsFunction_, NULL, 1, arguments, NULL);	   
		}
   }

private:
	JSObjectRef jsFunction_;
	JSContextRef ctx_;
};

GCHandle getHandleFromJSObjectRef(JSObjectRef object)
{
	void * ptr = JSObjectGetPrivate(object);
	return GCHandle::FromIntPtr(IntPtr(ptr));
}


Object ^ getObjectFromJSValueRef(JSContextRef ctx, Type ^ type, JSValueRef value, JSValueRef * exception)
{
	Object ^ val;

    if (type == double::typeid)
    {
        val = JSValueToNumber(ctx, value, exception);
    }
    else if (type == bool::typeid)
    {
        val = JSValueToBoolean(ctx, value);
    }
    else if (type == String::typeid)
    {
        JSStringRef temp = JSValueToStringCopy(ctx, value, exception);
        if (!*exception)
        {
            val = JSCoreMarshal::JSStringToString(temp);
            JSStringRelease(temp);
        }
    }
	else if (JSObjectIsFunction(ctx, (JSObjectRef) value)) {	
		// FIXME: Only supports one argument, void functions
		CallbackClass^ cls = gcnew CallbackClass(ctx, JSValueToObject(ctx, value, NULL));
		callbackFunction^ myCallback = gcnew callbackFunction(cls, &CallbackClass::callbackFunction);
		val = myCallback;		
	}
	return val;
}


JSValueRef getJSValueRefFromObject(JSContextRef ctx, Object ^ object, JSValueRef * exception)
{
    Type ^ type = object->GetType();
    JSValueRef val;
    if(object == nullptr)
    {
        return JSValueMakeUndefined(ctx);
    }
    if(type == double::typeid)
    {
        return JSValueMakeNumber(ctx, (double)object);
    }
    if(type == bool::typeid)
    {
        return JSValueMakeBoolean(ctx, (bool)object);
    }
    if(type == String::typeid)
    {
        JSStringRef temp = JSCoreMarshal::StringToJSString((String ^)object);
        val = JSValueMakeString(ctx, temp);
        JSStringRelease(temp);
        return val;
    }
    else
    {
        JSClassRef wrap = JSClassCreate(&wrapperClass);

        GCHandle handle = GCHandle::Alloc(object, GCHandleType::Normal);
        void * ptr = GCHandle::ToIntPtr(handle).ToPointer();

        val = JSObjectMake(ctx, wrap, ptr);

        JSClassRelease(wrap);
        return val;
    }
}


Object ^ getObjectFromJSObjectRef(JSObjectRef object)
{
    GCHandle handle = getHandleFromJSObjectRef(object);
    return handle.Target;
}

void wrapper_Finalize(JSObjectRef object)
{
	GCHandle handle = getHandleFromJSObjectRef(object);
	handle.Free();
}

bool wrapper_HasProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName)
{
    Object ^ obj = getObjectFromJSObjectRef(object);

    String ^ propName = JSCoreMarshal::JSStringToString(propertyName);
    MethodInfo ^ method = obj->GetType()->GetMethod(propName);
    return obj->GetType()->GetProperty(propName) != nullptr || obj->GetType()->GetMethod(propName) != nullptr;
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

	MethodInfo ^ method = obj->GetType()->GetMethod(propName);
	if(method != nullptr)
	{
		return JSObjectMakeFunctionWithCallback(ctx, propertyName, wrapper_CallAsFunction);
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
            Object ^ val = getObjectFromJSValueRef(ctx, prop->PropertyType, value, exception);

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
    Object ^ obj = getObjectFromJSObjectRef(thisObject);

    JSStringRef nameProperty = JSCoreMarshal::StringToJSString("name");
    JSValueRef val = JSObjectGetProperty(ctx, function, nameProperty, exception);
    JSStringRelease(nameProperty);

    if(*exception)
    {
        return NULL;
    }

    JSStringRef functionName = JSValueToStringCopy(ctx, val, exception);
    if(*exception)
    {
        return NULL;
    }

    String ^ methName = JSCoreMarshal::JSStringToString(functionName);
    JSStringRelease(functionName);

    MethodInfo ^ method = obj->GetType()->GetMethod(methName);
    cli::array<ParameterInfo ^,1> ^ parameters = method->GetParameters();

    if(parameters->Length != argumentCount)
    {
        *exception = getJSValueRefFromObject(ctx, String::Format("{0}.{1} called with wrong argument number.", obj->GetType()->Name, methName), exception);
        return JSValueMakeUndefined(ctx);
    }

    cli::array<Object ^, 1> ^ args = gcnew cli::array<Object ^, 1>(parameters->Length);
    for(int i = 0; i < argumentCount; i++)
    {
        ParameterInfo ^ parameter = (ParameterInfo ^)parameters->GetValue(i);
        Object ^ val = getObjectFromJSValueRef(ctx, parameter->ParameterType, arguments[i], exception);
        args->SetValue(val, i);
    }

    Object ^ ret = method->Invoke(obj, args);
	if (!ret)
	{
		return NULL;
	}

    JSValueRef jsVal = getJSValueRefFromObject(ctx, ret, exception);
    return jsVal;
}
