using System.Runtime.Serialization;

namespace MessagingEntities
{
    [DataContract]
    public class PingList
    {
        [DataMember]
        public string PingerSource { get; set; }

        [DataMember]
        public string PingerDestination { get; set; }

        [DataMember]
        public PingerProfile DestinationPingerProfile { get; set; }
    }
}
