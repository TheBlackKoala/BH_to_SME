using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	[ClockedProcess]
	public class instr0 : SimpleProcess
	{
		[OutputBus]
		public tdata a0l1;

		[InputBus]
		public tdata a1l0;

	int minLen ;
		protected override void OnTick()
		{
			if (a1l0.valid){
				minLen = a1l0.len;
				a0l1.valid = true;
				a0l1.len = minLen;
				for(int i = 0; i<ValuesConfig.len - 1+1; i++){
					if (i<minLen){
						a0l1.val[i] = a1l0.val[i] + a1l0.val[i];
					}
				}
			}
			else{
				a0l1.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class instr1 : SimpleProcess
	{
		[OutputBus]
		public tdata a2l2;

		[InputBus]
		public tdata a0l1;

	int minLen ;
		protected override void OnTick()
		{
			if (a0l1.valid){
				minLen = a0l1.len;
				a2l2.valid = true;
				a2l2.len = minLen;
				for(int i = 0; i<ValuesConfig.len - 1+1; i++){
					if (i<minLen){
						a2l2.val[i] = a0l1.val[i] + 2;
					}
				}
			}
			else{
				a2l2.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class instr2 : SimpleProcess
	{
		[OutputBus]
		public tdata a3l3;

		[InputBus]
		public tdata a0l2;

		[InputBus]
		public tdata a2l2;

	int minLen ;
		protected override void OnTick()
		{
			if (a0l2.valid && a2l2.valid){
				minLen = a0l2.len;
				if (minLen > a2l2.len){
					minLen = a2l2.len;
				}
				a3l3.valid = true;
				a3l3.len = minLen;
				for(int i = 0; i<ValuesConfig.len - 1+1; i++){
					if (i<minLen){
						a3l3.val[i] = a0l2.val[i] + a2l2.val[i];
					}
				}
			}
			else{
				a3l3.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera0l2 : SimpleProcess
	{
		[OutputBus]
		public tdata a0l2;

		[InputBus]
		public tdata a0l1;

		protected override void OnTick()
		{
			if (a0l1.valid){
				a0l2.valid=true;
				for(int i = 0; i<ValuesConfig.len -1+1; i++){
					if ( i < a0l1.len){
						a0l2.val[i] = a0l1.val[i];
					}
				}
				a0l2.len = a0l1.len;
			}
			else{
				a0l2.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera0l3 : SimpleProcess
	{
		[OutputBus]
		public tdata a0l3;

		[InputBus]
		public tdata a0l2;

		protected override void OnTick()
		{
			if (a0l2.valid){
				a0l3.valid=true;
				for(int i = 0; i<ValuesConfig.len -1+1; i++){
					if ( i < a0l2.len){
						a0l3.val[i] = a0l2.val[i];
					}
				}
				a0l3.len = a0l2.len;
			}
			else{
				a0l3.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera1l1 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l1;

		[InputBus]
		public tdata a1l0;

		protected override void OnTick()
		{
			if (a1l0.valid){
				a1l1.valid=true;
				for(int i = 0; i<ValuesConfig.len -1+1; i++){
					if ( i < a1l0.len){
						a1l1.val[i] = a1l0.val[i];
					}
				}
				a1l1.len = a1l0.len;
			}
			else{
				a1l1.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera1l2 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l2;

		[InputBus]
		public tdata a1l1;

		protected override void OnTick()
		{
			if (a1l1.valid){
				a1l2.valid=true;
				for(int i = 0; i<ValuesConfig.len -1+1; i++){
					if ( i < a1l1.len){
						a1l2.val[i] = a1l1.val[i];
					}
				}
				a1l2.len = a1l1.len;
			}
			else{
				a1l2.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera1l3 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l3;

		[InputBus]
		public tdata a1l2;

		protected override void OnTick()
		{
			if (a1l2.valid){
				a1l3.valid=true;
				for(int i = 0; i<ValuesConfig.len -1+1; i++){
					if ( i < a1l2.len){
						a1l3.val[i] = a1l2.val[i];
					}
				}
				a1l3.len = a1l2.len;
			}
			else{
				a1l3.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera2l3 : SimpleProcess
	{
		[OutputBus]
		public tdata a2l3;

		[InputBus]
		public tdata a2l2;

		protected override void OnTick()
		{
			if (a2l2.valid){
				a2l3.valid=true;
				for(int i = 0; i<ValuesConfig.len -1+1; i++){
					if ( i < a2l2.len){
						a2l3.val[i] = a2l2.val[i];
					}
				}
				a2l3.len = a2l2.len;
			}
			else{
				a2l3.valid = false;
			}
		}
	}

}
