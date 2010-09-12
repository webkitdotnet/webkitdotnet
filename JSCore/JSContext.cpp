#include "stdafx.h"

#include "JSValue.h"
#include "JSContext.h"


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

