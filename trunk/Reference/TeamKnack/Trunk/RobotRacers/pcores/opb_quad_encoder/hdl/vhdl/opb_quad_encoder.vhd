-------------------------------------------------------------------------------
-- OPB Quadrature Encoder Core
--
-- File: opb_quad_encoder
-- Author: Wade S. Fife
-- Date: February 2, 2006
--
-- DESCRIPTION
--
-- This file consits of an OPB slave bus interface to be used with the
-- quad_encoder entity (quadd_encoder.vhd).
--
-- DOCUMENTATION
--
-- See the quad_encoder documentation for information on its purpose and usage.
--
-- ENTITY PARAMETERS
--
-- A discription of the opb_motor_controller entity parameters that can
-- be modified is given below. These parameters can be set in the .mhs
-- (Microprocessor Hardware Specification) file for the EST project.
--
--   C_BASEADDR    - The lowest address that the OPB entity will recognize as
--                   being intended for itself.
--   C_HIGHADDR    - The highest address that the OPB entity will recognize as
--                   being intended for itself.
--   C_OPB_AWIDTH  - OPB address bus width. Should be 32 for Microblaze.
--   C_OPB_DWIDTH  - OPB data bus width. Should be 32 for Microblaze.
--
-------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

library unisim;
use unisim.vcomponents.all;


entity opb_quad_encoder is
  generic (
    C_BASEADDR    : std_logic_vector(0 to 31) := x"80000000";
    C_HIGHADDR    : std_logic_vector(0 to 31) := x"800000FF";
    C_OPB_AWIDTH  : integer   := 32;
    C_OPB_DWIDTH  : integer   := 32);
  
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

    Enc_DBus    : out std_logic_vector(0 to C_OPB_DWIDTH-1);
    Enc_errAck  : out std_logic;
    Enc_retry   : out std_logic;
    Enc_toutSup : out std_logic;
    Enc_xferAck : out std_logic;    

    -- Encoder channel signals
    enc_a       : in  std_logic;
    enc_b       : in  std_logic);
  
end entity opb_quad_encoder;

-------------------------------------------------------------------------------
-- Architecture section
-------------------------------------------------------------------------------

architecture imp of opb_quad_encoder is

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

  component quad_encoder
    generic (
      C_COUNT_WIDTH : integer);
    port (
      clk    : in std_logic;
      rst    : in std_logic;
      clr    : in std_logic;
      enc_a  : in  std_logic;
      enc_b  : in  std_logic;
      count  : out std_logic_vector(C_COUNT_WIDTH-1 downto 0));
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
  
  signal device_select : std_logic;     -- Indicates current device selected
  signal opb_dbus_reg : std_logic_vector(0 to C_OPB_DWIDTH-1);
  signal opb_abus_reg : std_logic_vector(0 to C_OPB_DWIDTH-1);
  signal opb_rnw_reg : std_logic;

  type opb_state_type is ( IDLE_STATE, EXEC_STATE );
  signal current_state, next_state : opb_state_type;

  signal enc_count : std_logic_vector(0 to C_OPB_DWIDTH-1);
  signal enc_clear : std_logic;

begin

  QUAD_ENCODER_I: quad_encoder
    generic map (
      C_COUNT_WIDTH => C_OPB_DWIDTH)
    port map (
      clk      => OPB_Clk,
      rst      => OPB_Rst,
      clr      => enc_clear,
      enc_a    => enc_a,
      enc_b    => enc_b,
      count    => enc_count);

  
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

  Enc_retry <= '0';                     -- No retry
  Enc_errAck <= '0';                    -- No error acknowledge
  Enc_toutSup <= '0';                   -- No timeout supress


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
                         opb_dbus_reg, opb_abus_reg, enc_count)
    variable motor_num : integer;
  begin
    -- Default values
    next_state <= current_state;
    Enc_xferAck <= '0';
    Enc_DBus <= (others => '0');  -- Enc_DBus MUST be driven to 0 when inactive
    enc_clear <= '0';

    -- State logic
    case current_state is
      when IDLE_STATE =>
        if device_select = '1' then
          next_state <= EXEC_STATE;
        end if;

      when EXEC_STATE =>
        if opb_rnw_reg = '1' then
          Enc_xferAck <= '1';
          Enc_DBus <= std_logic_vector(enc_count);
          next_state <= IDLE_STATE;
        else
          Enc_xferAck <= '1';
          enc_clear <= '1';
          next_state <= IDLE_STATE;            
        end if;

      when others =>
        next_state <= IDLE_STATE;
        
    end case;
  end process OPB_FSM_COMB;
  
end architecture imp;
