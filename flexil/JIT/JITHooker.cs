using flexil.JIT.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static flexil.JIT.WinAPI;

namespace flexil.JIT {
  public class JITHooker {
    public bool IsHooked { get; private set; }
    public Guid CLRVersionID { get; private set; }

    private IntPtr _jitAddr;
    private CompileMethodDelegate _originalCompileMethod;
    private CompileMethodDelegate _replacedCompileMethod;

    public JITHooker() {
      IsHooked = false;
    }

    public bool Hook() {
      foreach (ProcessModule module in Process.GetCurrentProcess().Modules) {
        if (Path.GetFileName(module.FileName) == "clrjit.dll") {
          var jitAddr = GetProcAddress(module.BaseAddress, "getJit");
          if (jitAddr != IntPtr.Zero) {
            ReplaceJit(jitAddr);
            return IsHooked;
          }
        }
      }
      return false;
    }

    public bool UnHook() {
      throw new NotImplementedException();
    }


    private void ReplaceJit(IntPtr jitAddr) {
      _jitAddr = jitAddr;
      var getJit = Marshal.GetDelegateForFunctionPointer<GetJitDelegate>(jitAddr);
      var jit = getJit();
      var jitTable = Marshal.ReadIntPtr(jit);
      var getVerIdPtr = Marshal.ReadIntPtr(jitTable, IntPtr.Size * 4);
      var getVerId = Marshal.GetDelegateForFunctionPointer<GetVersionIdentifierDelegate>(getVerIdPtr);
      getVerId(jitAddr, out var guid);
      CLRVersionID = guid;

      var compileMethodPtr = Marshal.ReadIntPtr(jitTable, 0);
      _originalCompileMethod = Marshal.GetDelegateForFunctionPointer<CompileMethodDelegate>(compileMethodPtr);
      _replacedCompileMethod = CompileMethod;
      var replacedCompileMethodPtr = Marshal.GetFunctionPointerForDelegate(_replacedCompileMethod);

      var trampolinePtr = AllocateTrampoline(replacedCompileMethodPtr);
      var trampoline = Marshal.GetDelegateForFunctionPointer<CompileMethodDelegate>(trampolinePtr);
      var emptyInfo = default(CORINFO_METHOD_INFO);

      trampoline(IntPtr.Zero, IntPtr.Zero, ref emptyInfo, 0, out var _, out var _);
      FreeTrampoline(trampolinePtr);

      VirtualProtect(jitTable, new IntPtr(IntPtr.Size), MemoryProtection.ReadWrite, out var oldFlags);
      Marshal.WriteIntPtr(jitTable, 0, replacedCompileMethodPtr);
      VirtualProtect(jitTable, new IntPtr(IntPtr.Size), oldFlags, out _);

      IsHooked = true;
    }

    readonly byte[] DelegateTrampolineCode = {
            // mov rax, 0000000000000000h ;
            0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            // jmp rax
            0xFF, 0xE0
        };

    private IntPtr AllocateTrampoline(IntPtr dest) {
      var jmp = VirtualAlloc(IntPtr.Zero, DelegateTrampolineCode.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
      Marshal.Copy(DelegateTrampolineCode, 0, jmp, DelegateTrampolineCode.Length);
      Marshal.WriteIntPtr(jmp, 2, dest);
      return jmp;
    }

    private void FreeTrampoline(IntPtr trampoline) {
      VirtualFree(trampoline, new IntPtr(DelegateTrampolineCode.Length), FreeType.Release);
    }


    private static readonly object jitLock = new object();
    private static readonly Dictionary<IntPtr, Assembly> assemblies = new Dictionary<IntPtr, Assembly>();

    int i = 0;

    private int CompileMethod(
        IntPtr thisPtr,
        IntPtr comp,
        ref CORINFO_METHOD_INFO info,
        uint flags,
        out IntPtr nativeEntry,
        out int nativeSizeOfCode) {
      if (!IsHooked) {
        nativeEntry = IntPtr.Zero;
        nativeSizeOfCode = 0;
        return 0;
      }

      // https://github.com/xoofx/ManagedJit/blob/master/ManagedJit/ManagedJit.cs
      var res = _originalCompileMethod(thisPtr, comp, ref info, flags, out nativeEntry, out nativeSizeOfCode);

      if (i != 0) {
        return res;
      }
      i++;

      var vtableCorJitInfo = Marshal.ReadIntPtr(comp);

      var getMethodDefFromMethodPtr = Marshal.ReadIntPtr(vtableCorJitInfo, IntPtr.Size * 116);
      var getMethodDefFromMethod = Marshal.GetDelegateForFunctionPointer<GetMethodDefFromMethodDelegate>(getMethodDefFromMethodPtr);
      var methodToken = getMethodDefFromMethod(comp, info.ftn);

      var getModuleAssemblyDelegatePtr = Marshal.ReadIntPtr(vtableCorJitInfo, IntPtr.Size * 48);
      var getModuleAssemblyDelegate = Marshal.GetDelegateForFunctionPointer<GetModuleAssemblyDelegate>(getModuleAssemblyDelegatePtr);
      var assemblyHandle = getModuleAssemblyDelegate(comp, info.scope);

      Assembly assembly = null;
      lock (jitLock) {
        if (!assemblies.TryGetValue(assemblyHandle, out assembly)) {
          var getAssemblyNamePtr = Marshal.ReadIntPtr(vtableCorJitInfo, IntPtr.Size * 49);
          var getAssemblyName = Marshal.GetDelegateForFunctionPointer<GetAssemblyNameDelegate>(getAssemblyNamePtr);
          var assemblyNamePtr = getAssemblyName(comp, assemblyHandle);

          var assemblyName = Marshal.PtrToStringAnsi(assemblyNamePtr);

          foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
            if (asm.GetName().Name == assemblyName) {
              assembly = asm;
              break;
						}
					}

          assemblies.Add(assemblyHandle, assembly);
				}
			}

      if (assembly != null) {
        MethodBase method = null;
        foreach (var module in assembly.Modules) {
          try {
            method = module.ResolveMethod(methodToken);
					} catch {
					}
				}

        if (method != null) {
          HandleMethodCompile(method, info.ILCode, info.ILCodeSize, nativeEntry, nativeSizeOfCode);
        }
			}
      i--;
      return res;
    }

    private void HandleMethodCompile(MethodBase method, IntPtr ilCodePtr, int ilSize, IntPtr nativeCodePtr, int nativeCodeSize) {
      var attrs = method.GetCustomAttributes<HookAttribute>(true);
      foreach (var attr in attrs) {
        attr.PostProcess(ilCodePtr, ilSize, nativeCodePtr, nativeCodeSize);
			}
    }
  }
}