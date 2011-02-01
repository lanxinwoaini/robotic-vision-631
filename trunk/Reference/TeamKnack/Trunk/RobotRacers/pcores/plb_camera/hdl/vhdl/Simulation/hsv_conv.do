quit -sim


#vcom -work plb_camera ../camera_rgb.vhd
vcom hdl/vhdl/inv_table.vhd
vcom hdl/vhdl/hsv_conv.vhd
#vcom -work plb_camera ../hsv_buffer.vhd


vsim work.hsv_conv(fixedarch)


add wave -divider "1st Stage"
add wave -unsigned  R_int
add wave -unsigned  G_int
add wave -unsigned  B_int
add wave -unsigned  valid0
add wave -unsigned  v1_next
add wave -unsigned  maxIsR_next
add wave -unsigned  maxIsG_next
add wave -unsigned  minVal
add wave -unsigned  diff1_next

add wave -divider "2nd Stage"
add wave -unsigned  diff_inv
add wave -hex       diff_inv2_next
add wave -unsigned  diff2_next
add wave -signed    h2_next
add wave -unsigned  max_inv
add wave -unsigned  s2_next
add wave -unsigned  v1_reg
add wave -unsigned  valid2_reg

add wave -divider "3rd Stage"
add wave -hex       H_int1
add wave -hex       h3_1_next
add wave -unsigned  h3_2_next
add wave -unsigned  s3_next
add wave -unsigned   v2_reg
add wave -unsigned  valid3_reg

add wave -divider "4th Stage"
add wave -hex       H_int2
add wave -unsigned  H_int3
add wave -unsigned  h4_next
add wave -unsigned  s3_reg
add wave -unsigned   v3_reg
add wave -unsigned  valid4_reg





alias f "force"
alias rn "run 20ns"

f clk 1,0 10ns -r 20ns
f reset 1
run 3
f valid_in 0
f r 16#10
f g 16#10
f b 16#10

rn
rn

f reset 0
f valid_in 1, 0 20ns -r 40ns

    force r "10#160"
    set value [expr $i %13]
    force g "10#228"
    set value [expr $i %30]
    force b "10#104"
	## 46  137   226   93 54 89  green
	rn
	rn
    force r "10#200"
    set value [expr $i %13]
    force g "10#252"
    set value [expr $i %30]
    force b "10#104"
	## 81 59 99  green
	rn
	rn
    force r "10#240"
    set value [expr $i %13]
    force g "10#72"
    set value [expr $i %30]
    force b "10#80"
	## 357 70 94  red
	rn
	rn



set i 13
set limit [expr $i + 5]

while {$i < $limit} {
    set value [expr $i %20]
    force r "10#$value"
    set value [expr $i %13]
    force g "10#$value"
    set value [expr $i %30]
    force b "10#$value"
    incr i
    rn
    rn
    echo $i
}


