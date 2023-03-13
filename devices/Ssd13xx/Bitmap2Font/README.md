# Bitmap2Font: autogenerate IFont class from pictures

Bitmap2Font is designed to generate fonts for the SSD1306 device for .NET nanoFramework.

It is using typical 'Bitmap Fonts' as font definitions, see examples [here](http://www.orangetide.com/fonts/DOS/). It can use any picture with a fixed font size in clabk and white. You can find plenty in different places.

Orangetide contains a number of decorative fonts (>450) including localized ones such as Hebrew, Arabic, Cyrillic, Greek, Farsi, etc...

Optimal bitmap files should contain black lettering on white background. Other bitmaps need to be first processed with graphical programs.

A bitmap in any graphical format (bmp, png, gif, jpg) can be opened with Bitmap2Font and is displayed in a picturebox.

The user needs then to specify several parameters related to the input bitmap using numeric up/down selectors such as:

- individual character width
- individual character height
- numbers of character columns in the input bitmap (usually 16 or 32)
- starting row to take into account
- last row not taken into account

Usually, 96 characters cover all alphanumeric needs.

A preview bitmap can be generated using the 'Preview' button to make sure all the characters are present without missing or extra lines.

If the preview is acceptable, the 'Generate' button will create:

1. the font as a C# class in a corresponding .cs file and,
2. a preview bitmap as visual proof.

Both will be saved at the same user-specified location. The .cs file needs to be added to the C# user project.

Bitmap2Font can generate a number of 8px-width fonts in a variety of heights. Narrower and wider fonts are under investigation.

During the generation, the first character is used as a limit to return any non valid character representation.

Three input bitmaps are included to test Bitmap2Font for the extraction:

|File name|Font|Rows|Column|Number ASCII character|
|---|---|---|---|---|
|rmrkbold-14.png|Width: 8, Height 14|Total 8. Start: 1, End: 4 for 32->127|32 chars|256 (0->255)|
|lbitalic-16.png|Width: 8, Height 16|Total 8. Start: 1, End: 4 for 32->127|32 chars|256 (0->255)|
|Retro8x16.bmp|Width: 8, Height 16|Total 6. Start: 0, End: 6 for 32->127|16 chars|128 (32->127)|
