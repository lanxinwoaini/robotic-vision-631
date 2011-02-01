#conv_buff_test.do

quit -sim
vcom convolution.vhd
vcom line_buffer.vhd
vcom threshold.vhd
vcom conv_buffer.vhd


vsim work.conv_buffer

alias f "force"
alias r "run 20ns"

view wave

add wave -divider "Inputs"
add wave clk
add wave reset
add wave -color "yellow" conv_din
add wave conv_valid_in

add wave -divider "Threshold values"
add wave min_in
add wave min_valid
add wave max_in
add wave max_valid
add wave min_out
add wave max_out

add wave -divider "Conv Outputs"
add wave -unsigned conv_data_out
add wave conv_valid_out

add wave -divider "Threshold Outputs"
add wave -color orange thresh_data_out
add wave thresh_out_valid




f clk 1, 0 10ns -r 20ns
f reset 1
f conv_valid_in 0
f min_in 10#60
f min_valid 1
f max_in 10#90
f max_valid 1
run 20ns
f reset 0
f conv_valid_in 1
f min_valid 0
f max_valid 0

#f conv_din 1 
#run 20ns
#f conv_din 0 
#r 4000ns
#f conv_valid_in 1,0 200ns -r 220ns
#f conv_din 1, 0 20ns -r 40ns



set i 0
set ilimit 40

while {$i < $ilimit} {

 force conv_din 1
 run 60ns
 force conv_din 0 
 run 60ns
 
	
 incr i
}


set i 0
set ilimit 40

while {$i < $ilimit} {

 force conv_din 0
 run 60ns
 force conv_din 1 
 run 60ns
 
	
 incr i
}

set i 0
set ilimit 40

while {$i < $ilimit} {

 force conv_din 0
 run 40ns
 force conv_din 1 
 run 60ns
 force conv_din 0
 run 20ns
 
	
 incr i
}
    