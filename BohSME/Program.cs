using SME;
using static BohSME.ValuesConfig;

namespace BohSME
{
	class MainClass
	{
		public static void setup(){
			var inst0 = new instr0();
			var repa1l1 = new repeater();
			var a1 = Scope.CreateBus<tdata>();
			repa1l1.input = a1;
			inst0.a1l0 = a1;
			var a0l1 = Scope.CreateBus<tdata>();
			inst0.a0l1 = a0l1;
			var a0 = a0l1;

                        //Simulation for reduction
                        var creater = new Creater();
                        var printer = new Printer();
                        var sink = new Sink();

                        creater.a1=a1;
                        printer.a3=a0l1;
                        repa1l1.output= Scope.CreateBus<tdata>();
                        sink.input=repa1l1.output;

			//Connect  a1 to all l0 channels
			Simulation.Current.AddTopLevelInputs( a1 );
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
