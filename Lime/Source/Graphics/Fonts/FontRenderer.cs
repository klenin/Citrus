﻿#if WIN
using System;
using System.Collections.Generic;

using SharpFont;

namespace Lime
{
	internal class FontRenderer : IDisposable
	{
		public class Glyph
		{
			public byte[] Pixels;
			public Vector2 ACWidths;
			public int VerticalOffset;
			public int Width;
			public int Height;
			public List<KerningPair> KerningPairs;
		}

		public string KerningCharacters =
			"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!\"" +
			"#$%&ˆ‘’“”´˜`'„()–—*+-÷~,.…/:;<>=?@[]\\^_¯{}|€‚ƒ‹›•§«»©®™¨°¿" +
			"ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿšœžŸŒŽŠ¡¢£¤¥µ¶ " +
			"АБВГДЕЁЖЗИЙКЛМНОПРСТУФХШЩЧЦЪЬЫЭЮЯабвгдеёжзийклмнопрстуфхшщчцъьыэюя";

		private Face face;
		private Library library;
		private int lastHeight;

		public FontRenderer(byte[] fontData)
		{
			library = new Library();
			face = library.NewMemoryFace(fontData, faceIndex: 0);
		}

		private static float CalcPixelSize(Face face, int height)
		{
			// See http://www.freetype.org/freetype2/docs/tutorial/step2.html
			// Chapter: Scaling Distances to Device Space
			// BBox suits better than Height (baseline-to-baseline distance), because it can enclose all the glyphs in the font face.
			var designHeight = (float)face.BBox.Top - (float)face.BBox.Bottom;
			var scale = height / designHeight;
			var pixelSize = scale * face.UnitsPerEM;
			return pixelSize;
		}

		/// <summary>
		/// Renders a glyph with a given height, measured as a distance between two text lines in the device pixels.</param>
		/// </summary>
		public Glyph Render(char @char, int height)
		{
			if (lastHeight != height) {
				lastHeight = height;
				var pixelSize = (uint)CalcPixelSize(face, height).Round();
				face.SetPixelSizes(pixelSize, pixelSize);
			}
			// Handle whitespace
			if (@char == ' ') {
				return new Glyph {
					Width = ((float)height / 5).Round(),
					Height = height,
				};
			}
			uint glyphIndex = face.GetCharIndex(@char);
			face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
			if (face.Glyph.Metrics.Width == 0) {
				return null;
			}
			 
			face.Glyph.RenderGlyph(RenderMode.Normal);
			var bitmap = face.Glyph.Bitmap;
			if (bitmap.PixelMode != PixelMode.Gray) {
				throw new System.Exception("Invalid pixel mode: " + bitmap.PixelMode);
			}
			if (bitmap.Pitch != bitmap.Width) {
				throw new System.Exception("Bitmap pitch doesn't match its width");
			}
			var verticalOffset = height - face.Glyph.BitmapTop + face.Size.Metrics.Descender.Round();
			var bearingX = (float)face.Glyph.Metrics.HorizontalBearingX;
			var glyph = new Glyph {
				Pixels = bitmap.BufferData,
				Width = bitmap.Width,
				Height = bitmap.Rows,
				VerticalOffset = verticalOffset,
				ACWidths = new Vector2(
					bearingX,
					(float)face.Glyph.Metrics.HorizontalAdvance - (float)face.Glyph.Metrics.Width - bearingX
				),
			};
			// Iterate through kerning pairs
			foreach (var prevChar in KerningCharacters) {
				uint prevGlyphIndex = face.GetCharIndex(prevChar);
				var kerning = (float)face.GetKerning(prevGlyphIndex, glyphIndex, KerningMode.Default).X;
				if (kerning != 0) {
					if (glyph.KerningPairs == null) {
						glyph.KerningPairs = new List<KerningPair>();
					}
					glyph.KerningPairs.Add(new KerningPair { Char = prevChar, Kerning = kerning });
				}
			}
			return glyph;
		}

		public void Dispose()
		{
			if (library != null) {
				library.Dispose();
				library = null;
				// Do not dispose the face, because library.Dispose() made it on its own.
				face = null;
			}
		}
	}
}
#endif