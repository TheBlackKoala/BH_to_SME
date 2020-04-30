#!/usr/bin/dython
import bohrium as bh
thresh = 200
a = bh.random.random_integers(0,255,9*100000)
a = a/int(thresh)
a = bh.add.reduce(a)
print(a)
