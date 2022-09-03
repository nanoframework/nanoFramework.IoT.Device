//
// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

var imageFilePath = args[0];
using var image = Image.Load<Rgba32>(imageFilePath);
image.Mutate(x => x.Resize(new Size(84, 48)));
image.Mutate(x => x.BlackWhite());

var colWhite = new Rgba32(255, 255, 255);
var width = 84;
var result = new byte[504];
for (var pos = 0; pos < result.Length; pos++)
{
    byte toStore = 0;
    for (int bit = 0; bit < 8; bit++)
    {
        var x = pos % width;
        var y = pos / width * 8 + bit;
        toStore = (byte)(toStore | ((image[x, y] == colWhite ? 0 : 1) << bit));
    }

    result[pos] = toStore;
}

var resultString = $"var bitmap = new byte[] {{{String.Join(",", result.Select(b => $"0x{b.ToString("X2")}"))}}}";
Console.WriteLine(resultString);
Console.ReadKey();