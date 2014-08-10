using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MessagingEntities
{
    [Table("Messages")]
    [DataContract]
    public class MessagePing : BaseEntity
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
    }
}
