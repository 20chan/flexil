using System;
using System.Runtime.InteropServices;

namespace flexil.JIT.Attributes {
	public class HookDumpAttribute : HookAttribute {
		public override void PostProcess(IntPtr ilCodePtr, int ilSize, IntPtr nativeCodePtr, int nativeCodeSize) {
			var codes = new byte[nativeCodeSize];
			Marshal.Copy(nativeCodePtr, codes, 0, nativeCodeSize);
			Console.WriteLine(BitConverter.ToString(codes));
		}
	}
}
