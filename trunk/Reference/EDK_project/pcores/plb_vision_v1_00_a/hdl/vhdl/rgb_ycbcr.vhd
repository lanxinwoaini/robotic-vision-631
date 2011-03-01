-------------------------------------------------------------------------------
-- RGB to YCbCr Conversion
--
-- FILE: rgb_ycbcr.vhd
-- AUTHOR: Wade Fife
-- DATE: June 9, 2006
-- MODIFIED: June 23, 2006
--
--
-- DESCRIPTION
--
-- Converts RGB pixels to YCbCr. Data is written to the core by asserting r, g,
-- and b with the RGB pixel data and holding wr_en high for one clock
-- cycle. After conversion, the YCbCr data is asserted on the y, cb, and cr
-- outputs and valid is held high for one clock cycle. No flow control is
-- supported. You can adjust the pixel input/output width by changing the
-- C_DATA_WIDTH generic. The precision of the fixed point computation can be
-- adjusted by changing the C_PRECISION generic. Up to 32-bit precision is
-- supported, but high precision comes at a great cost.

--
-- The arithmetic for the conversion is taken from Keith Jack's book, Video
-- Demystified, 4th Edition. The equations (assuming gamma corrected, 8-bit,
-- RGB values) are as follows. For inputs in the range 0-255:
--
--     Y =  0.257R + 0.504G + 0.098B + 16
--    Cb = -0.148R - 0.291G + 0.439B + 128
--    Cr =  0.439R - 0.368G - 0.071B + 128
--
-- For inputs in the range 16-235:
--
--     Y  =  0.299R + 0.587G + 0.114B
--     Cb = -0.172R - 0.339G + 0.511B + 128
--     Cr =  0.511R - 0.428G - 0.083B + 128
--
-- These equations need to be slightly modified for a general, n-bit pixel
-- components. In the general case 128 should be replaced with 2^(n-1) and 16
-- with 2^(n-4).  The coefficients above are rounded to the nearest 3
-- significant digits, but the actual constants used in the VHDL below may be
-- more precise.
--
-- In general, the code refers the the constant coefficients as follows:
--
--     Y  =  C_Y_R*R +  C_Y_G*G +  C_Y_B*B + C_I_16
--     Cb = C_CB_R*R + C_CB_G*G + C_CB_B*B + C_I_128
--     Cr = C_CR_R*R + C_CR_G*G + C_CR_G*B + C_I_128
--
-- After the multiplication, the equations are referred to as follows:
--
--     Y  =  y_r  + y_g  + y_b  + C_I_16
--     Cb = -cb_r - cb_g + cb_b + C_I_128
--     Cr =  cr_r - cr_g - cr_b + C_I_128
--
-- The constant C_I_16 will be 0 when the core is configured for the input
-- range 16-235.
--
--
-- INPUT RANGE
--
-- In video systems, the full range 0-255 is often not used for pixel values,
-- and instead the range 16-235 is used. If the RGB input pixel values have
-- range 0-255 then C_FULL_RANGE should be set to TRUE. If they have range
-- 16-235 then C_FULL_RANGE should be FALSE.
--
--
-- PRECISION
--
-- You can set C_DATA_WIDTH to indicate the input pixel width (e.g., 8 for 8
-- bits per pixel) and C_PRECISION to indicate the precision to be used in the
-- arithmetic computation. For simplicity, rounding is only performed at the
-- end of computation, which generally provides very good results.
--
-- In a bit-accurate software version I found that rounding intermediate
-- results (e.g., after the multiplication) had only a very small effect on the
-- final result. Assuming a C_DATA_WIDTH of 8, I found that a C_PRECISION of 12
-- will cause the resulting output values to be inexact about 6% of the time. A
-- C_PRECISION of 14 gives inexact results about 1.5% of the time. A
-- C_PRECISION of 16 gives inexact results about 0.4% of the time. Resulting
-- pixel values are never off by more than 1 in any of these examples.
-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

entity rgb_ycbcr is
  generic (
    C_DATA_WIDTH    : integer := 8;      -- Should be < 18 for best results
    C_PRECISION     : integer := 12;     -- Should be < 18 for best results
    C_FULL_RANGE    : boolean := TRUE);  -- RGB input from 0-255 (true)
  port (
    clk       : in  std_logic;
    rst       : in  std_logic;
    r         : in  std_logic_vector(C_DATA_WIDTH-1 downto 0);
    g         : in  std_logic_vector(C_DATA_WIDTH-1 downto 0);
    b         : in  std_logic_vector(C_DATA_WIDTH-1 downto 0);
    wr_en     : in  std_logic;
    y         : out std_logic_vector(C_DATA_WIDTH-1 downto 0);
    cb        : out std_logic_vector(C_DATA_WIDTH-1 downto 0);
    cr        : out std_logic_vector(C_DATA_WIDTH-1 downto 0);
    valid     : out std_logic);
end rgb_ycbcr;


architecture imp of rgb_ycbcr is

  constant C_1_PRE : unsigned(C_PRECISION-1 downto 0) :=
    to_unsigned(1, C_PRECISION);
  
  -- Coefficients as 0.32 format 32-bit fixeds point numbers
  -- Each is computed as C*(2^32)+0.5, then rounded down.
  signal C_I32_Y_R : unsigned(31 downto 0);
  signal C_I32_Y_G : unsigned(31 downto 0);
  signal C_I32_Y_B : unsigned(31 downto 0);
  signal C_I32_CB_R : unsigned(31 downto 0);
  signal C_I32_CB_G : unsigned(31 downto 0);
  signal C_I32_CB_B : unsigned(31 downto 0);
  signal C_I32_CR_R : unsigned(31 downto 0);
  signal C_I32_CR_G : unsigned(31 downto 0);
  signal C_I32_CR_B : unsigned(31 downto 0);

  -- Coefficients in desired precision
  signal C_I_Y_R : unsigned(C_PRECISION-1 downto 0);
  signal C_I_Y_G : unsigned(C_PRECISION-1 downto 0);
  signal C_I_Y_B : unsigned(C_PRECISION-1 downto 0);
  signal C_I_CB_R : unsigned(C_PRECISION-1 downto 0);
  signal C_I_CB_G : unsigned(C_PRECISION-1 downto 0);
  signal C_I_CB_B : unsigned(C_PRECISION-1 downto 0);
  signal C_I_CR_R : unsigned(C_PRECISION-1 downto 0);
  signal C_I_CR_G : unsigned(C_PRECISION-1 downto 0);
  signal C_I_CR_B : unsigned(C_PRECISION-1 downto 0);
  
  signal C_I_128 : unsigned(C_DATA_WIDTH-1 downto 0);
  signal C_I_16  : unsigned(C_DATA_WIDTH-1 downto 0);
  

  -- Stage 0 signals
  signal r_0 : unsigned(C_DATA_WIDTH-1 downto 0);
  signal g_0 : unsigned(C_DATA_WIDTH-1 downto 0);
  signal b_0 : unsigned(C_DATA_WIDTH-1 downto 0);
  signal en_0 : std_logic;

  -- Stage 1 signals
  signal y_r, y_g, y_b    : unsigned(C_PRECISION-1 downto 0);
  signal cb_r, cb_g, cb_b : unsigned(C_PRECISION-1 downto 0);
  signal cr_r, cr_g, cr_b : unsigned(C_PRECISION-1 downto 0);
  signal en_1 : std_logic;

  -- Stage 2 signals
  signal y_2, cb_2, cr_2 : unsigned(C_DATA_WIDTH downto 0);
  signal en_2 : std_logic;

  -- Stage 3 signals
  signal y_3, cb_3, cr_3 : unsigned(C_DATA_WIDTH-1 downto 0);
  signal en_3 : std_logic;

begin

  -- Assign constants based on current mode (full range or not)
  GEN_FULL_RANGE_CONSTANTS_T: if C_FULL_RANGE generate
    C_I32_Y_R  <= x"41bcec85";
    C_I32_Y_G  <= x"810e9920";
    C_I32_Y_B  <= x"19105e1c";
    C_I32_CB_R <= x"25f1f14a";
    C_I32_CB_G <= x"4a7e73a3";
    C_I32_CB_B <= x"707064ed";
    C_I32_CR_R <= x"707064ed";
    C_I32_CR_G <= x"5e276b7f";
    C_I32_CR_B <= x"1248f96e";
    C_I_16 <= shift_left(to_unsigned(1,C_DATA_WIDTH), C_DATA_WIDTH-4);
  end generate;
  GEN_FULL_RANGE_CONSTANTS_F: if not C_FULL_RANGE generate
    C_I32_Y_R  <= x"4c8b4396";
    C_I32_Y_G  <= x"9645a1cb";
    C_I32_Y_B  <= x"1d2f1aa0";
    C_I32_CB_R <= x"2c2e989a";
    C_I32_CB_G <= x"56bd6e8b"; 
    C_I32_CB_B <= x"82ec0725";
    C_I32_CR_R <= x"82ec0725";
    C_I32_CR_G <= x"6da187a5";
    C_I32_CR_B <= x"154a7f80";
    C_I_16 <= (others => '0');
  end generate;

  -- Compute coefficients constants in desired precsion, with a bit of rounding
  C_I_Y_R <= C_I32_Y_R(31 downto 31-C_PRECISION+1)
             when C_I32_Y_R(31-C_PRECISION) = '0'
             else C_I32_Y_R(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_Y_G <= C_I32_Y_G(31 downto 31-C_PRECISION+1)
             when C_I32_Y_G(31-C_PRECISION) = '0'
             else C_I32_Y_G(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_Y_B <= C_I32_Y_B(31 downto 31-C_PRECISION+1)
             when C_I32_Y_B(31-C_PRECISION) = '0'
             else C_I32_Y_B(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_CB_R <= C_I32_CB_R(31 downto 31-C_PRECISION+1)
             when C_I32_CB_R(31-C_PRECISION) = '0'
             else C_I32_CB_R(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_CB_G <= C_I32_CB_G(31 downto 31-C_PRECISION+1)
             when C_I32_CB_G(31-C_PRECISION) = '0'
             else C_I32_CB_G(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_CB_B <= C_I32_CB_B(31 downto 31-C_PRECISION+1)
             when C_I32_CB_B(31-C_PRECISION) = '0'
             else C_I32_CB_B(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_CR_R <= C_I32_CR_R(31 downto 31-C_PRECISION+1)
             when C_I32_CR_R(31-C_PRECISION) = '0'
             else C_I32_CR_R(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_CR_G <= C_I32_CR_G(31 downto 31-C_PRECISION+1)
             when C_I32_CR_G(31-C_PRECISION) = '0'
             else C_I32_CR_G(31 downto 31-C_PRECISION+1) + C_1_PRE;
  C_I_CR_B <= C_I32_CR_B(31 downto 31-C_PRECISION+1)
             when C_I32_CR_B(31-C_PRECISION) = '0'
             else C_I32_CR_B(31 downto 31-C_PRECISION+1) + C_1_PRE;

  C_I_128 <= shift_left(to_unsigned(1,C_DATA_WIDTH), C_DATA_WIDTH-1);
    

  -----------------------------------------------------------------------------
  -- STAGE 0: Input registers
  -----------------------------------------------------------------------------

  STAGE_0_PROC: process (clk, rst, r, g, b, wr_en)
  begin
    if rst = '1' then
      en_0 <= '0';
      r_0 <= (others => '0');
      g_0 <= (others => '0');
      b_0 <= (others => '0');
    elsif clk'event and clk = '1' then
      r_0 <= unsigned(r);
      g_0 <= unsigned(g);
      b_0 <= unsigned(b);
      en_0 <= wr_en;
    end if;
  end process;

  
  -----------------------------------------------------------------------------
  -- STAGE 1: Multiplication
  -----------------------------------------------------------------------------

  STAGE_1_PROC: process (clk, rst, en_0, r_0, g_0, b_0)
    variable temp : unsigned(C_DATA_WIDTH+C_PRECISION-1 downto 0);
  begin
    if rst = '1' then
      en_1 <= '0';
      y_r <= (others => '0');
      y_g <= (others => '0');
      y_b <= (others => '0');
      cb_r <= (others => '0');
      cb_g <= (others => '0');
      cb_b <= (others => '0');
      cr_r <= (others => '0');
      cr_g <= (others => '0');
      cr_b <= (others => '0');
    elsif clk'event and clk = '1' then
      en_1 <= en_0;
      
      -- Perform multiplication at full precision. C_PRECISION-bit *
      -- C_DATA_WIDTH-bit to produce (C_PRECISION+C_DATA_WIDTH)-bit result.
      
      temp := C_I_Y_R * r_0;
      y_r <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_Y_G * g_0;
      y_g <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_Y_B * b_0;
      y_b <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_CB_R * r_0;
      cb_r <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_CB_G * g_0;
      cb_g <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_CB_B * b_0;
      cb_b <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_CR_R * r_0;
      cr_r <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_CR_G * g_0;
      cr_g <= temp(temp'left downto temp'left-C_PRECISION+1);
      
      temp := C_I_CR_B * b_0;
      cr_b <= temp(temp'left downto temp'left-C_PRECISION+1);
    end if;
  end process;

  
  -----------------------------------------------------------------------------
  -- STAGE 2: Addition
  -----------------------------------------------------------------------------

  STAGE_2_PROC: process (clk, rst, en_1,
                         y_r, y_g, y_b,
                         cb_r, cb_g, cb_b,
                         cr_r, cr_g, cr_b)
    variable temp_y   : unsigned(C_PRECISION-1 downto 0);
    variable temp_cb  : unsigned(C_PRECISION-1 downto 0);
    variable temp_cr  : unsigned(C_PRECISION-1 downto 0);
  begin
    if rst = '1' then
      en_2 <= '0';
      y_2 <= (others => '0');
      cb_2 <= (others => '0');
      cr_2 <= (others => '0');
    elsif clk'event and clk = '1' then
      en_2 <= en_1;
      
      -- Do addition
      temp_y := y_r + y_g + y_b;
      temp_cb := cb_b - cb_g - cb_r;
      temp_cr := cr_r - cr_g - cr_b;

      -- Truncate result to C_DATA_WIDTH+1 bits (1 bit for rounding later)
      y_2 <= temp_y(temp_y'left downto temp_y'left-C_DATA_WIDTH);
      cb_2 <= temp_cb(temp_cb'left downto temp_cb'left-C_DATA_WIDTH);
      cr_2 <= temp_cr(temp_cr'left downto temp_cr'left-C_DATA_WIDTH);
      
    end if;
  end process;


  -----------------------------------------------------------------------------
  -- STAGE 3: Final Addition and Rounding
  -----------------------------------------------------------------------------

  STAGE_3_PROC: process (clk, rst, en_2,
                         y_r, y_g, y_b,
                         cb_r, cb_g, cb_b,
                         cr_r, cr_g, cr_b)
    variable y_round  : unsigned(C_DATA_WIDTH-1 downto 0);
    variable cb_round : unsigned(C_DATA_WIDTH-1 downto 0);
    variable cr_round : unsigned(C_DATA_WIDTH-1 downto 0);
  begin
    if rst = '1' then
      en_3 <= '0';
      y_3 <= (others => '0');
      cb_3 <= (others => '0');
      cr_3 <= (others => '0');
    elsif clk'event and clk = '1' then
      en_3 <= en_2;
      
      -- Determine rounding, combine with the 128 constant
      if y_2(0) = '1' then
        if C_FULL_RANGE then
          y_round := C_I_16+1;
        else
          y_round := to_unsigned(1, C_DATA_WIDTH);
        end if;
      else
        if C_FULL_RANGE then
          y_round := C_I_16;
        else
          y_round := (others => '0');
        end if;
      end if;
      if cb_2(0) = '1' then
        cb_round := resize(C_I_128+1, C_DATA_WIDTH);
      else
        cb_round := C_I_128;
      end if;
      if cr_2(0) = '1' then
        cr_round := resize(C_I_128+1, C_DATA_WIDTH);
      else
        cr_round := C_I_128;
      end if;

      -- Do final addition and round
      y_3 <= y_2(y_2'left downto 1) + y_round;
      cb_3 <= cb_2(cb_2'left downto 1) + cb_round;
      cr_3 <= cr_2(cr_2'left downto 1) + cr_round;
      
    end if;
  end process;

  -- Output results
  y <= std_logic_vector(y_3);
  cb <= std_logic_vector(cb_3);
  cr <= std_logic_vector(cr_3);
  valid <= en_3;
  

end imp;


