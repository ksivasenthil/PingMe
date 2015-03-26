using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MessagingEntities
{
    [Table("Messages")]
    [DataContract]
    public class MessagePing : BaseEntity, IComparable, IEquatable<MessagePing>
    {
        [Column]
        [DataMember]
        [Required]
        [MaxLength(15), MinLength(13)]
        public string Source { get; set; }

        [Column]
        [DataMember]
        [Required]
        [MaxLength(15), MinLength(13)]
        public string Destination { get; set; }

        [Column]
        [DataMember]
        [MaxLength(139), MinLength(1)]
        public string Message { get; set; }

        [Column]
        [DataMember]
        public byte[] Asset { get; set; }

        [Column]
        [DataMember]
        [Required]
        public Nullable<DateTime> MessageSentUTC { get; set; }

        [Column]
        [DataMember]
        public Nullable<DateTime> MessageRecievedUTC { get; set; }

        public int CompareTo(object otherObject)
        {
            bool otherObjectIsMessagePing = otherObject is MessagePing;
            int returnValue = -1;
            if (otherObjectIsMessagePing)
            {
                returnValue = this.Id.CompareTo((otherObject as MessagePing).Id);
            }
            return returnValue;
        }

        public bool Equals(MessagePing other)
        {
            return this.Id.Equals(other.Id);
        }

        public override bool Equals(object otherObject)
        {
            bool otherObjectIsMessagePing = otherObject is MessagePing;
            bool returnValue = false;
            if (otherObjectIsMessagePing)
            {
                returnValue = this.Equals(otherObject as MessagePing);
            }
            return returnValue;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
