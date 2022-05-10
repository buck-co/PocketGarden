using System.Collections.Generic;

namespace Buck
{
    [System.Serializable]
    public class LocalStorageData
    {
        public List<GroupData> Groups;
        public LocalStorageData(List<GroupData> groups)
        {
            Groups = groups;
        }
    }
}
