using SME;

namespace BohSME
{
	[ClockedProcess]
	public class  instr0 : SimpleProcess
	{
		[OutputBus]
		public tdata a0;

		[InputBus]
		public tdata a1;

		protected override void OnTick()
		{
			a0.val = a1.val + a1.val;
		}
	}

	[ClockedProcess]
	public class  instr1 : SimpleProcess
	{
		[OutputBus]
		public tdata a2;

		[InputBus]
		public tdata a0;

		protected override void OnTick()
		{
			a2.val = a0.val + 2;
		}
	}

}
