using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	[ClockedProcess]
	public class instr0 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l1;

		[InputBus]
		public tdata a2l0;

		int minLen ;
		protected override void OnTick()
		{
			if (a2l0.valid){
				minLen = a2l0.len;
				a1l1.valid = true;
				a1l1.len = minLen;
				for(int i = 0; i< ValuesConfig.len - 1+1; i++){
					if (i<minLen){
						a1l1.val[i] = a2l0.val[i] + a2l0.val[i];
					}
				}
			}
			else{
				a1l1.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class instr1 : SimpleProcess
	{
		[OutputBus]
		public tdata a3l2;

		[InputBus]
		public tdata a1l1;

		int minLen ;
		protected override void OnTick()
		{
			if (a1l1.valid){
				minLen = a1l1.len;
				a3l2.valid = true;
				a3l2.len = minLen;
				for(int i = 0; i< ValuesConfig.len - 1+1; i++){
					if (i<minLen){
						a3l2.val[i] = a1l1.val[i] - 2;
					}
				}
			}
			else{
				a3l2.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class instr2 : SimpleProcess
	{
		[OutputBus]
		public tdata a4l3;

		[InputBus]
		public tdata a1l2;

		[InputBus]
		public tdata a3l2;

		int minLen ;
		protected override void OnTick()
		{
			if (a1l2.valid && a3l2.valid){
				minLen = a1l2.len;
				if (minLen > a3l2.len){
					minLen = a3l2.len;
				}
				a4l3.valid = true;
				a4l3.len = minLen;
				for(int i = 0; i< ValuesConfig.len - 1+1; i++){
					if (i<minLen){
						a4l3.val[i] = a1l2.val[i] + a3l2.val[i];
					}
				}
			}
			else{
				a4l3.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class instr3 : SimpleProcess
	{
		[OutputBus]
		public tdata a0l4;

		[InputBus]
		public tdata a4l3;

		int minLen ;
		private readonly int[] acc = new int[len/2];
		int lenReduc ;
		int minLen2 ;
		protected override void OnTick()
		{
			if (a4l3.valid){
				minLen = a4l3.len;
				a0l4.valid = true;
				a0l4.len = 1;
				lenReduc = len;
				for(int j = 0; j< ValuesConfig.reduceLen; j++){
					lenReduc = lenReduc/2;
					minLen2 = minLen/2;
					for(int i = 0; i< lenReduc; i++){
						if (i<minLen2){
							if(j==0){
								if(i==0){
									acc[0] = acc[0] + a4l3.val[i*2] + a4l3.val[i*2+1];
								}
								else{
									acc[i] = a4l3.val[i*2] + a4l3.val[i*2+1];
								}
							}
							else{
								acc[i] = acc[i*2] + acc[i*2+1];
							}
						}
						else if(i==minLen2 && minLen-minLen2!=minLen2){
							if(j==0){
								acc[i] = a4l3.val[i*2];
							}
							else{
								acc[i] = acc[i*2];
							}
						}
					}
					if(minLen-minLen2!=minLen2){
						minLen = minLen2+1;
					}
					else{
						minLen= minLen2;
					}
				}
				a0l4.val[0] = acc[0];
			}
			else{
				a0l4.valid = false;
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
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
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
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
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
	public class repeatera1l4 : SimpleProcess
	{
		[OutputBus]
		public tdata a1l4;

		[InputBus]
		public tdata a1l3;

		protected override void OnTick()
		{
			if (a1l3.valid){
				a1l4.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a1l3.len){
						a1l4.val[i] = a1l3.val[i];
					}
				}
				a1l4.len = a1l3.len;
			}
			else{
				a1l4.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera2l1 : SimpleProcess
	{
		[OutputBus]
		public tdata a2l1;

		[InputBus]
		public tdata a2l0;

		protected override void OnTick()
		{
			if (a2l0.valid){
				a2l1.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a2l0.len){
						a2l1.val[i] = a2l0.val[i];
					}
				}
				a2l1.len = a2l0.len;
			}
			else{
				a2l1.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera2l2 : SimpleProcess
	{
		[OutputBus]
		public tdata a2l2;

		[InputBus]
		public tdata a2l1;

		protected override void OnTick()
		{
			if (a2l1.valid){
				a2l2.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a2l1.len){
						a2l2.val[i] = a2l1.val[i];
					}
				}
				a2l2.len = a2l1.len;
			}
			else{
				a2l2.valid = false;
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
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
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

	[ClockedProcess]
	public class repeatera2l4 : SimpleProcess
	{
		[OutputBus]
		public tdata a2l4;

		[InputBus]
		public tdata a2l3;

		protected override void OnTick()
		{
			if (a2l3.valid){
				a2l4.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a2l3.len){
						a2l4.val[i] = a2l3.val[i];
					}
				}
				a2l4.len = a2l3.len;
			}
			else{
				a2l4.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera3l3 : SimpleProcess
	{
		[OutputBus]
		public tdata a3l3;

		[InputBus]
		public tdata a3l2;

		protected override void OnTick()
		{
			if (a3l2.valid){
				a3l3.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a3l2.len){
						a3l3.val[i] = a3l2.val[i];
					}
				}
				a3l3.len = a3l2.len;
			}
			else{
				a3l3.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera3l4 : SimpleProcess
	{
		[OutputBus]
		public tdata a3l4;

		[InputBus]
		public tdata a3l3;

		protected override void OnTick()
		{
			if (a3l3.valid){
				a3l4.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a3l3.len){
						a3l4.val[i] = a3l3.val[i];
					}
				}
				a3l4.len = a3l3.len;
			}
			else{
				a3l4.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeatera4l4 : SimpleProcess
	{
		[OutputBus]
		public tdata a4l4;

		[InputBus]
		public tdata a4l3;

		protected override void OnTick()
		{
			if (a4l3.valid){
				a4l4.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < a4l3.len){
						a4l4.val[i] = a4l3.val[i];
					}
				}
				a4l4.len = a4l3.len;
			}
			else{
				a4l4.valid = false;
			}
		}
	}

}
