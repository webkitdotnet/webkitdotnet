#include "stdafx.h"

#include "JSValue.h"
#include "JSObject.h"


WebKit::JSCore::JSObject::JSObject(JSContext ^ context, System::Object ^object)
: WebKit::JSCore::JSValue(context, NULL)
{
}

bool WebKit::JSCore::JSObject::HasProperty(System::String ^propertyName)
{
    return false;
}

WebKit::JSCore::JSValue ^ WebKit::JSCore::JSObject::GetProperty(System::String ^ propertyName)
{
    return nullptr;
}

void WebKit::JSCore::JSObject::SetProperty(System::String ^ propertyName, int value)
{

}
