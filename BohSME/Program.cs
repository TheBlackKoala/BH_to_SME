using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	class MainClass
	{
		public static void setup(){
			var instr0 = new instr0();
			var a0l1 = Scope.CreateBus<tdata>();
			instr0.a0l1 = a0l1;
			var instr1 = new instr1();
			var a2l2 = Scope.CreateBus<tdata>();
			instr1.a2l2 = a2l2;
			var instr2 = new instr2();
			var a3l3 = Scope.CreateBus<tdata>();
			instr2.a3l3 = a3l3;
			var repeatera0l2 = new repeatera0l2();
			var a0l2 = Scope.CreateBus<tdata>();
			repeatera0l2.a0l2 = a0l2;
			var repeatera0l3 = new repeatera0l3();
			var a0l3 = Scope.CreateBus<tdata>();
			repeatera0l3.a0l3 = a0l3;
			var repeatera1l1 = new repeatera1l1();
			var a1l1 = Scope.CreateBus<tdata>();
			repeatera1l1.a1l1 = a1l1;
			var repeatera1l2 = new repeatera1l2();
			var a1l2 = Scope.CreateBus<tdata>();
			repeatera1l2.a1l2 = a1l2;
			var repeatera1l3 = new repeatera1l3();
			var a1l3 = Scope.CreateBus<tdata>();
			repeatera1l3.a1l3 = a1l3;
			var repeatera2l3 = new repeatera2l3();
			var a2l3 = Scope.CreateBus<tdata>();

                        //Simulation
                        var creater = new Creater();
                        var printer = new Printer();

                        var a1 = Scope.CreateBus<tdata>();
                        creater.a1=a1;
                        var a3 = a3l3;
                        printer.a3=a3;
                        var a1l0= a1;

			repeatera2l3.a2l3 = a2l3;
			instr0.a1l0 = a1l0;
			instr1.a0l1 = a0l1;
			instr2.a0l2 = a0l2;
			instr2.a2l2 = a2l2;
			repeatera0l2.a0l1 = a0l1;
			repeatera0l3.a0l2 = a0l2;
			repeatera1l1.a1l0 = a1l0;
			repeatera1l2.a1l1 = a1l1;
			repeatera1l3.a1l2 = a1l2;
			repeatera2l3.a2l2 = a2l2;
			//Connect  a1 to all l0 channels
			//Simulation.Current.AddTopLevelInputs( a1 );
			//Connect  a2 to the highest level channels with the corresponding name
			Simulation.Current.AddTopLevelOutputs( a3 );
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
