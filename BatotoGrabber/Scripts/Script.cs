using System.IO;

namespace BatotoGrabber.Scripts
{
    public static class Script
    {
        public static string GetFollowedSeries => GetResource("GetFollowedSeries.js");
        public static string GetSeriesInfo => GetResource("GetSeriesInfo.js");
        public static string GetGroupInfo => GetResource("GetGroupInfo.js");
        public static string CreateDatabbase => GetResource("CreateDatabase.sql");

        private static string GetResource(string localName)
        {
            var t = typeof(Script);

            using (var sr = new StreamReader(t.Assembly.GetManifestResourceStream(t, localName)))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
