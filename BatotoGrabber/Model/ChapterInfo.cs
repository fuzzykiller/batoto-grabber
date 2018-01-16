namespace BatotoGrabber.Model
{
    public class ChapterInfo
    {
        public string Title { get; set; }
        public string Language { get; set; }
        public GroupRef[] Groups { get; set; }
        public string Contributor { get; set; }
        public string Date { get; set; }
        public string Url { get; set; }
    }
}