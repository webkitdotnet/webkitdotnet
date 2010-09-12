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

        JSContextRef context();
    };

    
    public ref class JSValue
    {
    protected:
        JSValueRef _value;
        JSContext ^ _context;

    public:
        ~JSValue();

        virtual String ^ ToString() override;

        // JSValueRef.h functions

        // properties
        property bool IsUndefined { bool get(); };
        property bool IsNull { bool get(); };
        property bool IsBoolean { bool get(); };
        property bool IsNumber { bool get(); };
        property bool IsString { bool get(); };
        property bool IsObject { bool get(); };

        // conversion methods
        String ^ ToJSONString();
        bool ToBoolean();
        double ToNumber();
        Object ^ ToObject();

        void Protect();
        void Unprotect();


    internal:
        JSValue(JSContext ^ context, JSValueRef value);
    };


    public ref class JSObject : public JSValue
    {
    internal:
        JSObject(JSContext ^ context, Object ^ object);
    };

}}