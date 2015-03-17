using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Tunepal.Core.Extensions;

namespace Tunepal.Core.Services {
	public delegate void SearchByTitleHandler(bool success, List<Tune> results);
	public delegate void SearchByTranscriptionHandler(bool success, List<Tune> results);
	public delegate void GetTuneDetailHandler(bool success, Tune tune);
	public delegate void GetTuneDotsHandler(bool success, Tune tune);
	public delegate void GetTuneDiscographyHandler(bool success, Tune tune, List<Disc> results);
	public delegate void GetSourcesHandler(bool success, List<Source> sources);

	class RequestState {
		public HttpWebRequest Request { get; set; }
		public object Handler { get; set; }
		public object Subject { get; set; }
	}

	public class TunepalWebService {
		private const string Version = "1.8";

		#region SearchByTitle

		public void SearchByTitle(string title, SearchByTitleHandler handler) {
			double lat = 0;
			double lng = 0;
			
			/*if (GlobalState.Settings.AllowSendGps) {
				var position = LocationService.MostRecentPosition;

				if (position != null) {
					lat = position.Location.Latitude;
					lng = position.Location.Longitude;
				}
			}
            */
			var qSources = from src in GlobalState.Sources
						   where src.IncludeInSearch
						   select src.Id;

			//var sourceIds = qSources.ToList().Concatenate();
            var sourceIds = "";
			if (string.IsNullOrEmpty(sourceIds) || sourceIds.StartsWith("0,") || sourceIds.Contains(",0")) {
				sourceIds = "0";
			}

		    var url = "http://tunepal.org/tunepal/search_titles.php?version={0}&q={1}&sources={2}&latitude={3}&longitude={4}&client=WinPhone7&xml=true";

		    url = string.Format(
				url, 
				Version, 
				Uri.EscapeUriString(title), 
				Uri.EscapeUriString(sourceIds), 
				lat, 
				lng
			);

		    var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
		    var state = new RequestState { Request = request, Handler = handler };

		    try {
		        request.BeginGetResponse(CompleteSearchByTitle, state);
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		private void CompleteSearchByTitle(IAsyncResult result) {
		    var state = (RequestState)result.AsyncState;
		    var request = state.Request;
		    var handler = (SearchByTitleHandler)state.Handler;

		    try {
		        var response = (HttpWebResponse)request.EndGetResponse(result);

		        if (response.StatusCode == HttpStatusCode.OK) {
		            string content;

		            using (var reader = new StreamReader(response.GetResponseStream())) {
		                content = reader.ReadToEnd();
		            }

		            using (var reader = new StringReader(content)) {
		                var doc = XDocument.Load(reader);

		                var q = from t in doc.Descendants("tune")
		                        select new Tune {
									TunepalId = (string)t.Attribute("tunepalid"),
		                            Name = (string)t.Attribute("title"),
		                            AltName = (string)t.Attribute("alt_title"),
		                            Rhythm = (string)t.Attribute("tune_type"),
		                            KeySignature = new KeySignature((string)t.Attribute("key_sig")),
									SourceId = (int)t.Attribute("sourceid")
		                        };

		                handler.Invoke(true, q.ToList());
		            }
		        } else {
		            handler.Invoke(false, null);
		        }
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		#endregion

		#region SearchByTranscription

		public void SearchByTranscription(string transcription, SearchByTranscriptionHandler handler) {
			double lat = 0;
			double lng = 0;

            /*
			var position = LocationService.MostRecentPosition;

			if (position != null) {
				lat = position.Location.Latitude;
				lng = position.Location.Longitude;
			}
            */
            /*
			var qSources = from src in GlobalState.Sources
						   where src.IncludeInSearch
						   select src.Id;
            */
            var sourceIds = ""; // qSources.ToList().Concatenate();

			if (string.IsNullOrEmpty(sourceIds) || sourceIds.StartsWith("0,") || sourceIds.Contains(",0")) {
				sourceIds = "0";
			}
			 
		    var url = "http://tunepal.org/tunepal/search_top_list_ooo.jsp?q={0}&corpus={1}&latitude={2}&longitude={3}&client=WinPhone8&xml=true";

		    url = string.Format(
				url, 
				Uri.EscapeUriString(transcription), 
				Uri.EscapeUriString(sourceIds), 
				lat, 
				lng
			);

		    var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
		    var state = new RequestState { Request = request, Handler = handler };

		    try {
		        request.BeginGetResponse(CompleteSearchByTranscription, state);
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		private void CompleteSearchByTranscription(IAsyncResult result) {
		    var state = (RequestState)result.AsyncState;
		    var request = state.Request;
		    var handler = (SearchByTranscriptionHandler)state.Handler;

		    try {
		        var response = (HttpWebResponse)request.EndGetResponse(result);

		        if (response.StatusCode == HttpStatusCode.OK) {
		            string content;

		            using (var reader = new StreamReader(response.GetResponseStream())) {
		                content = reader.ReadToEnd();
		            }

					if (!content.StartsWith("<?xml")) {
						handler.Invoke(true, null);
						return;
					}

		            using (var reader = new StringReader(content)) {
		                var doc = XDocument.Load(reader);

		                var q = from t in doc.Descendants("tune")
		                        select new Tune {
									TunepalId = (string)t.Attribute("tunepalid"),
		                            Name = (string)t.Attribute("title"),
		                            AltName = (string)t.Attribute("alt_title"),
		                            Rhythm = (string)t.Attribute("tune_type"),
		                            KeySignature = new KeySignature((string)t.Attribute("key_sig")),
									SourceId = (int)t.Attribute("sourceid"),
									Score = (1.0f - float.Parse((string)t.Attribute("normalEd"))) * 100
		                        };

		                handler.Invoke(true, q.ToList().OrderByDescending(t => t.Score).ToList());
		            }
		        } else {
		            handler.Invoke(false, null);
		        }
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		#endregion

		#region GetTuneDetail

		public void GetTuneDetail(Tune tune, GetTuneDetailHandler handler) {
		    var url = "http://tunepal.org/tunepal/get_tune.php?tunepalid={0}";
		    url = string.Format(url, Uri.EscapeUriString(tune.TunepalId));

		    var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
		    var state = new RequestState { Request = request, Handler = handler, Subject = tune };

		    try {
		        request.BeginGetResponse(CompleteGetTuneDetail, state);
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		private void CompleteGetTuneDetail(IAsyncResult result) {
		    var state = (RequestState)result.AsyncState;
		    var request = state.Request;
		    var handler = (GetTuneDetailHandler)state.Handler;
			var tune = state.Subject as Tune;

		    try {
		        var response = (HttpWebResponse)request.EndGetResponse(result);

		        if (response.StatusCode == HttpStatusCode.OK) {
		            string content;

		            using (var reader = new StreamReader(response.GetResponseStream())) {
		                content = reader.ReadToEnd();
		            }

					if (!content.StartsWith("<?xml")) {
						handler.Invoke(true, null);
						return;
					}

		            using (var reader = new StringReader(content)) {
		                var doc = XDocument.Load(reader);

		                tune.Abc = doc.Descendants("notation").First().Value;
						tune.SourceTuneId = doc.Descendants("x").First().Value;

		                handler.Invoke(true, tune);
		            }
		        } else {
		            handler.Invoke(false, null);
		        }
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		#endregion

		#region GetTuneDots

		public void GetTuneDots(Tune tune, GetTuneDotsHandler handler) {
		    var url = "http://tunepal.org/tunepal/abc2png/get_stave.php?tunepalid={0}";
		    url = string.Format(url, Uri.EscapeUriString(tune.TunepalId));

		    var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
		    var state = new RequestState { Request = request, Handler = handler, Subject = tune };

		    try {
		        request.BeginGetResponse(CompleteGetTuneDots, state);
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		private void CompleteGetTuneDots(IAsyncResult result) {
		    var state = (RequestState)result.AsyncState;
		    var request = state.Request;
		    var handler = (GetTuneDotsHandler)state.Handler;
			var tune = state.Subject as Tune;

		    try {
		        var response = (HttpWebResponse)request.EndGetResponse(result);

		        if (response.StatusCode == HttpStatusCode.OK) {
					var memStream = new MemoryStream();
					var buffer = new byte[4096];
					int bytesRead;

		            using (var stream = response.GetResponseStream()) {
		                do {
							bytesRead = stream.Read(buffer, 0, buffer.Length);
							memStream.Write(buffer, 0, bytesRead);
						}
						while (bytesRead > 0);
		            }

					tune.Dots = memStream.ToArray();

		            handler.Invoke(true, tune);
		        } else {
		            handler.Invoke(false, null);
		        }
		    }
		    catch (WebException ex) {
		        handler.Invoke(false, null);
		    }
		}

		#endregion

        #region GetTuneDiscography

        public void GetTuneDiscography(Tune tune, GetTuneDiscographyHandler handler) {
            var url = "http://tunepal.org/tunepal/search_discog.php?&q={0}";
            url = string.Format(url, Uri.EscapeUriString(tune.Name));

            var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            var state = new RequestState { Request = request, Handler = handler, Subject = tune };

            try {
                request.BeginGetResponse(CompleteGetTuneDiscography, state);
            }
            catch (WebException ex) {
                handler.Invoke(false, tune, null);
            }
        }

        private void CompleteGetTuneDiscography(IAsyncResult result) {
            var state = (RequestState)result.AsyncState;
            var request = state.Request;
            var handler = (GetTuneDiscographyHandler)state.Handler;
			var tune = state.Subject as Tune;

            try {
                var response = (HttpWebResponse)request.EndGetResponse(result);

                if (response.StatusCode == HttpStatusCode.OK) {
                    string content;

		            using (var reader = new StreamReader(response.GetResponseStream())) {
		                content = reader.ReadToEnd();
		            }

		            using (var reader = new StringReader(content)) {
		                var doc = XDocument.Load(reader);

		                var q = from album in doc.Descendants("album")
		                        select new Disc {
									Id = (string)album.Attribute("id"),
		                            Name = (string)album.Attribute("title"),
		                            Artist = (string)album.Attribute("artist")
		                        };

		                handler.Invoke(true, tune, q.ToList());
		            }
                } else {
                    handler.Invoke(false, tune, null);
                }
            }
            catch (WebException ex) {
                handler.Invoke(false, tune, null);
            }
        }

#endregion

		#region GetSources

		public void GetSources(GetSourcesHandler handler) {
			var url = "http://tunepal.org/tunepal/sources.php";

			var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
			var state = new RequestState { Request = request, Handler = handler };

			try {
				request.BeginGetResponse(CompleteGetSources, state);
			}
			catch (WebException ex) {
				handler.Invoke(false, null);
			}
		}

		private void CompleteGetSources(IAsyncResult result) {
			var state = (RequestState)result.AsyncState;
			var request = state.Request;
			var handler = (GetSourcesHandler)state.Handler;

			try {
				var response = (HttpWebResponse)request.EndGetResponse(result);

				if (response.StatusCode == HttpStatusCode.OK) {
					string content;

					using (var reader = new StreamReader(response.GetResponseStream())) {
						content = reader.ReadToEnd();
					}

					using (var reader = new StringReader(content)) {
						var doc = XDocument.Load(reader);

						var q = from t in doc.Descendants("source")
								select new Source {
									Id = (int)t.Attribute("sourceId"),
									Name = (string)t.Attribute("name"),
									Abbreviation = (string)t.Attribute("shortName"),
									Url = (string)t.Attribute("url"),
									Extra = (string)t.Attribute("extra"),
									IncludeInSearch = true
								};

						handler.Invoke(true, q.ToList());
					}
				} else {
					handler.Invoke(false, null);
				}
			}
			catch (WebException ex) {
				handler.Invoke(false, null);
			}
		}

		#endregion
	}
}
