using SME;
using System;
using System.Threading.Tasks;

/*Add to main program:
                        //Simulation
                        var creater = new Creater();
                        var printer = new Printer();

                        var a1 = Scope.CreateBus<tdata>();
                        creater.a1=a1;
                        printer.a2=a2;
                        inst0.a1=a1;
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

        private int len = 9;
        private int[] data = new int[]{0,1,2,3,4,5,6,7,8};

        protected override void OnTick()
        {
            if(a<len){
                a1.val=data[a];
                a=a+1;
            }
        }
    }

    public class  Printer : SimulationProcess
    {
        [InputBus]
        public tdata a2;

        public override async Task Run()
        {
            for(int i =0; i<15; i++){
                await ClockAsync();
                int a = a2.val;
            }
        }
    }
}
