library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;


--C_RGB_STYLE
--	0 RGB 555
--	1 RGB 565
--	2 RGB x444
--	3 RGB 444x
entity camera_rgb is
	generic
	(
		C_IN_WIDTH	: integer := 8;
		C_OUT_WIDTH : integer := 8;
		C_RGB_STYLE : integer := 1
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
end camera_rgb;


architecture imp of camera_rgb is
	constant CAMERA_PIXEL_SIZE : integer := 16;

	constant MAX_COUNT : integer := (CAMERA_PIXEL_SIZE/C_IN_WIDTH) - 1;
	subtype chunk_count_type is integer range 0 to MAX_COUNT;
	signal chunk_count_reg, chunk_count_next : chunk_count_type;

	signal data_reg, data_next : std_logic_vector(CAMERA_PIXEL_SIZE-1 downto 0);
	
	signal camera_pixel_complete_reg, camera_pixel_complete_next : std_logic;
begin
	
	process (clk, reset)
	begin
		if reset = '1' then
			data_reg <= (others=>'0');
			chunk_count_reg <= 0;
			camera_pixel_complete_reg <= '0';
		elsif clk'event and clk = '1' then
			data_reg <= data_next;
			chunk_count_reg <= chunk_count_next;
			camera_pixel_complete_reg <= camera_pixel_complete_next;
		end if;
	end process;

	process (in_valid, data_reg, data_in, chunk_count_reg)
	begin
		camera_pixel_complete_next <= '0';
		data_next <= data_reg;
		chunk_count_next <= chunk_count_reg;

		if in_valid = '1' then
			data_next <= data_reg(CAMERA_PIXEL_SIZE-C_IN_WIDTH-1 downto 0) & data_in;
			if chunk_count_reg = MAX_COUNT then
				chunk_count_next <= 0;
				camera_pixel_complete_next <= '1';
			else
				chunk_count_next <= chunk_count_reg + 1;
			end if;
		end if;
	end process;

	RGB555:
	if C_RGB_STYLE = 0 generate
		r_out <= data_reg(14 downto 10) & "000";
		g_out <= data_reg(9 downto 5) & "000";
		b_out <= data_reg(4 downto 0) & "000";
	end generate;
	RGB565:
	if C_RGB_STYLE = 1 generate
		r_out <= data_reg(15 downto 11) & "000";
		g_out <= data_reg(10 downto 5) & "00";
		b_out <= data_reg(4 downto 0) & "000";
	end generate;
	RGBx444:
	if C_RGB_STYLE = 2 generate
		r_out <= data_reg(11 downto 8) & "0000";
		g_out <= data_reg(7 downto 4) & "0000";
		b_out <= data_reg(3 downto 0) & "0000";
	end generate;
	RGB444x:
	if C_RGB_STYLE = 3 generate
		r_out <= data_reg(15 downto 12) & "0000";
		g_out <= data_reg(11 downto 7) & "0000";
		b_out <= data_reg(6 downto 3) & "0000";
	end generate;

	out_valid <= camera_pixel_complete_reg;
end imp;



