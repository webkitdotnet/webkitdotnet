#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace WebKit::Interop;
using namespace WebKit::JSCore;

namespace WebKit {
namespace JSCore {

ref class JSValue;
ref class JSObject;
class JSCoreMarshal;

public ref class JSContext
{
protected:
    JSContextRef _context;
    bool _contextCreated;
public:
    JSContext();
    JSContext(IntPtr context);
    JSContext(WebKit::Interop::IWebFrame ^ webFrame);
    ~JSContext();

    /* JSBase.h functions */

    JSValue ^ EvaluateScript(String ^ script);
    JSValue ^ EvaluateScript(String ^ script, Object ^ thisObject);
    JSValue ^ EvaluateScript(String ^ script, Object ^ thisObject, String ^ sourceUrl, int startingLineNumber);

    bool CheckScriptSyntax(String ^ script);
    bool CheckScriptSyntax(String ^ script, Object ^ thisObject);
    bool CheckScriptSyntax(String ^ script, Object ^ thisObject, String ^ sourceUrl, int startingLineNumber);

    void GarbageCollect();

    /* JSValueRef.h functions */

    JSValue ^ MakeUndefined();
    JSValue ^ MakeNull();
    JSValue ^ MakeBoolean(bool boolean);
    JSValue ^ MakeNumber(double number);
    JSValue ^ MakeString(String ^ string);
    JSValue ^ MakeValueFromJSONString(String ^ jsonString);

    /* JSObjectRef.h functions */

    JSObject ^ MakeObject(Object ^ object);

    /* JSContextRef.h functions */

    JSObject ^ GetGlobalObject();


internal:
    JSContext(JSContextRef context);

    JSContextRef context();
};


}} // namespace WebKit::JSCore
