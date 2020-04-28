using SME;
using System;
using System.Threading.Tasks;
using static BohSME.ValuesConfig;

/*Add to main program:
                        //Simulation
                        var creater = new Creater();
                        var printer = new Printer();

                        var a1 = Scope.CreateBus<tdata>();
                        creater.a1=a1;
                        var a3 = a3l3;
                        var a2 = a2l3;
                        printer.a3=a3;
                        var a1l0= a1;

                        //Simulation for reduction
                        var creater = new Creater();
                        var printer = new Printer();

                        var a2 = Scope.CreateBus<tdata>();
                        creater.a1=a2;
                        var a0 = a0l4;
                        printer.a3=a0;
                        var a2l0= a2;
                        var a3 = a3l4;
 */
namespace BohSME
{
    [InitializedBus]
    public interface CountOutput : IBus
    {
        [InitialValue(0)]
        int Dat{get;set;}
    }

    [ClockedProcess]
    public class Creater : SimulationProcess
    {
        [OutputBus]
        public tdata a1;

        private int a = 0;

        public override async Task Run()
        {
            for(int j=0;;j++){
                await ClockAsync();
                for(int i =0; i<len-j; i++){
                    a1.val[i]=a+i;
                }
                a1.valid=true;
                a1.len=len-j;
                a=a+len-j;
                Console.WriteLine("Vals: {0}", a);
            }
        }
    }

    public class  Printer : SimulationProcess
    {
        [InputBus]
        public tdata a3;

        public override async Task Run()
        {
            int a = 0;
            for(int i=0; i<20; i++){
                await ClockAsync();
                Console.WriteLine("I: {0}", i);
                if(a3.valid){
                    for(int j = 0; j<a3.len; j++){
                        Console.WriteLine(a3.val[j]);
                    }
                    a += a3.len;
                    if(a>=ValuesConfig.len*3){
                        break;
                    }
                }
            }
            Simulation.Current.RequestStop();
        }
    }

    public class Sink : SimpleProcess
    {
        [InputBus]
        public tdata input;

        private int a = 0;

        protected override void OnTick()
        {
            if(input.valid){
                for(int i =0; i<len; i++){
                    if(i<input.len){
                        a= a+input.val[i];
                    }
                }
            }
        }
    }
}
