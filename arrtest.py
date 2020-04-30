#!/usr/bin/dython
#BH_OPENMP_VERBOSE=true
import bohrium as bh
# n= 2
# a = bh.array(range(32*n)).reshape(8*n,4)

a = bh.array(range(10000))
a = a+a
b = a+2
a = a+b

#a = a+a
#b = a-2
#a = a+b
#a = bh.add.reduce(a)
#b = a[:-1]
#c = b+2
#a = a+a[::-1]
print(a)
# print(b)
# print(c)
