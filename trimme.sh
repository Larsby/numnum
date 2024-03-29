#!/bin/bash
rm animated.gif
convert *.png -flatten flat.gif



IFS=" x+" read a b c d < <(convert flat.gif -format %@ info:)
echo "${a}x${b}+${c}+${d}"
convert *.png  -crop "${a}x${b}+${c}+${d}"  -set filename:f '%t' '%[filename:f].png' 
rm flat.gif
convert  -delay 3.33  -dispose Background -alpha Background *.png animatedDispose.gif
