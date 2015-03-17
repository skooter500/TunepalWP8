namespace Tunepal.Core {
	public class Scale {
#region Static members
		public static Scale Major = new Scale("Major", "");
		public static Scale Minor = new Scale("Minor", "min");
		public static Scale Mixolydian = new Scale("Mixolydian", "mix");
		public static Scale Dorian = new Scale("Dorian", "dor");
		public static Scale Phrygian = new Scale("Phrygian", "phr");
		public static Scale Lydian = new Scale("Lydian", "lyd");
		public static Scale Locrian = new Scale("Locrian", "loc");
#endregion

		private string _name;
		private string _abbreviation;

		public Scale() {
		}

		public Scale(string name, string abbreviation) {
			_name = name;
			_abbreviation = abbreviation;
		}

		public string Name { 
			get {
				return _name;
			}

			set {
				_name = value;
			}
		}

		public string Abbreviation {
			get {
				return _abbreviation;
			}

			set {
				_abbreviation = value;
			}
		}

		public static Scale Parse(string abbreviation) {
			switch (abbreviation.ToUpper()) {
				case "M":
				case "MIN":
					return Minor;
				case "MIX":
					return Mixolydian;
				case "DOR":
					return Dorian;
				case "PHR":
					return Phrygian;
				case "LYD":
					return Lydian;
				case "LOC":
					return Locrian;
				default:
					return Major;
			}
		}
	}
}
