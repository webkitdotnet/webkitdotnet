#include "stdafx.h"

#include "JSCoreMarshal.h"
#include "JSValue.h"
#include "JSObject.h"
#include "JSContext.h"

JSContext::JSContext()
{
    _context = JSGlobalContextCreate(NULL);
    _contextCreated = true;
}

JSContext::JSContext(JSContextRef context)
: _context(context)
{
}

JSContext::JSContext(IntPtr context)
: _context((JSContextRef) context.ToPointer())
{
}

JSContext::JSContext(WebKit::Interop::IWebFrame ^ webFrame)
{
    ::IWebFrame * unmgdFrame = (::IWebFrame *) Marshal::GetComInterfaceForObject(webFrame, 
        WebKit::Interop::IWebFrame::typeid).ToPointer();

    _context = unmgdFrame->globalContext();
}

JSContext::~JSContext()
{
    if(_contextCreated)
    {
        JSGlobalContextRelease((JSGlobalContextRef)_context);
    }
    // TODO: clean up
}

JSValue ^ JSContext::EvaluateScript(String ^ script)
{
    return EvaluateScript(script, nullptr, nullptr, 0);
}

JSValue ^ JSContext::EvaluateScript(String ^ script, Object ^ thisObject)
{
    return EvaluateScript(script, thisObject, nullptr, 0);
}

JSValue ^ JSContext::EvaluateScript(String ^ script, Object ^ thisObject,
    String ^ sourceUrl, int startingLineNumber)
{
    // TODO: lets not worry about exceptions just yet...

    JSStringRef jsScript = JSCoreMarshal::StringToJSString(script);

    // TODO: marshal thisObject to JSObject
    JSObjectRef jsObj = NULL;

    // TODO: handle nulls and stuff
    JSStringRef jsSrc = JSCoreMarshal::StringToJSString(sourceUrl);

    JSValueRef exception = NULL;
    
    JSValueRef result = JSEvaluateScript(_context, jsScript, jsObj, jsSrc, 0, &exception);
    JSValue ^ retval = result != NULL ? gcnew JSValue(this, result) : nullptr;

    // clean up
    if (jsScript != NULL)
        JSStringRelease(jsScript);
    if (jsSrc != NULL)
        JSStringRelease(jsSrc);

    return retval;
}

bool JSContext::CheckScriptSyntax(String ^ script)
{
    return false;
}

bool JSContext::CheckScriptSyntax(String ^ script, Object ^ thisObject)
{
    return false;
}

bool JSContext::CheckScriptSyntax(String ^ script, Object ^ thisObject, 
    String ^ sourceUrl, int startingLineNumber)
{
    return false;
}

void JSContext::GarbageCollect()
{
    JSGarbageCollect(_context);
}

JSValue ^ JSContext::MakeUndefined()
{
    return nullptr;
}

JSValue ^ JSContext::MakeNull()
{
    return nullptr;
}

JSValue ^ JSContext::MakeBoolean(bool boolean)
{
    return nullptr;
}

JSValue ^ JSContext::MakeNumber(double number)
{
    return nullptr;
}

JSValue ^ JSContext::MakeString(String ^ string)
{
    return nullptr;
}

JSValue ^ JSContext::MakeValueFromJSONString(String ^ jsonString)
{
    return nullptr;
}

JSObject ^ JSContext::MakeObject(Object ^ object)
{
    return nullptr;
}

JSContextRef JSContext::context()
{
    return _context;
}

JSObject ^ JSContext::GetGlobalObject()
{
    return gcnew JSObject(this, ::JSContextGetGlobalObject(_context));
}
