#!/bin/bash
cat engine_openmp.cpp > git_engine_openmp.cpp
git add .
git commit -m "$1"
git push origin
