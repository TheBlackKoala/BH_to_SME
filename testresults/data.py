#!/usr/bin/python
import numpy as np

def readInput(inputfile):
    inp = open(inputfile,"r")
    line = inp.readline()

    numbers = list()
    while line:
        numbers.append(float(line))
        line=inp.readline()
    op = numbers[-1]
    numbers=np.array(numbers[:-1])
    ops = op/numbers
    avg = np.average(ops)
    diff = max(np.abs(avg-ops)/avg)
    return(avg,diff)

files = ["arrOMPt.txt","arrOCLt.txt","reduceOMPt.txt","reduceOCLt.txt","sampleOMPt.txt","sampleOCLt.txt"]
for f in files:
    print(f.split("t.txt")[0],"avg: %.0f diff: %.03f" % readInput(f))
