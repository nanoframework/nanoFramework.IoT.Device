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
