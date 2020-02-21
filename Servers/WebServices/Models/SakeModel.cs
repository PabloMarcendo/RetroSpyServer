using System.Runtime.Serialization;

namespace Models
{
    [DataContract]
    public class SakeModel
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}