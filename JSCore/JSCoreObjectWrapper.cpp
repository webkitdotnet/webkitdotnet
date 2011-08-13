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

Object ^ getObjectFromJSValueRef(JSContextRef ctx, Type ^ type, JSValueRef value, JSValueRef * exception);

GCHandle getHandleFromJSObjectRef(JSObjectRef object)
{
	void * ptr = JSObjectGetPrivate(object);
	return GCHandle::FromIntPtr(IntPtr(ptr));
}

public delegate Object^ EventDelegate(array<Object^>^ args);

ref class DelegateFunctionWrapper
{
public:
	DelegateFunctionWrapper(JSObjectRef _func) : func(_func)
    {       
        nativeCallback_ = gcnew EventDelegate(this, &DelegateFunctionWrapper::Callback);
        delegateHandle_ = GCHandle::Alloc(nativeCallback_);
    }

    ~DelegateFunctionWrapper()
    {
        if (delegateHandle_.IsAllocated)
			delegateHandle_.Free();
    }

	EventDelegate^ CallbackFunction() {
		return nativeCallback_;
	}


private:
	GCHandle delegateHandle_;
    EventDelegate^ nativeCallback_;
	JSObjectRef func;
	
    Object^ Callback(array<Object^>^ args)
    {							
		JSContextRef context = JSGlobalContextCreate(NULL);

		JSValueRef *arguments = new JSValueRef[args->Length];
		for(int i = 0; i < args->Length; i++)
		{
			arguments[i] = getJSValueRefFromObject(context, args[i], NULL);
		}

		JSValueRef ret = JSObjectCallAsFunction(context, func, NULL, args->Length, arguments, NULL);	   
		Object^ retObj = nullptr;
		if (ret) {
			Type^ t = nullptr;
			retObj = getObjectFromJSValueRef(context, t, ret, NULL);			
		}

		JSGlobalContextRelease((JSGlobalContextRef) context);		

		return retObj;
    }
};

bool JSValueIsArray(JSContextRef ctx, JSValueRef value) {

	JSObjectRef global = JSContextGetGlobalObject(ctx);
	JSValueRef arrayVal = JSObjectGetProperty(ctx, global, JSCoreMarshal::StringToJSString("Array"), NULL);

	if (JSValueIsObject(ctx, arrayVal)) {
		JSObjectRef arrayObj = JSValueToObject(ctx, arrayVal, NULL);
		if (JSObjectIsFunction(ctx, arrayObj) || JSObjectIsConstructor(ctx, arrayObj)) {
			return JSValueIsInstanceOfConstructor(ctx, value, arrayObj, NULL);
		}
	}
	
	return false;
};

Object ^ getObjectFromJSValueRef(JSContextRef ctx, Type ^ type, JSValueRef value, JSValueRef * exception)
{
	Object ^ val;
	if (type == nullptr) {
		type = JSValueIsString(ctx, value) ? String::typeid :
			JSValueIsBoolean(ctx, value) ? bool::typeid :
			JSValueIsNumber(ctx, value) ? double::typeid : nullptr;
	}

	if (type == int::typeid)
	{
		val = (int)JSValueToNumber(ctx, value, exception);
	}
	else if (type == float::typeid)
	{
		val = (float)JSValueToNumber(ctx, value, exception);
	}
	else if (type == double::typeid)
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
        val = JSCoreMarshal::JSStringToString(temp);
        JSStringRelease(temp);
    }
	else if (JSObjectIsFunction(ctx, (JSObjectRef) value))
	{	
		// Create a delegate wrapper around function
		JSObjectRef functionObj = JSValueToObject(ctx, value, exception);
		DelegateFunctionWrapper^ ni = gcnew DelegateFunctionWrapper(functionObj);				
		val = ni->CallbackFunction();
	}
	else if (JSValueIsArray(ctx, value)) 
	{
		JSObjectRef o = JSValueToObject(ctx, value, NULL);	
		JSPropertyNameArrayRef properties = JSObjectCopyPropertyNames(ctx, o);
		size_t count =  JSPropertyNameArrayGetCount(properties);
		JSPropertyNameArrayRelease(properties);
		
		array<Object^>^ results = gcnew array<Object^>(count);

		for (size_t i = 0; i < count; i++) {						
			JSValueRef propertyValue = JSObjectGetPropertyAtIndex(ctx, o, i, NULL);
			Object^ value = getObjectFromJSValueRef(ctx, nullptr, propertyValue, NULL);

			results->SetValue(value, (int) i);
		}

		val = results;
	} 
	else if (JSValueIsObject(ctx, value))
	{
		JSObjectRef o = JSValueToObject(ctx, value, NULL);
		JSPropertyNameArrayRef properties = JSObjectCopyPropertyNames(ctx, o);
		size_t count = JSPropertyNameArrayGetCount(properties);
		
		System::Collections::Generic::Dictionary<Object^, Object^>^ results = gcnew System::Collections::Generic::Dictionary<Object^, Object^>();

		for (size_t i = 0; i < count; i++) {
			JSStringRef jsNameRef = JSPropertyNameArrayGetNameAtIndex(properties, i);
			
			String^ name = JSCoreMarshal::JSStringToString(jsNameRef);
			JSValueRef propertyValue = JSObjectGetProperty(ctx, o, jsNameRef, NULL);
			Object^ value = getObjectFromJSValueRef(ctx, nullptr, propertyValue, NULL);

			results->Add((Object^)name, value);
		}

		JSPropertyNameArrayRelease(properties);

		val = results;
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
	if (type == float::typeid)
	{
		float floatVal = (float)object;
		return JSValueMakeNumber(ctx, (double)floatVal);
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
	if (type->IsArray) {
		Array ^arr = (Array^)object;
		JSValueRef *arguments = new JSValueRef[arr->Length];
		for(int i = 0; i < arr->Length; i++)
		{
			arguments[i] = getJSValueRefFromObject(ctx, arr->GetValue(i), NULL);
		}
		val = JSObjectMakeArray(ctx, arr->Length, arguments, NULL);
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
    for(size_t i = 0; i < argumentCount; i++)
    {
        ParameterInfo ^ parameter = (ParameterInfo ^)parameters->GetValue((int)i);
        Object ^ val = getObjectFromJSValueRef(ctx, parameter->ParameterType, arguments[i], exception);
        args->SetValue(val, (int)i);
    }

    Object ^ ret = method->Invoke(obj, args);
	if (!ret)
	{
		return NULL;
	}

    JSValueRef jsVal = getJSValueRefFromObject(ctx, ret, exception);
    return jsVal;
}
