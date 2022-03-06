using System;
using System.Runtime.CompilerServices;
using flexil.JIT;
using flexil.JIT.Attributes;

namespace flexil {
	class Program {
		static void Main(string[] args) {
			var hooker = new JITHooker();
			if (hooker.Hook()) {
				Console.WriteLine("hook sucucess");
			}

			Console.WriteLine(ExOne());
			Console.WriteLine(ExAdd());
			Console.Read();
		}

		[HookDump]
		[HookOverwrite(0x25, 2)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static int ExOne() {
			/*
				0:  55                      push   ebp
				1:  57                      push   edi
				2:  56                      push   esi
				3:  48                      dec    eax
				4:  83 ec 30                sub    esp,0x30
				7:  48                      dec    eax
				8:  8b ec                   mov    ebp,esp
				a:  33 c0                   xor    eax,eax
				c:  89 45 24                mov    DWORD PTR [ebp+0x24],eax
				f:  48                      dec    eax
				10: 89 45 28                mov    DWORD PTR [ebp+0x28],eax
				13: 83 3d 46 6e 09 00 00    cmp    DWORD PTR ds:0x96e46,0x0
				1a: 74 05                   je     0x21
				1c: e8 4f 4b c2 5f          call   0x5fc24b70
				21: 90                      nop
				22: c7 45 24 ff 00 00 00    mov    DWORD PTR [ebp+0x24],0x1 ; <- 0x1 부분, addr 0x25를 치환
				29: 90                      nop
				2a: eb 00                   jmp    0x2c
				2c: 8b 45 24                mov    eax,DWORD PTR [ebp+0x24]
				2f: 48                      dec    eax
				30: 8d 65 30                lea    esp,[ebp+0x30]
				33: 5e                      pop    esi
				34: 5f                      pop    edi
				35: 5d                      pop    ebp
				36: c3                      ret
			 */
			return 1;
		}

		[HookOverwrite(0x2b, 1)]
		[HookOverwrite(0x32, 2)]
		static int ExAdd() {
			/*
				0:  55                      push   ebp
				1:  57                      push   edi
				2:  56                      push   esi
				3:  48                      dec    eax
				4:  83 ec 40                sub    esp,0x40
				7:  48                      dec    eax
				8:  8b ec                   mov    ebp,esp
				a:  33 c0                   xor    eax,eax
				c:  89 45 34                mov    DWORD PTR [ebp+0x34],eax
				f:  89 45 30                mov    DWORD PTR [ebp+0x30],eax
				12: 89 45 2c                mov    DWORD PTR [ebp+0x2c],eax
				15: 48                      dec    eax
				16: 89 45 38                mov    DWORD PTR [ebp+0x38],eax
				19: 83 3d 60 d3 08 00 00    cmp    DWORD PTR ds:0x8d360,0x0
				20: 74 05                   je     0x27
				22: e8 69 b0 c1 5f          call   0x5fc1b090
				27: 90                      nop
				28: c7 45 34 0a 00 00 00    mov    DWORD PTR [ebp+0x34],0xa  ; 10, addr 0x2b를 1로
				2f: c7 45 30 14 00 00 00    mov    DWORD PTR [ebp+0x30],0x14 ; 20, addr 0x32를 2로
				36: 8b 4d 34                mov    ecx,DWORD PTR [ebp+0x34]
				39: 8b 55 30                mov    edx,DWORD PTR [ebp+0x30]
				3c: e8 c7 9f fe ff          call   0xfffea008
				41: 89 45 28                mov    DWORD PTR [ebp+0x28],eax
				44: 8b 45 28                mov    eax,DWORD PTR [ebp+0x28]
				47: 89 45 2c                mov    DWORD PTR [ebp+0x2c],eax
				4a: 90                      nop
				4b: eb 00                   jmp    0x4d
				4d: 8b 45 2c                mov    eax,DWORD PTR [ebp+0x2c]
				50: 48                      dec    eax
				51: 8d 65 40                lea    esp,[ebp+0x40]
				54: 5e                      pop    esi
				55: 5f                      pop    edi
				56: 5d                      pop    ebp
				57: c3                      ret
			 */
			int a = 10;
			int b = 20;
			return Add(a, b);
		}

		static int Add(int a, int b) => a + b;
	}
}
