#!/usr/bin/python
from sme import *

class Id(SimulationProcess):
    def setup(self, ins, outs, result):
        self.map_outs(outs, "out")
        self.map_ins(ins, "inp")
    def run(self):
        result[0] = self.out["val"]
        self.out["val"] = self.inp["val"]

@extends("ckernel2.sme",
    ["−t", "trace.csv"])
class AddOne(Network):
    def wire(self, result):
        add_out = ExternalBus("addone inst.addout")
        id_out = ExternalBus("idout")
        p = Id("Id", [add_out],
               [id_out], result)
        self.add(add_out)
        self.add(id_out)
        self.add(p)

if __name__ == "__main__":
    sme = SME()
    result = [0]
    sme.network = AddOne("AddOne",
                     result)
sme.network.clock(100)
print("Final result was ", result[0])
