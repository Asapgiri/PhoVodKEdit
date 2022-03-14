using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoVodKEdit.Port.APS {
	public class AppliedLanguage {
		#region Properties
		public string Language { get; set; }
		public string MENU_FILE { get; set; }
		public string MENU_FILE_NEW { get; set; }
		public string MENU_FILE_OPEN { get; set; }
		public string MENU_FILE_SAVE { get; set; }
		public string MENU_FILE_SAVE_AS { get; set; }
		public string MENU_FILE_CLOSE { get; set; }
		public string MENU_EDIT { get; set; }
		public string MENU_EDIT_COPY { get; set; }
		public string MENU_EDIT_PASTE { get; set; }
		public string MENU_EDIT_CUT { get; set; }
		#endregion Properties

		public AppliedLanguage(string lang = "en") {
			switch (lang)
			{
				case "en": LoadFromStaticClass(typeof(Settings.LanguageDictionary.English)); break;
				case "hu": LoadFromStaticClass(typeof(Settings.LanguageDictionary.Hungarian)); break;
				default:
					break;
			}
		}

		private void LoadFromStaticClass(Type type) {
			var list = type.GetFields(BindingFlags.Static | BindingFlags.Public);

			foreach (var en in list) {
				GetType().GetProperty(en.Name).SetValue(this, en.GetValue(null), null);
			}
		}
	}
}
