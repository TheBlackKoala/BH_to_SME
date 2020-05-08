#!/usr/bin/dython2
import bohrium as bh
thresh = 200
a = bh.random.random_integers(0,255,9*100000)
a = bh.floor_divide(a,thresh)
a = bh.add.reduce(a)
print(a)
