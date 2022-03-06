# flexil

.net core JIT native code AOT

## examples

```csharp
	class Program {
		static void Main(string[] args) {
			var hooker = new JITHooker();
			if (hooker.Hook()) {
				Console.WriteLine("hook sucucess");
			}

			Console.WriteLine(ExOne()); // prints 2, not 1
			Console.WriteLine(ExAdd()); // prints 3, not 30
			Console.Read();
		}

		[HookDump]
		[HookOverwrite(0x25, 2)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		static int ExOne() {
			return 1;
		}

		[HookOverwrite(0x2b, 1)]
		[HookOverwrite(0x32, 2)]
		static int ExAdd() {
			int a = 10;
			int b = 20;
			return Add(a, b);
		}

		static int Add(int a, int b) => a + b;
	}
```

## explains

todo