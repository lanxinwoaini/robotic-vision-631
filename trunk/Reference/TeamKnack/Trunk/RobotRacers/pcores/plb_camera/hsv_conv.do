quit -sim


#vcom -work plb_camera ../camera_rgb.vhd
#vcom -work plb_camera ../inv_table.vhd
#vcom -work hsv_conv.vhd
#vcom -work plb_camera ../hsv_buffer.vhd


vsim work.hsv_conv


add wave -unsigned * 



alias f "force"
alias rn "run 20ns"

f clk 1,0 10ns -r 20ns
f reset 1
f valid_in 0
f r 16#10
f g 16#10
f b 16#10

rn
rn

f reset 0
f valid_in 1

set i 1
set limit [expr $i + 32]

while {$i < $limit} {
    set value [expr $i %255]
    force r "10#$value"
    set value [expr $i %13]
    force g "10#$value"
    set value [expr $i %23]
    force b "10#$value"
    incr i
    run 20ns
    echo $i
}

