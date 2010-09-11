#pragma once

using namespace System;

namespace WebKit {
namespace JSCore {

    public ref class Context
    {
    protected:
        ::JSContextRef _context;

    public:
        ~Context();

    internal:
        Context(::JSContextRef context);
    };

    public ref class Value
    {
    protected:
        ::JSValueRef _value;

    public:
        ~Value();

    internal:
        Value(::JSValueRef value);
    };

}}