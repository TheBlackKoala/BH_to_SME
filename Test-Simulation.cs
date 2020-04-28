using SME;
using System;
using System.Threading.Tasks;
using static BohSME.ValuesConfig;

/*Add to main program:
                        //Simulation
                        var creater = new Creater();
                        var printer = new Printer();
                        creater.a1 = a2;
                        var a0 = Scope.CreateBus<tdata>();
                        printer.a3=a0;
 */
namespace BohSME
{
    [InitializedBus]
    public interface CountOutput : IBus
    {
        [InitialValue(0)]
        int Dat{get;set;}
    }

    public class Creater : SimulationProcess
    {
        [OutputBus]
        public tdata a1;

        private int c = 0;

        public override async Task Run()
        {
            Random rnd = new Random();
            for(int j = 0; j<20; j++){
                await ClockAsync();
                for(int i =0; i<len; i++){
                    int val = rnd.Next(256);
                    if(val>=200)
                        c++;
                    a1.val[i]=val;
                }
                a1.valid=true;
                a1.len=len;
                Console.WriteLine("Total number of values above 200 in iteration {0} : {1}",j,c);
            }
            await ClockAsync();
            a1.valid=false;
            for (int j = 0; j<10; j++){await ClockAsync();}
            Console.WriteLine("Total number of values above 200: {0}", c);
        }
    }

    public class  Printer : SimulationProcess
    {
        [InputBus]
        public tdata a3;

        public override async Task Run()
        {
            int a = 0;
            for(int i=0; i<25; i++){
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
