#!/bin/python
inputfile = "inputkernel.txt"
#Input file
inp = open(inputfile,"r")
#Output file
outputfile = "kernel.sme"
outp = open(outputfile,"w+")

def startFile(typ):
    outp.write("type vdata: "+ typ + ";\n")
    outp.write("type tdata: { val: vdata; };\n\n")

#Write the process
def writeProc(i,out,ins,sym):

    #Setup input busses
    def inputbus(i,numinp,ins):
        #Run through list and use names - dict to list removes doubles
        for n in list(dict.fromkeys(ins[1:])):
            if(n[0]=="var"):
                outp.write("\tbus " + n[1] + " {\n\t\tval: vdata;\n\t};\n")
        outp.write("\n")

    def outputbus(i,out):
        outp.write("\tbus " + out + " {\n\t\tval: vdata;\n\t};\n")
        outp.write("\n")

    #Writing the function/main part of the process
    def writeFunc(i,sym,numinp):
        outp.write("{\n")
        outp.write("\t"+out+".val =")
        for j in range(len(ins)-1):
            if (j!=0):
                outp.write(" " + sym)
            if(ins[j+1][0]=="var"):
                outp.write(" " + ins[j+1][1] + ".val")
            else:
                outp.write(" " + ins[j+1][1])
        outp.write(";\n")
        outp.write("}\n\n")

    outp.write("proc func" + str(i) + "()\n")
    inputbus(i,ins[0],ins)
    outputbus(i,out)
    writeFunc(i,sym,ins[0])

#Create the network
#number of process, in-, out-, all-, and freed arrays/channels, name of the network
def connect(numProcs, ins, outs, chans, free, name):
    #Flatten the ins list, keep their process number and remove all litterals
    flatIns = [(i[1],ind) for ind,ls in enumerate(ins) for i in ls[1:] if i[0]!= "lit"]

    #Couple of with function number (index)
    outs = [(val,i) for i,val in enumerate(outs)]
    #Find all inputs and outputs for the network that are not created inside the network
    fileIn  = [i for i in flatIns if not i[0] in [i for i,ind in outs]]
    fileOut = [i for i in outs if not i[0] in [i for i,ind in flatIns]]
    #Input and output
    outp.write("network "+name+"(")
    #list to dict to list to remove duplicates
    for n in list(dict.fromkeys(fileIn)):
        outp.write("in " + n[0] + ": tdata, ")
    for n in fileOut:
        outp.write("out " + n[0] + ": tdata")
        if(n!=fileOut[-1]):
            outp.write(", ")
    outp.write("){\n")
    #Functions
    for i in range(numProcs):
        outp.write("\tinstance " + str(i) + "_inst of func" + str(i) + "();\n")
    outp.write("\n")
    #Connect to input and output
    outp.write("\tconnect\n")

    #Connect the different functions with correct ins and outs
    for n in list(dict.fromkeys(fileIn)):
        outp.write("\t\t" + n[0] + ".val -> " + str(n[1]) + "_inst." + n[0] + ".val,\n")
    for n in list(dict.fromkeys(fileOut)):
        outp.write("\t\t" + str(n[1]) + "_inst." + n[0] + ".val -> " +  n[0] + ".val")
        if(n!=fileOut[-1]):
            outp.write(",\n")
    outp.write(";\n")
   #outp.write("\t\t" + str(numProcs-1) + "_inst.output"+ str(numProcs-1) + ".val -> dest.val;\n")
    outp.write("}\n")




#This parser only parses blocklists, not rank or any other
def parse(inp):
    def getSymbol(func):
        symbols = {
            #functionname : functionsymbol
            "BH_ADD": "+",
            "BH_SUBTRACT": "-",
            "BH_MULTIPLY": "*",
            "BH_DIVIDE": "/",
        }
        return symbols.get(func,"function not defined")

    #Make a channel name for each stride - if an array has not been created or has the same stride as an earlier array channel
    #If not it gets a name for each new version - fx a2v1 has a different stride from a2 and a2v2
    def getChanName(arr,stride):
        ver = 0
        while True:
            try:
                #Throws error if the array is not already there
                if(chans[arr]==stride):
                    #The channel with exact stride has already been created, use the same channel.
                    return arr
                else:
                    ver+=1
                    new = ''
                    if ver>1:
                        for a in arr.split("v")[:-1]:
                            new += a+"v"
                    else:
                        new=arr+"v"
                    arr=new+str(ver)
            except:
                #Add the array
                chans.setdefault(arr,stride)
                return arr

    #Process counter
    c=0
    #Read the first line and remove the newline
    line = inp.readline().split("\n")[0]
    #Setup the return lists
    syms = list()
    outs = list()
    ins = list()
    free = list()
    chans = dict()
    while(line != ''):
        #Remove spaces for rank-recognition
        while(line.startswith(" ")):
              line=line[1:]
        #Parse ranks
        if(line.startswith("rank:")):
            if("news" in line):
                ls = []
                if("frees" in line):
                    ls = line.split("frees: {")[1].split("}",1)[0].split(",")
                for l in ls:
                    if(l!=''):
                        free.append(l)
                line = inp.readline().split("\n")[0]
                continue
            #we only need the last rank because it contains which arrays are not to be output
            else:
               line = inp.readline().split("\n")[0]
               continue

        #Parse block list
        ls = [l for l in line.split(" ") if l!='']
        syms.append( getSymbol(ls[0]) )
        arr = ls[1].split("[")[0]
        stride = ls[1].split("[")[1].split("]")[0]
        arr = getChanName(arr,stride)
        outs.append(arr)
        inps = 0;
        ins.append([0])
        for l in ls[2:]:
            if("[" in l):
                arr = l.split("[")[0]
                stride = l.split("[")[1].split("]")[0]
                arr = getChanName(arr,stride)
                ins[c].append(("var",arr))
                inps +=1
            else:
                ins[c].append(("lit",l))
        ins[c][0]=inps
        c+=1
        line=inp.readline().split("\n")[0]
    return (free,syms,outs,ins,c,chans)


#Main
#This is an ex-kernel!
(free,syms,outs,ins,numprocs,chans)  = parse(inp)

#print(free,syms,outs,ins,numprocs,chans)
print(chans)
#The ex-kernel really wanted to be SMEIL
startFile("i32")
for i in range(len(syms)):
    writeProc(i,outs[i],ins[i],syms[i])
connect(numprocs,ins, outs,[i[0] for i in chans],free,"bohrium")
