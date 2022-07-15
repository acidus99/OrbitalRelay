using System;
namespace OrbitalRelay
{
	public class Feed
	{
		public string Title { get; set; }

		public List<FeedItem> Items { get; set; } = new List<FeedItem>();
	}
}

