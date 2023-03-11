Bitmap2Font
===========
Bitmap2Font is designed to generate fonts for the SSD1306 device under NanoFramework.
It is using typical 'Bitmap Fonts' as font definitions, see examples here: http://www.orangetide.com/fonts/DOS/ and in many different places.
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
(1) the font as a C# class in a corresponding .cs file and 
(2) a preview bitmap as visual proof 
Both will be saved at the same user-specified location. The .cs file needs to be added to the C# user project.
Bitmap2Font can generate a number of 8px-width fonts in a variety of heights. Narrower and wider fonts are under investigation.  

Three input bitmaps are included to test Bitmap2Font for the extraction of 96 alphanumerics:

rmrkbold-14.png	parameters:	font width:			8
					font height:		14
					Bitmap columns:		32
					Bitmap starting row:	1
					Bitmap end row:		4	

lbitalic-16		parameters:	font width:			8
					font height:		16
					Bitmap columns:		32
					Bitmap starting row:	1
					Bitmap end row:		4

Retro8x16.bmp	parameters:	font width:			8
					font height:		16
					Bitmap columns:		16
					Bitmap starting row:	0
					Bitmap end row:		6