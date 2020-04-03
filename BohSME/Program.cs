using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	class MainClass
	{
		public static void setup(){
			var inst0 = new instr0();
			var inst1 = new instr1();
			var repa1l2 = new repeater();
			var repa2l1 = new repeater();
			var repa2l2 = new repeater();
			var repa3l1 = new repeater();
			var repa3l2 = new repeater();
			var a2 = Scope.CreateBus<tdata>();
			repa2l1.input = a2;
			var a2l1 = Scope.CreateBus<tdata>();
			repa2l1.output = a2l1;
			repa2l2.input = a2l1;
			var a3 = Scope.CreateBus<tdata>();
			repa3l1.input = a3;
			var a3l1 = Scope.CreateBus<tdata>();
			repa3l1.output = a3l1;
			repa3l2.input = a3l1;
			var a1l1 = Scope.CreateBus<tdata>();
			inst0.a1l1 = a1l1;
			repa1l2.input = a1l1;
			inst0.a2l0 = a2;
			inst0.a3l0 = a3;
			inst0.a1l1 = a1l1;
			inst1.a1l1 = a1l1;
			var a0l2 = Scope.CreateBus<tdata>();
			inst1.a0l2 = a0l2;
                        //Simulation
                        var creater = new Creater();
                        var printer = new Printer();
                        creater.a1 = a2;
                        var a0 = Scope.CreateBus<tdata>();
                        printer.a3=a0;
			a0 = a0l2;
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
