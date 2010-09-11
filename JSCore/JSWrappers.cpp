#include "stdafx.h"

#include "JSWrappers.h"

// Context

WebKit::JSCore::Context::Context(JSContextRef context)
: _context(context)
{
}

WebKit::JSCore::Context::~Context()
{
}


// Value

WebKit::JSCore::Value::Value(JSValueRef value)
: _value(value)
{
}

WebKit::JSCore::Value::~Value()
{
}
