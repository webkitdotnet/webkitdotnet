#include "stdafx.h"

#include "JSValue.h"
#include "JSObject.h"
#include "JSCoreMarshal.h"


JSStringRef JSCoreMarshal::StringToJSString(String ^ value)
{
    if (value != nullptr)
    {
        JSChar * chars = (JSChar *) Marshal::StringToBSTR(value).ToPointer();
        JSStringRef str = JSStringCreateWithCharacters(chars, wcslen(chars));
        Marshal::FreeBSTR(IntPtr(chars));

        // TODO: find out if we should return JSStringRetain(str) instead
        return str;
    }
    else
    {
        return NULL;
    }
}

String ^ JSCoreMarshal::JSStringToString(JSStringRef string)
{
    size_t len = JSStringGetLength(string);
    JSChar * cStr = (JSChar *) JSStringGetCharactersPtr(string);

    // TODO: does this copy the string, or point to it?
    // Do we need to clean up afterwards?
    return Marshal::PtrToStringAuto(IntPtr((void *)cStr), len);
}