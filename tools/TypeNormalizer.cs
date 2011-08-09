/*
 * Copyright (c) 2009, Peter Nelson (charn.opcode@gmail.com)
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
*/

/* TypeNormalizer.cs -- this tool is used during the WebKit .NET build
 * process to replace references to _RemotableHandle with int32 in
 * WebKit.Interop.dll.  This is required as it appears that MIDL would prefer
 * that we marshal values of type HDC to a reference to some undocumented
 * _RemotableHandle structure, which doesn't appear to work correctly.
 * Since an HDC on Win32 is simply a (void *), we can treat it as an int32
 * here.
 *
 * TODO: Fix these to IntPtrs for 64-bit compat
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

class TypeNormalizer
{
    static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("No file specified");
            return 1;
        }

        string file = File.ReadAllText(args[0]);

        file = file
            .Replace("valuetype WebKit.Interop._RemotableHandle&", "int32")
            .Replace("[in] class WebKit.Interop.IWebURLRequest", "[in] class WebKit.Interop.WebURLRequest")
            .Replace("instance class WebKit.Interop.IWebURLRequest", "instance class WebKit.Interop.WebURLRequest");

        File.WriteAllText(args[0], file);
        return 0;
    }
}
