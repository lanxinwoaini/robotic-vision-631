library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;

entity conv_buffer is 
	generic
	(
		C_DATA_WIDTH	: integer := 8
	);
	port(
		clk, reset 			: in std_logic;
		frame_reset			: in std_logic;
		conv_valid_in 		: in std_logic;
		conv_din 			: in std_logic;
		conv_data_out		: out std_logic_vector(7 downto 0); --this output will output the total pixel count in each convolution frame. This output should be attached to a threshold block to produce a single pixel denoting whether the count satisfies the designated threshold value.
		conv_valid_out		: out std_logic;
	  
		reg_val_in				: in std_logic_vector(15 downto 0);
		col_init_we				: in std_logic;
		row_init_we				: in std_logic;
		between_frames_init_we	: in std_logic;
		initial_invalid_init_we	: in std_logic;
		initial_zero_init_we	: in std_logic;
		zeros_val_we			: in std_logic;
	  
		min_valid		: in std_logic;
		min_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
		max_valid		: in std_logic;
		max_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
		min_out			: out	std_logic_vector(C_DATA_WIDTH-1 downto 0); --always output the min threshold value
		max_out			: out	std_logic_vector(C_DATA_WIDTH-1 downto 0); --always output the max threshold value

		thresh_out_valid		: out	std_logic;
		thresh_data_out		: out	std_logic --binary threshold (1 or 0)
	);

end conv_buffer;

architecture kernel_conv of conv_buffer is


	component convolution
		generic (
			C_ROWS : integer := 480;
			C_COLS: integer := 640;
			CONV_HEIGHT : integer := 33;
			CONV_WIDTH : integer := 3;
			C_WIDTH : integer := 1);
		port(
			clk, rst 			: in std_logic;
			frame_reset			: in std_logic;
			valid_in 			: in std_logic;
			din 				: in std_logic;

			reg_val_in				: in std_logic_vector(15 downto 0);
			col_init_we				: in std_logic;
			row_init_we				: in std_logic;
			between_frames_init_we	: in std_logic;
			initial_invalid_init_we	: in std_logic;
			initial_zero_init_we	: in std_logic;
			zeros_val_we			: in std_logic;

			data_out			: out std_logic_vector(7 downto 0); --this output will output the total pixel count in each convolution frame. This output should be attached to a threshold block to produce a single pixel denoting whether the count satisfies the designated threshold value.
			valid_out			: out std_logic);
    end component;
	
	component threshold
		generic
		(
			C_DATA_WIDTH	: integer := 8
		);
		port
		(
			clk				: in	std_logic;
			reset			: in	std_logic;

			min_valid		: in std_logic;
			min_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
			max_valid		: in std_logic;
			max_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
			min_out			: out	std_logic_vector(C_DATA_WIDTH-1 downto 0); --always output the min threshold value
			max_out			: out	std_logic_vector(C_DATA_WIDTH-1 downto 0); --always output the max threshold value

			in_valid		: in	std_logic;
			data_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
			out_valid		: out	std_logic;
			data_out		: out	std_logic --binary threshold (1 or 0)
		);
	end component;	
	
	signal sig_conv_data_out : std_logic_vector(7 downto 0);
	signal sig_conv_valid_out : std_logic;
	
	begin
	
	conv_valid_out <= sig_conv_valid_out;
	conv_data_out  <= sig_conv_data_out;
	

	lil_conv : convolution
	generic map(
		C_ROWS => 480,
		C_COLS => 640,
		CONV_HEIGHT => 33,
		CONV_WIDTH => 3,
		C_WIDTH => 1)
	port map(
	  clk 				=> clk,
	  rst 				=> reset,
	  frame_reset       => frame_reset,
	  valid_in 			=> conv_valid_in,
	  din 				=> conv_din,

	  reg_val_in				=> reg_val_in,
	  col_init_we				=> col_init_we,
	  row_init_we				=> row_init_we,
	  between_frames_init_we	=> between_frames_init_we,
	  initial_invalid_init_we	=> initial_invalid_init_we,
	  initial_zero_init_we		=> initial_zero_init_we,
	  zeros_val_we				=> zeros_val_we,
	  
	  data_out			=> sig_conv_data_out,		
	  valid_out			=> sig_conv_valid_out);
	
	
	lil_thresh : threshold
		generic map(
			C_DATA_WIDTH => 8)
		port map(
			clk				=> clk,
			reset			=> reset,

			min_valid		=> min_valid,
			min_in			=> min_in,
			max_valid		=> max_valid,
			max_in			=> max_in,
			min_out			=> min_out,
			max_out			=> max_out,

			in_valid		=> sig_conv_valid_out,
			data_in			=> sig_conv_data_out,
			out_valid		=> thresh_out_valid,
			data_out		=> thresh_data_out
		);
	
end kernel_conv;