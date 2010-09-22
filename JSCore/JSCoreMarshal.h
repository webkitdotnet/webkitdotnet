#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace WebKit::JSCore;

namespace WebKit {
namespace JSCore {
    
    class JSCoreMarshal
    {
    private:
        
        JSCoreMarshal() {}

    public:

        static JSStringRef StringToJSString(String ^ value);
        static String ^ JSStringToString(JSStringRef string);
    
    };

}} // namespace JSCore / namespace JSCore