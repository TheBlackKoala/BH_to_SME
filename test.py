#!/usr/bin/dython
import bohrium as bh
thresh = 200
a = bh.array(range(32))
a = a/200
a = bh.add.reduce(a)
print(a)
