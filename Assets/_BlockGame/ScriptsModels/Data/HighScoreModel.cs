using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;

namespace ArtTest.Models
{
    [Serializable, DataContract, Preserve]
    public struct HighScoreEntry
    {
        [DataMember]
        public string PlayerName;
        [DataMember]
        public int Score;
    }
}
