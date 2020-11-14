using System.Runtime.Serialization;

using ServiceStack;

namespace WebAppSSRedis
{
	[DataContract]
	public class CustomUserSession : AuthUserSession
	{
		[DataMember]
		public string ExtraMetaData { get; set; }
	}
}