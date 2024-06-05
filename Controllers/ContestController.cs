using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

public class ContestController : Controller
{
    public IActionResult Contest()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetContestListcodeforces()
    {
        try
        {
            // Create an instance of HtmlWeb to load the webpage
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync("https://codeforces.com/contests");

            // Select the table containing upcoming contests
            var upcomingContestsTable = doc.DocumentNode.SelectSingleNode("//div[@class='datatable']//table");

            if (upcomingContestsTable == null)
            {
                return StatusCode(500, "Failed to find the contests table on the Codeforces page.");
            }

            // Extract rows from the table
            var rows = upcomingContestsTable.SelectNodes(".//tr");

            if (rows == null || rows.Count == 0)
            {
                return StatusCode(500, "No contests found on the Codeforces page.");
            }

            var upcomingContests = new List<dynamic>();

            foreach (var row in rows.Skip(1)) // Skip the header row
            {
                var columns = row.SelectNodes(".//td");
                if (columns != null && columns.Count >= 6)
                {
                    // Extract contest details
                    string contestId = row.GetAttributeValue("data-contestId", "");
                    string name = columns[0].InnerText.Trim();
                    string startTimeStr = columns[2].InnerText.Trim();
                    string durationStr = columns[3].InnerText.Trim();

                    // Parse start time and duration
                    if (DateTime.TryParseExact(startTimeStr, "MMM/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startTime) &&
                        TimeSpan.TryParse(durationStr, out TimeSpan duration))
                    {
                        startTime = startTime.AddHours(3);
                        var formattedStartTime = startTime.ToString("dd MMMM hh:mm tt", CultureInfo.InvariantCulture);

                        var contest = new
                        {
                            Id = contestId,
                            Name = name,
                            StartTime = formattedStartTime,
                            Duration = durationStr
                        };

                        upcomingContests.Add(contest);
                    }
                }
            }

            // Sort contests by start time
            upcomingContests = upcomingContests
                .OrderBy(c => DateTime.ParseExact(c.StartTime, "dd MMMM hh:mm tt", CultureInfo.InvariantCulture))
                .ToList();

            return Ok(upcomingContests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    //atcoder
    [HttpGet]
    public async Task<IActionResult> GetContestListAtcoder()
    {
        try
        {
            // Create an instance of HtmlWeb to load the webpage
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync("https://atcoder.jp/contests/");

            // Select the table containing upcoming contests
            var upcomingContestsTable = doc.DocumentNode.SelectSingleNode("//div[@id='contest-table-upcoming']//table");

            if (upcomingContestsTable == null)
            {
                return StatusCode(500, "Failed to find the contests table on the Atcoder page.");
            }

            // Extract rows from the table
            var rows = upcomingContestsTable.SelectNodes(".//tr");

            if (rows == null || rows.Count == 0)
            {
                return StatusCode(500, "No contests found on the Atcoder page.");
            }

            var upcomingContests = new List<dynamic>();

            foreach (var row in rows.Skip(1)) // Skip the header row
            {
                var columns = row.SelectNodes(".//td");
                if (columns != null && columns.Count >= 4)
                {
                    // Extract contest details
                    string contestLink = columns[1].SelectSingleNode(".//a").GetAttributeValue("href", "");
                    string contestId = row.SelectSingleNode(".//a").GetAttributeValue("href", "").Split('/').Last();
                    string name = columns[1].InnerText.Trim();
                    string startTimeStr = columns[0].InnerText.Trim();
                    string durationStr = columns[2].InnerText.Trim();

                    contestLink = "https://atcoder.jp" + contestLink;

                    name = Regex.Replace(name, @"[^\w\s\-]", "");
                    // Log or print the input strings for debugging
                    Console.WriteLine($"Parsing: startTimeStr={startTimeStr}, durationStr={durationStr}");

                    // Parse start time and duration
                    if (DateTimeOffset.TryParseExact(startTimeStr, "yyyy-MM-dd HH:mm:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset startTimeOffset) &&
                        TimeSpan.TryParse(durationStr, out TimeSpan duration))
                    {
                        // Convert to UTC, add 3 hours, and convert to local time
                        DateTimeOffset startTimeAdjusted = startTimeOffset.ToUniversalTime().AddHours(3).ToLocalTime();
                        var formattedStartTime = startTimeAdjusted.ToString("dd MMMM hh:mm tt", CultureInfo.InvariantCulture);
                        string[] nameParts = name.Split(new[] { "AtCoder" }, StringSplitOptions.None);
                        string cleanName = "AtCoder " + nameParts.Last().Trim();
                        // Only include upcoming contests
                        if (startTimeAdjusted > DateTimeOffset.Now)
                        {
                            var contest = new
                            {
                                Id = contestId,
                                Name = cleanName,
                                StartTime = formattedStartTime,
                                Duration = durationStr,
                                Link = contestLink
                            };

                            upcomingContests.Add(contest);
                        }
                    }
                    else
                    {
                        // Log parsing errors
                        Console.WriteLine($"Failed to parse: startTimeStr={startTimeStr}, durationStr={durationStr}");
                    }
                }
            }

            // Sort contests by start time
            upcomingContests = upcomingContests
                .OrderBy(c => DateTime.ParseExact(c.StartTime, "dd MMMM hh:mm tt", CultureInfo.InvariantCulture))
                .ToList();

            return Ok(upcomingContests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    //codechef

    [HttpGet]
    public async Task<IActionResult> GetContestListCodeChef()
    {
        try
        {
            // Create an instance of HtmlWeb to load the webpage
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync("https://www.codechef.com/contests");

            // Select the table containing upcoming contests
            var upcomingContestsTable = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'future-contests')]//table[contains(@class, 'dataTable')]");

            if (upcomingContestsTable == null)
            {
                return StatusCode(500, "Failed to find the contests table on the CodeChef page.");
            }

            // Extract rows from the table
            var rows = upcomingContestsTable.SelectNodes(".//tr");

            if (rows == null || rows.Count == 0)
            {
                return StatusCode(500, "No contests found on the CodeChef page.");
            }

            var upcomingContests = new List<dynamic>();

            foreach (var row in rows.Skip(1)) // Skip the header row
            {
                var columns = row.SelectNodes(".//td");
                if (columns != null && columns.Count >= 4)
                {
                    // Extract contest details
                    string contestLink = columns[1].SelectSingleNode(".//a").GetAttributeValue("href", "");
                    string contestId = contestLink.Split('/').Last();
                    string name = columns[1].InnerText.Trim();
                    string startTimeStr = columns[2].InnerText.Trim();
                    string endTimeStr = columns[3].InnerText.Trim();

                    contestLink = "https://www.codechef.com" + contestLink;

                    name = Regex.Replace(name, @"[^\w\s\-]", "");
                    // Log or print the input strings for debugging
                    Console.WriteLine($"Parsing: startTimeStr={startTimeStr}, endTimeStr={endTimeStr}");

                    // Parse start time and duration
                    if (DateTimeOffset.TryParseExact(startTimeStr, "dd MMM yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset startTimeOffset) &&
                        DateTimeOffset.TryParseExact(endTimeStr, "dd MMM yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset endTimeOffset))
                    {
                        // Convert to UTC, add 3 hours, and convert to local time
                        DateTimeOffset startTimeAdjusted = startTimeOffset.ToUniversalTime().AddHours(3).ToLocalTime();
                        var formattedStartTime = startTimeAdjusted.ToString("dd MMMM hh:mm tt", CultureInfo.InvariantCulture);
                        string durationStr = (endTimeOffset - startTimeOffset).ToString(@"hh\:mm");

                        // Only include upcoming contests
                        if (startTimeAdjusted > DateTimeOffset.Now)
                        {
                            var contest = new
                            {
                                Id = contestId,
                                Name = name,
                                StartTime = formattedStartTime,
                                Duration = durationStr,
                                Link = contestLink
                            };

                            upcomingContests.Add(contest);
                        }
                    }
                    else
                    {
                        // Log parsing errors
                        Console.WriteLine($"Failed to parse: startTimeStr={startTimeStr}, endTimeStr={endTimeStr}");
                    }
                }
            }

            // Sort contests by start time
            upcomingContests = upcomingContests
                .OrderBy(c => DateTime.ParseExact(c.StartTime, "dd MMMM hh:mm tt", CultureInfo.InvariantCulture))
                .ToList();

            return Ok(upcomingContests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

}
