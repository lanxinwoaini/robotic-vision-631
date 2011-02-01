library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity size_buffer_onebit is
  generic (
    C_OUT_WIDTH : integer := 8);
  port (
    clk          : in  std_logic;
    reset        : in  std_logic;
    in_valid     : in  std_logic;
    data_in      : in  std_logic;
    out_valid    : out std_logic;
    data_out     : out std_logic_vector(C_OUT_WIDTH-1 downto 0));
end size_buffer_onebit;

architecture imp_1bit of size_buffer_onebit is
  constant MAX_COUNT : integer := (C_OUT_WIDTH) - 1;
  subtype chunk_count_type is integer range 0 to MAX_COUNT;

  signal chunk_count : chunk_count_type;

  signal data_reg : std_logic_vector(C_OUT_WIDTH-1 downto 0);
  
begin

  data_out <= data_reg;
  
  process (clk, reset, in_valid, data_reg, data_in, chunk_count)
  begin
    if reset = '1' then
      out_valid <= '0';
	  chunk_count <= 0;
    elsif clk'event and clk = '1' then
      out_valid <= '0';
      if in_valid = '1' then
        data_reg <= data_reg(C_OUT_WIDTH-2 downto 0) & data_in;
        if chunk_count = MAX_COUNT then
          chunk_count <= 0;
          out_valid <= '1';
        else
          chunk_count <= chunk_count + 1;
        end if;
      end if;
    end if;
  end process;

end imp_1bit;



