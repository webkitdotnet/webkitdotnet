

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 7.00.0500 */
/* at Fri Sep 10 13:28:35 2010
 */
/* Compiler settings for ..\Interfaces\IWebDesktopNotificationsDelegate.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __IWebDesktopNotificationsDelegate_h__
#define __IWebDesktopNotificationsDelegate_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IWebDesktopNotification_FWD_DEFINED__
#define __IWebDesktopNotification_FWD_DEFINED__
typedef interface IWebDesktopNotification IWebDesktopNotification;
#endif 	/* __IWebDesktopNotification_FWD_DEFINED__ */


#ifndef __IWebDesktopNotificationsDelegate_FWD_DEFINED__
#define __IWebDesktopNotificationsDelegate_FWD_DEFINED__
typedef interface IWebDesktopNotificationsDelegate IWebDesktopNotificationsDelegate;
#endif 	/* __IWebDesktopNotificationsDelegate_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IWebDesktopNotification_INTERFACE_DEFINED__
#define __IWebDesktopNotification_INTERFACE_DEFINED__

/* interface IWebDesktopNotification */
/* [unique][uuid][oleautomation][object] */ 


EXTERN_C const IID IID_IWebDesktopNotification;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("0A0AAFA8-C698-4cff-BD28-39614622EEA4")
    IWebDesktopNotification : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE isHTML( 
            /* [retval][out] */ BOOL *result) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE contentsURL( 
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE iconURL( 
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE title( 
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE text( 
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE notifyDisplay( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE notifyError( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE notifyClose( 
            /* [in] */ BOOL xplicit) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IWebDesktopNotificationVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IWebDesktopNotification * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IWebDesktopNotification * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IWebDesktopNotification * This);
        
        HRESULT ( STDMETHODCALLTYPE *isHTML )( 
            IWebDesktopNotification * This,
            /* [retval][out] */ BOOL *result);
        
        HRESULT ( STDMETHODCALLTYPE *contentsURL )( 
            IWebDesktopNotification * This,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( STDMETHODCALLTYPE *iconURL )( 
            IWebDesktopNotification * This,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( STDMETHODCALLTYPE *title )( 
            IWebDesktopNotification * This,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( STDMETHODCALLTYPE *text )( 
            IWebDesktopNotification * This,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( STDMETHODCALLTYPE *notifyDisplay )( 
            IWebDesktopNotification * This);
        
        HRESULT ( STDMETHODCALLTYPE *notifyError )( 
            IWebDesktopNotification * This);
        
        HRESULT ( STDMETHODCALLTYPE *notifyClose )( 
            IWebDesktopNotification * This,
            /* [in] */ BOOL xplicit);
        
        END_INTERFACE
    } IWebDesktopNotificationVtbl;

    interface IWebDesktopNotification
    {
        CONST_VTBL struct IWebDesktopNotificationVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IWebDesktopNotification_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IWebDesktopNotification_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IWebDesktopNotification_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IWebDesktopNotification_isHTML(This,result)	\
    ( (This)->lpVtbl -> isHTML(This,result) ) 

#define IWebDesktopNotification_contentsURL(This,result)	\
    ( (This)->lpVtbl -> contentsURL(This,result) ) 

#define IWebDesktopNotification_iconURL(This,result)	\
    ( (This)->lpVtbl -> iconURL(This,result) ) 

#define IWebDesktopNotification_title(This,result)	\
    ( (This)->lpVtbl -> title(This,result) ) 

#define IWebDesktopNotification_text(This,result)	\
    ( (This)->lpVtbl -> text(This,result) ) 

#define IWebDesktopNotification_notifyDisplay(This)	\
    ( (This)->lpVtbl -> notifyDisplay(This) ) 

#define IWebDesktopNotification_notifyError(This)	\
    ( (This)->lpVtbl -> notifyError(This) ) 

#define IWebDesktopNotification_notifyClose(This,xplicit)	\
    ( (This)->lpVtbl -> notifyClose(This,xplicit) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IWebDesktopNotification_INTERFACE_DEFINED__ */


#ifndef __IWebDesktopNotificationsDelegate_INTERFACE_DEFINED__
#define __IWebDesktopNotificationsDelegate_INTERFACE_DEFINED__

/* interface IWebDesktopNotificationsDelegate */
/* [unique][uuid][oleautomation][object] */ 


EXTERN_C const IID IID_IWebDesktopNotificationsDelegate;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("09DA073E-38B3-466a-9828-B2915FDD2ECB")
    IWebDesktopNotificationsDelegate : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE showDesktopNotification( 
            /* [in] */ IWebDesktopNotification *notification) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE cancelDesktopNotification( 
            /* [in] */ IWebDesktopNotification *notification) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE notificationDestroyed( 
            /* [in] */ IWebDesktopNotification *notification) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE checkNotificationPermission( 
            /* [in] */ BSTR origin,
            /* [retval][out] */ int *result) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE requestNotificationPermission( 
            /* [in] */ BSTR origin) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IWebDesktopNotificationsDelegateVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IWebDesktopNotificationsDelegate * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IWebDesktopNotificationsDelegate * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IWebDesktopNotificationsDelegate * This);
        
        HRESULT ( STDMETHODCALLTYPE *showDesktopNotification )( 
            IWebDesktopNotificationsDelegate * This,
            /* [in] */ IWebDesktopNotification *notification);
        
        HRESULT ( STDMETHODCALLTYPE *cancelDesktopNotification )( 
            IWebDesktopNotificationsDelegate * This,
            /* [in] */ IWebDesktopNotification *notification);
        
        HRESULT ( STDMETHODCALLTYPE *notificationDestroyed )( 
            IWebDesktopNotificationsDelegate * This,
            /* [in] */ IWebDesktopNotification *notification);
        
        HRESULT ( STDMETHODCALLTYPE *checkNotificationPermission )( 
            IWebDesktopNotificationsDelegate * This,
            /* [in] */ BSTR origin,
            /* [retval][out] */ int *result);
        
        HRESULT ( STDMETHODCALLTYPE *requestNotificationPermission )( 
            IWebDesktopNotificationsDelegate * This,
            /* [in] */ BSTR origin);
        
        END_INTERFACE
    } IWebDesktopNotificationsDelegateVtbl;

    interface IWebDesktopNotificationsDelegate
    {
        CONST_VTBL struct IWebDesktopNotificationsDelegateVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IWebDesktopNotificationsDelegate_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IWebDesktopNotificationsDelegate_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IWebDesktopNotificationsDelegate_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IWebDesktopNotificationsDelegate_showDesktopNotification(This,notification)	\
    ( (This)->lpVtbl -> showDesktopNotification(This,notification) ) 

#define IWebDesktopNotificationsDelegate_cancelDesktopNotification(This,notification)	\
    ( (This)->lpVtbl -> cancelDesktopNotification(This,notification) ) 

#define IWebDesktopNotificationsDelegate_notificationDestroyed(This,notification)	\
    ( (This)->lpVtbl -> notificationDestroyed(This,notification) ) 

#define IWebDesktopNotificationsDelegate_checkNotificationPermission(This,origin,result)	\
    ( (This)->lpVtbl -> checkNotificationPermission(This,origin,result) ) 

#define IWebDesktopNotificationsDelegate_requestNotificationPermission(This,origin)	\
    ( (This)->lpVtbl -> requestNotificationPermission(This,origin) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IWebDesktopNotificationsDelegate_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


