using Windows.Devices.Geolocation;
/*
namespace Tunepal.Core.Services {
	public class LocationService {
		private static GeoCoordinateWatcher _watcher;
		private static GeoPosition<GeoCoordinate> _currentPosition;

		public static GeoPositionStatus CurrentStatus {
			get {
				return _watcher.Status;
			}
		}

		public static GeoPosition<GeoCoordinate> MostRecentPosition {
			get {
				if (GlobalState.Settings.AllowGps) {
					return _currentPosition;
				}

				return null;
			}
		}

		public static void Initialize() {
			_watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
			_watcher.PositionChanged += PositionChanged;
		}

		public static void Start() {
			_watcher.Start();
		}

		public static void Stop() {
			_watcher.Stop();
		}
		
		private static void PositionChanged(object source, GeoPositionChangedEventArgs<GeoCoordinate> args) {
			if (_watcher.Status == GeoPositionStatus.Ready) {
				_currentPosition = args.Position;
			}
		}
	}
}
*/