call "$(DevEnvDir)..\..\VC\vcvarsall.bat" x86
if not exist "$(SolutionDir)tools\TypeNormalizer.exe" csc /out:"$(SolutionDir)tools\TypeNormalizer.exe" "$(SolutionDir)tools\TypeNormalizer.cs"
tlbimp "$(SolutionDir)webkit\webkit.tlb" /silent /keyfile:"$(ProjectDir)WebKit .NET.snk" /namespace:WebKit.Interop /out:"$(SolutionDir)webkit\WebKit.Interop.dll"
ildasm "$(SolutionDir)webkit\WebKit.Interop.dll" /out="$(SolutionDir)webkit\temp_webkit_interop.il" /nobar
"$(SolutionDir)tools\TypeNormalizer.exe" "$(SolutionDir)webkit\temp_webkit_interop.il"
ilasm "$(SolutionDir)webkit\temp_webkit_interop.il" /dll /output="$(SolutionDir)webkit\WebKit.Interop.dll" /key="$(ProjectDir)WebKit .NET.snk"
del /F /Q "$(SolutionDir)webkit\temp_webkit_interop.*"