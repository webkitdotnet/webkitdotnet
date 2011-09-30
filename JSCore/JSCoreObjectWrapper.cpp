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
    wrapper_GetPropertyNames,/* JSObjectGetPropertyNamesCallback getPropertyNames; */
    wrapper_CallAsFunction,  /* JSObjectCallAsFunctionCallback callAsFunction; */
    NULL,                    /* JSObjectCallAsConstructorCallback callAsConstructor; */
    NULL,                    /* JSObjectHasInstanceCallback hasInstance; */
    NULL//wrapper_ConvertToType    /* JSObjectConvertToTypeCallback convertToType; */
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
	DelegateFunctionWrapper(JSContextRef _context, JSObjectRef _func) : context(_context), func(_func)
    {       		
		JSGlobalContextRetain((JSGlobalContextRef)context);
        nativeCallback_ = gcnew EventDelegate(this, &DelegateFunctionWrapper::Callback);
        delegateHandle_ = GCHandle::Alloc(nativeCallback_);
    }

    ~DelegateFunctionWrapper()
    {
        if (delegateHandle_.IsAllocated)
			delegateHandle_.Free();

		JSGlobalContextRelease((JSGlobalContextRef)context);	
    }

	EventDelegate^ CallbackFunction() {
		return nativeCallback_;
	}


private:
	GCHandle delegateHandle_;
    EventDelegate^ nativeCallback_;
	JSObjectRef func;	
	JSContextRef context;
	
    Object^ Callback(array<Object^>^ args)
    {							
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
		DelegateFunctionWrapper^ ni = gcnew DelegateFunctionWrapper(ctx, functionObj);
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
		JSObjectRef o = (JSObjectRef)value; //JSValueToObject(ctx, value, NULL);
		JSPropertyNameArrayRef properties = JSObjectCopyPropertyNames(ctx, o);
		size_t count = JSPropertyNameArrayGetCount(properties);
		
		Dictionary<Object^, Object^>^ resultsDict = gcnew Dictionary<Object^, Object^>();

		for (size_t i = 0; i < count; i++) {
			JSStringRef jsNameRef = JSPropertyNameArrayGetNameAtIndex(properties, i);
			
			String^ name = JSCoreMarshal::JSStringToString(jsNameRef);
			JSValueRef propertyValue = JSObjectGetProperty(ctx, o, jsNameRef, NULL);
			Object^ value = getObjectFromJSValueRef(ctx, nullptr, propertyValue, NULL);

			resultsDict->Add((Object^)name, value);
		}

		JSPropertyNameArrayRelease(properties);

		val = resultsDict;
	}
	return val;
}


JSValueRef getJSValueRefFromObject(JSContextRef ctx, Object ^ object, JSValueRef * exception)
{	
    if(object == nullptr)
    {
        return JSValueMakeUndefined(ctx);
    }

	Type ^ type = object->GetType();
    JSValueRef val;

	if (type == bool::typeid)
    {
        return JSValueMakeBoolean(ctx, (bool)object);
    }
	if (type == int::typeid) {
		int i = (int) object;
		return JSValueMakeNumber(ctx, (double)i);
	}
	if (type == float::typeid) {
		float f = (float) object;
		return JSValueMakeNumber(ctx, (double)f);
	}
	if (type == double::typeid) 
	{		
        return JSValueMakeNumber(ctx,(double)object);
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



static Dictionary<Object^,Object^> ^ TryGetDictionary(Object ^ obj)
{
	Type ^type = obj->GetType();
	if (type->IsGenericType) {
		Type ^t2 = type->GetGenericTypeDefinition();
		Type ^t3 = Dictionary<int,int>::typeid->GetGenericTypeDefinition();

		if (t2 == t3) {
			Dictionary<Object^,Object^> ^ dict = static_cast<Dictionary<Object^,Object^>^>(obj);
			return dict;
		}
	}

	return nullptr;
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
	Type ^type = obj->GetType();

	
	Dictionary<Object^,Object^> ^dict = TryGetDictionary(obj);
	if (dict != nullptr) {
		return dict->ContainsKey(propName);	
	}
	
	return type->GetProperty(propName) != nullptr ||
		   type->GetField(propName) != nullptr ||
		   type->GetMethod(propName) != nullptr;
}

JSValueRef wrapper_GetProperty(JSContextRef ctx, JSObjectRef object, JSStringRef propertyName, JSValueRef* exception)
{
    Object ^ obj = getObjectFromJSObjectRef(object);
    String ^ propName = JSCoreMarshal::JSStringToString(propertyName);
	Type ^ objType = obj -> GetType();
	
	Dictionary<Object^,Object^> ^dict = TryGetDictionary(obj);
	if (dict != nullptr) {
		Object ^ value;
		if (dict->TryGetValue(propName, value))
		{
			return getJSValueRefFromObject(ctx, value, NULL);
		}

		return nullptr;
	}

    PropertyInfo ^ prop = objType->GetProperty(propName);
    if (prop != nullptr)
    {       
        if (prop->CanRead)
        {			
			Object ^propObj = prop->GetValue(obj, nullptr);
			return getJSValueRefFromObject(ctx, propObj, NULL);
        }
        else
        {
			return NULL;            
        }
    }

	FieldInfo ^field = obj->GetType()->GetField(propName);
    if (field != nullptr) {
		Object ^fieldObj = field->GetValue(obj);
		return getJSValueRefFromObject(ctx, fieldObj, NULL);
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
	Type   ^ type = obj->GetType();

	Dictionary<Object^,Object^> ^dict = TryGetDictionary(obj);
	if (dict != nullptr) 
	{
		dict->Remove(propName);

		Object ^ val = getObjectFromJSValueRef(ctx, nullptr, value, exception);
		dict->Add(propName, val);

		return true;
	}

    PropertyInfo ^ prop = type->GetProperty(propName);
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

void wrapper_GetPropertyNames(JSContextRef ctx, JSObjectRef object, JSPropertyNameAccumulatorRef propertyNames)
{
	Object ^ obj = getObjectFromJSObjectRef(object);
	Type ^ type = obj->GetType();

	Dictionary<Object^,Object^> ^dict = TryGetDictionary(obj);
	if (dict != nullptr) {

		// If the object is a dictionary return the keys
		Dictionary<Object^,Object^>::KeyCollection^ keys = dict->Keys;
		for each( Object^ s in keys)
        {
			String^name = (String ^)s;
			JSStringRef jsString = JSCoreMarshal::StringToJSString(name);
			JSPropertyNameAccumulatorAddName(propertyNames, jsString);
			JSStringRelease(jsString);             
        }
	} else {

		// otherwise just list the public instance members
		array<MemberInfo^>^ members = type->GetMembers( static_cast<BindingFlags>(BindingFlags::Public | BindingFlags::Instance | BindingFlags::DeclaredOnly) );
		for each (MemberInfo ^member in members) {
			JSStringRef jsString = JSCoreMarshal::StringToJSString(member->Name);
			JSPropertyNameAccumulatorAddName(propertyNames, jsString);
			JSStringRelease(jsString);    
		}    
	}
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


/*JSValueRef wrapper_ConvertToType(JSContextRef ctx, JSObjectRef object, JSType type, JSValueRef* exception)
{
	if (type == kJSTypeObject) {
		String ^ output = "default output";
		
		JSStringRef str = JSCoreMarshal::StringToJSString(output);
		return JSValueMakeString(ctx, str);
	}

	return nullptr;
}*/