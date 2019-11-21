all: test export

testbench: bohsme_tb
export: BohSME_export
build: export testbench

# Use a temporary folder for compiled stuff
WORKDIR=work

# All code should be VHDL93 compliant, 
# but 93c is a bit easier to work with
STD=93c

# Eveything should compile with clean IEEE,
# but the test-bench and CSV util's require
# std_logic_textio from Synopsys
IEEE=synopsys

# VCD trace file for GTKWave
VCDFILE=trace.vcd



$(WORKDIR):
	mkdir $(WORKDIR)

$(WORKDIR)/system_types.o: system_types.vhdl $(WORKDIR)
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) system_types.vhdl

$(WORKDIR)/Types_BohSME.o: Types_BohSME.vhdl $(WORKDIR)
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) Types_BohSME.vhdl

$(WORKDIR)/instr0.o: instr0.vhdl $(WORKDIR)/system_types.o $(WORKDIR)/Types_BohSME.o $(WORKDIR)
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) instr0.vhdl
$(WORKDIR)/instr1.o: instr1.vhdl $(WORKDIR)/system_types.o $(WORKDIR)/Types_BohSME.o $(WORKDIR)
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) instr1.vhdl
$(WORKDIR)/Creater.o: Creater.vhdl $(WORKDIR)/system_types.o $(WORKDIR)/Types_BohSME.o $(WORKDIR)
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) Creater.vhdl
$(WORKDIR)/csv_util.o: csv_util.vhdl $(WORKDIR)/system_types.o $(WORKDIR)/Types_BohSME.o $(WORKDIR)
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) csv_util.vhdl



$(WORKDIR)/BohSME.o: BohSME.vhdl $(WORKDIR)/system_types.o $(WORKDIR)/Types_BohSME.o $(WORKDIR)/instr0.o $(WORKDIR)/instr1.o $(WORKDIR)/Creater.o $(WORKDIR)/csv_util.o 

	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) BohSME.vhdl

$(WORKDIR)/TestBench_BohSME.o: TestBench_BohSME.vhdl $(WORKDIR)/BohSME.o
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) TestBench_BohSME.vhdl

bohsme_tb: $(WORKDIR)/TestBench_BohSME.o
	ghdl -e --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) BohSME_tb

BohSME_export: $(WORKDIR)/BohSME.o
	ghdl -a --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) Export_BohSME.vhdl

test: bohsme_tb
	cp "../trace.csv" .
	ghdl -r --std=$(STD) --ieee=$(IEEE) --workdir=$(WORKDIR) BohSME_tb --vcd=$(VCDFILE)

clean:
	rm -rf $(WORKDIR) *.o bohsme_tb


.PHONY: all clean test export build
