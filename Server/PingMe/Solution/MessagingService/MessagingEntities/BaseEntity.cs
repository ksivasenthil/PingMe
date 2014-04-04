using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;

namespace MessagingEntities
{
    [DataContract]
    public class BaseEntity
    {
        private Guid _id;

        [Column]
        [DataMember]
        [Required]
        [Key]
        public Guid Id
        {
            get
            {
                bool IdDoesNotHaveValue = null == _id;
                if (IdDoesNotHaveValue)
                {
                    _id = Guid.NewGuid();
                }
                return _id;
            }
            set
            {
                _id = value;
            }
        }
    }
}
