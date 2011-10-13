using System;
using System.Text;
using ProtoBuf;

namespace Lemon
{
	[ProtoContract]
    public enum MarkerAction
    {
		[ProtoEnum]
        Play,
		[ProtoEnum]
        Stop,
		[ProtoEnum]
        Jump,
		[ProtoEnum]
        Destroy
    }

    [ProtoContract]
    public class Marker
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        
        [ProtoMember(2)]
        public int Frame { get; set; }
        
        [ProtoMember(3)]
        public MarkerAction Action { get; set; }
        
        [ProtoMember(4)]
        public string JumpTo { get; set; }
	}
}
