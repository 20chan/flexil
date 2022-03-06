using System;

namespace flexil.JIT.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public abstract class HookAttribute : Attribute {
		public abstract void PostProcess(IntPtr ilCodePtr, int ilSize, IntPtr nativeCodePtr, int nativeCodeSize);
	}
}
