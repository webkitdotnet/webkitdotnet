@call "$(DevEnvDir)..\..\VC\vcvarsall.bat" x86
if not exist "tools\TypeNormalizer.exe" "C:\Windows\Microsoft.NET\Framework\v2.0.50727/csc.exe" /out:"tools\TypeNormalizer.exe" "tools\TypeNormalizer.cs"
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\tlbimp.exe" "webkit\webkit.tlb" /silent /keyfile:"WebKit.NET.snk" /namespace:WebKit.Interop /out:"webkit\WebKit.Interop.dll"
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe" "webkit\WebKit.Interop.dll" /out="webkit\temp_webkit_interop.il" /nobar
"tools\TypeNormalizer.exe" "webkit\temp_webkit_interop.il"
"C:\Windows\Microsoft.NET\Framework\v2.0.50727/ilasm.exe" "webkit\temp_webkit_interop.il" /dll /output="webkit\WebKit.Interop.dll" /key="WebKit.NET.snk"
del /F /Q "webkit\temp_webkit_interop.*"