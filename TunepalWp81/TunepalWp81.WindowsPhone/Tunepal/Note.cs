using System;

namespace Tunepal.Core {
	public class Note {
		#region Static members

		public static Note C = new Note("C", 0);
		public static Note Db = new Note("Db", 1);
		public static Note D = new Note("D", 2);
		public static Note Eb = new Note("Eb", 3);
		public static Note E = new Note("E", 4);
		public static Note F = new Note("F", 5);
		public static Note Gb = new Note("Gb", 6);
		public static Note G = new Note("G", 7);
		public static Note Ab = new Note("Ab", 8);
		public static Note A = new Note("A", 9);
		public static Note Bb = new Note("Bb", 10);
		public static Note B = new Note("B", 11);

		public static Note Parse(string name) {
			if (name.IndexOf("/") >= 0) {
				name = name.Split('/')[0];
			}

			switch (name.ToUpper()) {
				case "C":
					return C;
				case "DB":
					return Db;
				case "D":
					return D;
				case "EB":
					return Eb;
				case "E":
					return E;
				case "F":
					return F;
				case "GB":
					return Gb;
				case "G":
					return G;
				case "AB":
					return Ab;
				case "A":
					return A;
				case "BB":
					return Bb;
				case "B":
					return B;
				default:
					throw new ArgumentException("name");
			}
		}

		#endregion

		public Note() {
		}

		private Note(string name, int fromC) {
			Name = name;
			FromC = fromC;
		}

		public string Name { 
			get;
			set;
		}

		public int FromC {
			get;
			set;
		}

		public override string ToString() {
			return Name;
		}
	}
}
