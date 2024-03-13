using Google.Maps;
using Google.Maps.StaticMaps;

namespace MVCtest3d.Other
{
    public class MapsCollector
    {
        public static Uri GenerateMap(string location)
        {
            GoogleSigned.AssignAllServices(new GoogleSigned("AIzaSyBrc1jgMPrlzvkqYWLWhu0gQcjIfwTLdfc"));

            var map = new StaticMapRequest();

            map.Center = new Location(location);
            map.Size = new Google.Maps.MapSize(400, 400);
            map.Zoom = 12;

            var staticMapUrl = new UriBuilder(map.ToUri())
            {
                Query = $"key=AIzaSyBrc1jgMPrlzvkqYWLWhu0gQcjIfwTLdfc&secret=E3Iwv-pXttZKaVcmNxRNgDLs1mk=&size={map.Size.Width}x{map.Size.Height}&&center={map.Center}&markers=color:red|{map.Center}"
            }.Uri;

            return staticMapUrl;
        }
    }
}
