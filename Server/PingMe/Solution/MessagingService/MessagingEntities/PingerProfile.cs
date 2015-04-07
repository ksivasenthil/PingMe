using System.Runtime.Serialization;

namespace MessagingEntities
{
    [DataContract]
    public class PingerProfile
    {
        [DataMember]
        public string PingerSource { get; set; }

        [DataMember]
        public string PingerDestination { get; set; }

        [DataMember]
        public string PingerImage { get; set; }

        [DataMember]
        public string LastMessage { get; set; }
    }
}
