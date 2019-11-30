using SME;

namespace BohSME
{
	class MainClass
	{
		public static void setup(){
			var inst0 = new instr0();
			var a0l1 = Scope.CreateBus<tdata>();
			inst0.a0l1 = a0l1;
			var inst1 = new instr1();
			var a2l2 = Scope.CreateBus<tdata>();
			inst1.a2l2 = a2l2;
			var inst2 = new instr2();
			var a3l3 = Scope.CreateBus<tdata>();
			inst2.a3l3 = a3l3;
			var inst3 = new instr3();
			var a0l2 = Scope.CreateBus<tdata>();
			inst3.a0l2 = a0l2;
			var inst4 = new instr4();
			var a1l1 = Scope.CreateBus<tdata>();
			inst4.a1l1 = a1l1;
			var inst5 = new instr5();
			var a1l2 = Scope.CreateBus<tdata>();
			inst5.a1l2 = a1l2;
			inst1.a0l1 = a0l1;
			inst2.a0l2 = a0l2;
			inst2.a2l2 = a2l2;
			inst3.a0l1 = a0l1;
			inst5.a1l1 = a1l1;
			Simulation.Current.AddTopLevelInputs( a1 );
			Simulation.Current.AddTopLevelOutputs( a2 );
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
