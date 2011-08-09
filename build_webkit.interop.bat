@rem Run this whenever the WebKit binaries are updated.
call "%VS90COMNTOOLS%..\..\VC\vcvarsall.bat" x86
if not exist "tools\TypeNormalizer.exe" csc /out:"tools\TypeNormalizer.exe" "tools\TypeNormalizer.cs"
tlbimp "webkit\webkit.tlb" /silent /keyfile:"WebKit.NET.snk" /namespace:WebKit.Interop /out:"webkit\WebKit.Interop.dll"
ildasm "webkit\WebKit.Interop.dll" /out="webkit\temp_webkit_interop.il" /nobar
"tools\TypeNormalizer.exe" "webkit\temp_webkit_interop.il"
ilasm "webkit\temp_webkit_interop.il" /dll /output="webkit\WebKit.Interop.dll" /key="WebKit.NET.snk"
del /F /Q "webkit\temp_webkit_interop.*"