using System;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OperaBookmarkConverter {
    class Program {
        static StringBuilder output;
        static DateTime unixTime = new DateTime(1970, 1, 1);
        // this is the time base used in the chrome/opera bookmark timestamps (microseconds since 1/1/1601)
        static DateTime chromeTime = new DateTime(1601, 1, 1);

        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Please enter the path to the bookmarks file (obc path_to_bookmark_file).");
                Console.ReadLine();
                return;
            }

            string filePath = args[0];

            if (String.IsNullOrEmpty(filePath)) {
                Console.WriteLine("Error: invalid file path ({0}).", filePath.ToString());
                Console.ReadLine();
                return;
            }

            output = new StringBuilder(@"<!DOCTYPE NETSCAPE-Bookmark-file-1>
<Title>Bookmarks</Title>
<H1>Bookmarks</H1>
<DL PERSONAL_TOOLBAR_FOLDER=""true"">");
            output.AppendLine();

            try {
                using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8)) {
                    var json = JObject.Parse(streamReader.ReadToEnd());

                    var bar = json["roots"]["bookmark_bar"]["children"];

                    if (bar == null) {
                        Console.WriteLine("Could not find bookmark bar data inside bookmarks file");
                        Console.ReadLine();
                        return;
                    }

                    Console.WriteLine("Processing...");

                    foreach (JToken child in bar.Children()) {
                        ProcessItem(child, 1);
                    }

                    output.Append("</DL>");
                }
            } catch(Exception e) {
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine("Error: Could not process file, stopped.");
            }

            //System.Diagnostics.Debug.Write(output.ToString());

            try {
                string filename = string.Format("opera_bookmarks-{0}-{1}-{2}_{3}-{4}.html", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Second);

                using (StreamWriter file = new StreamWriter(filename)) {
                    file.Write(output.ToString());
                }

                Console.WriteLine("Output saved to {0}", filename);
            } catch(Exception e) {
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine("Error: Could not write to output file, stopped.");              
            }

            Console.ReadLine();
        }

        static void ProcessItem(JToken item, int depth) {
            if (item == null)
                return;

            if (item.Value<string>("type") == "folder") {
                output.Append(' ', 4 * depth);
                output.AppendFormat("<DT><H3 FOLDED ADD_DATE={0}>{1}</H3>", ConvertDate(item.Value<long>("date_added")), item.Value<string>("name"));
                output.AppendLine();

                var children = item["children"];

                if (children != null && children.Children().Count() > 0) {
                    output.Append(' ', 4 * depth);
                    output.Append("<DL><p>");
                    output.AppendLine();

                    foreach (JToken child in children.Children()) {
                        ProcessItem(child, depth + 1);
                    }

                    output.Append(' ', 4 * depth);
                    output.Append("</DL><p>");
                    output.AppendLine();
                }
            } else if (item.Value<string>("type") == "url") {
                output.Append(' ', 4 * depth);
                output.AppendFormat("<DT><A HREF=\"{0}\" ADD_DATE={1}>{2}</A>", item.Value<string>("url"), ConvertDate(item.Value<long>("date_added")), item.Value<string>("name"));
                output.AppendLine();
            }
        }

        // the chrome/opera bookmark file stores dates as microseconds since 1/1/1601
        // the netscape bookmark file format stores them in unix time     
        static long ConvertDate(long chromeDate) {
            long seconds = chromeDate / 1000000L;

            return (long)(chromeTime.AddSeconds(seconds)).Subtract(unixTime).TotalSeconds;
        }       
    }
}
