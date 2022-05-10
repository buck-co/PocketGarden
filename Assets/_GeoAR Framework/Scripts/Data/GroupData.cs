using System.Collections.Generic;

namespace Buck
{
    [System.Serializable]
    public class GroupData
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Heading;
        public List<PlaceableObjectData> PlaceableDataList;

        public GroupData(double latitude, double longitude, double altitude, double heading, List<PlaceableObjectData> placeableDataList)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Heading = heading;
            PlaceableDataList = placeableDataList;
        }
    }
}
