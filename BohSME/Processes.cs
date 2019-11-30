using SME;

namespace BohSME
{
	[ClockedProcess]
	public class  instr0 : SimpleProcess
	{
		[OutputBus]
		public tdata a0l1;

		[InputBus]
		public tdata a1l0;

		protected override void OnTick()
		{
			a0l1.val = a1l0.val + a1l0.val;
		}
	}

	[ClockedProcess]
	public class  instr1 : SimpleProcess
	{
		[OutputBus]
		public tdata a2l2;

		[InputBus]
		public tdata a0l1;

		protected override void OnTick()
		{
			a2l2.val = a0l1.val + 2;
		}
	}

	[ClockedProcess]
	public class  instr2 : SimpleProcess
	{
		[OutputBus]
		public tdata a3l3;

		[InputBus]
		public tdata a0l2;

		[InputBus]
		public tdata a2l2;

		protected override void OnTick()
		{
			a3l3.val = a0l2.val + a2l2.val;
		}
	}

	[ClockedProcess]
	public class  repeatera0l2 : SimpleProcess
	{
		[OutputBus]
		public tdata a0l2;

		[InputBus]
		public tdata a0l1;

		protected override void OnTick()
		{
			a0l2.val =a0l1.val;
		}
	}

	[ClockedProcess]
	public class  repeatera1l1 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l1;

		[InputBus]
		public tdata a1l0;

		protected override void OnTick()
		{
			a1l1.val =a1l0.val;
		}
	}

	[ClockedProcess]
	public class  repeatera1l2 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l2;

		[InputBus]
		public tdata a1l1;

		protected override void OnTick()
		{
			a1l2.val =a1l1.val;
		}
	}

}
