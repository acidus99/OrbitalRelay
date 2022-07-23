using System;
using System.Linq;
using System.Text.RegularExpressions;

using CodeHollow.FeedReader;
using OrbitalRelay.GemText;

using Gemini.Net;

namespace OrbitalRelay
{
	public class Parser
	{
		Regex regex = new Regex(@"^(\d{4}-\d{2}-\d{2})\s+(.+)");

		GeminiUrl FeedUrl;
		string FeedTitle;


		public Parser(GeminiUrl feedUrl)
        {
			FeedUrl = feedUrl;
        }

		public Feed Parse(string contents)
        {
			if(contents.Contains("<feed"))
            {
				var feed = ParseAtom(contents);
				if(feed != null)
                {
					return feed;
                }
            }
			return ParseGemtext(contents);

        }

		public Feed ParseAtom(string xml)
        {
			try
			{
				var feed = FeedReader.ReadFromString(xml);

				FeedTitle = feed.Title;
				return new Feed
				{
					Title = FeedTitle,
					Items = feed.Items.Select(x => CreateItem(x)).ToList()
				};
			} catch (Exception)
            {
				Console.WriteLine("ERROR: BAD FEED: " + FeedUrl);
            }
			return null;
        }


		public Feed ParseGemtext(string gemtext)
        {
			FeedTitle = TitleFinder.ExtractTitle(gemtext);

			var links = LinkFinder.ExtractBodyLinks(FeedUrl, gemtext).ToList();
			var items = links.Select(x => CreateItem(x)).ToList();


			return new Feed
			{
				Title = FeedTitle,
				Items = items.Where(x => x != null).ToList()
			};
        }

		private FeedItem CreateItem(CodeHollow.FeedReader.FeedItem feedItem)
        {
			var atomItem = (CodeHollow.FeedReader.Feeds.AtomFeedItem)feedItem.SpecificItem;

			var date = atomItem.PublishedDate ?? atomItem.UpdatedDate;

			return new FeedItem
			{
				Published = date.Value,
				FeedTitle = FeedTitle,
				FeedUrl = FeedUrl,
				Title = feedItem.Title,
				Url = new GeminiUrl(feedItem.Link)
			};
        }

		private FeedItem CreateItem(FoundLink x)
        {
			var match = regex.Match(x.LinkText);
			if(!match.Success)
            {
				return null;
            }
			try
			{
				return new FeedItem
				{
					Published = DateTime.Parse(match.Groups[1].Value),
					Title = match.Groups[2].Value,
					Url = x.Url,
					FeedTitle = FeedTitle,
					FeedUrl = FeedUrl,
				};
			} catch(FormatException)
            {

            }
			return null;
        }

	}
}

