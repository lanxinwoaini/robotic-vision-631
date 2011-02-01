--------------------------------------------------------------------------------
--     This file is owned and controlled by Xilinx and must be used           --
--     solely for design, simulation, implementation and creation of          --
--     design files limited to Xilinx devices or technologies. Use            --
--     with non-Xilinx devices or technologies is expressly prohibited        --
--     and immediately terminates your license.                               --
--                                                                            --
--     XILINX IS PROVIDING THIS DESIGN, CODE, OR INFORMATION "AS IS"          --
--     SOLELY FOR USE IN DEVELOPING PROGRAMS AND SOLUTIONS FOR                --
--     XILINX DEVICES.  BY PROVIDING THIS DESIGN, CODE, OR INFORMATION        --
--     AS ONE POSSIBLE IMPLEMENTATION OF THIS FEATURE, APPLICATION            --
--     OR STANDARD, XILINX IS MAKING NO REPRESENTATION THAT THIS              --
--     IMPLEMENTATION IS FREE FROM ANY CLAIMS OF INFRINGEMENT,                --
--     AND YOU ARE RESPONSIBLE FOR OBTAINING ANY RIGHTS YOU MAY REQUIRE       --
--     FOR YOUR IMPLEMENTATION.  XILINX EXPRESSLY DISCLAIMS ANY               --
--     WARRANTY WHATSOEVER WITH RESPECT TO THE ADEQUACY OF THE                --
--     IMPLEMENTATION, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OR         --
--     REPRESENTATIONS THAT THIS IMPLEMENTATION IS FREE FROM CLAIMS OF        --
--     INFRINGEMENT, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS        --
--     FOR A PARTICULAR PURPOSE.                                              --
--                                                                            --
--     Xilinx products are not intended for use in life support               --
--     appliances, devices, or systems. Use in such applications are          --
--     expressly prohibited.                                                  --
--                                                                            --
--     (c) Copyright 1995-2005 Xilinx, Inc.                                   --
--     All rights reserved.                                                   --
--------------------------------------------------------------------------------
-- You must compile the wrapper file hfdfifo.vhd when simulating
-- the core, hfdfifo. When compiling the wrapper file, be sure to
-- reference the XilinxCoreLib VHDL simulation library. For detailed
-- instructions, please refer to the "CORE Generator Help".

-- The synopsys directives "translate_off/translate_on" specified
-- below are supported by XST, FPGA Compiler II, Mentor Graphics and Synplicity
-- synthesis tools. Ensure they are correct for your synthesis tool(s).

LIBRARY ieee;
USE ieee.std_logic_1164.ALL;
-- synopsys translate_off
Library XilinxCoreLib;
-- synopsys translate_on
ENTITY hfdfifo IS
	port (
	clk: IN std_logic;
	din: IN std_logic_VECTOR(11 downto 0);
	rd_en: IN std_logic;
	rst: IN std_logic;
	wr_en: IN std_logic;
	dout: OUT std_logic_VECTOR(11 downto 0);
	empty: OUT std_logic;
	full: OUT std_logic);
END hfdfifo;

ARCHITECTURE hfdfifo_a OF hfdfifo IS
-- synopsys translate_off
component wrapped_hfdfifo
	port (
	clk: IN std_logic;
	din: IN std_logic_VECTOR(11 downto 0);
	rd_en: IN std_logic;
	rst: IN std_logic;
	wr_en: IN std_logic;
	dout: OUT std_logic_VECTOR(11 downto 0);
	empty: OUT std_logic;
	full: OUT std_logic);
end component;

-- Configuration specification 
	for all : wrapped_hfdfifo use entity XilinxCoreLib.fifo_generator_v2_1(behavioral)
		generic map(
			c_wr_response_latency => 1,
			c_has_rd_data_count => 0,
			c_din_width => 12,
			c_has_wr_data_count => 0,
			c_implementation_type => 0,
			c_family => "virtex4",
			c_has_wr_rst => 0,
			c_underflow_low => 0,
			c_has_meminit_file => 0,
			c_has_overflow => 0,
			c_preload_latency => 1,
			c_dout_width => 12,
			c_rd_depth => 1024,
			c_default_value => "BlankString",
			c_mif_file_name => "BlankString",
			c_has_underflow => 0,
			c_has_rd_rst => 0,
			c_has_almost_full => 0,
			c_has_rst => 1,
			c_data_count_width => 2,
			c_has_wr_ack => 0,
			c_wr_ack_low => 0,
			c_common_clock => 1,
			c_rd_pntr_width => 10,
			c_has_almost_empty => 0,
			c_rd_data_count_width => 2,
			c_enable_rlocs => 0,
			c_wr_pntr_width => 10,
			c_overflow_low => 0,
			c_prog_empty_type => 0,
			c_optimization_mode => 0,
			c_wr_data_count_width => 2,
			c_preload_regs => 0,
			c_dout_rst_val => "0",
			c_has_data_count => 0,
			c_prog_full_thresh_negate_val => 768,
			c_wr_depth => 1024,
			c_prog_empty_thresh_negate_val => 256,
			c_prog_empty_thresh_assert_val => 256,
			c_has_valid => 0,
			c_init_wr_pntr_val => 0,
			c_prog_full_thresh_assert_val => 768,
			c_has_backup => 0,
			c_valid_low => 0,
			c_prim_fifo_type => 1024,
			c_count_type => 0,
			c_prog_full_type => 0,
			c_memory_type => 1);
-- synopsys translate_on
BEGIN
-- synopsys translate_off
U0 : wrapped_hfdfifo
		port map (
			clk => clk,
			din => din,
			rd_en => rd_en,
			rst => rst,
			wr_en => wr_en,
			dout => dout,
			empty => empty,
			full => full);
-- synopsys translate_on

END hfdfifo_a;

