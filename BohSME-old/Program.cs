using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	class MainClass
	{
		public static void setup(){
			var instr0 = new instr0();
			var a1l1 = Scope.CreateBus<tdata>();
			instr0.a1l1 = a1l1;
			var instr1 = new instr1();
			var a0l2 = Scope.CreateBus<tdata>();
			instr1.a0l2 = a0l2;
			var repeater = new repeater();
			var output = Scope.CreateBus<tdata>();
			repeater.output = output;
			instr0.a2l0 = a2l0;
			instr0.a3l0 = a3l0;
			instr1.a1l1 = a1l1;
			repeater.input = input;
			//Connect  a2 to all l0 channels
			Simulation.Current.AddTopLevelInputs( a2 );
			//Connect  a0 to the highest level channels with the corresponding name
			Simulation.Current.AddTopLevelOutputs( a0 );
		}

		public static void Main(string[] args)
		{

			using(var sim = new Simulation())
			{

				setup();

				// Use fluent syntax to configure the simulator.
				// The order does not matter, but `Run()` must be
				// the last method called.

				// The top-level input and outputs are exposed
				// for interfacing with other VHDL code or board pins

				sim
					.AddTopLevelOutputs()
					.AddTopLevelInputs()
					.BuildCSVFile()
					.BuildVHDL()
					.Run();

				// After `Run()` has been invoked the folder
				// `output/vhdl` contains a Makefile that can
				// be used for testing the generated design
			}
		}
	}
}
