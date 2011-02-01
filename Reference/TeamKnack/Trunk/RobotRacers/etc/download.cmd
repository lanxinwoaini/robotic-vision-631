setMode -bscan
setCable -p auto
identify
assignFile -p 2 -file implementation/download.bit
program -e -prog -p 2
quit
