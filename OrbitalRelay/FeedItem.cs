using System;
using Gemini.Net;
namespace OrbitalRelay
{
	public class FeedItem
	{

		public string FeedTitle { get; set; }


		public DateTime Published { get; set; }

		public GeminiUrl Url { get; set; }

		public GeminiUrl FeedUrl { get; set; }

		public bool IsSameSite
			=> (Url.Hostname == FeedUrl.Hostname);

		public string Title { get; set; }
	}
}

