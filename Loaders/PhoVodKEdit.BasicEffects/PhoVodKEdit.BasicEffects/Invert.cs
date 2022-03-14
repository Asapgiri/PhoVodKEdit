﻿using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit.BasicEffects
{
	public class Invert : PortEffect
	{
		public Invert(AppliedSettings _applied) : base(_applied)
		{
		}

		public override void Apply(Bitmap image, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
		{
			BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, pixelFormat);
			int stride = bitmapData.Stride;
			System.IntPtr Scan0 = bitmapData.Scan0;

			stopwatch.Reset();
			stopwatch.Start();

			unsafe
			{
				byte* p = (byte*)(void*)Scan0;
				int nOffset = stride - image.Width * 3;
				int nWidth = image.Width * 3;
				for (int y = 0; y < image.Height; ++y)
				{
					for (int x = 0; x < nWidth; ++x)
					{
						p[0] = (byte)(255 - p[0]);
						++p;
					}
					p += nOffset;
				}
			}

			stopwatch.Stop();

			image.UnlockBits(bitmapData);
		}

		public override FrameworkElement GetView()
		{
			Grid grid = new Grid();

			Label lable = new Label
			{
				Content = "No settings to show",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			grid.Children.Add(lable);
			return grid;
		}
	}
}
