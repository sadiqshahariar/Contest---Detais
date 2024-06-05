namespace Programmingflow.Models
{
    public class codeforcescontest
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string phase { get; set; }
        public int durationSeconds { get; set; }

        public int startTimeSeconds { get; set; }
        public int relativeTimeSeconds { get; set;}
        public string DateTime { get; set; }
    }
}
