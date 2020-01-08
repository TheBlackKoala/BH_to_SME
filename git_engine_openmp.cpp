/*
This file is part of Bohrium and copyright (c) 2012 the Bohrium
team <http://www.bh107.org>.

Bohrium is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as
published by the Free Software Foundation, either version 3
of the License, or (at your option) any later version.

Bohrium is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the
GNU Lesser General Public License along with Bohrium.

If not, see <http://www.gnu.org/licenses/>.
*/

#include <vector>
#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <iomanip>
#include <dlfcn.h>
#include <bohrium/jitk/codegen_util.hpp>
#include <bohrium/jitk/compiler.hpp>
#include <bohrium/jitk/fuser_cache.hpp>
#include <bohrium/jitk/codegen_cache.hpp>
#include <bohrium/jitk/block.hpp>
#include <thread>

#include <bohrium/bh_util.hpp>
#include "engine_openmp.hpp"
#include "openmp_util.hpp"

using namespace std;
using namespace bohrium::jitk;
namespace fs = boost::filesystem;

namespace bohrium {

EngineOpenMP::EngineOpenMP(component::ComponentVE &comp, jitk::Statistics &stat) : EngineCPU(comp, stat), compiler(
        comp.config.get<string>("compiler_cmd"), comp.config.file_dir.string(), verbose), compiler_openmp(
        comp.config.defaultGet<bool>("compiler_openmp", false)), compiler_openmp_simd(
        comp.config.defaultGet<bool>("compiler_openmp_simd", false)) {

    compilation_hash = util::hash(compiler.cmd_template);

    // Initiate cache limits
    malloc_cache_limit_in_percent = comp.config.defaultGet<int64_t>("malloc_cache_limit", 80);
    if (malloc_cache_limit_in_percent < 0 or malloc_cache_limit_in_percent > 100) {
        throw std::runtime_error("config: `malloc_cache_limit` must be between 0 and 100");
    }

    int64_t sys_mem_unused = bh_main_memory_unused();
    if (sys_mem_unused == -1) {
        // If `bh_main_memory_unused()` isn't available, we use 20% of the total amount of memory
        malloc_cache_limit_in_bytes = static_cast<int64_t>(std::floor(bh_main_memory_total() * 0.20));
    } else {
        // Else we use the percentage specified in the config under `malloc_cache_limit`
        malloc_cache_limit_in_bytes = static_cast<int64_t>(std::floor(sys_mem_unused *
                                                                      (malloc_cache_limit_in_percent / 100.0)));
    }
    bh_set_malloc_cache_limit(static_cast<uint64_t>(malloc_cache_limit_in_bytes));
}

EngineOpenMP::~EngineOpenMP() {
    const bool use_cache = not (cache_readonly or cache_bin_dir.empty());

    // Move JIT kernels to the cache dir
    if (use_cache) {
        try {
            for (const auto &kernel: _functions) {
                const fs::path src = tmp_bin_dir / jitk::hash_filename(compilation_hash, kernel.first, ".so");
                if (fs::exists(src)) {
                    const fs::path dst = cache_bin_dir / jitk::hash_filename(compilation_hash, kernel.first, ".so");
                    fs::copy_file(src, dst, fs::copy_option::overwrite_if_exists);
                }
            }
        } catch (const boost::filesystem::filesystem_error &e) {
            cout << "Warning: couldn't write JIT kernels to disk to " << cache_bin_dir
                 << ". " << e.what() << endl;
        }
    }

    // File clean up
    if (not verbose) {
        fs::remove_all(tmp_src_dir);
    }

    if (cache_file_max != -1 and use_cache) {
        util::remove_old_files(cache_bin_dir, cache_file_max);
    }

    // If this cleanup is enabled, the application segfaults
    // on destruction of the EngineOpenMP class.
    //
    // The reason for that is that OpenMP currently has no
    // means in the definition of the standard to do a proper
    // cleanup and hence internally does something funny.
    // See https://stackoverflow.com/questions/5200418/destroying-threads-in-openmp-c
    // for details.
    //
    // TODO A more sensible way to get around this issue
    //      would be to also dlopen the openmp library
    //      itself and therefore increase our reference count
    //      to it. This enables the kernel so files to be unlinked
    //      and cleaned up, but prevents the problematic code on
    //      openmp cleanup to be triggered at bohrium runtime.

    // for(void *handle: _lib_handles) {
    //     dlerror(); // Reset errors
    //     if (dlclose(handle)) {
    //         cerr << dlerror() << endl;
    //     }
    // }
}

KernelFunction EngineOpenMP::getFunction(const string &source, const string &func_name, const string &compile_cmd) {
    uint64_t hash = util::hash(source);
    ++stat.kernel_cache_lookups;

    // Do we have the function compiled and ready already?
    if (_functions.find(hash) != _functions.end()) {
        return _functions.at(hash);
    }

    // The path to the shared library file.
    fs::path binfile = cache_bin_dir / jitk::hash_filename(compilation_hash, hash, ".so");
    
    // Let's try to load the shared library. If it fails for any reason, we try again after a compilation.
    void *lib_handle = dlopen(binfile.string().c_str(), RTLD_NOW);

    // If the binary file couldn't load, we compile it.
    if (verbose or cache_bin_dir.empty() or lib_handle == nullptr) {
        ++stat.kernel_cache_misses;

        // We create the binary file in the tmp dir
        binfile = tmp_bin_dir / jitk::hash_filename(compilation_hash, hash, ".so");

        // Write the source file and compile it (reading from disk)
        // NB: this is a nice debug option, but will hurt performance
        if (verbose) {
            std::string source_filename = jitk::hash_filename(compilation_hash, hash, ".c");
            fs::path srcfile = jitk::write_source2file(source, tmp_src_dir, source_filename, true);
            if (compile_cmd.empty()) {
                compiler.compile(binfile, srcfile);
            } else {
                compiler.compile(binfile, srcfile, compile_cmd);
            }
        } else {
            // Pipe the source directly into the compiler thus no source file is written
            if (compile_cmd.empty()) {
                compiler.compile(binfile, source);
            } else {
                compiler.compile(binfile, source, compile_cmd);
            }
        }
    }

    // If the library wasn't loaded before compilation, we try one more time
    if (lib_handle == nullptr) {
        lib_handle = dlopen(binfile.string().c_str(), RTLD_NOW);
        if (lib_handle == nullptr) {
            cerr << "Cannot load library: " << dlerror() << endl;
            throw runtime_error("VE-OPENMP: Cannot load library");
        }
    }
    _lib_handles.push_back(lib_handle);

    // Load the launcher function
    // The (clumsy) cast conforms with the ISO C standard and will
    // avoid any compiler warnings.
    dlerror(); // Reset errors
    *(void **) (&_functions[hash]) = dlsym(lib_handle, func_name.c_str());
    const char *dlsym_error = dlerror();
    if (dlsym_error != nullptr) {
        cerr << "Cannot load function launcher(): " << dlsym_error << endl;
        throw runtime_error("VE-OPENMP: Cannot load function launcher()");
    }
    return _functions.at(hash);
}


void EngineOpenMP::execute(const jitk::SymbolTable &symbols,
                           const std::string &source,
                           uint64_t codegen_hash,
                           const std::vector<const bh_instruction *> &constants) {
    // Notice, we use a "pure" hash of `source` to make sure that the `source_filename` always
    // corresponds to `source` even if `codegen_hash` is buggy.
    uint64_t hash = util::hash(source);
    std::string source_filename = jitk::hash_filename(compilation_hash, hash, ".c");

    // Make sure all arrays are allocated
    for (bh_base *base: symbols.getParams()) {
        bh_data_malloc(base);
    }

    // Compile the kernel
    auto tbuild = chrono::steady_clock::now();
    string func_name;
    {
        stringstream t;
        t << "launcher_" << codegen_hash;
        func_name = t.str();
    }
    KernelFunction func = getFunction(source, func_name);
    assert(func != nullptr);
    stat.time_compile += chrono::steady_clock::now() - tbuild;

    // Create a 'data_list' of data pointers
    vector<void *> data_list;
    data_list.reserve(symbols.getParams().size());
    for (bh_base *base: symbols.getParams()) {
        assert(base->getDataPtr() != nullptr);
        data_list.push_back(base->getDataPtr());
    }

    // And the offset-and-strides
    vector<uint64_t> offset_and_strides;
    offset_and_strides.reserve(symbols.offsetStrideViews().size());
    for (const bh_view *view: symbols.offsetStrideViews()) {
        const uint64_t t = (uint64_t) view->start;
        offset_and_strides.push_back(t);
        for (int i = 0; i < view->ndim; ++i) {
            const uint64_t s = (uint64_t) view->stride[i];
            offset_and_strides.push_back(s);
        }
    }

    // And the constants
    vector<bh_constant_value> constant_arg;
    constant_arg.reserve(constants.size());
    for (const bh_instruction *instr: constants) {
        constant_arg.push_back(instr->constant.value);
    }

    auto start_exec = chrono::steady_clock::now();
    // Call the launcher function, which will execute the kernel
    func(&data_list[0], &offset_and_strides[0], &constant_arg[0]);
    auto texec = chrono::steady_clock::now() - start_exec;
    stat.time_exec += texec;
    stat.time_per_kernel[source_filename].register_exec_time(texec);

}

// Writes the OpenMP specific for-loop header
void EngineOpenMP::loopHeadWriter(const jitk::SymbolTable &symbols,
                                  jitk::Scope &scope,
                                  const jitk::LoopB &block,
                                  const vector<uint64_t> &thread_stack,
                                  stringstream &out) {
    // Let's write the OpenMP loop header
    int64_t for_loop_size = block.size;
    // No need to parallel one-sized loops
    if (for_loop_size > 1) {
        writeHeader(symbols, scope, block, out);
    }
    // Write the for-loop header
    string itername;
    {
        stringstream t;
        t << "i" << block.rank;
        itername = t.str();
    }
    out << "for(uint64_t " << itername << " = 0; ";
    out << itername << " < " << block.size << "; ++" << itername << ") {\n";
}

// Writing the OpenMP header, which include "parallel for" and "simd"
void EngineOpenMP::writeHeader(const jitk::SymbolTable &symbols,
                               jitk::Scope &scope,
                               const jitk::LoopB &block,
                               std::stringstream &out) {
    if (not compiler_openmp) {
        return;
    }

    // All reductions that can be handle directly be the OpenMP header e.g. reduction(+:var)
    std::vector<jitk::InstrPtr> openmp_reductions;

    // Order all sweep instructions by the viewID of their first operand.
    // This makes the source of the kernels more identical, which improve the code and compile caches.
    const std::vector<jitk::InstrPtr> ordered_block_sweeps = order_sweep_set(block._sweeps, symbols);

    stringstream ss;
    // "OpenMP for" goes to the outermost loop
    if (block.rank == 0 and openmp_compatible(block)) {
        ss << " parallel for";
        // Since we are doing parallel for, we should either do OpenMP reductions or protect the sweep instructions
        for (const jitk::InstrPtr &instr: ordered_block_sweeps) {
            assert(instr->operand.size() == 3);
            const bh_view &view = instr->operand[0];
            if (openmp_reduce_compatible(instr->opcode) and (scope.isScalarReplaced(view) or scope.isTmp(view.base))) {
                openmp_reductions.push_back(instr);
            } else if (openmp_atomic_compatible(instr->opcode)) {
                scope.insertOpenmpAtomic(instr);
            } else {
                scope.insertOpenmpCritical(instr);
            }
        }
    }

    // "OpenMP SIMD" goes to the innermost loop (which might also be the outermost loop)
    if (compiler_openmp_simd and block.isInnermost() and simd_compatible(block, scope)) {
        ss << " simd";
        if (block.rank > 0) { // NB: avoid multiple reduction declarations
            for (const jitk::InstrPtr &instr: ordered_block_sweeps) {
                openmp_reductions.push_back(instr);
            }
        }
    }

    //Let's write the OpenMP reductions
    for (const jitk::InstrPtr &instr: openmp_reductions) {
        assert(instr->operand.size() == 3);
        ss << " reduction(" << openmp_reduce_symbol(instr->opcode) << ":";
        scope.getName(instr->operand[0], ss);
        ss << ")";
    }
    const string ss_str = ss.str();
    if (not ss_str.empty()) {
        out << "#pragma omp" << ss_str << "\n";
        util::spaces(out, 4 + block.rank * 4);
    }
}

  //FPGA implementation
  void getNameSub(const Scope &scope,
                  const bh_view &view,
                  stringstream &out,
                  map<string, string> *chanSub
                  ){
    //Get regular name and subscription
    stringstream ss;
    scope.getName(view, ss);
    string name = ss.str();
    ss.str("");
    write_array_subscription(scope, view, ss);
    string sub = ss.str();
    //Find a proper name for the array channel
    int i = 0;
    while(true){
      if(chanSub->find(name)==chanSub->end()){
      //If the current name is unused
        pair<string,string> p = pair<string,string>(name,sub);
        chanSub->insert(p);
        break;
      }
      //If the current name has a subscription
      else{
        string curSub = (*chanSub)[name];
        //If the name matches
        if(curSub == sub) {
          break;
        }
        //Otherwise find a new name
        else{
          if(i>0){
            int index = name.rfind("v");
            name = name.substr(0,index);
          }
          i++;
          name = name + "v" + to_string(i);
        }
      }
    }
    out << name;
  }

  pair<string,bool> smeGetName(const bh_instruction &instr,
                               const Scope &scope,
                               const bh_view &view,
                               vector<string> *chans,
                               map<string, string> *chanSub,
                               int *c){
    stringstream ss;
    bool isChan = false;
    if (view.isConstant()) {
      instr.constant.pprint(ss, false);
    }
    else{
      getNameSub(scope,view,ss,chanSub);
      chans->push_back(ss.str());
      (*c)++;
      isChan = true;
    }
    return pair<string,bool>(ss.str(),isChan);
  }

  //Sets the level of all channels in the process and returns the level of the process
  int findLevel(vector<pair<string,bool>> ops, map<string, int> *chanLevel, vector<string> *opsWithLevel){
    int size = ops.size();
    int level = 0;
    //Find the highest level of the input buses
    for(int i=1; i<size; i++){
      if(ops[i].second){
        string name = ops[i].first;
        //If the channel has no level it is from memory - ergo 0
        if(chanLevel->find(name)==chanLevel->end())
          chanLevel->insert(pair<string,int>(name,0));
        else
          level = max(level,(*chanLevel)[name]);
      }
    }
    //Add the output bus at the level over the highest input
    chanLevel->insert(pair<string,int>(ops[0].first,level+1));
    //We now have the highest input level that is required - all inputs must be on this level and the output on the level above - the process level
    stringstream ss;
    for(int i = 0; i<size; i++){
      auto op = ops[i];
      ss << op.first;
      if(op.second){
        if(i==0){
          ss << "l" << level+1 << ".val" << "[i]";
        }
        else{
          ss << "l" << level << ".val" << "[i]";
        }
      }
      opsWithLevel->push_back(ss.str());
      ss.str(string());
    }

    //The level of the process is the level of the output = highest level of input plus 1
    return level+1;
  }

  //Functions to reduce the size of blockWriter
  int instrWriter(const bh_instruction &instr,
                  const SymbolTable &symbols,
                  Scope *parent_scope,
                  vector<string> *chans,
                  vector<int> *chanDist,
                  stringstream &ss,
                  int i,
                  map<string, string> *chanSub,
                  map<string, int> *chanLevel,
                  vector<int> *procLevel
                  ){
    //Setting the process up in sme
    ss << "proc instr" << i << "()" << "\n";
    jitk::Scope scope(symbols,parent_scope);
    //Vector containing operands (arrays and constants) along with whether they are a channel or not. True means channel
    vector<pair<string,bool>> ops;
    //Vector containing the channel-names including level
    vector<string> opsWithLevel;
    //Counting number of channels in the process
    int c = 0;
    for (uint i=0; i<instr.operand.size(); i++){
      ops.push_back(smeGetName(instr, scope, instr.operand[i],chans,chanSub,&c));
    }
    int level = findLevel(ops,chanLevel,&opsWithLevel);
    procLevel->push_back(level);
    chanDist->push_back(c);
    //Set the channels up in each process
    uint offset = chans->size() - c;
    //Vector to check for duplicate channels
    vector<string> chan;
    for (uint i=offset; i<chans->size(); i++){
      string ch = (*chans)[i];
      //Do not put in duplicate
      if(std::find(chan.begin(), chan.end(), ch) == chan.end()) {
        if(i==offset){
          ss<< "\t//Output\n";
          chan.push_back(ch);
          ss << "\tbus " <<  ch << "l" << level << ": tdata;" << "\n";
        }
        else {
          chan.push_back(ch);
          ss << "\tbus " <<  ch << "l" << level-1 << ": tdata;" << "\n";
        }
      }
    }
    ss << "\t" << "var minLen : u32;" << "\n";
    //The process body
    ss << "{" << "\n";
    //Valid guard
    ss << "\t" << "if (";
    for (uint i=1; i<chan.size(); i++){
      ss << chan[i] << "l" << level-1 << ".valid && ";
    }
    ss.seekp(-4,ss.cur);
    ss << "){" << "\n";
    //Find the minimal length of arrays
    ss << "\t" << "\t" << "minLen = " << chan[1] << "l" << level-1 << ".len;" << "\n";
    for (uint i=2; i<chan.size(); i++){
      ss << "\t" << "\t" << "if (minLen > " << chan[i] << "l" << level-1 << ".len){" << "\n";
      ss <<"\t" << "\t" << "\t" << "minLen = " << chan[i] << "l" << level-1 << ".len;"<< "\n";
      ss << "\t" << "\t" << "}" << "\n";
    }
    //The output will then be valid
    ss << "\t" << "\t" << chan[0] << "l" << level << ".valid = true;" << "\n";
    ss << "\t" << "\t" << chan[0] << "l" << level << ".len = minLen;" << "\n";
    //Set the for-loop
    ss << "\t" << "\t" << "for i = 0 to len - 1 {" << "\n";
    //Length guard
    ss << "\t" << "\t" << "\t" << "if (i<minLen";
    ss << "){" << "\n";
    //Correct indentation
    ss << "\t" << "\t" << "\t" << "\t";
    write_operation(instr, opsWithLevel, ss, false);
    //End for-loop and process
    ss << "\t" << "\t" << "\t" << "}" << "\n";
    ss << "\t" << "\t" << "}" << "\n";
    ss << "\t" << "}" << "\n";
    //If the inputs are not valid then the output is not valid
    ss << "\t" << "else{" << "\n";
    ss << "\t" << "\t" << chan[0] << "l" << level << ".valid = false;" << "\n";
    ss << "\t" << "}" << "\n";
    //Close the process
    ss << "}" << "\n" << "\n";
    return level;
  }

  void writeNetworkStart(vector<string> news, vector<string> frees, vector<string> chans, stringstream &ss){
    string name = "bohrium";
    ss << "network " << name <<"(";
    //Outside channels - currently memory
    vector<string> ins = news;
    //Write input channels
    //Any channel not in news and not already added will be written as input
    for (string c : chans){
      if( std::find(ins.begin(), ins.end(), c) == ins.end()){
        ss << "in " << c << ": tdata, ";
        ins.push_back(c);
      }
    }
    //Write output channels
    //Take all channels from news that are not freed and write them out
    for (string c : news){
      if( std::find(frees.begin(), frees.end(), c) == frees.end()){
        ss << "out " << c << ": tdata, ";
      }
    }
    //Fix the end of the function by removing the last two character (", ") and instead putting in "){".
    ss.seekp(-2,ss.cur);
    ss << "){" << "\n";
  }

  //Write all repeaters for a channel. Start at the level after it is created (memory processes are 0 so start at 1 and end at the highest level of any process
  void writeRepeaters(int start, int end, stringstream &ss, string chanName){
    for(int i =start+1; i<=end; i++){
      ss << "proc repeater" << chanName << "l" << i << "()" << "\n";
      ss << "\t" << "//Output" << "\n";
      ss << "\t" << "bus " << chanName << "l" << i << ": tdata;" << "\n";
      ss << "\t" << "bus " << chanName << "l" << i-1 << ": tdata;" << "\n";
      ss << "{" << "\n";
      ss << "\t" << "if (" << chanName << "l" << i-1 << ".valid" << "){" << "\n";
      ss << "\t" << "\t" << chanName << "l" << i << ".valid=true;" << "\n";
      ss << "\t" << "\t" << "for i = 0 to len -1 {" << "\n";
      ss << "\t" << "\t" << "\t" << "if ( i < " << chanName << "l" << i-1 << ".len" << "){" << "\n";
      ss << "\t" << "\t" << "\t" << "\t" << chanName << "l" << i << ".val[i] = " << chanName << "l" << i-1 << ".val[i];" << "\n";
      //End for-loop and process
      ss << "\t" << "\t" << "\t" << "}" << "\n";
      ss << "\t" << "\t" << "}" << "\n";
      ss << "\t" << "\t" << chanName << "l" << i << ".len = " << chanName << "l" << i-1<< ".len;" << "\n";
      ss << "\t" << "}" << "\n";
      //If the input is not valid then the output is not valid
      ss << "\t" << "else{" << "\n";
      ss << "\t" << "\t" << chanName << "l" << i << ".valid = false;" << "\n";
      ss << "\t" << "}" << "\n";
      //Close the process
      ss << "}" << "\n" << "\n";
    }
  }

  //Create instances of all repeaters of a channel. Like writeRepeaters.
  void instanceRepeaters(int start, int end, stringstream &ss, string chanName){
    for(int i =start+1; i<=end; i++){
      ss << "\t" << "instance " << "rep" << chanName << "l" << i << " of " << "repeater" << chanName << "l" << i << "();\n";
    }
  }

  //Write all instances of processes
  void writeInstance(uint size,stringstream &ss){
    for(uint i=0; i<size; i++){
      ss << "\t" << "instance " << i << "_inst of " << "instr" << i << "();" << "\n";
    }
    ss << "\n";
  }

  void connectRepeaters(int start, int end, stringstream &ss, string chanName){
    if(start==0){
      ss << "\t\t" << chanName  << " -> " << "rep" << chanName << "l1." << chanName << "l0" << "," << "\n";
    }
    for(int i =start+2; i<=end; i++){
      ss << "\t\t" << "rep" << chanName << "l" << i-1 << ".";
      ss << chanName << "l" << i-1  << " -> ";
      ss << "rep" << chanName << "l" << i << ".";
      ss << chanName << "l" << i-1 << "," << "\n";
    }
  }

  void writeOutCh(vector<string> out,
                  vector<string> frees,
                  stringstream &ss,
                  int level,
                  vector<int> procLevel){
    for(uint i = 0; i< out.size(); i++){
      string c = out[i];
      if(std::find(frees.begin(), frees.end(), c) == frees.end()){
        if(procLevel[i]==level){
          ss << "\t\t" << i << "_inst." << c << "l" << level  << " -> ";
          ss << c  << "," << "\n";
        }
        else{
          ss << "\t\t" << "rep" << c << "l" << level  << "."
             << c << "l" << level  << " -> ";
          ss << c  << "," << "\n";
        }
      }
    }
    //Remove the last , and put in a ;
    ss.seekp(-2,ss.cur);
    ss << ";" << "\n";
  }

  void blockWriter(const LoopB &kernel,
                   const SymbolTable &symbols,
                   const Scope *parent_scope,
                   stringstream &ss
                   ){
    //Scope for names and such
    jitk::Scope scope(symbols,parent_scope);
    //List of channels
    vector<string> chans;
    //Number of channels in each process
    vector<int> chanDist;
    //Level of each process
    vector<int> procLevel;
    //A map where each channelname is mapped to a stride
    map<string, string> chanSub;
    //A map of channelnames and creation level
    map<string, int> chanLevel;
    //Number of processes/instructions
    int count = 0;
    //The highest level for repeaters
    int level = 0;


    for (const Block &b: kernel._block_list){
      //Make recursive function
      if(b.isInstr()){
        if(count==0){
          //Set the start of the sme-file up.
          //If vdata is changed so might the neutral element ne
          ss << "type vdata: i32;" << "\n";
          ss << "const len: u32 = 32;" << "\n";
          ss << "type adata: vdata [ len ];" << "\n";
          ss << "type tdata: { val:adata; valid:bool = false; len:u32;};" << "\n" << "\n";
        }
        //Write the instruction to the stream - this is where stuff happens
        const InstrPtr &instr = b.getInstr();
        int instLevel = instrWriter(*instr,symbols,&scope,&chans, &chanDist, ss, count, &chanSub,&chanLevel,&procLevel);
        level = max(instLevel,level);
        count++;
      }
      else{
        //Loop information is for reading from memory with strides
        blockWriter(b.getLoop(),symbols, &scope, ss);
      }
    }
    //If this was instruktions write the end of the file containing the network
    if (count>0){
      //Make a list of created channels inside the network (not from memory)
      vector<string> news;
      for (auto c : kernel._news){
        news.push_back(string("a") + to_string(symbols.baseID(c)));
      }
      //Make a list of all channels freed in the network
      vector<string> frees;
      for (auto c : kernel._frees){
        frees.push_back(string("a") + to_string(symbols.baseID(c)));
      }

      //Before setting the network up we need to create all the repeater processes
      for(auto it = chanLevel.begin(); it != chanLevel.end(); it++){
        writeRepeaters(it->second, level, ss, it->first);
      }

      //Set the network up
      writeNetworkStart(news, frees, chans, ss);
      //Create instances of all processes
      writeInstance(chanDist.size(),ss);
      //Create an instance for all repeaters
      for(auto it = chanLevel.begin(); it != chanLevel.end(); it++){
        instanceRepeaters(it->second, level, ss, it->first);
      }

      //Put the network together
      ss << "\t" << "connect" << "\n";
      //Connect all the repeaters and the base inputs
      for(auto it = chanLevel.begin(); it != chanLevel.end(); it++){
        connectRepeaters(it->second, level, ss, it->first);
      }
      ss << "\n";
      //Use exclusive scan plus on chandist
      vector<int> scanDist = {0};
      for(uint i =0; i<chanDist.size(); i++){
        scanDist.push_back(scanDist[i]+chanDist[i]);
      }
      //Write internal network and inputs to processes
      int proc = 0;
      vector<string> out;
      vector<string> added;
      for(uint i = 0; i<chans.size(); i++){
        string c = chans[i];
        if(i == scanDist[proc]){
          proc++;
          out.push_back(c);
          vector<string> added = {};
          //If the output channel has a repeater then connect the data to it
          if(procLevel[proc-1]<level){
            ss << "\t\t";
            ss << proc-1 << "_inst." << c << "l" << procLevel[proc-1]  << " -> ";
            ss << "rep" << c << "l" << procLevel[proc-1]+1 << "." << c << "l" << procLevel[proc-1] << ",";
            ss << "\n";
          }
        }
        else{
          if(std::find(added.begin(), added.end(), c) == added.end()) {
            added.push_back(c);
            auto it = std::find(out.begin(), out.end(), c);
            if( it == out.end()) {
              if(procLevel[proc-1]==1){
                ss << "\t\t" << c << " -> ";
                ss << proc-1 << "_inst." << c << "l" << procLevel[proc-1]-1;
                ss << "," << "\n";
              }
              else{
                ss << "\t\t" << "rep" << c << "l" << procLevel[proc-1]-1  << "."
                   << c << "l" << procLevel[proc-1]-1  <<  " -> ";
                ss << proc-1 << "_inst." << c << "l" << procLevel[proc-1]-1;
                ss << "," << "\n";
              }
            }
            else {
              int index = std::distance(out.begin(), it);
              if(procLevel[proc-1]==procLevel[index]+1){
                ss << "\t\t" << index << "_inst." << c << "l" << procLevel[proc-1]-1 << " -> ";
                ss << proc-1 << "_inst." << c << "l" << procLevel[proc-1]-1;
                ss << "," << "\n";
              }
              else{
                ss << "\t\t" << "rep" << c << "l" << procLevel[proc-1]-1  << "."
                   << c << "l" << procLevel[proc-1]-1  <<  " -> ";
                ss << proc-1 << "_inst." << c << "l" << procLevel[proc-1]-1;
                ss << "," << "\n";
              }
            }
          }
        }
      }
      //Then write the output channels
      writeOutCh(out, frees, ss, level, procLevel);
      //End the network definition
      ss << "}" << "\n";
    }
  }
  //fpga end

void EngineOpenMP::writeKernel(const LoopB &kernel,
                               const jitk::SymbolTable &symbols,
                               const std::vector<bh_base *> &kernel_temps,
                               uint64_t codegen_hash,
                               std::stringstream &ss) {
  //fpga implementation
  cout << kernel << "\n";
  stringstream sme;
  blockWriter(kernel, symbols, nullptr, sme);

  //Write to file
  ofstream smeFile;
  smeFile.open("/home/tor/Desktop/UNI/Speciale/Code/ckernel2.sme");
  smeFile << sme.str();
  smeFile.close();

  //fpga end
    assert(kernel.rank == -1);

    // Write the need includes
    ss << "#include <stdint.h>\n";
    ss << "#include <stdlib.h>\n";
    ss << "#include <stdbool.h>\n";
    ss << "#include <complex.h>\n";
    ss << "#include <tgmath.h>\n";
    ss << "#include <math.h>\n";
    if (symbols.useRandom()) { // Write the random function
        ss << "#include <kernel_dependencies/random123_openmp.h>\n";
    }
    writeUnionType(ss); // We always need to declare the union of all constant data types
    ss << "\n";

    // Write the header of the execute function
    ss << "void execute_" << codegen_hash;
    writeKernelFunctionArguments(symbols, ss, nullptr);

    // Write the block that makes up the body of 'execute()'
    ss << "{\n";
    // Write allocations of the kernel temporaries
    for (const bh_base *b: kernel_temps) {
        util::spaces(ss, 4);
        ss << writeType(b->dtype()) << " * __restrict__ a" << symbols.baseID(b) << " = malloc(" << b->nbytes()
           << ");\n";
    }
    ss << "\n";

    writeBlock(symbols, nullptr, kernel, {}, false, ss);

    // Write frees of the kernel temporaries
    ss << "\n";
    for (const bh_base *b: kernel_temps) {
        util::spaces(ss, 4);
        ss << "free(" << "a" << symbols.baseID(b) << ");\n";
    }
    ss << "}\n\n";

    // Write the launcher function, which will convert the data_list of void pointers
    // to typed arrays and call the execute function
    {
        ss << "void launcher_" << codegen_hash
           << "(void* data_list[], uint64_t offset_strides[], union dtype constants[]) {\n";
        for (size_t i = 0; i < symbols.getParams().size(); ++i) {
            util::spaces(ss, 4);
            bh_base *b = symbols.getParams()[i];
            ss << writeType(b->dtype()) << " *a" << symbols.baseID(b);
            ss << " = data_list[" << i << "];\n";
        }

        util::spaces(ss, 4);
        ss << "execute_" << codegen_hash << "(";

        // We create the comma separated list of args and saves it in `stmp`
        stringstream stmp;
        for (size_t i = 0; i < symbols.getParams().size(); ++i) {
            bh_base *b = symbols.getParams()[i];
            stmp << "a" << symbols.baseID(b) << ", ";
        }

        uint64_t count = 0;
        for (const bh_view *view: symbols.offsetStrideViews()) {
            stmp << "offset_strides[" << count++ << "], ";
            for (int i = 0; i < view->ndim; ++i) {
                stmp << "offset_strides[" << count++ << "], ";
            }
        }

        if (not symbols.constIDs().empty()) {
            uint64_t i = 0;
            for (auto it = symbols.constIDs().begin(); it != symbols.constIDs().end(); ++it) {
                const jitk::InstrPtr &instr = *it;
                stmp << "constants[" << i++ << "]." << bh_type_text(instr->constant.type) << ", ";
            }
        }

        // And then we write `stmp` into `ss` excluding the last comma
        const string strtmp = stmp.str();
        if (not strtmp.empty()) {
            ss << strtmp.substr(0, strtmp.size() - 2);
        }
        ss << ");\n";
        ss << "}\n";
    }
}

std::string EngineOpenMP::info() const {
    stringstream ss;
    ss << std::boolalpha; // Printing true/false instead of 1/0
    ss << "----" << "\n";
    ss << "OpenMP:" << "\n";
    ss << "  Main memory: " << bh_main_memory_total() / 1024 / 1024 << " MB\n";
    ss << "  Hardware threads: " << std::thread::hardware_concurrency() << "\n";
    ss << "  Malloc cache limit: " << malloc_cache_limit_in_bytes / 1024 / 1024
       << " MB (" << malloc_cache_limit_in_percent << "% of unused memory)\n";
    ss << "  Cache dir: " << comp.config.defaultGet<boost::filesystem::path>("cache_dir", "NONE")  << "\n";
    ss << "  Temp dir: " << jitk::get_tmp_path(comp.config) << "\n";

    ss << "  Codegen flags:\n";
    ss << "    OpenMP: " << comp.config.defaultGet<bool>("compiler_openmp", false) << "\n";
    ss << "    OpenMP+SIMD: " << comp.config.defaultGet<bool>("compiler_openmp_simd", false) << "\n";
    ss << "    Index-as-var: " << comp.config.defaultGet<bool>("index_as_var", true) << "\n";
    ss << "    Strides-as-var: " << comp.config.defaultGet<bool>("strides_as_var", true) << "\n";
    ss << "    Const-as-var: " << comp.config.defaultGet<bool>("const_as_var", true) << "\n";

    ss << "  JIT Command: \"" << compiler.cmd_template << "\"\n";
    return ss.str();
}

// Return C99 types, which are used inside the C99 kernels
const std::string EngineOpenMP::writeType(bh_type dtype) {
    switch (dtype) {
        case bh_type::BOOL:
            return "bool";
        case bh_type::INT8:
            return "int8_t";
        case bh_type::INT16:
            return "int16_t";
        case bh_type::INT32:
            return "int32_t";
        case bh_type::INT64:
            return "int64_t";
        case bh_type::UINT8:
            return "uint8_t";
        case bh_type::UINT16:
            return "uint16_t";
        case bh_type::UINT32:
            return "uint32_t";
        case bh_type::UINT64:
            return "uint64_t";
        case bh_type::FLOAT32:
            return "float";
        case bh_type::FLOAT64:
            return "double";
        case bh_type::COMPLEX64:
            return "float complex";
        case bh_type::COMPLEX128:
            return "double complex";
        case bh_type::R123:
            return "r123_t"; // Defined by `write_c99_dtype_union()`
        default:
            std::cerr << "Unknown C99 type: " << bh_type_text(dtype) << std::endl;
            throw std::runtime_error("Unknown C99 type");
    }
}

string EngineOpenMP::userKernel(const std::string &kernel, std::vector<bh_view> &operand_list,
                                const std::string &compile_cmd, const std::string &tag, const std::string &param) {

    for (const bh_view &op: operand_list) {
        if (op.isConstant()) {
            return "[UserKernel] fatal error - operands cannot be constants";
        }
        bh_data_malloc(op.base);
    }
    string kernel_with_launcher;
    vector<void *> data_list;
    {
        stringstream ss;
        ss << kernel << "\n";
        ss << "void _bh_launcher(void *data_list[]) {\n";
        for (size_t i = 0; i < operand_list.size(); ++i) {
            ss << "    " << writeType(operand_list[i].base->dtype());
            ss << " *a" << i << " = data_list[" << i << "];\n";
            data_list.push_back(operand_list[i].base->getDataPtr());
        }
        ss << "    execute(";
        for (size_t i = 0; i < operand_list.size() - 1; ++i) {
            ss << "a" << i << ", ";
        }
        if (not operand_list.empty()) {
            ss << "a" << operand_list.size() - 1;
        }
        ss << ");\n";
        ss << "}\n";
        kernel_with_launcher = ss.str();
    }

    std::string source_filename = jitk::hash_filename(compilation_hash, util::hash(kernel_with_launcher), ".c");

    auto tcompile = chrono::steady_clock::now();
    UserKernelFunction func;
    try {
        KernelFunction f = getFunction(kernel_with_launcher, "_bh_launcher", compile_cmd);
        func = reinterpret_cast<UserKernelFunction>(f);
        assert(func != nullptr);
    } catch (const std::runtime_error &e) {
        return string(e.what());
    }
    stat.time_compile += chrono::steady_clock::now() - tcompile;

    auto start_exec = chrono::steady_clock::now();
    func(&data_list[0]);
    auto texec = chrono::steady_clock::now() - start_exec;
    stat.time_exec += texec;
    stat.time_per_kernel[source_filename].register_exec_time(texec);
    return "";
}


} // bohrium
