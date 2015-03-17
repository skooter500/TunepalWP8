namespace Tunepal.Core {
	public class KeySignature {
		public Note Note { get; set; }
		public Scale Scale { get; set; }

		public KeySignature() {
		}

		public KeySignature(string text) {
			if (text.Length > 1) {
				if (text[1] == 'b' || text[1] == 'B') {
					Note = Note.Parse(text.Substring(0, 2));
					Scale = Scale.Parse(text.Substring(2));
				} else {
					Note = Note.Parse(text.Substring(0, 1));
					Scale = Scale.Parse(text.Substring(1));
				}
			} else { 
				Note = Note.Parse(text.Substring(0, 1));
				Scale = Scale.Major;
			}
		}

		public override string ToString() {
			if (Note != null) {
				if (Scale != null) {
					return Note + Scale.Abbreviation;
				}

				return Note.ToString();
			}

			return "none";
		}
	}
}
