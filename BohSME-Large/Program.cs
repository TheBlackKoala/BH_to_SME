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
			var a3l2 = Scope.CreateBus<tdata>();
			instr1.a3l2 = a3l2;
			var instr2 = new instr2();
			var a4l3 = Scope.CreateBus<tdata>();
			instr2.a4l3 = a4l3;
			var instr3 = new instr3();
			var a0l4 = Scope.CreateBus<tdata>();
			instr3.a0l4 = a0l4;
			var repeatera1l2 = new repeatera1l2();
			var a1l2 = Scope.CreateBus<tdata>();
			repeatera1l2.a1l2 = a1l2;
			var repeatera1l3 = new repeatera1l3();
			var a1l3 = Scope.CreateBus<tdata>();
			repeatera1l3.a1l3 = a1l3;
			var repeatera1l4 = new repeatera1l4();
			var a1l4 = Scope.CreateBus<tdata>();
			repeatera1l4.a1l4 = a1l4;
			var repeatera2l1 = new repeatera2l1();
			var a2l1 = Scope.CreateBus<tdata>();
			repeatera2l1.a2l1 = a2l1;
			var repeatera2l2 = new repeatera2l2();
			var a2l2 = Scope.CreateBus<tdata>();
			repeatera2l2.a2l2 = a2l2;
			var repeatera2l3 = new repeatera2l3();
			var a2l3 = Scope.CreateBus<tdata>();
			repeatera2l3.a2l3 = a2l3;
			var repeatera2l4 = new repeatera2l4();
			var a2l4 = Scope.CreateBus<tdata>();
			repeatera2l4.a2l4 = a2l4;
			var repeatera3l3 = new repeatera3l3();
			var a3l3 = Scope.CreateBus<tdata>();
			repeatera3l3.a3l3 = a3l3;
			var repeatera3l4 = new repeatera3l4();
			var a3l4 = Scope.CreateBus<tdata>();
			repeatera3l4.a3l4 = a3l4;
			var repeatera4l4 = new repeatera4l4();
			var a4l4 = Scope.CreateBus<tdata>();
                        //Simulation for reduction
                        var creater = new Creater();
                        var printer = new Printer();

                        var a2 = Scope.CreateBus<tdata>();
                        creater.a1=a2;
                        var a0 = a0l4;
                        printer.a3=a0;
                        var a2l0= a2;
                        var a3 = a3l4;
            		repeatera4l4.a4l4 = a4l4;
			instr0.a2l0 = a2l0;
			instr1.a1l1 = a1l1;
			instr2.a1l2 = a1l2;
			instr2.a3l2 = a3l2;
			instr3.a4l3 = a4l3;
			repeatera1l2.a1l1 = a1l1;
			repeatera1l3.a1l2 = a1l2;
			repeatera1l4.a1l3 = a1l3;
			repeatera2l1.a2l0 = a2l0;
			repeatera2l2.a2l1 = a2l1;
			repeatera2l3.a2l2 = a2l2;
			repeatera2l4.a2l3 = a2l3;
			repeatera3l3.a3l2 = a3l2;
			repeatera3l4.a3l3 = a3l3;
			repeatera4l4.a4l3 = a4l3;
			//Connect  a2 to all l0 channels
			//Simulation.Current.AddTopLevelInputs( a2 );
			//Connect  a3, a0 to the highest level channels with the corresponding name
			Simulation.Current.AddTopLevelOutputs( a3, a0 );
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
