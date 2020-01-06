using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	public static class ValuesConfig{
		public const int len = 32;
	}
	[ClockedBus]
	public interface tdata : IBus
	{
		[InitialValue(false)]
		bool valid { get; set; }
		[FixedArrayLength(ValuesConfig.len)]
		IFixedArray<int> val{ get; set; }
		[InitialValue(0)]
		int len { get; set; }
	}
}
