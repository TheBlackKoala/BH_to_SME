#!/usr/bin/dython
#BH_OPENMP_VERBOSE=true
import bohrium as bh

a = bh.array(range(9)).reshape(3,3)
a = a+a
b = a+2
a = a+b
#b = a[:-1]
#c = b+2
#a = a+a[::-1]
print(a)
# print(b)
# print(c)
