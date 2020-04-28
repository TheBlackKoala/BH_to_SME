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

outps = [open(outputdir+"/"+f,"w+") for f in outputfiles[1:]]

#Write the csproj file if it isn't there already
if not os.path.exists(outputdir+"/"+outputfiles[0]):
    f = open(outputdir+"/"+outputfiles[0],"w+")
    f.write("<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"SME\" Version=\"0.4.0-beta\" />\n    <PackageReference Include=\"SME.Tracer\" Version=\"0.4.0-beta\" />\n    <PackageReference Include=\"SME.VHDL\" Version=\"0.4.0-beta\" />\n    <PackageReference Include=\"SME.GraphViz\" Version=\"0.4.0-beta\" />\n    \n    <PackageReference Include=\"System.Drawing.Common\" Version=\"4.5.1\" />\n    <PackageReference Include=\"runtime.linux-x64.CoreCompat.System.Drawing\" Version=\"1.0.0-beta002\" />\n  </ItemGroup>\n\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>netcoreapp3.1</TargetFramework>\n    <RootNamespace>"\
                   + projectname+\
                "</RootNamespace>\n  </PropertyGroup>\n\n</Project>\n")
    #the csproj has been written
    f.close()


def writeStart(f):
    f.write("using SME;\n")
    f.write("using static " + projectname + ".ValuesConfig;\n\n")
    f.write("namespace " + projectname + "\n{\n")

def endMainFile(f):
    f.write("\t\t}\n\n")
    f.write("\t\tpublic static void Main(string[] args)\n\t\t{\n\n\t\t\tusing(var sim = new Simulation())\n\t\t\t{\n\n\t\t\t\tsetup();\n\n\t\t\t\t// Use fluent syntax to configure the simulator.\n\t\t\t\t// The order does not matter, but `Run()` must be\n\t\t\t\t// the last method called.\n\n\t\t\t\t// The top-level input and outputs are exposed\n\t\t\t\t// for interfacing with other VHDL code or board pins\n\n\t\t\t\tsim\n\t\t\t\t\t.AddTopLevelOutputs()\n\t\t\t\t\t.AddTopLevelInputs()\n\t\t\t\t\t.BuildCSVFile()\n\t\t\t\t\t.BuildVHDL()\n\t\t\t\t\t.Run();\n\n\t\t\t\t// After `Run()` has been invoked the folder\n\t\t\t\t// `output/vhdl` contains a Makefile that can\n\t\t\t\t// be used for testing the generated design\n\t\t\t}\n\t\t}\n\t}\n}\n")

def parse(inp):
    indentlvl = 2
    index = 0
    line = inp.readline()
    #Whether we are inside a proccess currently
    proc = False
    #Whether the next channels is an output
    output = False
    #Number of instructions
    insts = 0
    #Name of all repeaters
    reps = list()
    #Instantiated buses
    createds = list()
    #Whether we are wiring up connections
    connecting = False
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
                    outps[index].write("\t[ClockedBus]\n")
                    outps[index].write("\tpublic interface tdata : IBus\n")
                    outps[index].write("\t{\n")
                    outps[index].write("\t\t[InitialValue(false)]\n")
                    outps[index].write("\t\tbool valid { get; set; }\n")
                    outps[index].write("\t\t[FixedArrayLength(ValuesConfig.len)]\n")
                    outps[index].write("\t\tIFixedArray<int> val{ get; set; }\n")
                    outps[index].write("\t\t[InitialValue(0)]\n")
                    outps[index].write("\t\tint len { get; set; }\n")
                    outps[index].write("\t}\n")
                elif "const len:" in line:
                    outps[index].write("\tpublic static class ValuesConfig{\n")
                    outps[index].write("\t\tpublic const int len = 32;\n")
                    outps[index].write("\t\tpublic const int halfLen = len/2;\n")
                    outps[index].write("\t\tpublic const int reduceLen = 5;\n")
                    outps[index].write("\t}\n")

        #Write the processes
        elif(index==1):
            if "network" in line:
                outps[index].write("}\n")
                index+=1
                writeStart(outps[index])
                continue
            else:
                if not proc:
                    if "proc" in line:
                        proc=True
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
                #Inside the process
                else:
                    #Write the buses
                    if line.startswith("\tbus"):
                        bus = line.split("bus ")[1].split(" {")[0].split(": ")[0]
                        if(output):
                            outps[index].write("\t\t[OutputBus]\n")
                            output= False
                        else:
                            outps[index].write("\t\t[InputBus]\n")
                        outps[index].write("\t\tpublic tdata " + bus + ";\n\n")
                    #The next bus will be an output bus
                    elif line == "\t//Output\n":
                        output=True
                    elif "var" in line:
                        if "acc" in line:
                            arrLen = line.split("[ ")[1].split(" ]")[0]
                            arrLen = arrLen.replace(" len "," ValuesConfig.len ")
                            outps[index].write("\t"+ line.split("var")[0])
                            outps[index].write("private readonly int[] acc = new int[" + arrLen + "/2];\n")
                        else:
                            line = line.split(":")[0].split("var ")[1]
                            outps[index].write("\t\tint " + line + ";\n")
                    elif line == "{\n":
                        outps[index].write("\t\tprotected override void OnTick()\n")
                        outps[index].write("\t\t{\n")
                    elif "elif(" in line or "elif (" in line:
                        outps[index].write("\t\t"+line.replace("elif","else if"))
                        indentlvl +=1
                    elif "if (" in line or "if(" in line:
                        outps[index].write("\t\t"+line)
                        indentlvl +=1
                    elif "else{" in line:
                        outps[index].write("\t\t"+line)
                        indentlvl +=1
                    elif "for " in line:
                        assign = line.split("for ")[1].split(" to ")[0]
                        val = assign.split(" =")[0]
                        end = line.split(" to")[1].split(" {")[0]
                        end = end.replace(" len "," ValuesConfig.len ")
                        end = end.replace(" reduceLen "," ValuesConfig.reduceLen ")
                        outps[index].write("\t\t")
                        indent = line.split("for ")[0]
                        outps[index].write(indent + "for(int " + assign + "; " + val +"<" + end + "+1; "+ val + "++){\n")
                        indentlvl +=1
                    #create the processes function
                    elif ".val[" in line or "acc[" in line:
                        outps[index].write("\t\t"+line)
                    elif "minLen=" in line or "minLen =" in line:
                        outps[index].write("\t\t"+line)
                    elif line=="}\n":
                        outps[index].write("\t\t}\n")
                        outps[index].write("\t}\n\n")
                        proc=False
                    elif "}\n" in line:
                        outps[index].write(("\t"*indentlvl) + "}\n")
                        indentlvl -=1
                    elif ".len" in line:
                        outps[index].write("\t\t"+line)
                    elif ".valid" in line:
                        outps[index].write("\t\t"+line)
                    elif "lenReduc = " in line or "minLen2 = " in line:
                        outps[index].write("\t\t"+line)
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
                    bus = outline.split("out ")[1].split(": tdata")[0]
                    ends[1].append(bus)
                    outline = outline.split("out ",1)[1].split(": tdata",1)[1]

            elif "instance" in line:
                (name,func)= line.split("instance ")[1].split(" of ")
                if "_" in name:
                    (n1,n2) = name.split("_")
                    name = n2+n1
                outps[index].write("\t\t\tvar " + name + " = new " + func)

            elif "connect" in line:
                connecting=True
            elif connecting:
                if line!="\n":
                    outbus = line.split(" -> ")[0].split("		")[1]
                    inbus  = line.split(" -> ")[1].split(",")[0]
                    if ";" in inbus:
                        connecting = False
                        inbus = inbus.split(";")[0]
                    name = ""
                    if "rep" in outbus:
                        name = outbus.split("rep")[1].split(".")[0]
                    elif "inst" in outbus:
                        name = outbus.split(".")[1]
                    else:
                        name= outbus
                    if "_" in inbus:
                        (n1,n0) = inbus.split(".")
                        (n1,n2) = n1.split("_")
                        inbus = n2+n1+"."+n0
                    if "_" in outbus:
                        (n1,n0) = outbus.split(".")
                        (n1,n2) = n1.split("_")
                        outbus = n2+n1+"."+n0
                    if not name in createds:
                        outps[index].write("\t\t\tvar " + name + " = Scope.CreateBus<tdata>();\n")
                        createds.append(name)
                    if(name != outbus):
                        if not outbus in createds:
                            outps[index].write("\t\t\t" + outbus + " = " + name + ";\n")
                            createds.append(outbus)
                    outps[index].write("\t\t\t" + inbus  + " = " + name + ";\n")

        line = inp.readline()
    sim = ""
    for c in ends[0]:
        sim = " " + c + ","
    outps[index].write("\t\t\t//Connect " + sim[:-1] + " to all l0 channels\n")
    outps[index].write("\t\t\tSimulation.Current.AddTopLevelInputs(" + sim[:-1] + " );\n")
    sim = ""
    for c in ends[1]:
        sim = sim + " " + c + ","
    outps[index].write("\t\t\t//Connect " + sim[:-1] + " to the highest level channels with the corresponding name\n")
    outps[index].write("\t\t\tSimulation.Current.AddTopLevelOutputs(" + sim[:-1] + " );\n")
    endMainFile(outps[index])


parse(inp)
