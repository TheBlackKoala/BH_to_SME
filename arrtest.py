#!/usr/bin/dython2
import bohrium as bh

a = bh.array(range(9*100000))
a = a+a
b = a+2
a = a+b

print(a)
