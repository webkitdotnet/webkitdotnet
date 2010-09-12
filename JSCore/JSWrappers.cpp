#include "stdafx.h"

#include "JSWrappers.h"

// Context

WebKit::JSCore::JSContext::JSContext(JSContextRef context)
: _context(context)
{
}

WebKit::JSCore::JSContext::JSContext(WebKit::Interop::IWebFrame ^ webFrame)
{
    ::IWebFrame * unmgdFrame = (::IWebFrame *) Marshal::GetComInterfaceForObject(webFrame, 
        WebKit::Interop::IWebFrame::typeid).ToPointer();

    _context = unmgdFrame->globalContext();    
}

WebKit::JSCore::JSContext::~JSContext()
{
    // TODO: clean up
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::EvaluateScript(System::String ^ script)
{
    return EvaluateScript(script, nullptr, nullptr, 0);
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::EvaluateScript(System::String ^ script, System::Object ^ thisObject)
{
    return EvaluateScript(script, thisObject, nullptr, 0);
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::EvaluateScript(System::String ^ script, System::Object ^ thisObject,
    System::String ^ sourceUrl, int startingLineNumber)
{    
    // TODO: lets not worry about exceptions just yet...
    
    JSStringRef jsScript = NULL;
    if (script != nullptr)
    {
        BSTR unmgdScript = (BSTR) Marshal::StringToBSTR(script).ToPointer();
        jsScript = JSStringCreateWithCharacters((const JSChar *)unmgdScript, wcslen(unmgdScript));
        Marshal::FreeBSTR(IntPtr(unmgdScript));
    }

    // TODO: marshal thisObject to JSObject
    JSObjectRef jsObj = NULL;
    
    JSStringRef jsSrc = NULL;
    if (sourceUrl != nullptr)
    {
        BSTR unmgdSrc = (BSTR) Marshal::StringToBSTR(sourceUrl).ToPointer();
        jsSrc = JSStringCreateWithCharacters((const JSChar *)unmgdSrc, wcslen(unmgdSrc));
        Marshal::FreeBSTR(IntPtr(unmgdSrc));
    }

    JSValueRef result = JSEvaluateScript(_context, jsScript, jsObj, jsSrc, 0, NULL);
    JSValue ^ retval = result != NULL ? gcnew JSValue(this, result) : nullptr;

    // clean up
    if (jsScript != NULL)
        JSStringRelease(jsScript);
    if (jsSrc != NULL)
        JSStringRelease(jsSrc);

    return retval;
}

bool WebKit::JSCore::JSContext::CheckScriptSyntax(System::String ^ script)
{
    return false;
}

bool WebKit::JSCore::JSContext::CheckScriptSyntax(System::String ^ script, System::Object ^ thisObject)
{
    return false;
}

bool WebKit::JSCore::JSContext::CheckScriptSyntax(System::String ^script, System::Object ^ thisObject, 
    System::String ^sourceUrl, int startingLineNumber)
{
    return false;
}

void WebKit::JSCore::JSContext::GarbageCollect()
{
    JSGarbageCollect(_context);
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::MakeUndefined()
{
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::MakeNull()
{
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::MakeBoolean(bool boolean)
{
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::MakeNumber(double number)
{
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::MakeString(System::String ^ string)
{
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::MakeValueFromJSONString(System::String ^ jsonString)
{
    return nullptr;
}

WebKit::JSCore::JSObject ^ WebKit::JSCore::JSContext::MakeObject(System::Object ^ object)
{
    return nullptr;
}

JSContextRef WebKit::JSCore::JSContext::context()
{
    return _context;
}



// Value

WebKit::JSCore::JSValue::JSValue(JSContext ^ context, JSValueRef value)
: _context(context), _value(value)
{
}

WebKit::JSCore::JSValue::~JSValue()
{
}

String ^ WebKit::JSCore::JSValue::ToString()
{
    JSStringRef jsStr = JSValueToStringCopy(_context->context(), _value, NULL);
    int len = JSStringGetLength(jsStr);
    JSChar * cStr = (JSChar *) JSStringGetCharactersPtr(jsStr);
    cStr[len] = L'\0';

    // TODO: clean up
    return Marshal::PtrToStringAuto(IntPtr((void *) cStr));
}

bool WebKit::JSCore::JSValue::IsBoolean::get()
{
    return JSValueIsBoolean(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsNull::get()
{
    return JSValueIsNull(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsNumber::get()
{
    return JSValueIsNumber(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsObject::get()
{
    return JSValueIsObject(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsString::get()
{
    return JSValueIsString(_context->context(), _value);
}

bool WebKit::JSCore::JSValue::IsUndefined::get()
{
    return JSValueIsUndefined(_context->context(), _value);
}

System::String ^ WebKit::JSCore::JSValue::ToJSONString()
{
    return "";
}

bool WebKit::JSCore::JSValue::ToBoolean()
{
    return JSValueToBoolean(_context->context(), _value);
}

double WebKit::JSCore::JSValue::ToNumber()
{
    return JSValueToNumber(_context->context(), _value, NULL);
}

System::Object ^ WebKit::JSCore::JSValue::ToObject()
{
    return nullptr;
}

void WebKit::JSCore::JSValue::Protect()
{
}

void WebKit::JSCore::JSValue::Unprotect()
{
}




// JSObject

WebKit::JSCore::JSObject::JSObject(JSContext ^ context, System::Object ^object)
: WebKit::JSCore::JSValue(context, NULL)
{
}
