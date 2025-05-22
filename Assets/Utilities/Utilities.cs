using System.Collections.Generic;
using UnityEngine;

namespace ArtTest.Utilities
{
    public static class Utilities
    {
        private static Dictionary<string, FileIO> fileIOMap;

        public static FileIO GetFileIO(string key = "default")
        {
            fileIOMap ??= new Dictionary<string, FileIO>();

            if (!fileIOMap.ContainsKey(key))
            {
                fileIOMap[key] = new FileIO();
            }

            return fileIOMap[key];
        }
    }
}
