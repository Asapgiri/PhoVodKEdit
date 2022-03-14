using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhoVodKEdit.Port.APS {
	public static class Settings {
		public static class Colors {
			public static class Dark {
				public static Color Main => Color.FromRgb(0x3e, 0x3e, 0x40);
				public static Color Secondary => Color.FromRgb(0x00, 0x7a, 0xcc);
				public static Color Foreground => Color.FromRgb(0xf1, 0xf1, 0xf1);
				public static Color Background => Color.FromRgb(0x25, 0x25, 0x26);
				public static Color Border => Color.FromRgb(0x5e, 0x5e, 0x5e);
			}

			public static class Light {
				public static Color Main => Color.FromRgb(0xf5, 0xf5, 0xf5);
				public static Color Secondary => Color.FromRgb(0x00, 0x7a, 0xcc);
				public static Color Foreground => Color.FromRgb(0x1e, 0x1e, 0x1e);
				public static Color Background => Color.FromRgb(0xf7, 0xf7, 0xf7);
				public static Color Border => Color.FromRgb(0x25, 0x25, 0x26);
			}
		}

		public static class Font {
			public static double Size = 14;
		}

		public static class LanguageDictionary {
			public static class Shortcuts {
				public static string SC_MENU_FILE_NEW	=	"Ctrl + N";
				public static string SC_MENU_FILE_OPEN	=	"Ctrl + O";
				public static string SC_MENU_FILE_SAVE	=	"Ctrl + S";
				public static string SC_MENU_EDIT_COPY	=	"Ctrl + C";
				public static string SC_MENU_EDIT_PASTE =	"Ctrl + V";
				public static string SC_MENU_EDIT_CUT	=	"Ctrl + X";
			}

			public static class English {
				public static string Language = "en";

				#region Menus
				public static string MENU_FILE = "File";
				public static string MENU_FILE_NEW = "New";
				public static string MENU_FILE_OPEN = "Open";
				public static string MENU_FILE_SAVE = "Save";
				public static string MENU_FILE_SAVE_AS = "Save as..";
				public static string MENU_FILE_CLOSE = "Close";
				public static string MENU_EDIT = "Edit";
				public static string MENU_EDIT_COPY = "Copy";
				public static string MENU_EDIT_PASTE = "Paste";
				public static string MENU_EDIT_CUT = "CUT";
				#endregion Menus
			}

			public static class Hungarian {
				public static string Language = "hu";

				#region Menus
				public static string MENU_FILE = "Fájl";
				public static string MENU_FILE_NEW = "Új";
				public static string MENU_FILE_OPEN = "Megnyit";
				public static string MENU_FILE_SAVE = "Mentés";
				public static string MENU_FILE_SAVE_AS = "Mentés mint..";
				public static string MENU_FILE_CLOSE = "Bezár";
				public static string MENU_EDIT = "Szerkeszt";
				public static string MENU_EDIT_COPY = "Másolás";
				public static string MENU_EDIT_PASTE = "Beillesztés";
				public static string MENU_EDIT_CUT = "Kivágás";
				#endregion Menus
			}
		}
	}
}
