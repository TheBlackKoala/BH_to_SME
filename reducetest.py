#!/usr/bin/dython
import bohrium as bh
a = bh.array(range(9*100000))
a = bh.add.sum(a)
print(a)
