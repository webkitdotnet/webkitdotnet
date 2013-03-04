using System.ComponentModel;
using WebKit.Interop;

namespace WebKit
{
    /// <summary>
    /// 
    /// </summary>
    public enum CookieAcceptPolicy
    {
        Always,
        Never,
        OnlyFromMainDocumentDomain
    }

    internal static class EnumConversionExtensionMethods
    {
        internal static CookieAcceptPolicy ToCookieAcceptPolicy(this WebKitCookieStorageAcceptPolicy Policy)
        {
            switch (Policy)
            {
                case WebKitCookieStorageAcceptPolicy.WebKitCookieStorageAcceptPolicyAlways:
                    return CookieAcceptPolicy.Always;
                case WebKitCookieStorageAcceptPolicy.WebKitCookieStorageAcceptPolicyNever:
                    return CookieAcceptPolicy.Never;
                case WebKitCookieStorageAcceptPolicy.WebKitCookieStorageAcceptPolicyOnlyFromMainDocumentDomain:
                    return CookieAcceptPolicy.OnlyFromMainDocumentDomain;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        internal static WebKitCookieStorageAcceptPolicy ToWebKitCookieStorageAcceptPolicy(this CookieAcceptPolicy Policy)
        {
            switch (Policy)
            {
                case CookieAcceptPolicy.Always:
                    return WebKitCookieStorageAcceptPolicy.WebKitCookieStorageAcceptPolicyAlways;
                case CookieAcceptPolicy.Never:
                    return WebKitCookieStorageAcceptPolicy.WebKitCookieStorageAcceptPolicyNever;
                case CookieAcceptPolicy.OnlyFromMainDocumentDomain:
                    return WebKitCookieStorageAcceptPolicy.WebKitCookieStorageAcceptPolicyOnlyFromMainDocumentDomain;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
