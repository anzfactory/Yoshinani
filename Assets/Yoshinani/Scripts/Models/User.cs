/*********************************
 users
*********************************/
using System;

namespace Xyz.Anzfactory.NCMBUtil.Models
{

    [Serializable]
    public class User
    {
        public string objectId;
        public string userName;
        public string password;
        public string nickname;
        public string sessionToken;
    }

}
