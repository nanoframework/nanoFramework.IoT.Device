# Tips and tricks for .NET IoT to .NET nanoFramework migration

You'll find a list of tips and tricks to help in the migration. There is no specific order in this list.

## What's already done for you

The project conversion, transformation os `Span<byte>` to `SpanByte` and other associated elements are done automatically.

## Enums

- Issue: .NET nanoFramework supports a limited version of enum, so `GetValues`, `IsDefined` are not available. 
- Resolution: just remove the `IsDefined`, this is used for check only. If you need the nice names with the `GetValues` use either a switch or string or anything like this depending on the scenario.

## Multidimensional Arrays [,]

- Issue: multidimensional `Arrays [,]` are not supported in .NET nanoFramework
- Resolution:
  - Replace them with `Array [][]`
  - Initialization is different, example:
  ```csharp
  // byte[,] array = new byte[42,15];
  byte[][] array = new byte[42][];
  for(int i = 0, i < 15; i++)
  {
    array[i] = new byte[15];
  }
  ```
  - If you have something lile `public int this[int x, int y]`, you'll need to create a class, example:
  ```csharp
  class Point
  {
      public Point(int X, int y)
      {
          X = x;
          Y = y;
      }
      public int X { get; set; }
      public int Y { get; set; }
  }
  public int this[Point pt]
  // and get access to pt.X and pt.Y
  // ...
  // Access is then like:
  Somthing[new Point(12,34)];
  ```
## Queue<Something>

- Issue: There is no generic (yet) and no Queue in nanoFramework. Most of the time, the usage of those elements are simple.
- Resolution: Replace the Queue by a `ArrayList` and use `Add` and `Remove` with a cast to read the data.

## Console

- Issue: `Console` is not available in .NET nanoFramework. It does only appear on the sample side.
- Resolution: can be replaced by `Debug`. And the `Console.Read` and other elements like this from the samples can be replaced by hard coded data or constants. 

## Unsafe

You may have to compile some of the projects in unsafe mode. This is needed if you are using unsafe blocks or unsafe instructions into your project.

Add `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the nfproj in the property group `PropertyGroup` right after then language version.

## Adjust the documentation

When converting, you may move some code, change some properties or functions a bit, that may need adjustment in README and other documentation, don't forget to adjust those! This does include as well replacing schema from Raspberry Pi to our lovely MCU. Any ESP32 or any STM32 or any supported MCU will be enough.

## Nuget restore and updates

It is recommended to update the nugets with a `dotnet restore` before opening the solution.

You may be stuck sometimes because some references may be corrupted and you won't be able to update or add a new nuget. In this case, you can try to downgrade by one version all the nugets and then update them again, this will pull potential missing references.
