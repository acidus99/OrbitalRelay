using System;
using System.Linq;
using Gemini.Net;

namespace OrbitalRelay
{
    static class Program
    {

        static DateTime AfterTime = DateTime.Now.Subtract(TimeSpan.FromDays(7));

        static List<FeedItem> FoundItems = new List<FeedItem>();
        static int counter = 0;

        static void Main(string[] args)
        {
            var lines = File.ReadAllLines("/Users/billy/tmp/all-feeds.txt");

            Parallel.ForEach(lines, new ParallelOptions { MaxDegreeOfParallelism = 30 }, line =>
            {
                counter++;
                Console.WriteLine($"{counter} of {lines.Length}:\tChecking {line} ...");
                DoIt(line);
            });
            WriteBlast();
        }

        static void DoIt(string url)
        {
            
            var gurl = new GeminiUrl(url);

            var text = GetUrl(gurl);

            if (!String.IsNullOrEmpty(text))
            {
                Parser parser = new Parser(gurl);
                Feed feed = parser.Parse(text);
                if (feed != null)
                {
                    var found = feed.Items.Where(x => x.Published > AfterTime).ToList();

                    found.ForEach(x=> LogIt(x));
                    FoundItems.AddRange(found);
                }
            }
        }

        static void WriteBlast()
        {

            var distinct = FoundItems
                .GroupBy(x => x.Url)
                .Select(y => y.Where(z => z.IsSameSite).FirstOrDefault() ?? y.First()).ToList();

            distinct.Sort((x, y) => (x.Published != y.Published) ?
                x.Published.CompareTo(y.Published) * -1 :
                x.FeedTitle.CompareTo(y.FeedTitle));

            StreamWriter fout = new StreamWriter("/Users/billy/tmp/orbital-blast.gmi");

            var curr = DateTime.MinValue;

            fout.WriteLine("# 🛰📢 Orbital Relay");
            foreach(var item in distinct)
            {
                if(curr.Date != item.Published.Date)
                {
                    fout.WriteLine();
                    curr = item.Published;
                }

                fout.WriteLine($"=> {item.Url} {item.Published.ToString("yyyy-MM-dd")} {item.FeedTitle} {item.Title}");
            }
            fout.Close();
        }

        static void LogIt(FeedItem item)
        {
            Console.WriteLine($"{item.Url} {item.Published} {item.FeedTitle} {item.Title}");
        }

        static string GetUrl(GeminiUrl url)
        {
            GeminiRequestor requestor = new GeminiRequestor();
            var resp = requestor.Request(url);
            if(resp?.IsSuccess ?? false)
            {
                return resp.BodyText;
            }
            return "";
        }

 
    }
}
