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
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::EvaluateScript(System::String ^ script, System::Object ^ thisObject)
{
    return nullptr;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSContext::EvaluateScript(System::String ^ script, System::Object ^ thisObject,
    System::String ^ sourceUrl, int startingLineNumber)
{
    return nullptr;
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



// Value

WebKit::JSCore::JSValue::JSValue(JSContextRef context, JSValueRef value)
: _context(context), _value(value)
{
}

WebKit::JSCore::JSValue::JSValue(JSContextRef context, String ^ string)
: _context(context)
{
    BSTR str = (BSTR) Marshal::StringToBSTR(string).ToPointer();
    JSStringRef jsStr = JSStringCreateWithCharacters((JSChar *) str, wcslen(str));
    _value = JSValueMakeString(context, jsStr);
}

WebKit::JSCore::JSValue::~JSValue()
{
}

String ^ WebKit::JSCore::JSValue::ToString()
{
    JSStringRef jsStr = JSValueToStringCopy(_context, _value, NULL);
    int len = JSStringGetLength(jsStr);
    JSChar * cStr = (JSChar *) JSStringGetCharactersPtr(jsStr);
    cStr[len] = L'\0';

    // TODO: clean up
    return Marshal::PtrToStringBSTR(IntPtr((void *) cStr));
}


// JSObject

WebKit::JSCore::JSObject::JSObject(JSContextRef context, System::Object ^object)
: WebKit::JSCore::JSValue(context, "")
{
}