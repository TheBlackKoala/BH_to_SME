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

def parse(inp):
    index = 0
    line = inp.readline()
    output = False
    writeStart(outps[index])
    while(line != ''):
        if(index==0):
            if "proc" in line:
                outps[index].write("}\n")
                index +=1
                writeStart(outps[index])
                continue
            else:
                if"tdata" in line:
                    outps[index].write("\t[InitializedBus]\n")
                    outps[index].write("\tpublic interface tdata : IBus\n")
                    outps[index].write("\t{\n")
                    outps[index].write("\t\t[InitialValue(0)]\n")
                    outps[index].write("\t\tint val { get; set; }\n")
                    outps[index].write("\t}\n")
        elif(index==1):
            if "network" in line:
                outps[index].write("}\n")
                index+=1
                writeStart(outps[index])
                continue
            else:
                if "proc" in line:
                    line = line.split("proc")[1].split("()")[0]
                    outps[index].write("\t[ClockedProcess]\n")
                    outps[index].write("\tpublic class " + line + " : SimpleProcess\n")
                    outps[index].write("\t{\n")
                elif line.startswith("\tbus"):
                    if(output):
                        outps[index].write("\t\t[OutputBus]\n")
                        output= False
                    else:
                        outps[index].write("\t\t[InputBus]\n")
                    bus = line.split("bus ")[1].split(" {")[0]
                    outps[index].write("\t\tpublic tdata " + bus + ";\n\n")
                elif line == "\t//Output\n":
                    output=True
                elif "val =" in line:
                    outps[index].write("\t\tprotected override void OnTick()\n")
                    outps[index].write("\t\t{\n")
                    outps[index].write("\t\t"+line)
                    outps[index].write("\t\t}\n")
                elif line=="}\n":
                    outps[index].write("\t}\n\n")
        else:
            pass

        line = inp.readline()
    outps[index].write("}\n")


parse(inp)
