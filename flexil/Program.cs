using System;
using System.Runtime.CompilerServices;
using flexil.JIT;

namespace flexil {
	class Program {
		static void Main(string[] args) {
			var hooker = new JITHooker();
			if (hooker.Hook()) {
				Console.WriteLine("hook sucucess");
			}

			Console.WriteLine(Answer());
			Console.Read();
		}

		[HookHere]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static int Answer() {
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
				22: c7 45 24 ff 00 00 00    mov    DWORD PTR [ebp+0x24],0xff ; <- 0xff 부분, addr 0x25를 치환
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
	}
}
