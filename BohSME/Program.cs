using SME;

namespace BohSME
{
	class MainClass
	{
		public static void setup(){
			var inst0 = new instr0();
			var a0 = Scope.CreateBus<tdata>();
			inst0.a0 = a0;
			//Connect inst1.a2 to simulation output for a2 - do not create a channel
			var inst1 = new instr1();
			var a2 = Scope.CreateBus<tdata>();
			inst1.a2 = a2;
			//Connect inst0.a1 to simulation input for a1 - create it as a channel
			inst1.a0 = a0;

                        //Simulation
                        var creater = new Creater();
                        var printer = new Printer();

                        var a1 = Scope.CreateBus<tdata>();
                        creater.a1=a1;
                        printer.a2=a2;
                        inst0.a1=a1;

                        //Simulation.Current.AddTopLevelInputs( a1 );
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
