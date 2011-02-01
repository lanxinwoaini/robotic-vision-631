-------------------------------------------------------------------------------
-- Quadrature Encoder Core
--
-- File: quad_encoder.vhd
-- Author: Wade S. Fife
-- Date: February 2, 2006
--
-- DESCRIPTION
--
-- This core consists of a quadrature encoder detector for tracking position
-- and direction as indicated by a quadrature encoder. The output of this core
-- is simply a signed integer counter that gives the number of ticks of the
-- encoder. If the channel A encoder signal leads B then the counter is
-- incremented. If channel B leads A then the counter is decremented. By
-- frequently reading the encoder counter value you can determine position and
-- angular velocity of the encoder.
--
-- DOCUMENTATION
--
-- The C_COUNT_WIDTH generic determines the width of the counter. A small
-- counter will roll over sooner than a large counter. How long this takes
-- depends on the precision of the encoder used and the amount of rotation.
--
-- The "enc_a" and "enc_b" inputs are the two encoder channel signals. The
-- synchronous "clr" input allows you to reset the current encoder count. The
-- "count" output signal is the current encoder count, a signed 2's complement
-- number.
--
-------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

entity quad_encoder is
  generic (
    C_COUNT_WIDTH : integer := 32);
  port (
    clk    : in std_logic;
    rst    : in std_logic;
    clr    : in std_logic;
    enc_a  : in  std_logic;
    enc_b  : in  std_logic;
    count  : out std_logic_vector(C_COUNT_WIDTH-1 downto 0));
end entity quad_encoder;
	

architecture imp of quad_encoder is

  constant C_CTRL_WIDTH : integer := 2;

  -- Define enc_reg indices
  constant C_ENC_A : integer := 0;
  constant C_ENC_B : integer := 1;

  constant ENC_SAMPLE_PERIOD : integer := 50;  -- The system runs at 100Mhz but rise times for the encoders are 500ns, so I'm slowing down the sample rate to avoid glitching
 
  -- Encoder output states
  type enc_state_type is ( ENC_START,
                           ENC_00,
                           ENC_01,
                           ENC_10,
                           ENC_11);
  signal enc_state, next_enc_state : enc_state_type;

  signal enc_count_reg : unsigned(0 to C_COUNT_WIDTH-1);
  signal enc_count_inc : std_logic;
  signal enc_count_dec : std_logic;
  signal enc_count_clr : std_logic;
  signal enc_reg : unsigned(0 to 1);

  signal enc_a_buf_1 : std_logic;  -- buffer the encoder inputs to avoid glitching -- new by sf89, proposed by Wade, 2009
  signal enc_a_buf_2 : std_logic;  -- buffer the encoder inputs to avoid glitching -- new by sf89, proposed by Wade, 2009
  signal enc_b_buf_1 : std_logic;  -- buffer the encoder inputs to avoid glitching -- new by sf89, proposed by Wade, 2009
  signal enc_b_buf_2 : std_logic;  -- buffer the encoder inputs to avoid glitching -- new by sf89, proposed by Wade, 2009

  signal enc_sample_counter : unsigned(0 to 5);
  signal enc_sample_en : std_logic;     -- enable the encoder counter
 

begin

  enc_count_clr <= clr;
  count <= std_logic_vector(enc_count_reg);


  -- purpose: The encoder rise time is 500ns.  The Helios FPGA fabric runs at 100Mhz, so clk may be sampling much much faster than the encoder needs.  To avoid glitching during rise and fall times, we'll only sample every 50 clk cycles
  -- type   : sequential
  -- inputs : clk, rst, enc_sample_counter
  -- outputs: enc_sample_en
  SAMPLE_PERIOD: process (clk, rst)
  begin  -- process SAMPLE_PERIOD
   if clk'event and clk = '1' then  -- rising clock edge
      if rst = '1' then                   -- synchronous reset (active high)
        enc_sample_en <= '0';
        enc_sample_counter <= (others => '0');
      elsif enc_sample_counter = 50 then
        enc_sample_counter <= (others => '0');
        enc_sample_en <= '1';
      else
        enc_sample_counter <= enc_sample_counter + 1;
        enc_sample_en <= '0';
      end if;
    end if;
  end process SAMPLE_PERIOD;
  -----------------------------------------------------------------------------
  -- Buffer the encoder inputs to avoid glitching.  There was a problem with
  -- the encoders suddenly writing crazy negative values to the register and
  -- Wade thought buffering the inputs would fix some of that (plus it's better
  -- hardware practice) 
  -----------------------------------------------------------------------------
  INPUT_BUFFERS: process (clk, rst, enc_sample_en)
  begin  -- process INPUT_BUFFERS
    if clk'event and clk = '1' then  -- rising clock edge
      if rst = '1' then                   -- synchronous reset (active high)
        enc_a_buf_1 <= '0';
        enc_b_buf_1 <= '0';
        enc_a_buf_2 <= '0';
        enc_b_buf_2 <= '0';
      else
       enc_a_buf_1 <= enc_a;
        enc_b_buf_1 <= enc_b;
        if enc_sample_en = '1' then
          enc_a_buf_2 <= enc_a_buf_1;
          enc_b_buf_2 <= enc_b_buf_1;
        end if;
      end if;
    end if;
  end process INPUT_BUFFERS;
  -----------------------------------------------------------------------------
  -- Encoder counter
 ENC_COUNT_REG_PROC : process (clk, rst, enc_count_clr, enc_count_inc,
                                enc_count_reg, enc_count_dec)
  begin
    if clk'event and clk='1' then
      if rst = '1' then
        enc_count_reg <= (others => '0');
      else
        if enc_count_clr = '1' then
          enc_count_reg <= (others => '0');
        elsif enc_count_inc = '1' then
          enc_count_reg <= enc_count_reg + 1;
        elsif enc_count_dec = '1' then
          enc_count_reg <= enc_count_reg - 1;
        end if;
      end if;
    end if;
  end process ENC_COUNT_REG_PROC;

 
 -----------------------------------------------------------------------------
  -- State machine to determine motor direction based on encoder inputs

  ENC_FSM_REG : process (clk, rst, next_enc_state, enc_a_buf_2, enc_b_buf_2)
  begin
    if rst = '1' then
      enc_state <= ENC_START;
    elsif clk'event and clk='1' then
      enc_state <= next_enc_state;
      enc_reg <= enc_a_buf_2 & enc_b_buf_2;
    end if;
  end process ENC_FSM_REG;

  ENC_FSM_COMB : process (enc_state, enc_reg)
  begin
    -- Set default values
    next_enc_state <= enc_state;
    enc_count_inc <= '0';
    enc_count_dec <= '0';

    case enc_state is
      when ENC_START =>
        case enc_reg is
          when "00" =>
            next_enc_state <= ENC_00;
          when "01" =>
	            next_enc_state <= ENC_01;           
          when "10" =>
            next_enc_state <= ENC_10;           
          when others =>
            next_enc_state <= ENC_11;           
        end case;

      when ENC_00 =>
        if enc_reg(C_ENC_A) = '1' then
          enc_count_inc <= '1';
          next_enc_state <= ENC_10;
        elsif enc_reg(C_ENC_B) = '1' then
          enc_count_dec <= '1'; --
          next_enc_state <= ENC_01;
        end if;

      when ENC_01 =>
        if enc_reg(C_ENC_A) = '1' then
          enc_count_dec <= '1'; --
          next_enc_state <= ENC_11;
        elsif enc_reg(C_ENC_B) = '0' then
          enc_count_inc <= '1'; --
          next_enc_state <= ENC_00;
        end if;

     when ENC_10 =>
        if enc_reg(C_ENC_A) = '0' then
          enc_count_dec <= '1';
          next_enc_state <= ENC_00;
        elsif enc_reg(C_ENC_B) = '1' then
          enc_count_inc <= '1'; --
         next_enc_state <= ENC_11;
        end if;
       
      when ENC_11 =>
        if enc_reg(C_ENC_A) = '0' then
          enc_count_inc <= '1'; --
          next_enc_state <= ENC_01;
        elsif enc_reg(C_ENC_B) = '0' then
          enc_count_dec <= '1'; --
          next_enc_state <= ENC_10;
        end if;
      
      when others =>
        next_enc_state <= ENC_START;
    end case;
  end process ENC_FSM_COMB;
end imp;