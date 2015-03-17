using System.IO;
using Tunepal.Core.Extensions;
using Tunepal.Core.Transcription;

namespace Tunepal.Core {
	public class Settings : Base, IBinarySerializable {
		private Note _transcriptionFundamental = Note.D;
		private int _countdown = 3;
		private PitchModel _pitchModel = PitchModel.Flute;
		private int _transposeBy = 0;
		private bool _allowMic = true;
		private bool _allowGps = true;
		private bool _allowSendGpsPrompted = false;
		private bool _allowSendGps = false;

		#region Properties

		public readonly Note[] FundamentalNotes = new [] { Note.Bb, Note.B, Note.C, Note.D, Note.Eb, Note.F, Note.G };

		public Note TranscriptionFundamental {
			get {
				return _transcriptionFundamental;
			}

			set {
				_transcriptionFundamental = value;
				NotifyPropertyChanged("TranscriptionFundamental");
			}
		}

		public int CountDown {
			get {
				return _countdown;
			}

			set {
				_countdown = value;
				NotifyPropertyChanged("CountDown");
			}
		}

		public PitchModel PitchModel {
			get {
				return _pitchModel;
			}

			set {
				_pitchModel = value;
				NotifyPropertyChanged("PitchModel");
			}
		}

		public int TransposeBy {
			get {
				return _transposeBy;
			}

			set {
				_transposeBy = value;
				NotifyPropertyChanged("TransposeBy");
			}
		}

		public bool AllowMic {
			get {
				return _allowMic;
			}

			set {
				_allowMic = value;
				NotifyPropertyChanged("AllowMic");
			}
		}

		public bool AllowGps {
			get {
				return _allowGps;
			}

			set {
				_allowGps = value;
				NotifyPropertyChanged("AllowGps");
			}
		}

		public bool AllowSendGpsPrompted {
			get {
				return _allowSendGpsPrompted;
			}

			set {
				_allowSendGpsPrompted = value;
				NotifyPropertyChanged("AllowSendGpsPrompted");
			}
		}

		public bool AllowSendGps {
			get {
				return _allowSendGps;
			}

			set {
				_allowSendGps = value;
				NotifyPropertyChanged("AllowSendGps");
			}
		}

		#endregion

		public void Read(BinaryReader reader) {
			TranscriptionFundamental = Note.Parse(reader.ReadString());
			CountDown = reader.ReadInt32();
			PitchModel = reader.ReadBoolean() ? PitchModel.Whistle : PitchModel.Flute;
			TransposeBy = reader.ReadInt32();
			AllowMic = reader.ReadBoolean();
			AllowGps = reader.ReadBoolean();
			AllowSendGpsPrompted = reader.ReadBoolean();
			AllowSendGps = reader.ReadBoolean();
		}

		public void Write(BinaryWriter writer) {
			writer.Write(TranscriptionFundamental.ToString());
			writer.Write(CountDown);
			writer.Write((PitchModel == PitchModel.Whistle));
			writer.Write(TransposeBy);
			writer.Write(AllowMic);
			writer.Write(AllowGps);
			writer.Write(AllowSendGpsPrompted);
			writer.Write(AllowSendGps);
		}

		public void SetDefaults() {
			TranscriptionFundamental = Note.D;
			CountDown = 3;
			PitchModel = PitchModel.Flute;
			TransposeBy = 0;
			AllowMic = true;
			AllowGps = true;
			AllowSendGpsPrompted = false;
			AllowSendGps = false;
		}
	}
}
