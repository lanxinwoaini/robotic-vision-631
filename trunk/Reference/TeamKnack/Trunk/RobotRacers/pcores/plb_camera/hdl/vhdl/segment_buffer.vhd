library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;

entity segment_buffer is
	port
	(
			clk                : in  std_logic;
			reset              : in  std_logic;

			in_valid           : in  std_logic;
			data_in            : in  std_logic_vector(23 downto 0);

			out_valid          : out std_logic;
			data_out           : out std_logic;

			thresh_we          : in  std_logic;
			thresh_type_hue_we : in  std_logic;
			thresh_type_sat_we : in  std_logic;
			thresh_type_val_we : in  std_logic;
			thresh_max         : in  std_logic_vector(7 downto 0);
			thresh_min         : in  std_logic_vector(7 downto 0);
			
			thresh_hue_min_out : out std_logic_vector(7 downto 0);
			thresh_hue_max_out : out std_logic_vector(7 downto 0);
			thresh_sat_min_out : out std_logic_vector(7 downto 0);
			thresh_sat_max_out : out std_logic_vector(7 downto 0);
			thresh_val_min_out : out std_logic_vector(7 downto 0);
			thresh_val_max_out : out std_logic_vector(7 downto 0)
	);
end segment_buffer;

architecture hsv_imp of segment_buffer is


	component segmentation is
		port
		(
			clk         			: in  std_logic;
			reset        			: in  std_logic;

			in_valid     			: in  std_logic;
			color_channel_0_in      : in  std_logic_vector(7 downto 0);
			color_channel_1_in      : in  std_logic_vector(7 downto 0);
			color_channel_2_in      : in  std_logic_vector(7 downto 0);

			thresh_we               : in  std_logic;
			thresh_type_0_we        : in  std_logic;
			thresh_type_1_we        : in  std_logic;
			thresh_type_2_we        : in  std_logic;
			thresh_max              : in  std_logic_vector(7 downto 0);
			thresh_min              : in  std_logic_vector(7 downto 0);
			
			thresh_0_min_out        : out std_logic_vector(7 downto 0);
			thresh_0_max_out        : out std_logic_vector(7 downto 0);
			thresh_1_min_out        : out std_logic_vector(7 downto 0);
			thresh_1_max_out        : out std_logic_vector(7 downto 0);
			thresh_2_min_out        : out std_logic_vector(7 downto 0);
			thresh_2_max_out        : out std_logic_vector(7 downto 0);

			out_valid				: out std_logic;
			data_out				: out std_logic
		);
	end component;

	signal h, s, v : std_logic_vector (7 downto 0);
	signal hsv_valid : std_logic;

begin

	hsv_valid <= in_valid;
	h <= data_in(23 downto 16);
	s <= data_in(15 downto 8);
	v <= data_in(7 downto 0);
	
	TARGET_SEGMENTATION: segmentation
	port map
	(
		clk=>clk,
		reset=>reset,

		in_valid=>hsv_valid,
		color_channel_0_in=>h,
		color_channel_1_in=>s,
		color_channel_2_in=>v,

		thresh_we=>thresh_we,
		thresh_type_0_we=>thresh_type_hue_we,
		thresh_type_1_we=>thresh_type_sat_we,
		thresh_type_2_we=>thresh_type_val_we,
		thresh_max=>thresh_max,
		thresh_min=>thresh_min,
		
		thresh_0_min_out=>thresh_hue_min_out,
		thresh_0_max_out=>thresh_hue_max_out,
		thresh_1_min_out=>thresh_sat_min_out,
		thresh_1_max_out=>thresh_sat_max_out,
		thresh_2_min_out=>thresh_val_min_out,
		thresh_2_max_out=>thresh_val_max_out,

		out_valid=>out_valid,
		data_out=>data_out
	);
			
end hsv_imp;
