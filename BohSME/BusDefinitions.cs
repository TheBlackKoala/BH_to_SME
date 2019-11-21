using SME;

namespace BohSME
{
	[ClockedBus, InitializedBus]
	public interface tdata : IBus
	{
		[InitialValue(0)]
		int val { get; set; }
	}
}
