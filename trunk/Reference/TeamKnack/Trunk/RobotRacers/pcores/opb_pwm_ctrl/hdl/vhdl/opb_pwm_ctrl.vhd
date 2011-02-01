-------------------------------------------------------------------------------
-- OPB Slave Bus Interface for PWM for MicroBlaze
--
-- File: opb_pwm_ctrl.vhd
-- Author: Wade S. Fife
-- Date: January 19, 2005
--
-- DESCRIPTION
--
-- This file consits of an OPB slave bus interface to be used with the
-- pwm_ctrl entity (pwm_ctrl.vhd).
--
-- DOCUMENTATION
--
-- See the pwm_ctrl documentation for information on its
-- usage. Which PWM controller is accessed in a write or read
-- operation (and which register within the PWM controller) depends
-- on the OPB_ABus address that is accessed. The 32 address bits can
-- be devided as follows:
--
--   BBBB......MMRRXX
--
-- The BBBB... bits correspond to the most significant bits of
-- C_BASEADDR. The ...MM bits identify which PWM controller to
-- control. The number of M bits depends on the number of PWM
-- controllers (C_NUM_PWMS). The RR bits indicate which register of
-- the PWM to access, and correspond exactly to the addr input of the
-- pwm_ctrl entity (See pwm_ctrl.vhd for more
-- information). The lower to bits of OPB_ABus are ignored so that all
-- addresses are interpreted to be on word boundaries. Thus a write to
-- both C_BASEADDR and C_BASEADDR+1 will write to the duty_reg of
-- PWM controller 0 (unless the addresses have been changed).
--
-- This is a very simple slave interface. Features such as
-- byte-enable, sequential addressing, retry, time-out supression, and
-- error acknowledgement are not used.
--
-- ENTITY PARAMETERS
--
-- A discription of the opb_pwm_ctrl entity parameters that can
-- be modified is given below. These parameters can be set in the .mhs
-- (Microprocessor Hardware Specification) file for the EST project.
--
--   C_BASEADDR   - The lowest address that the OPB entity will recognize as
--                  being intended for itself.
--   C_HIGHADDR   - The highest address that the OPB entity will recognize as
--                  being intended for itself.
--   C_OPB_AWIDTH - OPB address bus width. Should be 32 for Microblaze.
--   C_OPB_DWIDTH - OPB data bus width. Should be 32 for Microblaze.
--   C_PWM_WIDTH  - PWM is controlled by two counters that count clock cycles
--                  in order to determine when the output pulse should go from
--                  high to low or low to high. This parameter allows you to
--                  specify the size of this counter, which determines the
--                  maximum PWM period. For example, with a 16-bit PWM width,
--                  the maximum PWM period can be 16,535 cycles. The value of
--                  the parameter should be the ceiling of
--                  [log(OPB_freq/PWM_freq) / log(2)]. 
--   C_NUM_PWMS   - This parameter specifies the number of PWM
--                  controllers to generate. The size of the pwm output
--                  will depend on this parameter.  NOTE: You may get
--                  an index size warning if C_NUM_PWMS isn't a power
--                  of 2, but the controller will still work.
--
-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

library unisim;
use unisim.vcomponents.all;


entity opb_pwm_ctrl is
  generic (
    C_BASEADDR   : std_logic_vector(0 to 31) := x"FFFFFF00";
    C_HIGHADDR   : std_logic_vector(0 to 31) := x"FFFFFFFF";
    C_OPB_AWIDTH : integer   := 32;
    C_OPB_DWIDTH : integer   := 32;
    C_PWM_WIDTH  : integer   := 22;     -- Should equal the ceiling of
                                        --   log(OPB_freq/PWM_freq) / log(2)
    C_NUM_PWMS : integer   := 2);       -- You may get an index size warning
                                        -- from the synthesizer is C_NUM_PWMS
                                        -- isn't a power of 2. That's OK.
  
  port (
    -- OPB bus signals
    OPB_Clk     : in  std_logic;
    OPB_Rst     : in  std_logic;
    OPB_ABus    : in  std_logic_vector(0 to C_OPB_AWIDTH-1);
    OPB_BE      : in  std_logic_vector(0 to C_OPB_DWIDTH/8-1);
    OPB_DBus    : in  std_logic_vector(0 to C_OPB_DWIDTH-1);
    OPB_RNW     : in  std_logic;
    OPB_select  : in  std_logic;
    OPB_seqAddr : in  std_logic;

    PWM_DBus    : out std_logic_vector(0 to C_OPB_DWIDTH-1);
    PWM_errAck  : out std_logic;
    PWM_retry   : out std_logic;
    PWM_toutSup : out std_logic;
    PWM_xferAck : out std_logic;    

    -- PWM control signals
    pwm         : out std_logic_vector(0 to C_NUM_PWMS-1));
end entity opb_pwm_ctrl;

-------------------------------------------------------------------------------
-- Architecture section
-------------------------------------------------------------------------------

architecture imp of opb_pwm_ctrl is

  -- Function to calculate number of bits needed for address comparison
  -- to determine if this device is being addressed.
  function AddrBits (x, y : std_logic_vector(0 to C_OPB_AWIDTH-1))
  return integer is
    variable addr_nor : std_logic_vector(0 to C_OPB_AWIDTH-1);
  begin
    addr_nor := x xor y;
    for i in 0 to C_OPB_AWIDTH-1 loop
      if addr_nor(i) = '1' then
        return i;
      end if;
    end loop;
    return C_OPB_AWIDTH;
  end function AddrBits;

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

  component pwm_ctrl
    generic (
      C_PWM_WIDTH : integer;
      C_DATA_WIDTH : integer);
    port (
      clk      : in  std_logic;
      reset    : in  std_logic;
      data_in  : in  std_logic_vector(0 to C_PWM_WIDTH-1);
      data_out : out std_logic_vector(0 to C_DATA_WIDTH-1);
      rd_en    : in  std_logic;
      wr_en    : in  std_logic;
      addr     : in  std_logic_vector(0 to 1);
      pwm      : out std_logic);
  end component;

  -- Entity pselect does address detection and is provided by Xilinx for
  -- performance reasons.
  component pselect
    generic (
      C_AB   : integer;
      C_AW   : integer;
      C_BAR  : std_logic_vector);
    port (
      A      : in std_logic_vector(0 to C_AW-1);
      AValid : in std_logic;
      CS     : out std_logic);
  end component;
  

  constant C_ADDR_BITS : integer := AddrBits(C_HIGHADDR, C_BASEADDR);
  constant C_PWM_ADDR_BITS : integer :=
    RequiredBits(C_NUM_PWMS-1, C_OPB_DWIDTH);
  
  signal device_select : std_logic;     -- Indicates current device selected
  signal opb_dbus_reg : std_logic_vector(0 to C_OPB_DWIDTH-1);
  signal opb_abus_reg : std_logic_vector(0 to C_OPB_DWIDTH-1);
  signal opb_rnw_reg : std_logic;

  type ctrl_data_in_array is array (0 to C_NUM_PWMS-1)
    of std_logic_vector(0 to C_PWM_WIDTH-1);
  type ctrl_data_out_array is array (0 to C_NUM_PWMS-1)
    of std_logic_vector(0 to C_OPB_DWIDTH-1);
  type ctrl_addr_array is array (0 to C_NUM_PWMS-1)
    of std_logic_vector(0 to 1);

  signal ctrl_data_in  : ctrl_data_in_array;
  signal ctrl_data_out : ctrl_data_out_array;
  signal ctrl_addr     : ctrl_addr_array;
  signal ctrl_rd_en    : std_logic_vector(0 to C_NUM_PWMS-1);
  signal ctrl_wr_en    : std_logic_vector(0 to C_NUM_PWMS-1);
  
  type opb_state_type is ( IDLE_STATE,
                           DECODE_STATE,
                           READ_STATE_1,
                           READ_STATE_2,
                           WRITE_STATE);
  signal current_state, next_state : opb_state_type;

begin

  -- Generate the C_NUM_PWMS PWM controllers
  GEN_CONTROLLERS : for i in 0 to C_NUM_PWMS-1 generate
    PWM_CONTROLLER_I: pwm_ctrl
      generic map (
        C_PWM_WIDTH => C_PWM_WIDTH,
        C_DATA_WIDTH => C_OPB_DWIDTH)
      port map (
        clk      => OPB_Clk,
        reset    => OPB_Rst,
        data_in  => ctrl_data_in(i),
        data_out => ctrl_data_out(i),
        rd_en    => ctrl_rd_en(i),
        wr_en    => ctrl_wr_en(i),
        addr     => ctrl_addr(i),
        pwm      => pwm(i));
  end generate;
  
  -- pselect entity does address comparison to determine if device selected
  PSELECT_I : pselect
    generic map (
      C_AB => C_ADDR_BITS,
      C_AW => C_OPB_AWIDTH,
      C_BAR => C_BASEADDR)
    port map (
      A      => OPB_ABus,
      AValid => OPB_select,
      CS     => device_select);


  -- Register OPB signals
  OPB_DBUS_FF_GEN : for i in 0 to C_OPB_DWIDTH-1 generate
    DBUS_FF_I : FDR
    port map (
      Q => opb_dbus_reg(i),
      C => OPB_Clk,
      D => OPB_DBus(i),
      R => OPB_Rst);
  end generate;
  OPB_ABUS_FF_GEN : for i in 0 to C_OPB_AWIDTH-1 generate
    ABUS_FF_I : FDR
    port map (
      Q => opb_abus_reg(i),
      C => OPB_Clk,
      D => OPB_ABus(i),
      R => OPB_Rst);
  end generate;
  OPB_RNW_FF : FDR
  port map (
    Q => opb_rnw_reg,
    C => OPB_Clk,
    D => OPB_RNW,
    R => OPB_Rst);

  PWM_retry <= '0';                     -- No retry
  PWM_errAck <= '0';                    -- No error acknowledge
  PWM_toutSup <= '0';                   -- No timeout supress


  -- OPB Read/Write state machine
  
  OPB_FSM_REG : process (OPB_Clk, OPB_Rst)
  begin
    if OPB_Rst = '1' then
      current_state <= IDLE_STATE;
    elsif OPB_Clk'event and OPB_Clk='1' then
      current_state <= next_state;
    end if;
  end process OPB_FSM_REG;

  OPB_FSM_COMB : process(current_state, device_select, opb_rnw_reg,
                         opb_dbus_reg, opb_abus_reg, ctrl_data_out)
    variable PWM_num : integer;
  begin
    -- Set index to determine which PWM is being accessed
    if C_NUM_PWMS > 1 then
      PWM_num:= to_integer(unsigned(opb_abus_reg(28-C_PWM_ADDR_BITS to 27)));
    else
      PWM_num := 0;
    end if;
    
    -- Default values
    next_state <= current_state;
    PWM_xferAck <= '0';
    PWM_DBus <= (others => '0');  -- PWM_DBus MUST be driven to 0 when inactive
    for i in 0 to C_NUM_PWMS-1 loop
      ctrl_rd_en(i) <= '0';
      ctrl_wr_en(i) <= '0';

      -- Rout address and data to all PWM controllers
      ctrl_addr(i) <=
        opb_abus_reg(C_OPB_DWIDTH-ctrl_addr(i)'length-2 to C_OPB_DWIDTH-3);
      ctrl_data_in(i) <=
        opb_dbus_reg(C_OPB_DWIDTH-C_PWM_WIDTH to C_OPB_DWIDTH-1);
    end loop;

    -- State logic
    case current_state is
      when IDLE_STATE =>
        if device_select = '1' then
          next_state <= DECODE_STATE;
        end if;

      when DECODE_STATE =>
        if opb_rnw_reg = '1' then
          next_state <= READ_STATE_1;
        else
          next_state <= WRITE_STATE;
        end if;

      when READ_STATE_1 =>
        ctrl_rd_en(PWM_num) <= '1';
        next_state <= READ_STATE_2;

      when READ_STATE_2 =>
        PWM_xferAck <= '1';
        PWM_DBus <= std_logic_vector(ctrl_data_out(PWM_num));        
        next_state <= IDLE_STATE;
        
      when WRITE_STATE =>
        PWM_xferAck <= '1';
        ctrl_wr_en(PWM_num) <= '1';
        next_state <= IDLE_STATE;            

      when others =>
        next_state <= IDLE_STATE;
        
    end case;
  end process OPB_FSM_COMB;
  
end architecture imp;
