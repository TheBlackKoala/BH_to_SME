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
                        printer.a3=a3;
                        var a1l0= a1;
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
    public class Creater : SimpleProcess
    {
        [OutputBus]
        public tdata a1;

        private int a = 0;

        protected override void OnTick()
        {
            for(int i =0; i<len; i++){
                a1.val[i]=a+i;
            }
            a1.valid=true;
            a1.len=len;
            a=a+len;
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
        }
    }
}
