-- OPB_PWMController.do

-- This do file tests the file OPB_PWMController.vhd. Note that the
-- pselect component may need to be removed and device_select should
-- be tied directly to OPB_select for this do file to work.

-- To test different controllers, modify the appropriate bits in
-- OPB_ABus (5th and 6th bits from the right). The example below
-- controls motor controller number 2 ("...10XXXX").

-- Initialize signals
force OPB_Clk 0,1 5ns -r 10ns
force OPB_Rst 0
force OPB_ABus 16#00000000
force OPB_BE 16#0
force OPB_DBus 16#00000000
force OPB_RNW 0
force OPB_select 0
force OPB_seqAddr 0

-- Reset
force OPB_Rst 1
run 10 ns
force OPB_Rst 0

-- Simulate bus write (Set period to 6)
force OPB_select 1
force OPB_RNW 0
force OPB_ABus "11111111111111111111111100100100"
force OPB_DBus 16#6
run 30 ns
force OPB_select 0
run 10 ns

-- Simulate bus write (Set duty to 4)
force OPB_select 1
force OPB_RNW 0
force OPB_ABus "11111111111111111111111100100000"
force OPB_DBus 16#4
run 30 ns
force OPB_select 0
run 10 ns

-- Simulate bus read (Read back period)
force OPB_select 1
force OPB_ABus "11111111111111111111111100100100"
force OPB_RNW 1
run 40 ns
force OPB_select 0
run 10 ns

-- Run for a while to view output
run 100 ns

