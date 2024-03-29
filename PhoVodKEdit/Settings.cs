﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhoVodKEdit
{
	public static class Settings
	{
		public static class Colors
		{
			public static class Dark
			{
				public static Color Main => Color.FromRgb(0x3e, 0x3e, 0x40);
				public static Color Secondary => Color.FromRgb(0x00, 0x7a, 0xcc);
				public static Color Foreground => Color.FromRgb(0xf1, 0xf1, 0xf1);
				public static Color Background => Color.FromRgb(0x25, 0x25, 0x26);
				public static Color Border => Color.FromRgb(0x5e, 0x5e, 0x5e);
			}

			public static class Light
			{
				public static Color Main => Color.FromRgb(0xf5, 0xf5, 0xf5);
				public static Color Secondary => Color.FromRgb(0x00, 0x7a, 0xcc);
				public static Color Foreground => Color.FromRgb(0x1e, 0x1e, 0x1e);
				public static Color Background => Color.FromRgb(0xf7, 0xf7, 0xf7);
				public static Color Border => Color.FromRgb(0x25, 0x25, 0x26);
			}
		}

		public static class Font
		{
			public static double Size = 14;
		}
	}
}
