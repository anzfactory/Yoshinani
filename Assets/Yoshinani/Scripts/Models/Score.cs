/*********************************
 Score
*********************************/
using System;

namespace Xyz.Anzfactory.NCMBUtil.Models
{

    [Serializable]
    public class Score
    {
        public string objectId;
        public float score;
        public string userObjectId;
        public string nickname;
        [NonSerialized]
        public bool isSelf;
    }

}
