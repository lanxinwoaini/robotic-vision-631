library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;

-------------------------------------
--This entity converts data (sent from camera in 8-bit chunks)
--into HSV, and outputs the complete 24-bit value (HSV888)
entity hsv_buffer is
	generic
	(
		C_IN_WIDTH  : integer := 8;
		C_OUT_WIDTH : integer := 24
	);
	port
	(
		clk          : in  std_logic;
		reset        : in  std_logic;
		in_valid     : in  std_logic;
		data_in      : in  std_logic_vector(C_IN_WIDTH-1 downto 0);
		out_valid    : out std_logic;
		data_out     : out std_logic_vector(C_OUT_WIDTH-1 downto 0)
	);
end hsv_buffer;

architecture hsv_imp of hsv_buffer is
	component camera_rgb is
		generic
		(
			C_IN_WIDTH	: integer := 8;
			C_OUT_WIDTH : integer := 8;
			C_RGB_STYLE : integer := 1 --RGB565
		);
		port
		(
			clk				: in	std_logic;
			reset			: in	std_logic;
			in_valid		: in	std_logic;
			data_in			: in	std_logic_vector(C_IN_WIDTH-1 downto 0);

			out_valid		: out std_logic;
			r_out			: out std_logic_vector(C_OUT_WIDTH-1 downto 0);
			g_out			: out std_logic_vector(C_OUT_WIDTH-1 downto 0);
			b_out			: out std_logic_vector(C_OUT_WIDTH-1 downto 0)
		);
	end component;

	component hsv_conv is
		port
		(
			clk : IN std_logic;
			reset : IN std_logic;

			valid_in : IN std_logic;
			R : IN std_logic_vector(7 downto 0);
			G : IN std_logic_vector(7 downto 0);
			B : IN std_logic_vector(7 downto 0);

			H : OUT std_logic_vector(7 downto 0);
			S : OUT std_logic_vector(7 downto 0);
			V : OUT std_logic_vector(7 downto 0);
			valid_out : OUT std_logic
		);
	end component;

	signal r, g, b : std_logic_vector (7 downto 0);
	signal rgb_valid : std_logic;

	signal h, s, v : std_logic_vector (7 downto 0);
	signal hs_valid : std_logic;

begin
	MAKE_RGB: camera_rgb
		generic map
		(
			C_RGB_STYLE=>1
		)
		port map
		(
			clk=>clk,
			reset=>reset,
			in_valid=>in_valid,
			data_in=>data_in,

			out_valid=>rgb_valid,
			r_out=>r,
			g_out=>g,
			b_out=>b
		);

	RGB_TO_HSV:	hsv_conv
		port map
		(
			clk=>clk,
			reset=>reset,

			valid_in=>rgb_valid,
			R=>r,
			G=>g,
			B=>b,

			H=>h,
			S=>s,
			V=>v,
			valid_out=>hs_valid
		);

	out_valid <= hs_valid;
	data_out <= h & s & v;
end hsv_imp;

