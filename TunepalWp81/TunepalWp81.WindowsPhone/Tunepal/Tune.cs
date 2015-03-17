using System;
using Windows.Devices.Geolocation;

namespace Tunepal.Core {
	public class Tune : Base {
		private string _name;
		private string _altName;
		private int _sourceId;
		private string _rhythm;
		private KeySignature _keySignature;
		private string _abc;
		private byte[] _dots;
		private bool _isFavorite;

		public int LocalId { get; set; }
		public string TunepalId { get; set; }

		/// <summary>
		/// The source's ID for the tune.
		/// </summary>
		public string SourceTuneId { get; set; }

		public string Name {
			get {
				return _name;
			}

			set {
				_name = value;
				NotifyPropertyChanged("Name");
			}
		}

		public string AltName {
			get {
				return _altName;
			}

			set {
				_altName = value;
				NotifyPropertyChanged("AltName");
			}
		}

		public int SourceId { 
			get {
				return _sourceId;
			}

			set {
				_sourceId = value;
				NotifyPropertyChanged("SourceId");
				NotifyPropertyChanged("Source");
			}
		}

		public Source Source {
			get {
				foreach (var source in GlobalState.Sources) {
					if (source.Id.Equals(SourceId)) {
						return source;
					}
				}

				return null;
			}
		}

		public string Rhythm {
			get {
				return _rhythm;
			}

			set {
				_rhythm = value;
				NotifyPropertyChanged("Rhythm");
			}
		}

		public KeySignature KeySignature {
			get {
				return _keySignature;
			}

			set {
				_keySignature = value;
				NotifyPropertyChanged("KeySignature");
			}
		}

		public string Abc {
			get {
				return _abc;
			}

			set {
				_abc = value;
				NotifyPropertyChanged("Abc");
			}
		}

		public byte[] Dots {
			get {
				return _dots;
			}

			set {
				_dots = value;
				NotifyPropertyChanged("Dots");
			}
		}

		public float Score { get; set; }

		public bool IsFavorite {
			get {
				return _isFavorite;
			}

			set {
				_isFavorite = value;
				NotifyPropertyChanged("IsFavorite");
			}
		}

		public DateTime SearchDate { get; set; }
        public Geocoordinate SearchLocation { get; set; }
	}
}
