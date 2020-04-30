#!/usr/bin/dython2
import bohrium as bh
a = bh.array(range(9*100000))
a = bh.sum(a)
print(a)
