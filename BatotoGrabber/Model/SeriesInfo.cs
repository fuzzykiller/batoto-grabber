namespace BatotoGrabber.Model
{
    public class SeriesInfo
    {
        public string Name { get; set; }
        public string[] AltNames { get; set; }
        public string[] Genres { get; set; }
        public string[] Authors { get; set; }
        public string[] Artists { get; set; }
        public string Description { get; set; }
        public ChapterInfo[] Chapters { get; set; }
        public byte[] Image { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}