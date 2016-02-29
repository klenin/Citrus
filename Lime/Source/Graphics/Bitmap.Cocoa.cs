#if iOS || MAC || MONOMAC
using System;
using System.IO;
using CoreGraphics;
#if iOS
using Foundation;
using CocoaBitmap = UIKit.UIImage;
#elif MAC || MONOMAC
using AppKit;
using CocoaBitmap = AppKit.NSImage;
#endif

namespace Lime
{
	class BitmapImplementation : IBitmapImplementation
	{
		public BitmapImplementation(Stream stream)
		{
#if iOS
			using (var nsData = NSData.FromStream(stream)) {
				Bitmap = CocoaBitmap.LoadFromData(nsData);
			}
#elif MAC || MONOMAC
			Bitmap = CocoaBitmap.FromStream(stream);
#endif
		}

		public BitmapImplementation(Color4[] pixels, int width, int height)
		{
			int lengthInBytes = pixels.Length * 4;
			var data = new byte[lengthInBytes];
			int j = 0;
			for (int i = 0; i < pixels.Length; i++) {
				data[j++] = pixels[i].R;
				data[j++] = pixels[i].G;
				data[j++] = pixels[i].B;
				data[j++] = pixels[i].A;
			}
			using (var colorSpace = CGColorSpace.CreateDeviceRGB()) {
				using (var dataProvider = new CGDataProvider(data, 0, lengthInBytes)) {
					using (var img = new CGImage(width, height, 8, 32, 4 * width, colorSpace, CGBitmapFlags.Last, dataProvider, null, false,
						CGColorRenderingIntent.Default)) {
#if iOS
						Bitmap = new CocoaBitmap(img);
#elif MAC || MONOMAC
						Bitmap = new CocoaBitmap(img, new CGSize(width, height));
#endif
					}
				}
			}
		}

		private BitmapImplementation(CocoaBitmap bitmap)
		{
			Bitmap = bitmap;
		}

		public CocoaBitmap Bitmap { get; private set; }

		public int Width
		{
			get { return Bitmap != null ? (int)Bitmap.CGImage.Width : 0; }
		}

		public int Height
		{
			get { return Bitmap != null ? (int)Bitmap.CGImage.Height : 0; }
		}

		public bool IsValid
		{
			get { return (Bitmap != null && (Width > 0 && Height > 0)); }
		}

		public IBitmapImplementation Clone()
		{
#if iOS
			return Crop(new IntRectangle(0, 0, Width, Height));
#elif MAC || MONOMAC
			return new BitmapImplementation((CocoaBitmap)Bitmap.Copy());
#endif
		}

		public IBitmapImplementation Rescale(int newWidth, int newHeight)
		{
#if iOS
			return new BitmapImplementation(Bitmap.Scale(new CGSize(newWidth, newHeight)));
#elif MAC || MONOMAC
			var newImage = new CocoaBitmap(new CGSize(newWidth, newHeight));
			newImage.LockFocus();
			var ctx = NSGraphicsContext.CurrentContext;
			ctx.ImageInterpolation = NSImageInterpolation.High;
			Bitmap.DrawInRect(
			new CGRect(0, 0, newWidth, newHeight), 
			new CGRect(0, 0, Bitmap.Size.Width, Bitmap.Size.Height), 
			NSCompositingOperation.Copy, 1); 
			newImage.UnlockFocus();
			return new BitmapImplementation(newImage);
#endif
		}

		public IBitmapImplementation Crop(IntRectangle cropArea)
		{
			var rect = new CGRect(cropArea.Left, cropArea.Top, cropArea.Width, cropArea.Height);
#if iOS
			return new BitmapImplementation(new CocoaBitmap(Bitmap.CGImage.WithImageInRect(rect)));
#elif MAC || MONOMAC
			var size = new CGSize(cropArea.Width, cropArea.Height);
			return new BitmapImplementation(new CocoaBitmap(Bitmap.CGImage.WithImageInRect(rect), size));
#endif
		}

		public Color4[] GetPixels()
		{
			var isColorSpaceRGB = Bitmap.CGImage.ColorSpace.Model == CGColorSpaceModel.RGB;
			if (!isColorSpaceRGB && Bitmap.CGImage.BitsPerPixel != 32) {
				throw new FormatException("Can not return array of pixels if bitmap is not in 32 bit format or if not in RGBA format.");
			}

			var doSwap = Bitmap.CGImage.BitmapInfo.HasFlag(CGBitmapFlags.ByteOrder32Little);
			var isPremultiplied = Bitmap.CGImage.AlphaInfo == CGImageAlphaInfo.PremultipliedFirst ||
				Bitmap.CGImage.AlphaInfo == CGImageAlphaInfo.PremultipliedLast;
			var rowLength = Bitmap.CGImage.BytesPerRow / 4;
			var width = Width;
			var height = Height;
			var pixels = new Color4[width * height];
			using (var data = Bitmap.CGImage.DataProvider.CopyData()) {
				unsafe {
					byte* pBytes = (byte*)data.Bytes;
					byte r, g, b, a;
					int index = 0;
					for (int i = 0; i < height; i++) {
						for (int j = 0; j < width; j++) {
							r = (byte)(*pBytes++);
							g = (byte)(*pBytes++);
							b = (byte)(*pBytes++);
							a = (byte)(*pBytes++);
							if (isPremultiplied && a != 255 && a != 0) {
								r = (byte)(255 * r / a);
								g = (byte)(255 * g / a);
								b = (byte)(255 * b / a);
							}
							pixels[index++] = doSwap ? new Color4(b, g, r, a) : new Color4(r, g, b, a);
						}

						// Sometimes Width can be smaller then length of a row due to byte alignment.
						// It's just an empty bytes at the end of each row, so we can skip them here.
						for (int k = 0; k < rowLength - width; k++) {
							pBytes += 4;
						}
					}
				}
			}
			return pixels;
		}

		public void SaveTo(Stream stream)
		{
#if iOS
			using (var png = Bitmap.AsPNG()) {
				if (png != null) {
					using (var bitmapStream = png.AsStream()) {
						bitmapStream.CopyTo(stream);
					}
				}
			}
#elif MAC || MONOMAC
			using (var rep = new NSBitmapImageRep(Bitmap.CGImage)) {
				rep.Size = Bitmap.Size;
				using (var pngData = rep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, null))
				using (var bitmapStream = pngData.AsStream()) {
					bitmapStream.CopyTo(stream);
				}
			}
#endif
		}

		public void Dispose()
		{
			if (Bitmap != null) {
				Bitmap.Dispose();
			}
		}
	}
}
#endif