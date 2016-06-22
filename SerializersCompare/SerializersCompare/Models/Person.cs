﻿using ProtoBuf;
using System;

namespace SerializersCompare.Models
{
    [Serializable]
    [ProtoContract]
    public class Person
    {
        [ProtoMember(1)]
        public Int32 Id { get; set; }

        [ProtoMember(2)]
        public String Name { get; set; }

        [ProtoMember(3)]
        public Address Address { get; set; }

        [ProtoMember(4)]
        public Int32[] Phones { get; set; }
    }
}