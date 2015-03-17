using System.IO;
using Tunepal.Core.Extensions;

namespace Tunepal.Core {
	public class Source : IBinarySerializable {
		public int Id { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }
		public string Url { get; set; }
		public string Extra { get; set; }
		public bool IncludeInSearch { get; set; }

		public void Read(BinaryReader reader) {
			Id = reader.ReadInt32();
			Name = reader.ReadString();
			Abbreviation = reader.ReadString();
			Url = reader.ReadString();
			Extra = reader.ReadString();
			IncludeInSearch = reader.ReadBoolean();
		}

		public void Write(BinaryWriter writer) {
			writer.Write(Id);
			writer.Write(Name);
			writer.Write(Abbreviation);
			writer.Write(Url);
			writer.Write(Extra);
			writer.Write(IncludeInSearch);
		}
	}
}
