using ProtoBuf;
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
		public Guid TransportId { get; set; }

		[ProtoMember(3)]
		public String Name { get; set; }

		[ProtoMember(4)]
		public Int32 SequenceId { get; set; }

		[ProtoMember(5)]
		public Int32[] CreditCards { get; set; }

		[ProtoMember(6)]
		public Int32 Age { get; set; }

		[ProtoMember(7)]
		public String[] Phones { get; set; }

		[ProtoMember(8)]
		public DateTime BirthDate { get; set; }

		[ProtoMember(9)]
		public Double Salary { get; set; }

		[ProtoMember(10)]
		public Boolean IsMarred { get; set; }
	}
}