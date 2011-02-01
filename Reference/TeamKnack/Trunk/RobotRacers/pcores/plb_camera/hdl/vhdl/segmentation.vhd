library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;

entity segmentation is
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
end segmentation;

architecture hsv_imp of segmentation is
	component threshold is
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

	signal type_0_we : std_logic;
	signal type_1_we : std_logic;
	signal type_2_we : std_logic;
	signal in_valid_delayed : std_logic;
	signal color_channel_0_in_delayed : std_logic_vector (7 downto 0);
	signal color_channel_1_in_delayed : std_logic_vector (7 downto 0);
	signal color_channel_2_in_delayed : std_logic_vector (7 downto 0);

	signal channel_0_segmented_valid	: std_logic;
	signal channel_1_segmented_valid	: std_logic;
	signal channel_2_segmented_valid	: std_logic;
	signal channel_0_segmented			: std_logic;
	signal channel_1_segmented			: std_logic;
	signal channel_2_segmented			: std_logic;
	
	signal result_reg, result_next				: std_logic;
	signal result_valid_reg, result_valid_next	: std_logic;
begin
	--Specialize to the implementation
	type_0_we <= '1' when thresh_we = '1' and thresh_type_0_we = '1' else '0';
	type_1_we <= '1' when thresh_we = '1' and thresh_type_1_we = '1' else '0';
	type_2_we <= '1' when thresh_we = '1' and thresh_type_2_we = '1' else '0';

	
	data_out	<= result_reg;
	out_valid	<= result_valid_reg;
	
	--What it should be when not the correct object
	process (clk, reset)
	begin
		if reset = '1' then
			in_valid_delayed <= '0';
			color_channel_0_in_delayed <= (others => '0');
			result_reg <= '0';
			result_valid_reg <= '0';
		elsif clk'event and clk = '1' then
			in_valid_delayed <= in_valid;
			color_channel_0_in_delayed <= color_channel_0_in;
			color_channel_1_in_delayed <= color_channel_1_in;
			color_channel_2_in_delayed <= color_channel_2_in;
			result_reg <= result_next;
			result_valid_reg <= result_valid_next;
		end if;
	end process;
	result_valid_next <=	'1' when channel_0_segmented_valid = '1' and channel_1_segmented_valid = '1' and channel_2_segmented_valid = '1'
							else '0';
	result_next <=			'1' when channel_0_segmented = '1' and channel_1_segmented = '1' and channel_2_segmented = '1'
							else '0';

	HUE_SEGMENTATION:
	threshold
		generic map
		(
			C_DATA_WIDTH=>8
		)
		port map
		(
			clk=>clk,
			reset=>reset,

			min_valid=>type_0_we,
			min_in=>thresh_min,
			max_valid=>type_0_we,
			max_in=>thresh_max,
			min_out=>thresh_0_min_out,
			max_out=>thresh_0_max_out,

			in_valid=>in_valid_delayed,
			data_in=>color_channel_0_in_delayed,
			out_valid=>channel_0_segmented_valid,
			data_out=>channel_0_segmented
		);
	
	SATURATION_SEGMENTATION:
	threshold
		generic map
		(
			C_DATA_WIDTH=>8
		)
		port map
		(
			clk=>clk,
			reset=>reset,

			min_valid=>type_1_we,
			min_in=>thresh_min,
			max_valid=>type_1_we,
			max_in=>thresh_max,
			min_out=>thresh_1_min_out,
			max_out=>thresh_1_max_out,

			in_valid=>in_valid_delayed,
			data_in=>color_channel_1_in_delayed,
			out_valid=>channel_1_segmented_valid,
			data_out=>channel_1_segmented
		);

	VALUE_SEGMENTATION:
	threshold
		generic map
		(
			C_DATA_WIDTH=>8
		)
		port map
		(
			clk=>clk,
			reset=>reset,

			min_valid=>type_2_we,
			min_in=>thresh_min,
			max_valid=>type_2_we,
			max_in=>thresh_max,
			min_out=>thresh_2_min_out,
			max_out=>thresh_2_max_out,

			in_valid=>in_valid_delayed,
			data_in=>color_channel_2_in_delayed,
			out_valid=>channel_2_segmented_valid,
			data_out=>channel_2_segmented
		);

end hsv_imp;

