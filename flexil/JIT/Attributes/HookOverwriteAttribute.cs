using System;
using System.Runtime.InteropServices;

namespace flexil.JIT.Attributes {
	public class HookOverwriteAttribute : HookAttribute {
		public int Offset;
		public byte Value;

		public HookOverwriteAttribute(int offset, byte value) {
			Offset = offset;
			Value = value;
		}

		public override void PostProcess(IntPtr ilCodePtr, int ilSize, IntPtr nativeCodePtr, int nativeCodeSize) {
			Marshal.WriteByte(nativeCodePtr, Offset, Value);
		}
	}
}
