#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace WebKit::Interop;

namespace WebKit {
namespace JSCore {

    ref class JSContext;
    ref class JSObject;
    ref class JSValue;


    public ref class JSContext
    {
    protected:
        JSContextRef _context;

    public:
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


    internal:
        JSContext(JSContextRef context);
    };

    
    public ref class JSValue
    {
    protected:
        JSValueRef _value;
        JSContextRef _context;

    public:
        ~JSValue();

        virtual String ^ ToString() override;

    internal:
        JSValue(JSContextRef context, JSValueRef value);
        JSValue(JSContextRef context, String ^ string);
    };


    public ref class JSObject : public JSValue
    {
    internal:
        JSObject(JSContextRef context, Object ^ object);
    };

}}