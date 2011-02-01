-------------------------------------------------------------------------------
--
-- File: fpu_light.vhd
-- Author: Wade S. Fife
-- Date: February 22, 2005
--
--                     Copyright (c) 2005 Wade Fife
--
-- DESCRIPTION
--
-- This module contains a simplified floating point unit similar to the IEE 754
-- standard. It differs from IEEE in that it does not support +/-NaN, +/-INF,
-- denormalized numbers, or extra rounding modes. Only the "round towards
-- nearest even" rounding mode is supported, which is the IEEE default.
--
-- The number encoding is the same as IEEE 754's normalized numbers:
--
--    1   8             23
--    SEEEEEEEEMMMMMMMMMMMMMMMMMMMMMMM
--
-- where S is the sign bit, E is the 8-bit exponent, and M is the lower 23 bits
-- of the significand. A 1-bit is always implied at the far left, so the
-- significand is always 24 bits. Since there's no support for NaN, INF, or
-- denormalized numbers, this encoding is always used, except for zero. Zero,
-- as with IEEE 754, is represented by all zero bits in the exponent and
-- significand. However, the entities in this module always output 0 for the
-- sign bit if the result is zero.
--
-- The mult_light entity supports floating point multiply. The add_light entity
-- supports addition and subtraction, depending on the op input (0=add, 1=sub).
--
-- To begin an operation, assert the inputs and set the enable (en) input of
-- the entity to '1'. The result will appear on the output on the same cycle
-- that the valid output goes high.
--
-- The pipelining stages of these entities are configurable. The input
-- registers, output registers, and interlock registers between pipeline stages
-- can be enabled or disabled by appropriately setting the boolean generics for
-- each entity. See the schematics for each entity to gain insight into the
-- pipeline stages divide the logic.
--
-------------------------------------------------------------------------------



-------------------------------------------------------------------------------
-- ADD LIGHT ------------------------------------------------------------------
-------------------------------------------------------------------------------
-- Lightweight Single Precision Floating Point Adder/Subtracter
--
-- The critical path depends on the interlocks but is generally through the
-- leading zeros in the first normalization stage. Perhaps better results could
-- be obtained with a custom circuit rather than plain VHDL.
--
-- The default configuration runs at 86 MHz and consumes 343 slices of FPGA
-- realestate on a Virtex 2, speed grade -4.
--

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity add_light is
  generic (
    USE_INPUT_REG     : boolean := true;
    USE_1_2_INTERLOCK : boolean := false;
    USE_2_3_INTERLOCK : boolean := false;
    USE_3_4_INTERLOCK : boolean := true;
    USE_4_5_INTERLOCK : boolean := false;
    USE_5_6_INTERLOCK : boolean := true;
    USE_6_7_INTERLOCK : boolean := false;
    USE_7_8_INTERLOCK : boolean := true;
    USE_8_9_INTERLOCK : boolean := false;
    USE_OUTPUT_REG    : boolean := false);
  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    op_a   : in  unsigned(31 downto 0);
    op_b   : in  unsigned(31 downto 0);
    op     : in  std_logic;
    valid  : out std_logic;
    output : out unsigned(31 downto 0));
end add_light;



architecture imp of add_light is
  component leading_zeros_25
    port (
      input  : in  unsigned(24 downto 0);
      output : out unsigned(4 downto 0)); 
  end component;

  component post_norm
    port (
      value : in  unsigned(24 downto 0);
      exp_adj : out std_logic;
      output  : out unsigned(22 downto 0));
  end component;

  signal sign_b : std_logic;
  signal op_in  : std_logic;
  
  -- STAGE 1 SIGNALS
  signal en_1 : std_logic;
  signal exp_a_1, exp_b_1 : unsigned(7 downto 0);
  signal sig_a, sig_b : unsigned(22 downto 0);
  signal exp_diff_1, n_exp_diff_1 : unsigned(7 downto 0);
  signal sign_a_1, sign_b_1 : std_logic;
  signal sig_a_1, sig_b_1 : unsigned(23 downto 0);
  signal a_zero, b_zero : std_logic;
  signal swap_1 : std_logic;

  -- STAGE 2 SIGNALS
  signal en_2 : std_logic;
  signal exp_diff_2, n_exp_diff_2 : unsigned(7 downto 0);
  signal exp_diff_2_pos : unsigned(7 downto 0);
  signal swap_2 : std_logic;
  signal sign_a_2, sign_b_2 : std_logic;
  signal sig_a_2, sig_b_2 : unsigned(23 downto 0);
  signal exp_a_2, exp_b_2 : unsigned(7 downto 0);
  signal neg_left_2 : std_logic;
  signal sub_2 : std_logic;
  signal exp_2 : unsigned(7 downto 0);
  signal sig_left_2, sig_right_2 : unsigned(23 downto 0);

  -- STAGE 3 SIGNALS
  signal en_3 : std_logic;
  signal exp_diff_3 : unsigned(7 downto 0);
  signal sub_3 : std_logic;
  signal exp_3 : unsigned(7 downto 0);
  signal neg_left_3 : std_logic;
  signal sig_left_3 : unsigned(23 downto 0);
  signal sig_left_3_neg : unsigned(24 downto 0);
  signal sig_right_3 : unsigned(23 downto 0);
  signal sig_right_3_shift : unsigned(48 downto 0);

  -- STAGE 4 SIGNALS
  signal en_4 : std_logic;
  signal exp_4 : unsigned(7 downto 0);
  signal sub_4 : std_logic;
  signal sig_left_4 : unsigned(24 downto 0);
  signal sig_right_4 : unsigned(48 downto 0);
  signal sum_4 : unsigned(27 downto 0);
  signal in_sig_left, in_sig_right : unsigned(25 downto 0);
  signal sticky_4 : std_logic;
  signal gr : unsigned(1 downto 0);
  
  -- STAGE 5 SIGNALS
  signal en_5 : std_logic;
  signal sign_5 : std_logic;
  signal exp_5 : unsigned(7 downto 0);
  signal sum_5 : unsigned(27 downto 0);
  signal sum_5_neg : unsigned(26 downto 0);
  signal sub_5 : std_logic;
  signal sticky_5 : std_logic;
  signal sticky_adj : std_logic;
  signal sum_inv : unsigned(26 downto 0);
  
  -- STAGE 6 SIGNALS
  signal en_6 : std_logic;
  signal sign_6 : std_logic;
  signal exp_6 : unsigned(7 downto 0);
  signal sum_6 : unsigned(26 downto 0);
  signal zero_6 : std_logic;
  signal pre_norm_adj_6 : unsigned(4 downto 0);
  signal sticky_6 : std_logic;

  -- STAGE 7 SIGNALS
  signal en_7 : std_logic;
  signal sign_7 : std_logic;
  signal exp_7 : unsigned(7 downto 0);
  signal zero_7 : std_logic;
  signal pre_norm_adj_7 : unsigned(4 downto 0);
  signal sum_7 : unsigned(27 downto 0);
  signal sum_7_shift : unsigned(27 downto 0);
  signal sum_7_norm : unsigned(25 downto 0);
  
  -- STAGE 8 SIGNALS
  signal en_8 : std_logic;
  signal sign_8 : std_logic;
  signal exp_8 : unsigned(7 downto 0);
  signal zero_8 : std_logic;
  signal pre_norm_adj_8 : unsigned(4 downto 0);
  signal sum_8 : unsigned(25 downto 0);
  signal exp_8_adj : unsigned(8 downto 0);
  signal sum : unsigned(23 downto 0);
  signal lsb, round, sticky : std_logic;
  signal sum_8_round : unsigned(24 downto 0);
  signal exp_8_inc : unsigned(8 downto 0);

  -- STAGE 9 SIGNALS
  signal en_9 : std_logic;
  signal sign_9 : std_logic;
  signal zero_9 : std_logic;
  signal exp_9 : unsigned(8 downto 0);
  signal exp_9_inc : unsigned(8 downto 0);
  signal exp_9_norm : unsigned(8 downto 0);
  signal post_norm_adj : std_logic;
  signal sum_9 : unsigned(24 downto 0);
  signal sum_9_norm : unsigned(22 downto 0);
  signal ou_flow : std_logic;
  signal result : unsigned(31 downto 0);

begin

  -----------------------------------------------------------------------------
  -- INPUT REGISTERS ----------------------------------------------------------

  REG_INPUT: if USE_INPUT_REG generate
    process(clk, reset, op_a, op_b, op, en)
    begin
      if reset = '1' then
        en_1 <= '0';
      elsif clk'event and clk = '1' then
        sign_a_1 <= op_a(31);
        sign_b <= op_b(31);
        exp_a_1 <= op_a(30 downto 23);
        exp_b_1 <= op_b(30 downto 23);
        sig_a <= op_a(22 downto 0);
        sig_b <= op_b(22 downto 0);
        op_in <= op;
        en_1 <= en;
      end if;
    end process;
  end generate REG_INPUT;
  NO_REG_INPUT: if not USE_INPUT_REG generate
    sign_a_1 <= op_a(31);
    sign_b <= op_b(31);
    exp_a_1 <= op_a(30 downto 23);
    exp_b_1 <= op_b(30 downto 23);
    sig_a <= op_a(22 downto 0);
    sig_b <= op_b(22 downto 0);
    op_in <= op;
    en_1 <= en;
  end generate NO_REG_INPUT;
  
  
  -----------------------------------------------------------------------------
  -- STAGE 1 ------------------------------------------------------------------

  sign_b_1 <= sign_b xor op_in;

  a_zero <= '1' when (exp_a_1 & sig_a) = ("000" & x"0000000") else '0';
  b_zero <= '1' when (exp_b_1 & sig_b) = ("000" & x"0000000") else '0';

  exp_diff_1 <= exp_a_1 - exp_b_1;
  n_exp_diff_1 <= exp_b_1 - exp_a_1;  
  swap_1 <= '1' when exp_b_1 > exp_a_1 else '0';

  sig_a_1 <= (not a_zero) & sig_a;
  sig_b_1 <= (not b_zero) & sig_b;
  
  -- STAGE 1 to 2 INTERLOCKS --------------------------------------------------
  LOCK_1_2: if USE_1_2_INTERLOCK generate
    process (clk, reset, exp_diff_1, n_exp_diff_1, sign_a_1, sig_a_1,
             exp_a_1, exp_b_1, sign_b_1, sig_b_1, en_1, swap_1)
    begin
      if reset = '1' then
        en_2 <= '0';
      elsif clk'event and clk = '1' then
        exp_diff_2 <= exp_diff_1;
        n_exp_diff_2 <= n_exp_diff_1;
        sign_a_2 <= sign_a_1;
        sig_a_2 <= sig_a_1;
        exp_a_2 <= exp_a_1;
        exp_b_2 <= exp_b_1;
        sign_b_2 <= sign_b_1;
        sig_b_2 <= sig_b_1;
        en_2 <= en_1;
        swap_2 <= swap_1;
      end if;
    end process;
  end generate LOCK_1_2;
  NO_LOCK_1_2: if not USE_1_2_INTERLOCK generate
    exp_diff_2 <= exp_diff_1;
    n_exp_diff_2 <= n_exp_diff_1;
    sign_a_2 <= sign_a_1;
    sig_a_2 <= sig_a_1;
    exp_a_2 <= exp_a_1;
    exp_b_2 <= exp_b_1;
    sign_b_2 <= sign_b_1;
    sig_b_2 <= sig_b_1;
    en_2 <= en_1;
    swap_2 <= swap_1;
  end generate NO_LOCK_1_2;

  
  -----------------------------------------------------------------------------
  -- STAGE 2 ------------------------------------------------------------------

  exp_diff_2_pos <= exp_diff_2 when swap_2 = '0' else n_exp_diff_2;

  neg_left_2 <= sign_a_2 when swap_2 = '0'else sign_b_2;
  sub_2 <= sign_b_2 when swap_2 = '0' else sign_a_2;
  exp_2 <= exp_a_2 when swap_2 = '0' else exp_b_2;
  sig_left_2 <= sig_a_2 when swap_2 = '0' else sig_b_2;
  sig_right_2 <= sig_b_2 when swap_2 = '0' else sig_a_2;

  -- STAGE 2 to 3 INTERLOCKS --------------------------------------------------
  LOCK_2_3: if USE_2_3_INTERLOCK generate
    process (clk, reset, exp_diff_2_pos, sub_2, exp_2, sig_left_2, sig_right_2,
             en_2, neg_left_2)
    begin
      if reset = '1' then
        en_3 <= '0';
      elsif clk'event and clk = '1' then
        exp_diff_3 <= exp_diff_2_pos;
        sub_3 <= sub_2;
        exp_3 <= exp_2;
        sig_left_3 <= sig_left_2;
        sig_right_3 <= sig_right_2;
        en_3 <= en_2;
        neg_left_3 <= neg_left_2;
      end if;
    end process;
  end generate LOCK_2_3;
  NO_LOCK_2_3: if not USE_2_3_INTERLOCK generate
    exp_diff_3 <= exp_diff_2_pos;
    sub_3 <= sub_2;
    exp_3 <= exp_2;
    sig_left_3 <= sig_left_2;
    sig_right_3 <= sig_right_2;
    en_3 <= en_2;
    neg_left_3 <= neg_left_2;    
  end generate NO_LOCK_2_3;

  
  -----------------------------------------------------------------------------
  -- STAGE 3 ------------------------------------------------------------------

  sig_left_3_neg <= ((not ('0' & sig_left_3)) + 1) when neg_left_3 = '1' else
                    ('0' & sig_left_3);
  sig_right_3_shift<= (sig_right_3 & '0' & x"000000") srl
                      to_integer(exp_diff_3(4 downto 0))
                      when (exp_diff_3(7 downto 5) = "000") else
                      ("000" & x"0000000" & sig_right_3(23 downto 6));
  
  -- STAGE 3 to 4 INTERLOCKS --------------------------------------------------
  LOCK_3_4: if USE_3_4_INTERLOCK generate
    process (clk, reset, sub_3, exp_3, sig_left_3_neg, sig_right_3, en_3)
    begin
      if reset = '1' then
        en_4 <= '0';
      elsif clk'event and clk = '1' then
        sub_4 <= sub_3;
        exp_4 <= exp_3;
        sig_left_4 <= sig_left_3_neg;
        sig_right_4 <= sig_right_3_shift;
        en_4 <= en_3;
      end if;
    end process;
  end generate LOCK_3_4;
  NO_LOCK_3_4: if not USE_3_4_INTERLOCK generate
    sub_4 <= sub_3;
    exp_4 <= exp_3;
    sig_left_4 <= sig_left_3_neg;
    sig_right_4 <= sig_right_3_shift;
    en_4 <= en_3;
  end generate NO_LOCK_3_4;

  
  -----------------------------------------------------------------------------
  -- STAGE 4 ------------------------------------------------------------------

  sticky_4 <= '0' when sig_right_4(22 downto 0) = ("000" & x"00000") else '1';
  gr <= sig_right_4(24 downto 23);

  in_sig_left <= sig_left_4(24) & sig_left_4;
  in_sig_right <= ("00" & sig_right_4(48 downto 25)) when sub_4 = '0' else
                  ("11" & (not sig_right_4(48 downto 25)));
                   
  sum_4(27 downto 2) <= (in_sig_left + in_sig_right);
  sum_4(1 downto 0) <=  gr when sub_4 = '0' else (not gr);  
  
  -- STAGE 4 to 5 INTERLOCKS --------------------------------------------------
  LOCK_4_5: if USE_4_5_INTERLOCK generate
    process (clk, reset, exp_4, sum_4, en_4, sub_4, sticky_4)
    begin
      if reset = '1' then
        en_5 <= '0';
      elsif clk'event and clk = '1' then
        exp_5 <= exp_4;
        sum_5 <= sum_4;
        en_5 <= en_4;
        sub_5 <= sub_4;
        sticky_5 <= sticky_4;
      end if;
    end process;
  end generate LOCK_4_5;
  NO_LOCK_4_5: if not USE_4_5_INTERLOCK generate
    exp_5 <= exp_4;
    sum_5 <= sum_4;
    en_5 <= en_4;
    sub_5 <= sub_4;
    sticky_5 <= sticky_4;
  end generate NO_LOCK_4_5;

  
  -----------------------------------------------------------------------------
  -- STAGE 5 ------------------------------------------------------------------

  sign_5 <= sum_5(27);
  sticky_adj <= (not sticky_5) and (sub_5 xor sign_5);
  sum_inv <= sum_5(26 downto 0) when sign_5 = '0'
             else (not sum_5(26 downto 0));
  sum_5_neg <= sum_inv + ("00" & x"000000" & sticky_adj);

  -- STAGE 5 to 6 INTERLOCKS --------------------------------------------------
  LOCK_5_6: if USE_5_6_INTERLOCK generate
    process (clk, reset, sign_5, exp_5, sum_5_neg, en_5, sticky_5)
    begin
      if reset = '1' then
        en_6 <= '0';
      elsif clk'event and clk = '1' then
        sign_6 <= sign_5;
        exp_6 <= exp_5;
        sum_6 <= sum_5_neg;
        en_6 <= en_5;
        sticky_6 <= sticky_5;
      end if;
    end process;
  end generate LOCK_5_6;
  NO_LOCK_5_6: if not USE_5_6_INTERLOCK generate
        sign_6 <= sign_5;
        exp_6 <= exp_5;
        sum_6 <= sum_5_neg;
        en_6 <= en_5;
        sticky_6 <= sticky_5;
  end generate NO_LOCK_5_6;

  
  -----------------------------------------------------------------------------
  -- STAGE 6 ------------------------------------------------------------------

  zero_6 <= '1' when sum_6 = x"0000000" else '0';

  LEADING_ZEROS_25_I: leading_zeros_25
    port map (
        input  => sum_6(26 downto 2),
        output => pre_norm_adj_6);
  
  -- STAGE 6 to 7 INTERLOCKS --------------------------------------------------
  LOCK_6_7: if USE_6_7_INTERLOCK generate
    process (clk, reset, sign_6, exp_6, zero_6, pre_norm_adj_6, sum_6, en_6,
             sticky_6)
    begin
      if reset = '1' then
        en_7 <= '0';
      elsif clk'event and clk = '1' then
        sign_7 <= sign_6;
        exp_7 <= exp_6;
        zero_7 <= zero_6;
        pre_norm_adj_7 <= pre_norm_adj_6;
        sum_7 <= sum_6 & sticky_6;
        en_7 <= en_6;
      end if;
    end process;
  end generate LOCK_6_7;
  NO_LOCK_6_7: if not USE_6_7_INTERLOCK generate
    sign_7 <= sign_6;
    exp_7 <= exp_6;
    zero_7 <= zero_6;
    pre_norm_adj_7 <= pre_norm_adj_6;
    sum_7 <= sum_6 & sticky_6;
    en_7 <= en_6;
  end generate NO_LOCK_6_7;

  
  -----------------------------------------------------------------------------
  -- STAGE 7 ------------------------------------------------------------------

  sum_7_shift <= sum_7 sll to_integer(pre_norm_adj_7);
  sum_7_norm <= sum_7_shift(27 downto 3) &
                (sum_7_shift(2) or sum_7_shift(1) or sum_7_shift(0));
  
  -- STAGE 7 to 8 INTERLOCKS --------------------------------------------------
  LOCK_7_8: if USE_7_8_INTERLOCK generate
    process (clk, reset, sign_7, exp_7, zero_7, pre_norm_adj_7, sum_7_norm,
             en_7)
    begin
      if reset = '1' then
        en_8 <= '0';
      elsif clk'event and clk = '1' then
        sign_8 <= sign_7;
        exp_8 <= exp_7;
        zero_8 <= zero_7;
        pre_norm_adj_8 <= pre_norm_adj_7;
        sum_8 <= sum_7_norm;
        en_8 <= en_7;
      end if;
    end process;
  end generate LOCK_7_8;
  NO_LOCK_7_8: if not USE_7_8_INTERLOCK generate
    sign_8 <= sign_7;
    exp_8 <= exp_7;
    zero_8 <= zero_7;
    pre_norm_adj_8 <= pre_norm_adj_7;
    sum_8 <= sum_7_norm;
    en_8 <= en_7;
  end generate NO_LOCK_7_8;


  -----------------------------------------------------------------------------
  -- STAGE 8 ------------------------------------------------------------------

  exp_8_adj <= ('0' & exp_8) + ("1111" & (not pre_norm_adj_8)) + 2;
  sum <= sum_8(25 downto 2);

  lsb <= sum_8(2);
  round <= sum_8(1);
  sticky <= sum_8(0);

  sum_8_round <= ('0' & sum) + (x"000000" & (round and (sticky or lsb)));

  exp_8_inc <= exp_8_adj + 1;
  
  -- STAGE 8 to 9 INTERLOCKS --------------------------------------------------
  LOCK_8_9: if USE_8_9_INTERLOCK generate
    process (clk, reset, sign_8, zero_8, exp_8_adj, exp_8_inc, sum_8_round,
             en_8)
    begin
      if reset = '1' then
        en_9 <= '0';
      elsif clk'event and clk = '1' then
        sign_9 <= sign_8;
        zero_9 <= zero_8;        
        exp_9 <= exp_8_adj;
        exp_9_inc <= exp_8_inc;
        sum_9 <= sum_8_round;
        en_9 <= en_8;
      end if;
    end process;
  end generate LOCK_8_9;
  NO_LOCK_8_9: if not USE_8_9_INTERLOCK generate
    sign_9 <= sign_8;
    zero_9 <= zero_8;
    exp_9 <= exp_8_adj;
    exp_9_inc <= exp_8_inc;
    sum_9 <= sum_8_round;
    en_9 <= en_8;
  end generate NO_LOCK_8_9;

  
  -----------------------------------------------------------------------------
  -- STAGE 9 ------------------------------------------------------------------

  exp_9_norm <= exp_9 when post_norm_adj = '0' else exp_9_inc;
  ou_flow <= exp_9_norm(8);

  POST_NORM_I: post_norm
    port map (
        value => sum_9,
        exp_adj => post_norm_adj,
        output  => sum_9_norm);

  result <= sign_9 & exp_9_norm(7 downto 0) & sum_9_norm;

  -----------------------------------------------------------------------------
  -- OUTPUT REGISTERS ---------------------------------------------------------
  
  REG_OUTPUT: if USE_OUTPUT_REG generate
    process (clk, reset, en_9, result)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        valid <= en_9;
        if (ou_flow or zero_9) = '1' then
          output <= x"00000000";
        else
          output <= result;
        end if;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT: if not USE_OUTPUT_REG generate
    output <= x"00000000" when (ou_flow or zero_9) = '1' else result;
    valid <= en_9;
  end generate NO_REG_OUTPUT;
end imp;





-------------------------------------------------------------------------------
-- MULT LIGHT -----------------------------------------------------------------
-------------------------------------------------------------------------------
-- Ligntweight Single Precision Floating Point Multiplier
--
-- The critical path depends on the interlocks used but is generally through
-- the multiplier.
--
-- The default configuration runs at 95 MHz and consumes 134 slices of FPGA
-- realestate on a Virtex 2, speed grade -4. Note, however, that the speed is
-- dependent on the physical mapping and may be lower when integrated into a
-- larger design. For this reason, the multiply can be divided into two cycles
-- to allow for latency between the circuit and hardware dividers.
--

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity mult_light is
  generic (
    USE_INPUT_REG     : boolean := true;
    USE_1_2_INTERLOCK : boolean := true;
    USE_2_3_INTERLOCK : boolean := false;
    USE_3_4_INTERLOCK : boolean := true;
    USE_4_5_INTERLOCK : boolean := true;
    USE_5_6_INTERLOCK : boolean := true;
    USE_OUTPUT_REG    : boolean := false);

  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    op_a   : in  unsigned(31 downto 0);
    op_b   : in  unsigned(31 downto 0);
    valid  : out std_logic;
    output : out unsigned(31 downto 0));
end mult_light;



architecture imp of mult_light is
  component pre_norm_mult
    port (
      product : in  unsigned(26 downto 0);
      exp_adj : out std_logic;
      output  : out unsigned(25 downto 0));
  end component;

  component post_norm
    port (
      value : in  unsigned(24 downto 0);
      exp_adj : out std_logic;
      output  : out unsigned(22 downto 0));
  end component;

  
  -- STAGE 1 SIGNALS
  signal en_1 : std_logic;
  signal sign_a, sign_b : std_logic;
  signal exp_a, exp_b : unsigned(7 downto 0);
  signal sig_a, sig_b : unsigned(22 downto 0);
  signal sig_a_1, sig_b_1 : unsigned(22 downto 0);
  signal a_zero, b_zero : std_logic;
  signal exp_1 : unsigned(8 downto 0);
  signal zero_1, sign_1 : std_logic;

  -- STAGE 2 SIGNALS
  signal a_comp, c_comp : unsigned(11 downto 0);
  signal b_comp, d_comp : unsigned(11 downto 0);
  signal prod_ac_2 : unsigned(23 downto 0);
  signal prod_ad_2 : unsigned(23 downto 0);
  signal prod_bc_2 : unsigned(23 downto 0);
  signal prod_bd_2 : unsigned(23 downto 0);
  signal en_2 : std_logic;
  signal sign_2 : std_logic;
  signal exp_2 : unsigned(8 downto 0);
  signal zero_2 : std_logic;
  signal sig_a_2, sig_b_2 : unsigned(22 downto 0);

  -- STAGE 3 SIGNALS
  signal prod_ac_3 : unsigned(23 downto 0);
  signal prod_ad_3 : unsigned(23 downto 0);
  signal prod_bc_3 : unsigned(23 downto 0);
  signal prod_bd_3 : unsigned(23 downto 0);
  signal sum_ac_bd : unsigned(47 downto 0);
  signal sum_ad_bc : unsigned(24 downto 0);
  signal en_3 : std_logic;
  signal sign_3 : std_logic;
  signal exp_3 : unsigned(8 downto 0);
  signal zero_3 : std_logic;
  signal prod_3 : unsigned(47 downto 0);  
  
  -- STAGE 4 SIGNALS
  signal en_4 : std_logic;
  signal sign_4 : std_logic;
  signal exp_4 : unsigned(8 downto 0);
  signal exp_4_adj : unsigned(8 downto 0);
  signal zero_4 : std_logic;
  signal pre_norm_adj : std_logic;
  signal prod_4 : unsigned(47 downto 0);
  signal sticky_or : std_logic;
  signal pre_norm_in : unsigned(26 downto 0);
  signal prod_4_norm : unsigned(25 downto 0);
  
  -- STAGE 5 SIGNALS
  signal en_5 : std_logic;
  signal sign_5 : std_logic;
  signal exp_5, exp_5_inc : unsigned(8 downto 0);
  signal zero_5 : std_logic;
  signal prod_5 : unsigned(25 downto 0);
  signal prod_5_round : unsigned(24 downto 0);
  signal lsb, round, sticky : std_logic;

  -- STAGE 6 SIGNALS
  signal en_6 : std_logic;
  signal sign_6 : std_logic;
  signal ou_flow : std_logic;
  signal exp_6, exp_6_inc : unsigned(8 downto 0);
  signal exp_final : unsigned(8 downto 0);
  signal zero_6 : std_logic;
  signal post_norm_adj : std_logic;
  signal out_sel : std_logic;
  signal prod_6 : unsigned(24 downto 0);
  signal prod_6_norm : unsigned(22 downto 0);
  signal result : unsigned(31 downto 0);

begin

  -----------------------------------------------------------------------------
  -- INPUT REGISTERS ----------------------------------------------------------

  REG_INPUT: if USE_INPUT_REG generate
    process (clk, reset, op_a, op_b, en)
    begin
      if reset = '1' then
        en_1 <= '0';
      elsif clk'event and clk = '1' then
        sign_a <= op_a(31);
        sign_b <= op_b(31);
        exp_a <= op_a(30 downto 23);
        exp_b <= op_b(30 downto 23);
        sig_a <= op_a(22 downto 0);
        sig_b <= op_b(22 downto 0);
        en_1 <= en;
      end if;
    end process;
  end generate REG_INPUT;
  NO_REG_INPUT: if not USE_INPUT_REG generate
    sign_a <= op_a(31);
    sign_b <= op_b(31);
    exp_a <= op_a(30 downto 23);
    exp_b <= op_b(30 downto 23);
    sig_a <= op_a(22 downto 0);
    sig_b <= op_b(22 downto 0);
    en_1 <= en;
  end generate NO_REG_INPUT;
  
  -----------------------------------------------------------------------------
  -- STAGE 1 ------------------------------------------------------------------

  sign_1 <= sign_a xor sign_b;
  
  exp_1 <= "000000000" when zero_1 = '1' else
           (('0' & exp_a) + ('0' & exp_b) - 127);

  a_zero <= '1' when (exp_a & sig_a_1) = "000" & x"0000000" else '0';
  b_zero <= '1' when (exp_b & sig_b_1) = "000" & x"0000000" else '0';
  zero_1 <= a_zero or b_zero;

  sig_a_1 <= sig_a;
  sig_b_1 <= sig_b;
  
  -- STAGE 1 to 2 INTERLOCKS --------------------------------------------------
  LOCK_1_2: if USE_1_2_INTERLOCK generate
    process (clk, reset, sign_1, exp_1, zero_1, sig_a_1, sig_b_1, en_1)
    begin
      if reset = '1' then
        en_2 <= '0';
      elsif clk'event and clk = '1' then
        sign_2 <= sign_1;
        exp_2 <= exp_1;
        zero_2 <= zero_1;
        sig_a_2 <= sig_a_1;
        sig_b_2 <= sig_b_1;
        en_2 <= en_1;
      end if;
    end process;
  end generate LOCK_1_2;
  NO_LOCK_1_2: if not USE_1_2_INTERLOCK generate
    sign_2 <= sign_1;
    exp_2 <= exp_1;
    zero_2 <= zero_1;
    sig_a_2 <= sig_a_1;
    sig_b_2 <= sig_b_1;
    en_2 <= en_1;
  end generate NO_LOCK_1_2;
  
    
  -----------------------------------------------------------------------------
  -- STAGE 2 ------------------------------------------------------------------

  a_comp <= '1' & sig_a_2(22 downto 12);
  b_comp <= sig_a_2(11 downto 0);

  c_comp <= '1' & sig_b_2(22 downto 12);
  d_comp <= sig_b_2(11 downto 0);

  prod_ac_2 <= a_comp * c_comp;
  prod_ad_2 <= a_comp * d_comp;
  prod_bc_2 <= b_comp * c_comp;
  prod_bd_2 <= b_comp * d_comp;
  
  -- STAGE 2 to 3 INTERLOCKS -------------------------------------------------
  LOCK_2_3: if USE_2_3_INTERLOCK generate
    process (clk, reset, sign_2, exp_2, zero_2, en_2)
    begin
      if reset = '1' then
        en_3 <= '0';
      elsif clk'event and clk = '1' then
        sign_3 <= sign_2;
        exp_3 <= exp_2;
        zero_3 <= zero_2;
        en_3 <= en_2;
        prod_ac_3 <= prod_ac_2;
        prod_ad_3 <= prod_ad_2;
        prod_bc_3 <= prod_bc_2;
        prod_bd_3 <= prod_bd_2;
      end if;
    end process;
  end generate LOCK_2_3;
  NO_LOCK_2_3: if not USE_2_3_INTERLOCK generate
    sign_3 <= sign_2;
    exp_3 <= exp_2;
    zero_3 <= zero_2;
    en_3 <= en_2;
    prod_ac_3 <= prod_ac_2;
    prod_ad_3 <= prod_ad_2;
    prod_bc_3 <= prod_bc_2;
    prod_bd_3 <= prod_bd_2;
  end generate NO_LOCK_2_3;
  
    
  -----------------------------------------------------------------------------
  -- STAGE 3 ------------------------------------------------------------------

  --  prod_3 <= (prod_ac_3 & prod_bd_3) +
  --            (x"000" & prod_ad_3 & x"000") +
  --            (x"000" & prod_bc_3 & x"000");
  -- This is equivalent to the above, but easier for synthesis to optimize
  sum_ac_bd <= (prod_ac_3 & prod_bd_3);
  sum_ad_bc <= ('0' & prod_ad_3) + ('0' & prod_bc_3);
  prod_3(11 downto 0) <= sum_ac_bd(11 downto 0);
  prod_3(47 downto 12) <= sum_ac_bd(47 downto 12) + ("00000000000" & sum_ad_bc);
  
  -- STAGE 3 to 4 INTERLOCKS -------------------------------------------------
  LOCK_3_4: if USE_3_4_INTERLOCK generate
    process (clk, reset, sign_3, exp_3, zero_3, prod_3, en_3)
    begin
      if reset = '1' then
        en_4 <= '0';
      elsif clk'event and clk = '1' then
        sign_4 <= sign_3;
        exp_4 <= exp_3;
        zero_4 <= zero_3;
        prod_4 <= prod_3;
        en_4 <= en_3;
      end if;
    end process;
  end generate LOCK_3_4;
  NO_LOCK_3_4: if not USE_3_4_INTERLOCK generate
    sign_4 <= sign_3;
    exp_4 <= exp_3;
    zero_4 <= zero_3;
    prod_4 <= prod_3;
    en_4 <= en_3;
  end generate NO_LOCK_3_4;
  
    
  -----------------------------------------------------------------------------
  -- STAGE 4 ------------------------------------------------------------------

  exp_4_adj <= exp_4 when pre_norm_adj = '0' else exp_4 + 1;
  sticky_or <= '0' when prod_4(21 downto 0) = ("00" & x"00000") else '1';
  pre_norm_in <= prod_4(47 downto 22) & sticky_or;
  
  PRE_NORM_I: pre_norm_mult
    port map (
        product => pre_norm_in,
        exp_adj => pre_norm_adj,
        output  => prod_4_norm);
  
  -- STAGE 4 to 5 INTERLOCKS --------------------------------------------------
  LOCK_4_5: if USE_4_5_INTERLOCK generate
    process (clk, reset, sign_4, exp_4_adj, zero_4, prod_4_norm, en_4)
    begin
      if reset = '1' then
        en_5 <= '0';
      elsif clk'event and clk = '1' then
        sign_5 <= sign_4;
        exp_5 <= exp_4_adj;
        zero_5 <= zero_4;
        prod_5 <= prod_4_norm;
        en_5 <= en_4;
      end if;
    end process;
  end generate LOCK_4_5;
  NO_LOCK_4_5: if not USE_4_5_INTERLOCK generate
    sign_5 <= sign_4;
    exp_5 <= exp_4_adj;
    zero_5 <= zero_4;
    prod_5 <= prod_4_norm;
    en_5 <= en_4;
  end generate NO_LOCK_4_5;
  
    
  -----------------------------------------------------------------------------
  -- STAGE 5 ------------------------------------------------------------------

  exp_5_inc <= exp_5 + 1;
  
  lsb <= prod_5(2);
  round <= prod_5(1);
  sticky <= prod_5(0);
  
  prod_5_round <= ('0' & prod_5(25 downto 2)) +
                  ("00000000000000000000000" & (round and (lsb or sticky)));
  
  -- STAGE 5 to 6 INTERLOCKS --------------------------------------------------
  LOCK_5_6: if USE_5_6_INTERLOCK generate
    process (clk, reset, sign_5, exp_5, zero_5, exp_5_inc, prod_5_round, en_5)
    begin
      if reset = '1' then
        en_6 <= '0';
      elsif clk'event and clk = '1' then
        sign_6 <= sign_5;
        exp_6 <= exp_5;
        exp_6_inc <= exp_5_inc;
        zero_6 <= zero_5;
        prod_6 <= prod_5_round;
        en_6 <= en_5;
      end if;
    end process;
  end generate LOCK_5_6;
  NO_LOCK_5_6: if not USE_5_6_INTERLOCK generate
    sign_6 <= sign_5;
    exp_6 <= exp_5;
    exp_6_inc <= exp_5_inc;
    zero_6 <= zero_5;
    prod_6 <= prod_5_round;
    en_6 <= en_5;
  end generate NO_LOCK_5_6;

  
  -----------------------------------------------------------------------------
  -- STAGE 6 ------------------------------------------------------------------

  exp_final <= exp_6 when post_norm_adj = '0' else exp_6_inc;
  ou_flow <= exp_final(8);

  out_sel <= ou_flow or zero_6;
  result <= sign_6 & exp_final(7 downto 0) & prod_6_norm;
  
  POST_NORM_I: post_norm
    port map (
        value => prod_6,
        exp_adj => post_norm_adj,
        output  => prod_6_norm);

  -----------------------------------------------------------------------------
  -- OUTPUT REGISTERS ---------------------------------------------------------
  
  REG_OUTPUT: if USE_OUTPUT_REG generate
    process (clk, reset, en_6, result)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        valid <= en_6;
        if out_sel = '0' then
          output <= result;
        else
          output <= x"00000000";
        end if;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT: if not USE_OUTPUT_REG generate
    output <= result when out_sel = '0' else x"00000000";
    valid <= en_6;
  end generate NO_REG_OUTPUT;
end imp;





-------------------------------------------------------------------------------
-- DIV LIGHT ------------------------------------------------------------------
-------------------------------------------------------------------------------
-- Lightweight Single Precision Floating Point Divider
--
-- Unlike add_light and mult_light, this is not a fully pipelined
-- unit. Only the final stages are pipelined, where normalizationa and
-- rounding occur. The division of the significands is done
-- iteratively. This means that you can't start one divide operation
-- and immediately start the next. You must wait until the internal
-- divide completes before starting the next divide. The divide operation can
-- take 13 or 26 cycles, depending on the value of DIVIDES_PER_CYCLE.
--
-- Starting a new divide before the previous divide has run for a
-- sufficient amount of time essentially clobbers the previous divide.
--
-- If either operand is zero, the output will be zero. This means that
-- divide by zero returns 0, as will as dividing into 0. In either
-- case, the result is returned almost immediately since no actual
-- divide is performed.
--
-- As with other functional units, you can specify which interlocks
-- you would like to have enabled. However, the inputs are always
-- registered for this unit.
--
-- You can also confiure the number of divide iterations to be
-- performed per cycle. This parameter, DIVIDES_PER_CYCLE, can be set
-- to 1 or 2. If set to 2, two divide iterations will be performed for
-- each clock cycle and the result will be obtained twice as fast. Of
-- course, the maximum clock rate is also decreased.
--

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity div_light is
  generic (
    DIVIDES_PER_CYCLE : integer := 2;
    USE_3_4_INTERLOCK : boolean := false;
    USE_4_5_INTERLOCK : boolean := false;
    USE_OUTPUT_REG    : boolean := true);
  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    op_a   : in  unsigned(31 downto 0);
    op_b   : in  unsigned(31 downto 0);
    output : out unsigned(31 downto 0);
    valid  : out std_logic);
end div_light;



architecture imp of div_light is

  constant END_COUNT : integer := 26 / DIVIDES_PER_CYCLE;

  -- STAGE 1 SIGNALS
  signal active : std_logic;
  signal a_sign, b_sign : std_logic;
  signal a_exp, b_exp : unsigned(7 downto 0);
  signal a_sig, b_sig : unsigned(22 downto 0);
  signal a_zero, b_zero : std_logic;
  signal exp_diff : unsigned(8 downto 0);
  signal sign : std_logic;
  signal result_zero : std_logic;

  -- STAGE 2 SIGNALS
  signal partial : unsigned(24 downto 0);
  signal quotient : unsigned(25 downto 0);
  signal divisor : unsigned(23 downto 0);
  signal counter : unsigned(4 downto 0);

  -- STAGE 3 SIGNALS
  signal quotient_norm_3 : unsigned(24 downto 0);
  signal exp_norm_3 : unsigned(8 downto 0);
  signal partial_nonzero : std_logic;
  signal sticky_3 : std_logic;
  signal en_3 : std_logic;

  -- STAGE 4 SIGNALS
  signal quotient_norm_4 : unsigned(24 downto 0);
  signal quotient_round_4 : unsigned(24 downto 0);
  signal sign_4 : std_logic;
  signal lsb, round, sticky_4 : std_logic;
  signal exp_4 : unsigned(8 downto 0);
  signal en_4 : std_logic;

  -- STAGE 5 SIGNALS
  signal quotient_5 : unsigned(24 downto 0);
  signal sign_5 : std_logic;
  signal sig : unsigned(22 downto 0);
  signal exp_5, exp : unsigned(8 downto 0);
  signal result : unsigned(31 downto 0);
  signal en_5 : std_logic;
  signal final_result : unsigned(31 downto 0);
  
begin

  -- INPUT REGISTERS ----------------------------------------------------------

  IN_REG : process (clk, op_a, op_b, en)
  begin
    if clk'event and clk = '1' then
      if en = '1' then
        a_sign <= op_a(31);
        a_exp <= op_a(30 downto 23);
        a_sig <= op_a(22 downto 0);
        b_sign <= op_b(31);
        b_exp <= op_b(30 downto 23);
        b_sig <= op_b(22 downto 0);
      end if;
    end if;
  end process;


  -- STAGE 1, 2 ---------------------------------------------------------------

  ACTIVE_REG : process (clk, reset, en, en_3)
  begin
    if reset = '1' then
      active <= '0';
    elsif clk'event and clk = '1' then
      if en_3 = '1' then
        active <= '0';
      end if;

      if en = '1' then
        active <= '1';
      end if;
    end if;
  end process;
  
  COUNTER_REG : process (clk, en, counter)
  begin
    if clk'event and clk = '1' then
      if en = '1' then
        counter <= (others => '0');
      else
        counter <= counter + 1;
      end if;
    end if;
  end process;

  -- This process contains the partial and divisor registers and does the
  -- iterative division logic.
  DIVIDER_REGS : process (clk, reset, en, op_a, op_b, partial, divisor)
    variable diff_v : unsigned(25 downto 0);
    variable partial_v : unsigned(24 downto 0);
    variable quotient_v : unsigned(25 downto 0);
  begin
    if reset = '1' then
      divisor <= (others => '0');
    elsif clk'event and clk = '1' then 
      if en = '1' then
        partial <= "01" & op_a(22 downto 0);
        divisor <= '1' & op_b(22 downto 0);
      else
        if DIVIDES_PER_CYCLE = 1 then
          -- Do one divide iteration this cycle

          diff_v := ('0' & partial) - ('0' & divisor);
          if diff_v(25) = '1' then
            partial <= partial(23 downto 0) & '0';
            quotient <= quotient(24 downto 0) & '0';
          else
            partial <= diff_v(23 downto 0) & '0';
            quotient <= quotient(24 downto 0) & '1';
          end if;

        else
          -- Do two divide iterations this cycle

          diff_v := ('0' & partial) - ('0' & divisor);
          if diff_v(25) = '1' then
            partial_v := partial(23 downto 0) & '0';
            quotient_v := quotient(24 downto 0) & '0';
          else
            partial_v := diff_v(23 downto 0) & '0';
            quotient_v := quotient(24 downto 0) & '1';
          end if;

          diff_v := ('0' & partial_v) - ('0' & divisor);
          if diff_v(25) = '1' then
            partial <= partial_v(23 downto 0) & '0';
            quotient <= quotient_v(24 downto 0) & '0';
          else
            partial <= diff_v(23 downto 0) & '0';
            quotient <= quotient_v(24 downto 0) & '1';
          end if;

        end if;
      end if;
    end if;
  end process;

  a_zero <= '1' when (a_exp & a_sig) = ("000" & x"0000000") else '0';
  b_zero <= '1' when (b_exp & b_sig) = ("000" & x"0000000") else '0';

  -- Decided to register these to help the synthisis tool give accurate timing
  -- measurements, since it doesn't know that these really have about 26 cycles
  -- to set up.
  MISC_REG : process (clk, reset, a_exp, b_exp, a_zero, b_zero, active)
  begin
    if reset = '1' then
      result_zero <= '0';
    elsif clk'event and clk = '1' then
      exp_diff <= (('0' & a_exp) - ('0' & b_exp)) + 127;
      sign <= a_sign xor b_sign;

      if active = '1' then
        result_zero <= a_zero or b_zero;
      else
        result_zero <= '0';
      end if;
    end if;
  end process;


  -- STAGE 3 ------------------------------------------------------------------

  partial_nonzero <= '0' when partial = ("0" & x"000000") else '1';
  en_3 <= '1' when ((counter = END_COUNT or result_zero = '1')
                    and active = '1') else '0';

  -- Normalize quotient ouptput (2.24 to 1.24)
  quotient_norm_3 <= quotient(24 downto 0) when quotient(25) = '0' else
                   quotient(25 downto 1);
  exp_norm_3 <= exp_diff - 1 when quotient(25) = '0' else
              exp_diff;
  sticky_3 <= partial_nonzero when quotient(25) = '0' else
              partial_nonzero or quotient(0);

  -- STAGE 3 to 4 INTERLOCKS --------------------------------------------------

  LOCK_3_4 : if USE_3_4_INTERLOCK generate
    process (clk, reset, en_3, sign, quotient_norm_3, exp_norm_3, sticky_3)
    begin
      if reset = '1' then
        en_4 <= '0';
      elsif clk'event and clk = '1' then
        en_4 <= en_3;
        sign_4 <= sign;
        quotient_norm_4 <= quotient_norm_3;
        exp_4 <= exp_norm_3;
        sticky_4 <= sticky_3;
      end if;
    end process;
  end generate LOCK_3_4;
  NO_LOCK_3_4 : if not USE_3_4_INTERLOCK generate
    en_4 <= en_3;
    sign_4 <= sign;
    quotient_norm_4 <= quotient_norm_3;
    exp_4 <= exp_norm_3;
    sticky_4 <= sticky_3;
  end generate NO_LOCK_3_4;


  -- STAGE 4 ------------------------------------------------------------------

  -- Round quotient (1.24 to 2.23)
  lsb <= quotient_norm_4(1);
  round <= quotient_norm_4(0);
  quotient_round_4 <= ('0' & quotient_norm_4(24 downto 1)) +
                      (x"000000" & (round and (lsb or sticky_4)));

  -- STAGE 4 to 5 INTERLOCKS --------------------------------------------------

  LOCK_4_5 : if USE_4_5_INTERLOCK generate
    process (clk, reset, en_4, sign_4, quotient_round_4, exp_4)
    begin
      if reset = '1' then
        en_5 <= '0';
      elsif clk'event and clk = '1' then
        en_5 <= en_4;
        sign_5 <= sign_4;
        quotient_5 <= quotient_round_4;
        exp_5 <= exp_4;
      end if;
    end process;
  end generate LOCK_4_5;
  NO_LOCK_4_5 : if not USE_4_5_INTERLOCK generate
    en_5 <= en_4;
    sign_5 <= sign_4;
    quotient_5 <= quotient_round_4;
    exp_5 <= exp_4;
  end generate NO_LOCK_4_5;
  
  -- STAGE 5 ------------------------------------------------------------------

  -- Normalize, drop implied bit
  sig <= quotient_5(23 downto 1) when quotient_5(24) = '1' else
         quotient_5(22 downto 0);
  exp <= exp_5 + 1 when quotient_5(24) = '1' else
         exp_5;
  
  result <= sign_5 & exp(7 downto 0) & sig;

  final_result <= x"00000000" when (result_zero = '1' or exp(8) = '1') else
                  result;
  
  -- OUTPUT REGISTERS ---------------------------------------------------------

  REG_OUTPUT: if USE_OUTPUT_REG generate
    process (clk, reset, en_5, final_result)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        valid <= en_5;
        output <= final_result;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT : if not USE_OUTPUT_REG generate
    valid <= en_5;
    output <= final_result;
  end generate NO_REG_OUTPUT;
  
end imp;





-------------------------------------------------------------------------------
-- SQRT LIGHT -----------------------------------------------------------------
-------------------------------------------------------------------------------
-- Lightweight Single Precision Floating Point Square Root
--
-- Unlike add_light and mult_light, this is not a fully pipelined unit. Only
-- the final stages are pipelined, where normalizationa and rounding occur. The
-- square root of the significands is done iteratively. This means that you
-- can't start one square root operation and immediately start the next. You
-- must wait until the internal square root completes before starting the next
-- root. The square root operation can take 13 or 26 cycles, depending on the
-- value of ROOTS_PER_CYCLE.
--
-- Starting a new root before the previous root has run for a sufficient amount
-- of time essentially clobbers the previous operation.
--
-- As with other functional units, you can specify which interlocks you would
-- like to have enabled. However, the interlocks between stages 1 and 2 are
-- manditory, since values must be registered before entering the iterative
-- square root in stage 2.
--
-- You can also confiure the number of square root iterations to be performed
-- per cycle. This parameter, ROOTS_PER_CYCLE, can be set to 1 or 2. If set to
-- 2, two square root iterations will be performed for each clock cycle and the
-- result will be obtained twice as fast. Of course, the maximum clock rate is
-- also decreased.
--

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity sqrt_light is
  generic (
    ROOTS_PER_CYCLE   : integer := 1;
    USE_INPUT_REG     : boolean := true;
    USE_2_3_INTERLOCK : boolean := true;
    USE_3_4_INTERLOCK : boolean := true;
    USE_OUTPUT_REG    : boolean := true);
  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    input  : in  unsigned(31 downto 0);
    output : out unsigned(31 downto 0);
    valid  : out std_logic);
end sqrt_light;



architecture imp of sqrt_light is

  constant END_COUNT : integer := 26 / ROOTS_PER_CYCLE;

  signal sign : std_logic;
  signal exp : unsigned(7 downto 0);
  signal sig : unsigned(22 downto 0);

  -- STAGE 1 SIGNALS
  signal en_1 : std_logic;
  signal exp_1 : unsigned(7 downto 0);
  signal partial_1 : unsigned(25 downto 0);
  signal zero_1 : std_logic;

  -- STAGE 2 SIGNALS
  signal active : std_logic;
  signal exp_2 : unsigned(7 downto 0);
  signal sign_2 : std_logic;
  signal root_2 : unsigned(24 downto 0);
  signal partial_2 : unsigned(26 downto 0);
  signal or_val  : unsigned(24 downto 0);
  signal zero_2 : std_logic;
  signal counter : unsigned(4 downto 0);
  signal counter_done : std_logic;

  -- STAGE 3 SIGNALS
  signal en_3 : std_logic;
  signal sign_3 : std_logic;
  signal partial_3 : unsigned(26 downto 0);
  signal root_3 : unsigned(23 downto 0);
  signal exp_3 : unsigned(7 downto 0);
  signal zero_3 : std_logic;
  signal round_adjust_3 : std_logic;
  signal lsb, round, sticky : std_logic;

  -- STAGE 4 SIGNALS
  signal en_4 : std_logic;
  signal sign_4 : std_logic;
  signal root_4 : unsigned(22 downto 0);
  signal exp_4 : unsigned(7 downto 0);
  signal zero_4 : std_logic;
  signal round_adjust_4 : std_logic;
  signal root_round : unsigned(22 downto 0);
  signal result : unsigned(31 downto 0);


begin

  -- INPUT REGISTERS ----------------------------------------------------------

  REG_INPUT: if USE_INPUT_REG generate
    IN_REG : process (clk, reset, input, en)
    begin
      if reset = '1' then
        en_1 <= '0';
      elsif clk'event and clk = '1' then
        en_1 <= en;
        sign <= input(31);
        exp <= input(30 downto 23);
        sig <= input(22 downto 0);
      end if;
    end process;
  end generate REG_INPUT;
  NO_REG_INPUT: if not USE_INPUT_REG generate
    en_1 <= en;
    sign <= input(31);
    exp <= input(30 downto 23);
    sig <= input(22 downto 0);
  end generate NO_REG_INPUT;


  -- STAGE 1 ------------------------------------------------------------------
  -- Calculate initial partial value, final exponent, and determine if zero.
  
  exp_1 <= resize((('0' & exp) + 127) srl 1, 8);
  zero_1 <= '1' when (exp & sig) = to_unsigned(0,31) else '0';
  partial_1 <= ('1' & sig & "00") when exp(0) = '0' else ("01" & sig & '0');

  -- STAGE 1 to 2 INTERLOCKS --------------------------------------------------

  process (clk, reset, en_1, sign, exp_1, zero_1, counter_done)
  begin
    if reset = '1' then
      active <= '0';
    elsif clk'event and clk = '1' then
      if en_1 = '1' then
        sign_2 <= sign;
        exp_2 <= exp_1;
        zero_2 <= zero_1;
        active <= '1';
      elsif counter_done = '1' then
        active <= '0';
      end if;
    end if;
  end process;


  -- STAGE 2 ------------------------------------------------------------------

  -- Counter to decide when stage 2 is done
  COUNTER_REG : process (clk, en_1, counter)
  begin
    if clk'event and clk = '1' then
      if en_1 = '1' then
        counter <= (others => '0');
      else
        counter <= counter + 1;
      end if;
    end if;
  end process;

  counter_done <= '1' when counter = END_COUNT else '0';

  -- This process contains the iterative square root logic
  ROOT_REGS : process (clk, en_1, partial_1, partial_2, root_2, or_val)
    variable diff_v : unsigned(26 downto 0);
    variable or_val_v  : unsigned(24 downto 0);
    variable root_v : unsigned(24 downto 0);
    variable partial_v : unsigned(26 downto 0);
  begin
    if clk'event and clk = '1' then 
      if en_1 = '1' then
        or_val <= '1' & x"000000";
        root_2 <= (others => '0');
        partial_2 <= '0' & partial_1;
      else
        if ROOTS_PER_CYCLE = 1 then
          -- Do one square root iteration this cycle

          diff_v := partial_2 - ((root_2 & '0') or ('0' & or_val));
          if diff_v(26) = '1' then
            partial_2 <= partial_2(25 downto 0) & '0';
          else
            partial_2 <= diff_v(25 downto 0) & '0';
            root_2 <= root_2 or or_val;
          end if;
          or_val <= or_val srl 1;

        else
          -- Do two square root iterations this cycle

          diff_v := partial_2 - ((root_2 & '0') or ('0' & or_val));
          if diff_v(26) = '1' then
            partial_v := partial_2(25 downto 0) & '0';
            root_v := root_2;
          else
            partial_v := diff_v(25 downto 0) & '0';
            root_v := root_2 or or_val;
          end if;
          or_val_v := or_val srl 1;

          diff_v := partial_v - ((root_v & '0') or ('0' & or_val_v));
          if diff_v(26) = '1' then
            partial_2 <= partial_v(25 downto 0) & '0';
            root_2 <= root_v;
          else
            partial_2 <= diff_v(25 downto 0) & '0';
            root_2 <= root_v or or_val_v;
          end if;
          or_val <= or_val_v srl 1;

        end if;
      end if;
    end if;
  end process;

  -- STAGE 2 to 3 INTERLOCKS --------------------------------------------------

  LOCK_2_3: if USE_2_3_INTERLOCK generate
    process (clk, reset, partial_2, root_2, exp_2, zero_2, sign_2, active,
             counter_done)
    begin
      if reset = '1' then
        en_3 <= '0';
        zero_3 <= '0';
      elsif clk'event and clk = '1' then
        en_3 <= '0';
        if counter_done = '1' then
          partial_3 <= partial_2;
          root_3 <= root_2(23 downto 0);
          exp_3 <= exp_2;
          zero_3 <= zero_2;
          sign_3 <= sign_2;
          en_3 <= active;
        end if;
      end if;
    end process;
  end generate LOCK_2_3;
  NO_LOCK_2_3: if not USE_2_3_INTERLOCK generate
    partial_3 <= partial_2;
    root_3 <= root_2(23 downto 0);
    exp_3 <= exp_2;
    zero_3 <= zero_2;
    sign_3 <= sign_2;
    en_3 <= active when counter_done = '1' else '0';
  end generate NO_LOCK_2_3;

  
  -- STAGE 3 ------------------------------------------------------------------

  lsb <= root_3(1);
  round <= root_3(0);
  sticky <= '0' when partial_3 = to_unsigned(0,27) else '1';

  round_adjust_3 <= round and (lsb or sticky);

  -- STAGE 3 to 4 INTERLOCKS --------------------------------------------------

  LOCK_3_4 : if USE_3_4_INTERLOCK generate
    process (clk, reset, root_3, exp_3, zero_3, round_adjust_3, sign_3,
             en_3)
    begin
      if reset = '1' then
        en_4 <= '0';
        zero_4 <= '0';
      elsif clk'event and clk = '1' then
        root_4 <= root_3(23 downto 1);
        exp_4 <= exp_3;
        zero_4 <= zero_3;
        round_adjust_4 <= round_adjust_3;
        sign_4 <= sign_3;
        en_4 <= en_3;
      end if;
    end process;
  end generate LOCK_3_4;
  NO_LOCK_3_4 : if not USE_3_4_INTERLOCK generate
    root_4 <= root_3(23 downto 1);
    exp_4 <= exp_3;
    zero_4 <= zero_3;
    round_adjust_4 <= round_adjust_3;
    sign_4 <= sign_3;
    en_4 <= en_3;
  end generate NO_LOCK_3_4;


  -- STAGE 4 ------------------------------------------------------------------

  root_round <= root_4 + (x"00000" & "00" & round_adjust_4);

  result <= (others => '0') when zero_4 = '1' else
            sign_4 & exp_4 & root_round;
  
  -- OUTPUT REGISTERS ---------------------------------------------------------

  REG_OUTPUT: if USE_OUTPUT_REG generate
    process (clk, reset, en_4, result)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        valid <= en_4;
        output <= result;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT : if not USE_OUTPUT_REG generate
    valid <= en_4;
    output <= result;
  end generate NO_REG_OUTPUT;
  
end imp;





-------------------------------------------------------------------------------
-- FTOI LIGHT -----------------------------------------------------------------
-------------------------------------------------------------------------------
-- Lightweight Single Precision Floating Point to 2's Compliment Integer
--
-- Uses C-like rounding, where all floats are rounded towards 0. Therefore,
-- 2.99 becomes 2 and -2.99 becomes -2.
--
-- The s input indicates whether the conversion is to a signed (s = '1') or
-- unsigned (s = '0') integer.
--
-- Overflow and underflow causes saturated results to be returned. The table
-- below shows the saturation values for all situations.
--
--   Condition               | Occurs when        | output (hex) | output (dec)
--   ------------------------|--------------------|--------------|-------------
--   Neg overflow (unsigned) | input < 0          | 0            | 0
--   Neg overflow (signed)   | input < MIN_INT    | 0x80000000   | -2147483648
--   Pos overflow (unsigned) | input > 0xFFFFFFFF | 0xFFFFFFFF   | 4294967295
--   Pos overflow (signed)   | input > MAX_INT    | 0x7FFFFFFF   | 2147483647
--   Underflow    (unsigned) | |input| < 1        | 0            | 0
--   Underflow    (signed)   | |input| < 1        | 0            | 0
--
--


library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity ftoi_light is
  generic (
    USE_INPUT_REG     : boolean := true;
    USE_1_2_INTERLOCK : boolean := false;
    USE_2_3_INTERLOCK : boolean := false;    
    USE_OUTPUT_REG    : boolean := false);
  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    s      : in  std_logic;
    input  : in  unsigned(31 downto 0);
    valid  : out std_logic;
    output : out unsigned(31 downto 0));
end ftoi_light;



architecture imp of ftoi_light is

  -- STAGE 1 SIGNALS
  signal input_1 : unsigned(31 downto 0);
  signal en_1, s_1 : std_logic;
  signal sign_1 : std_logic;
  signal exp_1 : unsigned(7 downto 0);
  signal sig_1 : unsigned(22 downto 0);
  signal underflow_1, overflow_1, min_s_ovr_1 : std_logic;

  -- STAGE 2 SIGNALS
  signal en_2, s_2 : std_logic;
  signal sign_2 : std_logic;
  signal exp_2 : unsigned(4 downto 0);
  signal sig_2 : unsigned(22 downto 0);
  signal underflow_2, overflow_2, min_s_ovr_2 : std_logic;
  signal sig_shift_2 : unsigned(31 downto 0);
  signal shift : unsigned(4 downto 0);

  -- STAGE 3 SIGNALS
  signal en_3, s_3 : std_logic;
  signal sign_3 : std_logic;
  signal underflow_3, overflow_3, min_s_ovr_3 : std_logic;
  signal sig_shift_3 : unsigned(31 downto 0);
  signal sig_neg : unsigned(31 downto 0);
  signal result : unsigned(31 downto 0);
  
begin

  -----------------------------------------------------------------------------
  -- INPUT REGISTERS ----------------------------------------------------------

  REG_INPUT: if USE_INPUT_REG generate
    process(clk, reset, input, s, en)
    begin
      if reset = '1' then
        en_1 <= '0';
      elsif clk'event and clk = '1' then
        input_1 <= input;
        s_1 <= s;
        en_1 <= en;
      end if;
    end process;
  end generate REG_INPUT;
  NO_REG_INPUT: if not USE_INPUT_REG generate
    input_1 <= input;
    s_1 <= s;
    en_1 <= en;
  end generate NO_REG_INPUT;
  
  
  -----------------------------------------------------------------------------
  -- STAGE 1 ------------------------------------------------------------------

  sign_1 <= input_1(31);
  exp_1 <= input_1(30 downto 23);
  sig_1 <= input_1(22 downto 0);
  
  underflow_1 <= '1' when exp_1 < 127 else '0';
  min_s_ovr_1 <= '1' when exp_1 = 158 else '0';
  overflow_1 <= '1' when exp_1 > 158 else '0';

  -- STAGE 1 to 2 INTERLOCKS-------------------------------------------------

  LOCK_1_2: if USE_1_2_INTERLOCK generate
    process(clk, reset, s_1, sign_1, exp_1, sig_1, underflow_1, overflow_1,
            min_s_ovr_1, en_1)
    begin
      if reset = '1' then
        en_2 <= '0';
      elsif clk'event and clk = '1' then
        s_2 <= s_1;
        sign_2 <= sign_1;
        exp_2 <= exp_1(4 downto 0);
        sig_2 <= sig_1;
        underflow_2 <= underflow_1;
        overflow_2 <= overflow_1;
        min_s_ovr_2 <= min_s_ovr_1;
        en_2 <= en_1;
      end if;
    end process;
  end generate LOCK_1_2;
  NO_LOCK_1_2: if not USE_1_2_INTERLOCK generate
    s_2 <= s_1;
    sign_2 <= sign_1;
    exp_2 <= exp_1(4 downto 0);
    sig_2 <= sig_1;
    underflow_2 <= underflow_1;
    overflow_2 <= overflow_1;
    min_s_ovr_2 <= min_s_ovr_1;
    en_2 <= en_1;
  end generate NO_LOCK_1_2;


  -----------------------------------------------------------------------------
  -- STAGE 2 ------------------------------------------------------------------

  shift <= "11110" - exp_2;          -- 158 - exp_2
  sig_shift_2 <= ('1' & sig_2 & "00000000") srl to_integer(shift);

  -- STAGE 2 to 3 INTERLOCKS --------------------------------------------------

  LOCK_2_3: if USE_2_3_INTERLOCK generate
    process(clk, reset, sig_shift_2, underflow_2, overflow_2, min_s_ovr_2, s_2,
            sign_2, en_2)
    begin
      if reset = '1' then
        en_3 <= '0';
      elsif clk'event and clk = '1' then
        sig_shift_3 <= sig_shift_2;
        underflow_3 <= underflow_2;
        overflow_3 <= overflow_2;
        min_s_ovr_3 <= min_s_ovr_2;
        s_3 <= s_2;
        sign_3 <= sign_2;
        en_3 <= en_2;
      end if;
    end process;
  end generate LOCK_2_3;
  NO_LOCK_2_3: if not USE_2_3_INTERLOCK generate
    sig_shift_3 <= sig_shift_2;
    underflow_3 <= underflow_2;
    overflow_3 <= overflow_2;
    min_s_ovr_3 <= min_s_ovr_2;
    s_3 <= s_2;
    sign_3 <= sign_2;
    en_3 <= en_2;
  end generate NO_LOCK_2_3;


  -- STAGE 3 ------------------------------------------------------------------

  sig_neg <= ((not sig_shift_3) + 1) when (sign_3 and s_3) = '1' else
             sig_shift_3;
  
  process(s_3, sign_3, sig_neg, underflow_3, overflow_3, min_s_ovr_3)
  begin
    -- Signed/unsigned underflow (|result| < 1)
    if underflow_3 = '1' then
      result <= x"00000000";
    -- Unsigned underflow (result < 1)
    elsif s_3 = '0' and sign_3 = '1' then
      result <= x"00000000";
    -- Signed overflow
    elsif s_3='1' and (overflow_3='1' or min_s_ovr_3='1') and sign_3='0' then
      result <= x"7FFFFFFF";
    elsif s_3='1' and (overflow_3='1' or min_s_ovr_3='1') and sign_3='1' then
      result <= x"80000000";
    -- Unsigned overflow
    elsif s_3 = '0' and overflow_3 = '1' and sign_3 = '0' then
      result <= x"FFFFFFFF";
    -- No over/underflow
    else
      result <= sig_neg;
    end if;
  end process;

  -----------------------------------------------------------------------------
  -- OUTPUT REGISTERS ---------------p------------------------------------------
  
  REG_OUTPUT: if USE_OUTPUT_REG generate
    process(clk, reset, result, en_3)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        output <= result;
        valid <= en_3;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT: if not USE_OUTPUT_REG generate
    output <= result;
    valid <= en_3;
  end generate NO_REG_OUTPUT;
end imp;





-------------------------------------------------------------------------------
-- ITOF LIGHT -----------------------------------------------------------------
-------------------------------------------------------------------------------
-- Lightweight 2's Compliment Integer to Single Precision Floating Point
--
-- The s input indicates whether the conversion is from a signed (s = '1') or
-- unsigned (s = '0') integer.
--
-- Overflow and underflow can not occur in this operation. Only inexact
-- representation.
--
-- This function uses round towards nearest even to round numbers that can't be
-- represented exactly in the 24-bit significand.
--


library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity itof_light is
  generic (
    USE_INPUT_REG     : boolean := false;
    USE_1_2_INTERLOCK : boolean := false;
    USE_2_3_INTERLOCK : boolean := false;    
    USE_3_4_INTERLOCK : boolean := false;    
    USE_4_5_INTERLOCK : boolean := false;    
    USE_OUTPUT_REG    : boolean := false);
  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    s      : in  std_logic;
    input  : in  unsigned(31 downto 0);
    valid  : out std_logic;
    output : out unsigned(31 downto 0));
end itof_light;



architecture imp of itof_light is

  component leading_zeros_31
    port (
      input  : in  unsigned(30 downto 0);
      output : out unsigned(4 downto 0));
  end component;
  
  -- STAGE 1 SIGNALS
  signal input_1 : unsigned(31 downto 0);
  signal en_1, s_1 : std_logic;
  signal zero_1 : std_logic;
  signal sign_1 : std_logic;
  signal sig_1 : unsigned(31 downto 0);
  
  -- STAGE 2 SIGNALS
  signal en_2 : std_logic;
  signal zero_2 : std_logic;
  signal sign_2 : std_logic;
  signal sig_2 : unsigned(31 downto 0);
  signal shift_2 : unsigned(4 downto 0);

  -- STAGE 3 SIGNALS
  signal en_3 : std_logic;
  signal zero_3 : std_logic;
  signal sign_3 : std_logic;
  signal sig_3 : unsigned(31 downto 0);
  signal shift_3 : unsigned(4 downto 0);
  signal exp_3 : unsigned(7 downto 0);
  signal sig_shift : unsigned(31 downto 0);

  -- STAGE 4 SIGNALS
  signal en_4 : std_logic;
  signal zero_4 : std_logic;
  signal sign_4 : std_logic;
  signal sig_4 : unsigned(31 downto 0);
  signal exp_4 : unsigned(7 downto 0);
  signal lsb, round, sticky, round_adj : std_logic;
  signal sig_round : unsigned(24 downto 0);

  -- STAGE 5 SIGNALS
  signal en_5 : std_logic;
  signal zero_5 : std_logic;
  signal sign_5 : std_logic;
  signal sig_5 : unsigned(24 downto 0);
  signal exp_5 : unsigned(7 downto 0);
  signal sig_norm : unsigned(22 downto 0);
  signal exp_norm : unsigned(7 downto 0);
  signal result : unsigned(31 downto 0);

  
begin

  -----------------------------------------------------------------------------
  -- INPUT REGISTERS ----------------------------------------------------------

  REG_INPUT: if USE_INPUT_REG generate
    process(clk, reset, input, s, en)
    begin
      if reset = '1' then
        en_1 <= '0';
      elsif clk'event and clk = '1' then
        input_1 <= input;
        s_1 <= s;
        en_1 <= en;
      end if;
    end process;
  end generate REG_INPUT;
  NO_REG_INPUT: if not USE_INPUT_REG generate
    input_1 <= input;
    s_1 <= s;
    en_1 <= en;
  end generate NO_REG_INPUT;
  
  
  -----------------------------------------------------------------------------
  -- STAGE 1 ------------------------------------------------------------------

  zero_1 <= '1' when input_1 = x"00000000" else '0';

  sig_1 <= ((not input_1) + 1) when (s_1 = '1' and input_1(31) = '1') else
           input_1;
  sign_1 <= '1' when (s_1 = '1' and input_1(31) = '1') else '0';

  -- STAGE 1 to 2 INTERLOCKS-------------------------------------------------

  LOCK_1_2: if USE_1_2_INTERLOCK generate
    process(clk, reset, zero_1, sig_1, sign_1, en_1)
    begin
      if reset = '1' then
        en_2 <= '0';
      elsif clk'event and clk = '1' then
        zero_2 <= zero_1;
        sig_2 <= sig_1;
        sign_2 <= sign_1;
        en_2 <= en_1;
      end if;
    end process;
  end generate LOCK_1_2;
  NO_LOCK_1_2: if not USE_1_2_INTERLOCK generate
    zero_2 <= zero_1;
    sig_2 <= sig_1;
    sign_2 <= sign_1;
    en_2 <= en_1;
  end generate NO_LOCK_1_2;


  -----------------------------------------------------------------------------
  -- STAGE 2 ------------------------------------------------------------------

  LEADING_ZEROS_31_I: leading_zeros_31
    port map (
        input  => sig_2(31 downto 1),
        output => shift_2);

  -- STAGE 2 to 3 INTERLOCKS --------------------------------------------------

  LOCK_2_3: if USE_2_3_INTERLOCK generate
    process(clk, reset, zero_2, sig_2, sign_2, shift_2, en_2)
    begin
      if reset = '1' then
        en_3 <= '0';
      elsif clk'event and clk = '1' then
        zero_3 <= zero_2;
        sig_3 <= sig_2;
        sign_3 <= sign_2;
        shift_3 <= shift_2;
        en_3 <= en_2;
      end if;
    end process;
  end generate LOCK_2_3;
  NO_LOCK_2_3: if not USE_2_3_INTERLOCK generate
    zero_3 <= zero_2;
    sig_3 <= sig_2;
    sign_3 <= sign_2;
    shift_3 <= shift_2;
    en_3 <= en_2;
  end generate NO_LOCK_2_3;


  -----------------------------------------------------------------------------
  -- STAGE 3 ------------------------------------------------------------------

  exp_3 <= (127+31) - resize(shift_3, 8);
  sig_shift <= sig_3 sll to_integer(shift_3);

  -- STAGE 3 to 4 INTERLOCKS --------------------------------------------------

  LOCK_3_4: if USE_3_4_INTERLOCK generate
    process(clk, reset, zero_3, sign_3, exp_3, sig_shift, en_3)
    begin
      if reset = '1' then
        en_4 <= '0';
      elsif clk'event and clk = '1' then
        zero_4 <= zero_3;
        sign_4 <= sign_3;
        exp_4 <= exp_3;
        sig_4 <= sig_shift;
        en_4 <= en_3;
      end if;
    end process;
  end generate LOCK_3_4;
  NO_LOCK_3_4: if not USE_3_4_INTERLOCK generate
    zero_4 <= zero_3;
    sign_4 <= sign_3;
    exp_4 <= exp_3;
    sig_4 <= sig_shift;
    en_4 <= en_3;
  end generate NO_LOCK_3_4;


  -----------------------------------------------------------------------------
  -- STAGE 4 ------------------------------------------------------------------

  lsb <= sig_4(8);
  round <= sig_4(7);
  sticky <= '0' when sig_4(6 downto 0) = "0000000" else '1';
  round_adj <= round and (lsb or sticky);

  sig_round <= ('0' & sig_4(31 downto 8)) + resize('0' & round_adj, 25);

  -- STAGE 4 to 5 INTERLOCKS --------------------------------------------------

  LOCK_4_5: if USE_4_5_INTERLOCK generate
    process(clk, reset, zero_4, sign_4, exp_4, sig_round, en_4)
    begin
      if reset = '1' then
        en_5 <= '0';
      elsif clk'event and clk = '1' then
        zero_5 <= zero_4;
        sign_5 <= sign_4;
        exp_5 <= exp_4;
        sig_5 <= sig_round;
        en_5 <= en_4;
      end if;
    end process;
  end generate LOCK_4_5;
  NO_LOCK_4_5: if not USE_4_5_INTERLOCK generate
    zero_5 <= zero_4;
    sign_5 <= sign_4;
    exp_5 <= exp_4;
    sig_5 <= sig_round;
    en_5 <= en_4;
  end generate NO_LOCK_4_5;


  -- STAGE 5 ------------------------------------------------------------------

  sig_norm <= sig_5(23 downto 1) when sig_5(24) = '1' else sig_5(22 downto 0);
  exp_norm <= (exp_5 + 1) when sig_5(24) = '1' else exp_5;

  result <= (others => '0') when zero_5 = '1' else
            sign_5 & exp_norm & sig_norm;

  -----------------------------------------------------------------------------
  -- OUTPUT REGISTERS ---------------------------------------------------------
  
  REG_OUTPUT: if USE_OUTPUT_REG generate
    process(clk, reset, result, en_5)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        output <= result;
        valid <= en_5;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT: if not USE_OUTPUT_REG generate
    output <= result;
    valid <= en_5;
  end generate NO_REG_OUTPUT;
end imp;





-------------------------------------------------------------------------------
-- CMP LIGHT -----------------------------------------------------------------
-------------------------------------------------------------------------------
-- Lightweight Single Precision Floating Point Comparator
--
-- This module outputs a 2-bit signed result. The result is -1 if op_a < op_b,
-- 0 if op_a = op_b, and 1 if op_a > op_b.
--


library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;



entity cmp_light is
  generic (
    USE_INPUT_REG     : boolean := false;
    USE_1_2_INTERLOCK : boolean := false;
    USE_2_3_INTERLOCK : boolean := false;    
    USE_OUTPUT_REG    : boolean := false);
  port (
    clk    : in  std_logic;
    reset  : in  std_logic;
    en     : in  std_logic;
    op_a   : in  unsigned(31 downto 0);
    op_b   : in  unsigned(31 downto 0);
    valid  : out std_logic;
    output : out signed(1 downto 0));
end cmp_light;



architecture imp of cmp_light is

  -- STAGE 1 SIGNALS
  signal en_1, a_zero, b_zero : std_logic;
  signal op_a_1, op_b_1 : unsigned(31 downto 0);
  signal a_val_1, b_val_1 : unsigned(31 downto 0);
  signal signs_diff_1, signs_neg_1 : std_logic;
  
  -- STAGE 2 SIGNALS
  signal en_2 : std_logic;
  signal signs_diff_2, signs_neg_2 : std_logic;
  signal a_val_2, b_val_2 : unsigned(31 downto 0);
  signal result_2 : unsigned(31 downto 0);
  
  -- STAGE 2 SIGNALS
  signal en_3 : std_logic;
  signal signs_neg_3 : std_logic;
  signal output_3 : signed(1 downto 0);
  signal result_3 : unsigned(31 downto 0);
  
begin

  -----------------------------------------------------------------------------
  -- INPUT REGISTERS ----------------------------------------------------------

  REG_INPUT: if USE_INPUT_REG generate
    process(clk, reset, op_a, op_b, en)
    begin
      if reset = '1' then
        en_1 <= '0';
      elsif clk'event and clk = '1' then
        op_a_1 <= op_a;
        op_b_1 <= op_b;
        en_1 <= en;
      end if;
    end process;
  end generate REG_INPUT;
  NO_REG_INPUT: if not USE_INPUT_REG generate
    op_a_1 <= op_a;
    op_b_1 <= op_b;
    en_1 <= en;
  end generate NO_REG_INPUT;
  
  
  -----------------------------------------------------------------------------
  -- STAGE 1 ------------------------------------------------------------------

  a_zero <= '1' when op_a_1(30 downto 0) = to_unsigned(0, 31) else '0';
  b_zero <= '1' when op_b_1(30 downto 0) = to_unsigned(0, 31) else '0';

  a_val_1 <= op_a_1 when a_zero = '0' else x"00000000";
  b_val_1 <= op_b_1 when b_zero = '0' else x"00000000";

  signs_diff_1 <= (a_val_1(31) xor b_val_1(31));
  signs_neg_1 <= (a_val_1(31) and b_val_1(31));

  -- STAGE 1 to 2 INTERLOCKS-------------------------------------------------

  LOCK_1_2: if USE_1_2_INTERLOCK generate
    process(clk, reset, a_val_1, b_val_1, signs_diff_1, signs_neg_1, en_1)
    begin
      if reset = '1' then
        en_2 <= '0';
      elsif clk'event and clk = '1' then
        a_val_2 <= a_val_1;
        b_val_2 <= b_val_1;
        signs_diff_2 <= signs_diff_1;
        signs_neg_2 <= signs_neg_1;
        en_2 <= en_1;
      end if;
    end process;
  end generate LOCK_1_2;
  NO_LOCK_1_2: if not USE_1_2_INTERLOCK generate
    a_val_2 <= a_val_1;
    b_val_2 <= b_val_1;
    signs_diff_2 <= signs_diff_1;
    signs_neg_2 <= signs_neg_1;
    en_2 <= en_1;
  end generate NO_LOCK_1_2;


  -----------------------------------------------------------------------------
  -- STAGE 2 ------------------------------------------------------------------

  process(signs_diff_2, a_val_2, b_val_2)
  begin
    if signs_diff_2 = '1' then
      result_2 <= a_val_2(31) & to_unsigned(1, 31);
    else
      result_2 <= a_val_2 - b_val_2;
    end if;
  end process;

  -- STAGE 2 to 3 INTERLOCKS --------------------------------------------------

  LOCK_2_3: if USE_2_3_INTERLOCK generate
    process(clk, reset, result_2, signs_neg_2, en_2)
    begin
      if reset = '1' then
        en_3 <= '0';
      elsif clk'event and clk = '1' then
        result_3 <= result_2;
        signs_neg_3 <= signs_neg_2;
        en_3 <= en_2;
      end if;
    end process;
  end generate LOCK_2_3;
  NO_LOCK_2_3: if not USE_2_3_INTERLOCK generate
    result_3 <= result_2;
    signs_neg_3 <= signs_neg_2;
    en_3 <= en_2;
  end generate NO_LOCK_2_3;


  -----------------------------------------------------------------------------
  -- STAGE 3 ------------------------------------------------------------------

  process(result_3, signs_neg_3)
  begin
    if result_3 = x"00000000" then
      output_3 <= "00";
    else
      if signs_neg_3 = '1' then
        output_3 <= (not result_3(31)) & '1';
      else
        output_3 <= result_3(31) & '1';
      end if;
    end if;
  end process;

  
  -----------------------------------------------------------------------------
  -- OUTPUT REGISTERS ---------------------------------------------------------
  
  REG_OUTPUT: if USE_OUTPUT_REG generate
    process(clk, reset, output_3, en_3)
    begin
      if reset = '1' then
        valid <= '0';
      elsif clk'event and clk = '1' then
        output <= output_3;
        valid <= en_3;
      end if;
    end process;
  end generate REG_OUTPUT;
  NO_REG_OUTPUT: if not USE_OUTPUT_REG generate
    output <= output_3;
    valid <= en_3;
  end generate NO_REG_OUTPUT;
end imp;


