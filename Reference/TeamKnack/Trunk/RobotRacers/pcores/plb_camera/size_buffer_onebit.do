quit -sim


#vcom -work plb_camera ../camera_rgb.vhd
#vcom -work plb_camera ../inv_table.vhd
#vcom -work hsv_conv.vhd
#vcom -work plb_camera ../hsv_buffer.vhd

vsim work.size_buffer_1bit


add wave -unsigned * 



alias f "force"
alias rn "run 20ns"

f clk 1,0 10ns -r 20ns
f reset 1
f in_valid 0
f data_in 0


rn
rn

f reset 0
f in_valid 1

set i 1
set limit [expr $i + 32]

while {$i < $limit} {
    set value [expr $i %2]
    force data_in "$value"
    incr i
    run 20ns
    echo $i
}

force data_in 1
run 60ns
force reset 1
run 20ns
force reset 0
force data_in 0
run 120ns

