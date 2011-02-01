-------------------------------------------------------------------------------
-- PWM Controller
--
-- File: pwm_ctrl.vhd
-- Author: Wade S. Fife
-- Date: January 19, 2005
-- Last Revised: March 5, 2005
--
-- DESCRIPTION
--
-- This file consits of a simple PWM controller.
--
-- DOCUMENTATION
--
-- PWM Registers:
--
-- The PWM unit consists of a simple counter and two compare
-- registers, duty_reg and period_reg. In normal operation the pwm
-- output will output a '1' for duty_reg cycles and '0' for
-- period_reg-duty_reg cycles. The process then repeats. Thus, if
-- duty_reg is 0 then the duty cycle is 0%. If duty_reg equals
-- period_reg then the duty cycle is 100%. Use period_reg to configure
-- the period of the PWM signal.
--
-- Reading and Writing Registers:
--
-- To read a register, simply set rd_en high and set the addr input to
-- the appropriate address (see below). The data will appear on
-- data_out after the next positive clock edge. To write, assert the
-- data to write on data_in, set wr_en high, and assert the addr
-- input. The data will appear on data_out after the next positive
-- clock edge. The appropriate values for input addr are as follows:
--
--   00 - duty_reg
--   01 - period_reg
--
-- Behavior is undefined when duty_reg is set to a greater value than
-- period_reg.
--
-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

entity pwm_ctrl is
  generic (
    C_PWM_WIDTH : integer := 22;        -- Should equal the ceiling of
                                        --   log(OPB_freq/PWM_freq) / log(2)
    C_DATA_WIDTH : integer := 32);
  port (
    clk : in std_logic;
    reset : in std_logic;
    
    data_in : in std_logic_vector(0 to C_PWM_WIDTH-1);
    data_out : out std_logic_vector(0 to C_DATA_WIDTH-1);

    rd_en : in std_logic;
    wr_en : in std_logic;    

    addr : in std_logic_vector(0 to 1);

    pwm  : out std_logic);
end entity pwm_ctrl;


architecture imp of pwm_ctrl is

  constant C_CTRL_WIDTH : integer := 2;

  constant C_ZERO: unsigned(0 to C_PWM_WIDTH-1) := to_unsigned(0, C_PWM_WIDTH);
  constant C_ONE : unsigned(0 to C_PWM_WIDTH-1) := to_unsigned(1, C_PWM_WIDTH);
  constant C_PWM_PAD : unsigned(0 to C_DATA_WIDTH-C_PWM_WIDTH-1)
    := to_unsigned(0, C_DATA_WIDTH-C_PWM_WIDTH);
  constant C_CTRL_PAD : unsigned(0 to C_DATA_WIDTH-C_CTRL_WIDTH-1)
    := to_unsigned(0, C_DATA_WIDTH-C_CTRL_WIDTH);

  -- Define addr values
  constant ADDR_DUTY :   std_logic_vector(0 to addr'length-1) := "00";
  constant ADDR_PERIOD : std_logic_vector(0 to addr'length-1) := "01";

  signal pwm_count_reg : unsigned(0 to C_PWM_WIDTH-1);

  -- PWM controller registers
  signal duty_reg : unsigned(0 to C_PWM_WIDTH-1);
  signal period_reg : unsigned(0 to C_PWM_WIDTH-1);

begin

  -----------------------------------------------------------------------------
  -- Register data_out for reads
  SER_READ_LOGIC : process (clk, reset, addr, duty_reg,
                            period_reg)
  begin
    if clk'event and clk = '1' then
      if rd_en = '1' then
        case addr is
          when ADDR_DUTY =>
            data_out <= std_logic_vector(C_PWM_PAD & duty_reg); 
          when ADDR_PERIOD =>
            data_out <= std_logic_vector(C_PWM_PAD & period_reg);
          when others =>
            data_out <= std_logic_vector(C_PWM_PAD & period_reg);
        end case;
      end if;
    end if;
  end process SER_READ_LOGIC;

  
  -----------------------------------------------------------------------------
  -- Control updating of internal registers for writes
  SER_WRITE_LOGIC : process (clk, reset, wr_en, data_in, addr)
  begin
    if reset = '1' then
      duty_reg <= (others => '0');
      period_reg <= (others => '0');
    elsif clk'event and clk = '1' then
      if wr_en = '1' then
        if addr = ADDR_DUTY then
          duty_reg <= unsigned(data_in);
        elsif addr = ADDR_PERIOD then
          period_reg <= unsigned(data_in);
        end if;
      end if;
    end if;
  end process SER_WRITE_LOGIC;

  
  -----------------------------------------------------------------------------
  -- Control PWM register and output
  SER_OUTPUT_LOGIC : process (clk, reset, pwm_count_reg,
                              period_reg, duty_reg)
  begin
    if reset = '1' then
      pwm_count_reg <= C_ONE;
      pwm <= '0';
    elsif clk'event and clk = '1' then
      if pwm_count_reg >= period_reg then
        pwm_count_reg <= C_ONE;
      else
        pwm_count_reg <= pwm_count_reg + 1;
      end if;
      
      -- Drive PWM output
      if pwm_count_reg > duty_reg then
        pwm <= '0';
      else
        pwm <= '1';
      end if;
    end if;
  end process SER_OUTPUT_LOGIC;

end imp;
