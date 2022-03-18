﻿using System.Collections.Generic;

namespace PhoVodKEdit.Port.Utilities {
	public class Layer
	{
		public List<PortEffect> Effects { get; set; } = new List<PortEffect>();

		public string Name { get; set; }

		public bool Rendered { get; set; } = true;

		public Layer(string name) { 
			Name = name;
		}
	}
}
