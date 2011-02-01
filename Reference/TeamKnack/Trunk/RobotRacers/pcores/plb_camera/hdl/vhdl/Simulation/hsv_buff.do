quit -sim


#vcom -work plb_camera ../camera_rgb.vhd
vcom hdl/vhdl/inv_table.vhd
vcom hdl/vhdl/hsv_conv.vhd
vcom hdl/vhdl/hsv_buffer.vhd
vcom hdl/vhdl/camera_rgb.vhd
#vcom -work plb_camera ../hsv_buffer.vhd


vsim work.hsv_buffer


add wave -divider "External inputs"

add wave -hex data_in
add wave in_valid
add wave -hex data_out
add wave -hex out_valid


add wave -divider "inputs"

add wave -hex r
add wave -hex g
add wave -hex b
add wave hs_valid
add wave rgb_valid
add wave -hex h
add wave -hex s
add wave -hex v



alias f "force"
alias rn "run 20ns"

f clk 1,0 10ns -r 20ns
f reset 1
f in_valid 0
f data_in 16#10

rn
rn

f reset 0
f in_valid 1

set i 13
set limit [expr $i + 32]
set value 1
set value2 1
while {$i < $limit} {
    set value [expr ($value*4+3) %255]
    set value2 [expr ($value2*$value+2) %255]
    force data_in "10#$value"
	rn
    force data_in "10#$value2"
    incr i
    rn
    echo $i
}


