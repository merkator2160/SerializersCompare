using ProtoBuf;
using System;

namespace SerializersCompare.Models
{
    [Serializable]
    [ProtoContract]
    public class Address
    {
        [ProtoMember(1)]
        public Int32 Value1 { get; set; }
        [ProtoMember(2)]
        public Double Value2 { get; set; }
        [ProtoMember(3)]
        public Boolean Value3 { get; set; }
    }
}