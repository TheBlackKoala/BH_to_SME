#!/bin/python
import os

projectname = "BohSME"
inputfile = "ckernel2.sme"
#Input file
inp = open(inputfile,"r")
#Output file directory
outputdir = "BohSME"
if not os.path.exists(outputdir):
    os.mkdir(outputdir)

outputfiles = [projectname+".csproj","BusDefinitions.cs","Processes.cs", "Program.cs"]

outps = [open(outputdir+"/"+f,"w+") for f in outputfiles]

#Write the csproj file
outps[0].write("<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"SME\" Version=\"0.4.0-beta\" />\n    <PackageReference Include=\"SME.Tracer\" Version=\"0.4.0-beta\" />\n    <PackageReference Include=\"SME.VHDL\" Version=\"0.4.0-beta\" />\n    <PackageReference Include=\"SME.GraphViz\" Version=\"0.4.0-beta\" />\n    \n    <PackageReference Include=\"System.Drawing.Common\" Version=\"4.5.1\" />\n    <PackageReference Include=\"runtime.linux-x64.CoreCompat.System.Drawing\" Version=\"1.0.0-beta002\" />\n  </ItemGroup>\n\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>netcoreapp2.0</TargetFramework>\n    <RootNamespace>"\
               + projectname+\
               "</RootNamespace>\n  </PropertyGroup>\n\n</Project>\n")

#the csproj has been written
outps[0].close()
outps = outps[1:]

def writeStart(f):
    f.write("using SME;\n\n")
    f.write("namespace " + projectname + "\n{\n")

def endMainFile(f):
    f.write("\t\t}\n\n")
    f.write("\t\tpublic static void Main(string[] args)\n\t\t{\n\n\t\t\tusing(var sim = new Simulation())\n\t\t\t{\n\n\t\t\t\tsetup();\n\n\t\t\t\t// Use fluent syntax to configure the simulator.\n\t\t\t\t// The order does not matter, but `Run()` must be\n\t\t\t\t// the last method called.\n\n\t\t\t\t// The top-level input and outputs are exposed\n\t\t\t\t// for interfacing with other VHDL code or board pins\n\n\t\t\t\tsim\n\t\t\t\t\t.AddTopLevelOutputs()\n\t\t\t\t\t.AddTopLevelInputs()\n\t\t\t\t\t.BuildCSVFile()\n\t\t\t\t\t.BuildVHDL()\n\t\t\t\t\t.Run();\n\n\t\t\t\t// After `Run()` has been invoked the folder\n\t\t\t\t// `output/vhdl` contains a Makefile that can\n\t\t\t\t// be used for testing the generated design\n\t\t\t}\n\t\t}\n\t}\n}\n")

def parse(inp):
    index = 0
    line = inp.readline()
    #Whether the next channels is an output
    output = False
    #Number of instructions
    insts = 0
    #Name of all repeaters
    reps = list()
    #In channels
    ins = list()
    #Out channels
    outs = list()
    #Channels that either go out from or in to the network.
    ends = [list(),list()]
    curProc = ""
    writeStart(outps[index])
    while(line != ''):
        #To setup the bus
        if(index==0):
            #Go on to the next file - write the processes
            if "proc" in line:
                outps[index].write("}\n")
                index +=1
                writeStart(outps[index])
                continue
            #Hardcoded bus - C# is too different from smeil to be easily translatable in this respect
            else:
                if"tdata" in line:
                    outps[index].write("\t[ClockedBus, InitializedBus]\n")
                    outps[index].write("\tpublic interface tdata : IBus\n")
                    outps[index].write("\t{\n")
                    outps[index].write("\t\t[InitialValue(0)]\n")
                    outps[index].write("\t\tint val { get; set; }\n")
                    outps[index].write("\t}\n")
        #Write the processes
        elif(index==1):
            if "network" in line:
                outps[index].write("}\n")
                index+=1
                writeStart(outps[index])
                continue
            else:
                if "proc" in line:
                    #Write instructions
                    if "instr" in line:
                        insts += 1
                        line = line.split("proc ")[1].split("()")[0]
                        outps[index].write("\t[ClockedProcess]\n")
                        outps[index].write("\tpublic class " + line + " : SimpleProcess\n")
                        outps[index].write("\t{\n")
                        curProc=line
                    #Write repeaters
                    elif "repeater" in line:
                        line = line.split("proc ")[1].split("()")[0]
                        reps.append(line)
                        outps[index].write("\t[ClockedProcess]\n")
                        outps[index].write("\tpublic class " + line + " : SimpleProcess\n")
                        outps[index].write("\t{\n")
                        curProc=line
                #Write the buses
                elif line.startswith("\tbus"):
                    bus = line.split("bus ")[1].split(" {")[0].split(": ")[0]
                    if(output):
                        outps[index].write("\t\t[OutputBus]\n")
                        output= False
                        outs.append((bus,curProc))
                        ins.append(list())
                    else:
                        outps[index].write("\t\t[InputBus]\n")
                        ins[len(ins)-1].append((bus,curProc))
                    outps[index].write("\t\tpublic tdata " + bus + ";\n\n")
                #The next bus will be an output bus
                elif line == "\t//Output\n":
                    output=True
                #create the processes function
                elif "val =" in line:
                    outps[index].write("\t\tprotected override void OnTick()\n")
                    outps[index].write("\t\t{\n")
                    outps[index].write("\t\t"+line)
                    outps[index].write("\t\t}\n")
                elif line=="}\n":
                    outps[index].write("\t}\n\n")
        else:
            #Write the setup of the network
            if line.startswith("network"):
                outps[index].write("\tclass MainClass\n")
                outps[index].write("\t{\n")
                outps[index].write("\t\tpublic static void setup(){\n")
                inpline = line
                while("in " in inpline):
                    bus = inpline.split("in ")[1].split(": tdata",1)[0]
                    ends[0].append(bus)
                    inpline = inpline.split("in ")[1].split(": tdata",1)[1]

                outline = line
                while("out " in outline):
                    bus = inpline.split("out ")[1].split(": tdata",1)[0]
                    ends[1].append(bus)
                    outline = outline.split("out ")[1].split(": tdata",1)[1]
                for i in range(len(outs)):
                    #Instructions and repeaters created and set output bus
                    outps[index].write("\t\t\tvar " + outs[i][1] + " = new " + outs[i][1] + "();\n")
                    outps[index].write("\t\t\tvar " + outs[i][0] + " = Scope.CreateBus<tdata>();\n")
                    outps[index].write("\t\t\t" + outs[i][1]+ "." + outs[i][0] + " = " + outs[i][0] + ";\n")
                for i in range(len(ins)):
                    for c in ins[i]:
                        outps[index].write("\t\t\t" + c[1] + "." + c[0] + " = " + c[0] + ";\n")


        line = inp.readline()
    sim = ""
    for c in ends[0]:
        sim = " " + c + ","
    outps[index].write("\t\t\t//Connect " + sim[:-1] + " to all l0 channels\n")
    outps[index].write("\t\t\tSimulation.Current.AddTopLevelInputs(" + sim[:-1] + " );\n")
    sim = ""
    for c in ends[1]:
        sim = " " + c + ","
    outps[index].write("\t\t\t//Connect " + sim[:-1] + " to the highest level channels with the corresponding name\n")
    outps[index].write("\t\t\tSimulation.Current.AddTopLevelOutputs(" + sim[:-1] + " );\n")
    endMainFile(outps[index])


parse(inp)
