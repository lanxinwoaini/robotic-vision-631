#convolutiontest.do

quit -sim
vcom ./hdl/vhdl/convolution.vhd
vcom ./hdl/vhdl/line_buffer.vhd


vsim work.convolution

alias f "force"
alias r "run 20ns"

view wave

add wave clk
add wave rst
add wave -divider "State"
add wave -color "orange red" state_reg
add wave -color "orange" state_next
add wave -unsigned initial_invalid_count
add wave -unsigned initial_invalid_reg
add wave -unsigned initial_zero_count
add wave -unsigned initial_zero_reg
add wave -unsigned between_frames_count
add wave -unsigned between_frames_reg
add wave -unsigned col_count_next
add wave -color gray50 -unsigned col_count_reg
add wave -unsigned row_count_next
add wave -color gray20 -unsigned row_count_reg

add wave -divider "Inputs"
add wave -color "yellow" din
add wave valid_in
add wave -divider "Outputs"
add wave -unsigned data_out
add wave -unsigned pixel_count
add wave -unsigned pixel_count_reg
add wave -unsigned sum
add wave valid_out

add wave -divider "Kernel"

add wave -color "violet" left_FFs_reg
add wave -color "blue violet" middle_FFs_reg
add wave -color "slate blue" right_FFs_reg


add wave -divider "Kernel inputs"
add wave -color goldenrod right_FFs_next;

add wave -divider "First Line Buffer"
add wave -color "orange red" linebuffers0to15/din
add wave -color "orange red" linebuffers0to15/wr_en
add wave -color "orange red" linebuffers0to15/dout


add wave -divider "Last Line Buffer"
add wave -color "orange" linebuffers16to31/din
add wave -color "orange" linebuffers16to31/wr_en
add wave -color "orange" linebuffers16to31/dout

f clk 1, 0 10ns -r 20ns
f rst 1
f din 0
f valid_in 0
run 40ns
f rst 0
f valid_in 1,0 720ns -r 840ns

set pixelcount 0
set validdatacount 0

set i 0
set ilimit 40

while {$i < $ilimit} {

 force din 1
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 
 if { [examine valid_in] ==1 } {
	incr i
 }
 
 force din 0 
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 
 set pixelcount [expr $pixelcount +6]
 echo $validdatacount
 

}


set i 0
set ilimit 40


while {$i < $ilimit} {

 force din 0
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 
 if { [examine valid_in] ==1 } {
	incr i
 }
 
 force din 1 
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 
 set pixelcount [expr $pixelcount +6]
 echo $validdatacount
 

}




set i 0
set ilimit 40


while {$i < $ilimit} {

 force din 0
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
  force din 1
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 
 if { [examine valid_in] ==1 } {
	incr i
 }
 
 force din 1 
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 force din 0
 run 20ns
 if { [examine valid_in] == 1} {
     incr validdatacount
 }
 run 20ns
 
 set pixelcount [expr $pixelcount +6]
 echo $validdatacount
 

}

