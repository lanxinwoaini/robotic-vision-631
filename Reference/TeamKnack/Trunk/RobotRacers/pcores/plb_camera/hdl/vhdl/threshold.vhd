library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;


entity threshold is
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
end threshold;


architecture imp_unbuffered of threshold is
	signal min_reg, min_next : std_logic_vector(C_DATA_WIDTH-1 downto 0);
	signal max_reg, max_next : std_logic_vector(C_DATA_WIDTH-1 downto 0);

	signal in_bounds : std_logic;

	signal in_bounds_contig : std_logic;
	signal in_bounds_split : std_logic;
	signal is_contig : std_logic;
begin

	process (clk, reset)
	begin
		if reset = '1' then
			min_reg <= (others=>'0');
			max_reg <= (others=>'1');
		elsif clk'event and clk = '1' then
			min_reg <= min_next;
			max_reg <= max_next;
		end if;
	end process;
	min_next <= min_in when min_valid = '1' else min_reg;
	max_next <= max_in when max_valid = '1' else max_reg;


	is_contig <= '1' when min_reg < max_reg else '0';
	in_bounds_contig <= '1' when min_reg <= data_in and data_in <= max_reg else '0';
	in_bounds_split <= '1' when data_in <= max_reg or min_reg <= data_in else '0';
	in_bounds <= in_bounds_contig when is_contig = '1' else in_bounds_split;

	data_out <= in_bounds;
	out_valid <= in_valid;

	min_out <= min_reg;
	max_out <= max_reg;
end imp_unbuffered;

