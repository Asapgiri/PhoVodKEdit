using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoVodKEdit.Port.Utilities
{
	/// <summary>
	/// Files Dialog filers for project.
	/// </summary>
	public struct ContentFilter
	{
		/// <summary>
		/// The directory where tzhe dialog will search for files.
		/// </summary>
		public string InitDirectory { get; set; }

		/// <summary>
		/// The filterred filetypes. eg.: "txt files (*.txt)|*.txt|All files (*.*)|*.*"
		/// </summary>
		public string Filter { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool RestoreDirectory { get; set; }
	}
}
