using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MessagingEntities
{
    [DataContract]
    public class PingerProfile
    {
        [DataMember]
        public string PingerImage { get; set; }

        [DataMember]
        public string LastMessage { get; set; }
    }
}
