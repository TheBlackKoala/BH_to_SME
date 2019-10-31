using SME;

namespace BohSME
{
	[InitializedBus]
	public interface tdata : IBus
	{
		[InitialValue(0)]
		int val { get; set; }
	}
}
