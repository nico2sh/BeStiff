// Minimal replacement for MonoGame's fxccs helper.
// Invoked by MGFXC inside Wine as:
//   dotnet c:\fxccs.dll <src> <entry> <profile> <flags...> <displayPath> <dst>
// Compiles the HLSL in <src> with d3dcompiler_47 and writes raw bytecode to <dst>.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

static class Program
{
	[DllImport("d3dcompiler_47.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
	private static extern int D3DCompile(
		byte[] srcData, IntPtr srcDataSize, string sourceName, IntPtr defines, IntPtr include,
		string entryPoint, string target, uint flags1, uint flags2, out IntPtr code, out IntPtr errorMsgs);

	// ID3DBlob vtable: 0 QueryInterface, 1 AddRef, 2 Release, 3 GetBufferPointer, 4 GetBufferSize.
	private static unsafe IntPtr VCall(IntPtr obj, int slot)
	{
		IntPtr* vtable = *(IntPtr**)obj;
		return ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)vtable[slot])(obj);
	}

	private static byte[] BlobBytes(IntPtr blob)
	{
		int size = (int)VCall(blob, 4);
		byte[] data = new byte[size];
		Marshal.Copy(VCall(blob, 3), data, 0, size);
		return data;
	}

	private static unsafe void BlobRelease(IntPtr blob)
	{
		IntPtr* vtable = *(IntPtr**)blob;
		((delegate* unmanaged[Stdcall]<IntPtr, uint>)vtable[2])(blob);
	}

	// D3DCOMPILE_* flag values, keyed by the SharpDX ShaderFlags names MGFXC may pass.
	private static readonly Dictionary<string, uint> FlagNames = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
	{
		{ "None", 0 }, { "Debug", 1 }, { "SkipValidation", 2 }, { "SkipOptimization", 4 },
		{ "PackMatrixRowMajor", 8 }, { "PackMatrixColumnMajor", 16 }, { "PartialPrecision", 32 },
		{ "ForceVSSoftwareNoOpt", 64 }, { "ForcePSSoftwareNoOpt", 128 }, { "NoPreshader", 256 },
		{ "AvoidFlowControl", 512 }, { "PreferFlowControl", 1024 }, { "EnableStrictness", 2048 },
		{ "EnableBackwardsCompatibility", 4096 }, { "IeeeStrictness", 8192 },
		{ "OptimizationLevel0", 16384 }, { "OptimizationLevel1", 0 }, { "OptimizationLevel2", 49152 },
		{ "OptimizationLevel3", 32768 }, { "WarningsAreErrors", 262144 },
	};

	static int Main(string[] args)
	{
		if (args.Length < 5)
		{
			Console.Error.WriteLine("usage: fxccs <src> <entry> <profile> <flags> <displayPath> <dst>");
			return 2;
		}
		string src = args[0];
		string entry = args[1];
		string profile = args[2];
		string dst = args[args.Length - 1];
		string displayPath = args.Length >= 6 ? args[args.Length - 2] : src;
		uint flags = 0;
		int flagsEnd = args.Length >= 6 ? args.Length - 2 : args.Length - 1;
		for (int i = 3; i < flagsEnd; i++)
		{
			foreach (string token in args[i].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (uint.TryParse(token, out uint numeric))
				{
					flags |= numeric;
				}
				else if (FlagNames.TryGetValue(token, out uint named))
				{
					flags |= named;
				}
			}
		}

		byte[] source = File.ReadAllBytes(src);
		int hr = D3DCompile(source, (IntPtr)source.Length, displayPath, IntPtr.Zero, IntPtr.Zero,
			entry, profile, flags, 0, out IntPtr codePtr, out IntPtr errPtr);

		if (errPtr != IntPtr.Zero)
		{
			Console.Error.Write(Encoding.ASCII.GetString(BlobBytes(errPtr)).TrimEnd('\0'));
			BlobRelease(errPtr);
		}
		if (hr < 0 || codePtr == IntPtr.Zero)
		{
			Console.Error.WriteLine("D3DCompile failed with HRESULT 0x" + hr.ToString("X8"));
			return 1;
		}

		byte[] bytecode = BlobBytes(codePtr);
		BlobRelease(codePtr);
		File.WriteAllBytes(dst, bytecode);
		return 0;
	}
}
