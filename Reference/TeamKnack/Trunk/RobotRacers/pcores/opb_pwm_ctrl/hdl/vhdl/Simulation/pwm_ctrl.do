-- PWMController.do
-- Do file to simulate the VHDL entity PWMController

-- Initialize signals
force clk 0,1 5ns -r 10ns
force reset 0
force data_in 16#00000000
force rd_en 0
force wr_en 0
force addr "00"

-- Reset
force reset 1
run 10 ns
force reset 0
run 50 ns

run 200 ns

-- Set period to 4 cycles, leave duty at 0% (0 cycles)
force addr "01"
force data_in 16#4
force wr_en 1
run 10 ns
-- Make sure nothing happens
force wr_en 0
run 100 ns

-- Set duty to 100% (4 cycles)
force addr "00"
force data_in 16#4
force wr_en 1
run 10 ns
force wr_en 0
-- Examine output
run 100 ns

-- Set duty to 50% (2 cycles)
force addr "00"
force data_in 16#2
force wr_en 1
run 10 ns
force wr_en 0
-- Examine output
run 100 ns

-- Double period (duty now 25%)
force addr "01"
force data_in 16#8
force wr_en 1
run 10 ns
force wr_en 0
-- Examine output
run 170 ns

-- Triple duty (duty now 75%)
force addr "00"
force data_in 16#6
force wr_en 1
run 10 ns
force wr_en 0
-- Examine output
run 170ns

-- Test read
force addr "00"
force rd_en 1
run 10 ns
force addr "01"
run 10 ns
force addr "10"
run 10 ns
force addr "11"
run 10 ns

run 10 ns


