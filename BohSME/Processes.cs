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
		private readonly int[] acc = new int[halfLen];
		int lenReduc ;
		int minLen2 ;
		protected override void OnTick()
		{
			if (a1l0.valid){
				minLen = a1l0.len;
				a0l1.valid = true;
				a0l1.len = 1;
				lenReduc = len;
				for(int j = 0; j< ValuesConfig.reduceLen; j++){
					lenReduc = lenReduc/2;
					minLen2 = minLen/2;
					for(int i = 0; i< lenReduc; i++){
						if (i<minLen2){
							if(j==0){
								if(i==0){
									acc[0] = acc[0] + a1l0.val[i*2] + a1l0.val[i*2+1];
								}
								else{
									acc[i] = a1l0.val[i*2] + a1l0.val[i*2+1];
								}
							}
							else{
								acc[i] = acc[i*2] + acc[i*2+1];
							}
						}
						else if(i==minLen2 && minLen-minLen2!=minLen2){
							if(j==0){
								acc[i] = a1l0.val[i*2];
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
				a0l1.val[0] = acc[0];
			}
			else{
				a0l1.valid = false;
			}
		}
	}

	[ClockedProcess]
	public class repeater : SimpleProcess
	{
		[OutputBus]
		public tdata output;

		[InputBus]
		public tdata input;

		protected override void OnTick()
		{
			if (input.valid){
				output.valid=true;
				for(int i = 0; i< ValuesConfig.len -1+1; i++){
					if ( i < input.len){
						output.val[i] = input.val[i];
					}
				}
				output.len = input.len;
			}
			else{
				output.valid = false;
			}
		}
	}

}
