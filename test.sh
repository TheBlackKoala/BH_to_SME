#!/bin/bash
path="testresults"
names=("arr") # "reduce" "sample")

function run()
{
    export BH_OPENMP_VERBOSE=true
    export BH_STACK=openmp
    export BH_OPENMP_PROF=true

    echo "" > $path/"$name"OMP.txt
    i=1

    while [ $i -le 10 ]
    do
        ./"$name"test.py >> $path/"$name"OMP.txt
        i=$(( i+1 ))
    done

    export BH_OPENCL_VERBOSE=true
    export BH_STACK=opencl
    export BH_OPENCL_PROF=true
    export BH_OPENMP_PROF=false

    echo "" > $path/"$name"OCL.txt
    i="0"
    while [ $i -lt 10 ]
    do
        ./"$name"test.py >> $path/"$name"OCL.txt
        i=`expr $i + 1`
    done
}

for i in ${!names[@]}
do
    name=${names[i]}
    run
done
