﻿#if WIN
using System;
using System.Collections.Generic;

namespace Lime
{
	public class DynamicFont : IFont
	{
		public string About { get { return string.Empty; } }

		public IFontCharSource Chars { get; private set; }

		public List<ITexture> Textures { get; private set; }

		public DynamicFont(byte[] fontData)
		{
			Textures = new List<ITexture>();
			Chars = new DynamicFontCharSource(fontData, Textures);
		}

		public void Dispose()
		{
			if (Chars != null) { Chars.Dispose(); }
			foreach (var texture in Textures) {
				if (texture != null) { texture.Dispose(); }
			}
		}

		public void ClearCache()
		{
			(Chars as DynamicFontCharSource).ClearCache();
		}
	}

	internal class DynamicFontCharSource : IFontCharSource
	{
		private FontRenderer builder;
		private List<ITexture> textures;
		private Dictionary<int, CharCache> charCaches = new Dictionary<int, CharCache>();
		
		public DynamicFontCharSource(byte[] fontData, List<ITexture> textures)
		{
			builder = new FontRenderer(fontData);
			this.textures = textures;
		}

		public FontChar Get(char code, float heightHint)
		{
			var roundedHeight = heightHint.Round();
			CharCache charChache;
			if (!charCaches.TryGetValue(roundedHeight, out charChache)) {
				charCaches[roundedHeight] = charChache = new CharCache(roundedHeight, builder, textures);
			}
			return charChache.Get(code);
		}

		public void ClearCache()
		{
			charCaches.Clear();
		}

		public void Dispose()
		{
			if (builder != null) {
				builder.Dispose();
				builder = null;
			}
		}

		class CharCache
		{
			// Use inheritance instead of composition in a sake of performance
			class DynamicTexture : Texture2D
			{
				public readonly Color4[] Data;
				public bool Invalidated;

				public DynamicTexture(Size size)
				{
					ImageSize = SurfaceSize = size;
					Data = new Color4[size.Width * size.Height];
				}

				public override uint GetHandle()
				{
					if (Invalidated) {
						LoadImage(Data, ImageSize.Width, ImageSize.Height, generateMips: false);
						Invalidated = false;
					}
					return base.GetHandle();
				}
			}

			private FontRenderer fontRenderer;
			private DynamicTexture texture;
			private IntVector2 position;
			private List<ITexture> textures;
			private int textureIndex;
			private int fontHeight;

			public FontChar[][] CharMap = new FontChar[256][];

			public CharCache(int fontHeight, FontRenderer fontRenderer, List<ITexture> textures)
			{
				this.fontHeight = fontHeight;
				this.textures = textures;
				this.fontRenderer = fontRenderer;
			}

			public FontChar Get(char code)
			{
				byte hb = (byte)(code >> 8);
				byte lb = (byte)(code & 255);
				if (CharMap[hb] == null) {
					CharMap[hb] = new FontChar[256];
				}
				var c = CharMap[hb][lb];
				if (c != null) {
					return c;
				}
				c = CreateFontChar(code);
				if (c != null) {
					CharMap[hb][lb] = c;
					return c;
				}
				return FontCharCollection.TranslateKnownMissingChars(ref code) ?
					Get(code) : (CharMap[hb][lb] = FontChar.Null);
			}

			private FontChar CreateFontChar(char code)
			{
				var glyph = fontRenderer.Render(code, fontHeight);
				if (glyph == null)
					return null;
				if (texture == null) {
					CreateNewFontTexture();
				}
				if (position.X + glyph.Width + 1 >= texture.ImageSize.Width) {
					position.X = 0;
					position.Y += fontHeight + 1;
				}
				if (position.Y + fontHeight + 1 >= texture.ImageSize.Height) {
					CreateNewFontTexture();
					position = IntVector2.Zero;
				}
				DrawGlyphToTexture(glyph);
				var fontChar = new FontChar {
					Char = code,
					UV0 = (Vector2)position / (Vector2)texture.ImageSize,
					UV1 = ((Vector2)position + new Vector2(glyph.Width, fontHeight)) / (Vector2)texture.ImageSize,
					ACWidths = glyph.ACWidths,
					Width = glyph.Width,
					Height = fontHeight,
					KerningPairs = glyph.KerningPairs,
					TextureIndex = textureIndex
				};
				position.X += glyph.Width + 1;
				return fontChar;
			}

			private void CreateNewFontTexture()
			{
				texture = new DynamicTexture(CalcTextureSize());
				textureIndex = textures.Count;
				textures.Add(texture);
			}

			private Size CalcTextureSize()
			{
				const int glyphsPerTexture = 50;
				var glyphMaxArea = fontHeight * (fontHeight / 2);
				var size =
					CalcUpperPowerOfTwo((int)Math.Sqrt(glyphMaxArea * glyphsPerTexture))
					.Clamp(128, 1024);
				return new Size(size, size);
			}

			private int CalcUpperPowerOfTwo(int x)
			{
				x--;
				x |= (x >> 1);
				x |= (x >> 2);
				x |= (x >> 4);
				x |= (x >> 8);
				x |= (x >> 16);
				return (x + 1);
			}

			private void DrawGlyphToTexture(FontRenderer.Glyph glyph)
			{
				var data = glyph.Pixels;
				if (data == null)
					return; // Invisible glyph
				texture.Invalidated = true;
				var t = 0;
				for (int i = 0; i < glyph.Height; i++) {
					int w = (position.Y + glyph.VerticalOffset + i) * texture.ImageSize.Width + position.X;
					for (int j = 0; j < glyph.Width; j++) {
						texture.Data[w++] = new Color4(255, 255, 255, data[t++]);
					}
				}
			}
		}
	}
}
#endif
