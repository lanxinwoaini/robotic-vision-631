-------------------------------------------------------------------------------
-- Sobel line buffer
--
-- Filename: line_buffer.vhd
-- Auther: Reed Curtis
-- Date: 2/9/09
--
-- Data is buffered by asserting din and wr_en for one clock cycle. As soon as
-- C_DEPTH more words have been buffered, the first word then appears on dout
-- and valid will go high for once clock cycle. Exactly C_DEPTH values can be
-- buffered simultaneously. See the timing diagram below for an example.
--
-- Note that this is not a true FIFO nor is it a pure delay. The actual number
-- of clock cycles of delay depends on wr_en. Basically, data can only be
-- output on dout on the cycle after when wr_en is asserted. This allows you to
-- keep the input stream (din) in sync with the output stream (dout). However,
-- if wr_en is always asserted then the delay will be exactly C_DEPTH+1 cycles,
-- and this entity is a pure delay. See the timing diagram below for an
-- example.
--
-- The C_WIDTH generic is the width of the data ports. C_DEPTH is the
-- number of cycles to buffer or delay. Data written on cycle 0 will be output
-- on dout on cycle C_DEPTH (or C_DEPTH asserts of wr_en)
--
-- A timing example is shown below where C_DEPTH is 2 and the buffer is
-- initially empty.
--
--    Cycle | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | A | B | C | D | E | F |
--    ------|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
--      din | X | A | B | C | D | X | X | E | F | X | X | 2 | 3 | 4 | X | X |
--    wr_en | 0 | 1 | 1 | 1 | 1 | 0 | 0 | 1 | 1 | 0 | 0 | 1 | 1 | 1 | 0 | 0 |
--     dout | ? | ? | ? | ? | A | B | B | B | C | D | D | D | E | F | 2 | 2 |
--    valid | 0 | 0 | 0 | 0 | 1 | 1 | 0 | 0 | 1 | 1 | 0 | 0 | 1 | 1 | 1 | 0 |
--
-- Note that dout holds it's value until valid goes high again.
--
-- A single RAMB16_S19_S19 element is used for the buffering. As a result
-- C_DEPTH can be up to 2047 and C_WIDTH is 8. 
--
-- The reset of this entity is active-high and synchronous. Note that reset
-- does not clear the values in the block RAM. As a result, until the buffer is
-- full, dout may not be 0 after a reset. The valid signal must be used to
-- discriminate between latent and new values
--
-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

library unisim;
use unisim.vcomponents.all;


entity line_buffer is
  generic (
    C_DEPTH : integer := 640;
    C_WIDTH : integer := 16);
  port (
    clk   : in  std_logic;
    rst   : in  std_logic;
    din   : in  std_logic_vector(C_WIDTH-1 downto 0);
    wr_en : in  std_logic;
    dout  : out std_logic_vector(C_WIDTH-1 downto 0);
    valid : out std_logic);
end line_buffer;


architecture imp of line_buffer is

  -- Function to calculate number of bits needed to represent a given
  -- positive integer (up to max_bits)
  function RequiredBits (int_num, max_bits : integer)
  return integer is
    variable test_vector : unsigned(0 to max_bits-1);
  begin
    test_vector := (others => '0');
    for i in 1 to max_bits loop
      test_vector := test_vector(1 to max_bits-1) & '1';
      if to_integer(test_vector) >= int_num then
        return i;
      end if;
    end loop;
    return max_bits;
  end function RequiredBits;
  
  constant ADDRESS_WIDTH : integer := 10;
  constant RAM_ADDRESS_WIDTH : integer := 16;
  signal write_data : std_logic_vector(RAM_ADDRESS_WIDTH-1 downto 0); -- data to be written to the buffer
  signal read_data : std_logic_vector(RAM_ADDRESS_WIDTH-1 downto 0); -- data read from the buffer
  signal write_address  : std_logic_vector(ADDRESS_WIDTH-1 downto 0); -- address to write to
  signal read_address : std_logic_vector(ADDRESS_WIDTH-1 downto 0); -- address to read from 
  signal write_address_reg  : unsigned(ADDRESS_WIDTH-1 downto 0); --register write/read address data 
  signal read_address_reg : unsigned(ADDRESS_WIDTH-1 downto 0);

  signal valid_count_reg : std_logic;
  signal valid_counter :
    unsigned(RequiredBits(C_DEPTH+1, ADDRESS_WIDTH)-1 downto 0);
  
  signal valid_reg : std_logic;
  
begin

 

  assert C_DEPTH >= 1 and C_DEPTH <= 2047
    report "C_DEPTH must be from 1 to 2047."
    severity ERROR;

  -- Convert unsigned registers to std_logic_vector signals
  write_address <= std_logic_vector(write_address_reg);
  read_address <= std_logic_vector(read_address_reg);


  write_data <=  din;
  dout <= read_data;

  valid <= valid_reg and valid_count_reg;

  
  ADDR_REG_PROC: process (clk, rst, write_address_reg, read_address_reg)
  begin
    if clk'event and clk = '1' then
      if rst = '1' then
        write_address_reg <= to_unsigned(C_DEPTH, ADDRESS_WIDTH);
        read_address_reg <= (others => '0');
      elsif wr_en = '1' then
        write_address_reg <= write_address_reg + 1;
        read_address_reg <= read_address_reg + 1;
      end if;
    end if;
  end process;


  VALID_COUNT_PROC: process (clk, rst, valid_counter)
  begin
    if clk'event and clk = '1' then
      if rst = '1' then
        valid_counter <= (others => '0');
        valid_count_reg <= '0';
      elsif valid_counter /= C_DEPTH+1 then
        valid_counter <= valid_counter + 1;
      else
        valid_count_reg <= '1';
      end if;
    end if;
  end process;
  

  VALID_REG_PROC: process (clk, rst, wr_en)
  begin
    if clk'event and clk = '1' then
      if rst = '1' then
        valid_reg <= '0';
      else
        valid_reg <= wr_en;
      end if;
    end if;
  end process;
  

  LINE_BUFFER: RAMB16_S18_S18
    generic map (
      WRITE_MODE_A => "READ_FIRST",
      WRITE_MODE_B => "READ_FIRST")
    port map (
        CLKA  => clk,
        ENA   => '1', --always able to write to port A
        WEA   => wr_en,
        SSRA  => rst,
        ADDRA => write_address,
        DIA   => write_data,
        DIPA  => (others => '0'), --don't worry about parity bits
        DOA   => open, --never read from port a, only write to it.
        DOPA  => open,
        CLKB  => clk,
        ENB   => wr_en,
        WEB   => '0',
        SSRB  => rst,
        ADDRB => read_address,
        DIB   => (others => '0'), --never write to port b, only read from it.
        DIPB  => (others => '0'),
        DOB   => read_data,
        DOPB  => open);

end imp;
