using System.Collections.Generic;
using Tunepal.Core.Services;

namespace Tunepal.Core {
	public class GlobalState {
		private static List<Source> _sources;
		private static Settings _settings;

		#region Properties

		public static List<Source> Sources {
			get {
				return _sources;
			}
		}

		public static Settings Settings {
			get {
				return _settings;
			}
		}

		#endregion

		public static void Initialize() {
			var service = new TunepalWebService();
			service.GetSources(HandleGetSources);
		}

		private static void HandleGetSources(bool success, List<Source> sources) {
			if (success) {
				_sources = sources;
			}
		}

		public static void Shutdown() {
		}
	}
}
