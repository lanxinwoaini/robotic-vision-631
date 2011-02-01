-------------------------------------------------------------------------------
-- Floating Point Unit Fabric Coprocessor Module for PowerPC APU
-- (FPU FCM for PPC APU)
--
-- File: fcm_fpu.vhd
-- Author: Wade S. Fife
-- Date: 03/09/06
--
-- DESCRIPTION
--
-- This module provides the interface between the PowerPC's APU and FPULight
-- modules.  The FPULight modules make up a simplified floating point unit
-- derived from IEEE 754 standard. It differs in that it does not support
-- +/-NaN, +/-INF, denormalized numbers, or extra rounding modes. Only round
-- towards nearest even is supported. See fpu_light.vhd for details.
--
-- You can configure which functional units to include by setting the
-- approprate generic input to 1 (include) or 0 (omit). You can also modify the
-- pipeline interlock configuration of each functional unit by modifying the
-- generic inputs for a function unit's instantiation. This allows you to tune
-- the hardware to your target clock frequency.
--
-- The number of of functional units you include may also affect whether or not
-- you need to enable interlocks near the output of each functional unit in
-- order to reach your target clock rate. For example, if you choose to only
-- include the add/sub and multiply functional units then you may be able to
-- remove the final interlock in the add/sub unit and eliminate a cycle from
-- the unit's latency.
--
-- The instruction decoding assumes that the UDI instructions (udiNfcm), as
-- defined in the PowerPC Instruction Set Exension Guide
-- (ppc405_isaext_guide.pdf). The following table shows the mapping between UDI
-- instructions and FPU operations.
--
--           Instruction            | Operation |        C Equivalent
--   --------------------------------------------------------------------------
--    udi0fcm   dReg, aReg, bReg    |    add    | dReg = aReg + bReg
--    udi1fcm   dReg, aReg, bReg    |    sub    | dReg = aReg - bReg
--    udi2fcm   dReg, aReg, bReg    |    mul    | dReg = aReg * bReg
--    udi3fcm   dReg, aReg, bReg    |    div    | dReg = aReg / bReg
--    udi4fcm   dReg, aReg, bReg    |    sqrt   | dReg = sqrt(aReg)
--    udi5fcm   dReg, aReg, 1       |    ftoi   | dReg = (int)aReg
--    udi5fcm   dReg, aReg, 0       |    ftou   | dReg = (unsinged)aReg
--    udi6fcm   dReg, aReg, 1       |    itof   | dReg = (float)(int)aReg
--    udi6fcm   dReg, aReg, 0       |    utof   | dReg = (float)(unsigned)aReg
--    udi7fcm   dReg, aReg, bReg    |    cmp    | dReg = aReg > bReg ? 1 :
--                                                      (aReg < bReg ? -1 : 0)
--
-- The compare instruction simply returns 1 if aReg > bReg, 0 if aReg == bReg,
-- and -1 if aReg < bReg.
--
-- See the file fpu_light.vhd for details on how these operations work.
--
-------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

library unisim;
use unisim.vcomponents.all;


entity fcm_fpu_light is
  generic (
    C_EXT_RESET_HIGH : integer := 1;
    C_USE_ADD        : integer := 1;
    C_USE_MULT       : integer := 1;
    C_USE_DIV        : integer := 1;
    C_USE_SQRT       : integer := 1;
    C_USE_ITOF       : integer := 1;
    C_USE_FTOI       : integer := 1;
    C_USE_CMP        : integer := 1);
  port (
    clk   : in std_logic;
    sys_rst : in std_logic;

    -- Inputs from APU
    APUFCMINSTRUCTION        : in std_logic_vector(0 to 31);
    APUFCMINSTRVALID         : in std_logic;
    APUFCMRADATA             : in std_logic_vector(0 to 31);
    APUFCMRBDATA             : in std_logic_vector(0 to 31);
    APUFCMOPERANDVALID       : in std_logic;
    APUFCMFLUSH              : in std_logic;
    APUFCMWRITEBACKOK        : in std_logic;
    APUFCMLOADDATA           : in std_logic_vector(0 to 31);
    APUFCMLOADDVALID         : in std_logic;
    APUFCMLOADBYTEEN         : in std_logic_vector(0 to 3);
    APUFCMENDIAN             : in std_logic;
    APUFCMXERCA              : in std_logic;
    APUFCMDECODED            : in std_logic;
    APUFCMDECUDI             : in std_logic_vector(0 to 2);
    APUFCMDECUDIVALID        : in std_logic;

    -- Outputs to APU
    FCMAPUINSTRACK           : out std_logic;
    FCMAPURESULT             : out std_logic_vector(0 to 31);
    FCMAPUDONE               : out std_logic;
    FCMAPUSLEEPNOTREADY      : out std_logic;
    FCMAPUDECODEBUSY         : out std_logic;
    FCMAPUDCDGPRWRITE        : out std_logic;
    FCMAPUDCDRAEN            : out std_logic;
    FCMAPUDCDRBEN            : out std_logic;
    FCMAPUDCDPRIVOP          : out std_logic;
    FCMAPUDCDFORCEALIGN      : out std_logic;
    FCMAPUDCDXEROVEN         : out std_logic;
    FCMAPUDCDXERCAEN         : out std_logic;
    FCMAPUDCDCREN            : out std_logic;
    FCMAPUEXECRFIELD         : out std_logic_vector(0 to 2);
    FCMAPUDCDLOAD            : out std_logic;
    FCMAPUDCDSTORE           : out std_logic;
    FCMAPUDCDUPDATE          : out std_logic;
    FCMAPUDCDLDSTBYTE        : out std_logic;
    FCMAPUDCDLDSTHW          : out std_logic;
    FCMAPUDCDLDSTWD          : out std_logic;
    FCMAPUDCDLDSTDW          : out std_logic;
    FCMAPUDCDLDSTQW          : out std_logic;
    FCMAPUDCDTRAPLE          : out std_logic;
    FCMAPUDCDTRAPBE          : out std_logic;
    FCMAPUDCDFORCEBESTEERING : out std_logic;
    FCMAPUDCDFPUOP           : out std_logic;
    FCMAPUEXEBLOCKINGMCO     : out std_logic;
    FCMAPUEXENONBLOCKINGMCO  : out std_logic;
    FCMAPULOADWAIT           : out std_logic;
    FCMAPURESULTVALID        : out std_logic;
    FCMAPUXEROV              : out std_logic;
    FCMAPUXERCA              : out std_logic;
    FCMAPUCR                 : out std_logic_vector(0 to 3);
    FCMAPUEXCEPTION          : out std_logic
  );
  
end entity fcm_fpu_light;

-------------------------------------------------------------------------------
-- Architecture section
-------------------------------------------------------------------------------

architecture imp of fcm_fpu_light is
  
  component add_light
    generic (
      USE_INPUT_REG     : boolean;
      USE_1_2_INTERLOCK : boolean;
      USE_2_3_INTERLOCK : boolean;
      USE_3_4_INTERLOCK : boolean;
      USE_4_5_INTERLOCK : boolean;
      USE_5_6_INTERLOCK : boolean;
      USE_6_7_INTERLOCK : boolean;
      USE_7_8_INTERLOCK : boolean;
      USE_8_9_INTERLOCK : boolean;
      USE_OUTPUT_REG    : boolean);
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      op_a   : in  unsigned(31 downto 0);
      op_b   : in  unsigned(31 downto 0);
      op     : in  std_logic;
      valid  : out std_logic;
      output : out unsigned(31 downto 0));
  end component;

  component mult_light
    generic (
      USE_INPUT_REG     : boolean;
      USE_1_2_INTERLOCK : boolean;
      USE_2_3_INTERLOCK : boolean;
      USE_3_4_INTERLOCK : boolean;
      USE_4_5_INTERLOCK : boolean;
      USE_5_6_INTERLOCK : boolean;      
      USE_OUTPUT_REG    : boolean);
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      op_a   : in  unsigned(31 downto 0);
      op_b   : in  unsigned(31 downto 0);
      valid  : out std_logic;
      output : out unsigned(31 downto 0));
  end component;

  component div_light
    generic (
      DIVIDES_PER_CYCLE : integer;
      USE_3_4_INTERLOCK : boolean;
      USE_4_5_INTERLOCK : boolean;
      USE_OUTPUT_REG    : boolean); 
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      op_a   : in  unsigned(31 downto 0);
      op_b   : in  unsigned(31 downto 0);
      output : out unsigned(31 downto 0);
      valid  : out std_logic); 
  end component;

  component sqrt_light
    generic (
      ROOTS_PER_CYCLE   : integer;
      USE_INPUT_REG     : boolean;
      USE_2_3_INTERLOCK : boolean;
      USE_3_4_INTERLOCK : boolean;
      USE_OUTPUT_REG    : boolean);
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      input  : in  unsigned(31 downto 0);
      output : out unsigned(31 downto 0);
      valid  : out std_logic);
  end component;

  component ftoi_light
    generic (
      USE_INPUT_REG     : boolean;
      USE_1_2_INTERLOCK : boolean;
      USE_2_3_INTERLOCK : boolean;
      USE_OUTPUT_REG    : boolean);
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      s      : in  std_logic;
      input  : in  unsigned(31 downto 0);
      valid  : out std_logic;
      output : out unsigned(31 downto 0));
  end component;

  component itof_light
    generic (
      USE_INPUT_REG     : boolean;
      USE_1_2_INTERLOCK : boolean;
      USE_2_3_INTERLOCK : boolean;
      USE_3_4_INTERLOCK : boolean;
      USE_4_5_INTERLOCK : boolean;
      USE_OUTPUT_REG    : boolean);
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      s      : in  std_logic;
      input  : in  unsigned(31 downto 0);
      valid  : out std_logic;
      output : out unsigned(31 downto 0));
  end component;  
  
  component cmp_light
    generic (
      USE_INPUT_REG     : boolean;
      USE_1_2_INTERLOCK : boolean;
      USE_2_3_INTERLOCK : boolean;
      USE_OUTPUT_REG    : boolean);
    port (
      clk    : in  std_logic;
      reset  : in  std_logic;
      en     : in  std_logic;
      op_a   : in  unsigned(31 downto 0);
      op_b   : in  unsigned(31 downto 0);
      valid  : out std_logic;
      output : out signed(1 downto 0));
  end component;

  -- Operator constants
  constant OP_ADD  : std_logic_vector(0 to 2) := "000";
  constant OP_SUB  : std_logic_vector(0 to 2) := "001";
  constant OP_MULT : std_logic_vector(0 to 2) := "010";
  constant OP_DIV  : std_logic_vector(0 to 2) := "011";
  constant OP_SQRT : std_logic_vector(0 to 2) := "100";
  constant OP_FTOI : std_logic_vector(0 to 2) := "101";
  constant OP_ITOF : std_logic_vector(0 to 2) := "110";
  constant OP_CMP  : std_logic_vector(0 to 2) := "111";

  signal reset : std_logic;

  signal add_en, add_op, add_valid : std_logic;
  signal add_output : unsigned(31 downto 0);

  signal mult_en, mult_valid : std_logic;
  signal mult_output : unsigned(31 downto 0);

  signal div_en, div_valid : std_logic;
  signal div_output : unsigned(31 downto 0);

  signal sqrt_en, sqrt_valid : std_logic;
  signal sqrt_output : unsigned(31 downto 0);

  signal ftoi_en, ftoi_valid, ftoi_s : std_logic;
  signal ftoi_output : unsigned(31 downto 0);

  signal itof_en, itof_valid, itof_s : std_logic;
  signal itof_output : unsigned(31 downto 0);

  signal cmp_en, cmp_valid : std_logic;
  signal cmp_output : signed(1 downto 0);

  signal opcode       : std_logic_vector(0 to 2);
  signal instr_valid  : std_logic;
  signal udi_valid    : std_logic;
  signal op_valid     : std_logic;     -- Indicates operands are valid
  signal op_a, op_b   : std_logic_vector(0 to 31); -- Register operands
  signal op_sign         : std_logic;  -- Indicates signed operation
  signal fpu_enable   : std_logic;     -- Indicates operation should start
  signal output       : unsigned(31 downto 0);
  signal output_valid : std_logic;

begin
  -- Generate active high reset
  RESET_GEN: if C_EXT_RESET_HIGH = 1 generate
    reset <= sys_rst;
  end generate RESET_GEN;
  RESET_GEN_INV: if C_EXT_RESET_HIGH /= 1 generate
    reset <= not sys_rst;
  end generate RESET_GEN_INV;

  -- Instruction decode signals
  udi_valid <= APUFCMDECUDIVALID;
  instr_valid <= APUFCMINSTRVALID;
  op_valid <= APUFCMOPERANDVALID;

  -- Result enable signals
  FCMAPUDONE <= output_valid;
  FCMAPURESULTVALID <= output_valid;
  FCMAPURESULT <= std_logic_vector(output);

  -- Set FPU enables
  ENABLE_PROC : process(fpu_enable, opcode)
  begin
    add_en  <= '0';
    add_op  <= '0';
    mult_en <= '0';
    div_en  <= '0';
    sqrt_en <= '0';
    ftoi_en <= '0';
    itof_en <= '0';
    cmp_en  <= '0';

    if fpu_enable = '1' then
      case opcode is
        when OP_ADD  =>
          add_en <= '1';
          add_op <= '0';
        when OP_SUB  =>
          add_en <= '1';
          add_op <= '1';
        when OP_MULT =>
          mult_en <= '1';
        when OP_DIV  =>
          div_en <= '1';
        when OP_SQRT =>
          sqrt_en <= '1';
        when OP_FTOI =>
          ftoi_en <= '1';
        when OP_ITOF =>
          itof_en <= '1';
        when OP_CMP  =>
          cmp_en <= '1';
        when others  => null;
      end case;
    end if;
  end process;

  -- Register UDI operation
  UDI_REG : process(clk, reset, udi_valid)
  begin
    if clk'event and clk = '1' then
      if udi_valid = '1' then
        opcode <= APUFCMDECUDI;
      end if;
    end if;
  end process;

  -- Register parts of instruction
  INSTR_REG : process(clk, reset, instr_valid)
  begin
    if clk'event and clk = '1' then
      if instr_valid = '1' then
        op_sign <= APUFCMINSTRUCTION(20);
      end if;
    end if;
  end process;

  OPERAND_REG : process(clk, reset, op_valid)
  begin
    if clk'event and clk = '1' then
      if op_valid = '1' then
        op_a <= APUFCMRADATA;
        op_b <= APUFCMRBDATA;
        fpu_enable <= '1';
      else
        fpu_enable <= '0';
      end if;
    end if;
  end process;

  -- Output correct result to output, assert output enables
  RESULT_MUX: process (mult_valid, mult_output, div_valid, div_output,
                       add_valid, add_output, sqrt_valid, sqrt_output,
                       ftoi_valid, ftoi_output, itof_valid, itof_output,
                       cmp_valid, cmp_output)
  begin
    output <= (others => '-');
    output_valid <= '0';
    if mult_valid = '1' then
      if C_USE_MULT = 1 then
        output <= mult_output;
        output_valid <= '1';
      end if;
    elsif add_valid = '1' then
      if C_USE_ADD = 1 then
        output <= add_output;
        output_valid <= '1';
      end if;
    elsif div_valid = '1' then
      if C_USE_DIV = 1 then
        output <= div_output;
        output_valid <= '1';
      end if;
    elsif cmp_valid = '1' then
      if C_USE_CMP = 1 then
        output <= unsigned(resize(cmp_output, 32));
        output_valid <= '1';
      end if;
    elsif sqrt_valid = '1' then
      if C_USE_SQRT = 1 then
        output <= sqrt_output;
        output_valid <= '1';
      end if;
    elsif ftoi_valid = '1' then
      if C_USE_FTOI = 1 then
        output <= ftoi_output;
        output_valid <= '1';
      end if;
    elsif itof_valid = '1' then
      if C_USE_ITOF = 1 then
        output <= itof_output;
        output_valid <= '1';
      end if;
    end if;
  end process;

  -- Instantiate Arithmetic Units ---------------------------------------------
  
  GEN_ADD: if C_USE_ADD = 1 generate
    ADD_I : add_light
      generic map (
        USE_INPUT_REG     => false,
        USE_1_2_INTERLOCK => true,
        USE_2_3_INTERLOCK => false,
        USE_3_4_INTERLOCK => false,
        USE_4_5_INTERLOCK => true,
        USE_5_6_INTERLOCK => false,
        USE_6_7_INTERLOCK => true,
        USE_7_8_INTERLOCK => false,
        USE_8_9_INTERLOCK => true,
        USE_OUTPUT_REG    => false)
      port map (
        clk    => clk,
        reset  => reset,
        en     => add_en,
        op_a   => unsigned(op_a),
        op_b   => unsigned(op_b),
        op     => add_op,
        valid  => add_valid,
        output => add_output);
  end generate GEN_ADD;

  GEN_MULT: if C_USE_MULT = 1 generate
    MULT_I: mult_light
      generic map (
        USE_INPUT_REG     => true,
        USE_1_2_INTERLOCK => true,
        USE_2_3_INTERLOCK => true,
        USE_3_4_INTERLOCK => true,
        USE_4_5_INTERLOCK => true,
        USE_5_6_INTERLOCK => true,        
        USE_OUTPUT_REG    => true)
      port map (
        clk    => clk,
        reset  => reset,
        en     => mult_en,
        op_a   => unsigned(op_a),
        op_b   => unsigned(op_b),
        valid  => mult_valid,
        output => mult_output);
  end generate GEN_MULT;
  
  GEN_DIV: if C_USE_DIV = 1 generate
    DIV_I: div_light
      generic map (
        DIVIDES_PER_CYCLE => 2,
        USE_3_4_INTERLOCK => false,
        USE_4_5_INTERLOCK => true,
        USE_OUTPUT_REG    => false)
      port map (
        clk    => clk,
        reset  => reset,
        en     => div_en,
        op_a   => unsigned(op_a),
        op_b   => unsigned(op_b),
        output => div_output,
        valid  => div_valid);
  end generate GEN_DIV;

  GEN_SQRT: if C_USE_SQRT = 1 generate
    SQRT_I: sqrt_light
      generic map (
        ROOTS_PER_CYCLE   => 2,
        USE_INPUT_REG     => false,
        USE_2_3_INTERLOCK => true,
        USE_3_4_INTERLOCK => false,
        USE_OUTPUT_REG    => true)
      port map (
        clk    => clk,
        reset  => reset,
        en     => sqrt_en,
        input  => unsigned(op_a),
        output => sqrt_output,
        valid  => sqrt_valid);
  end generate GEN_SQRT;

  GEN_FTOI: if C_USE_FTOI = 1 generate
    FTOI_I: ftoi_light
      generic map (
          USE_INPUT_REG     => false,
          USE_1_2_INTERLOCK => true,
          USE_2_3_INTERLOCK => false,
          USE_OUTPUT_REG    => true)
      port map (
          clk    => clk,
          reset  => reset,
          en     => ftoi_en,
          s      => op_sign,
          input  => unsigned(op_a),
          valid  => ftoi_valid,
          output => ftoi_output);
  end generate GEN_FTOI;

  GEN_ITOF: if C_USE_ITOF = 1 generate
    ITOF_I: itof_light
      generic map (
          USE_INPUT_REG     => true,
          USE_1_2_INTERLOCK => false,
          USE_2_3_INTERLOCK => true,
          USE_3_4_INTERLOCK => false,
          USE_4_5_INTERLOCK => true,
          USE_OUTPUT_REG    => false)
      port map (
          clk    => clk,
          reset  => reset,
          en     => itof_en,
          s      => op_sign,
          input  => unsigned(op_a),
          valid  => itof_valid,
          output => itof_output);
  end generate GEN_ITOF;

  GEN_CMP: if C_USE_CMP = 1 generate
    CMP_I: cmp_light
      generic map (
        USE_INPUT_REG     => false,
        USE_1_2_INTERLOCK => true,
        USE_2_3_INTERLOCK => true,
        USE_OUTPUT_REG    => false)
      port map (
        clk    => clk,
        reset  => reset,
        en     => cmp_en,
        op_a   => unsigned(op_a),
        op_b   => unsigned(op_b),
        valid  => cmp_valid,
        output => cmp_output);
  end generate GEN_CMP;
  

  -- Unused Output Signals ----------------------------------------------------

  FCMAPULOADWAIT <= '0';
  FCMAPUSLEEPNOTREADY <= '0';
  FCMAPUINSTRACK <= '0';
  FCMAPUDECODEBUSY <= '0';
  FCMAPUDCDGPRWRITE <= '0';
  FCMAPUDCDRAEN <= '0';
  FCMAPUDCDRBEN <= '0';
  FCMAPUDCDPRIVOP <= '0';
  FCMAPUDCDFORCEALIGN <= '0';
  FCMAPUDCDXEROVEN <= '0';
  FCMAPUDCDXERCAEN <= '0';
  FCMAPUDCDCREN <= '0';
  FCMAPUEXECRFIELD <= "000";
  FCMAPUDCDLOAD <= '0';
  FCMAPUDCDSTORE <= '0';
  FCMAPUDCDUPDATE <= '0';
  FCMAPUDCDLDSTBYTE <= '0';
  FCMAPUDCDLDSTHW <= '0';
  FCMAPUDCDLDSTWD <= '0';
  FCMAPUDCDLDSTDW <= '0';
  FCMAPUDCDLDSTQW <= '0';
  FCMAPUDCDTRAPLE <= '0';
  FCMAPUDCDTRAPBE <= '0';
  FCMAPUDCDFORCEBESTEERING <= '0';
  FCMAPUDCDFPUOP <= '0';
  FCMAPUEXEBLOCKINGMCO <= '0';
  FCMAPUEXENONBLOCKINGMCO <= '0';
  FCMAPUXEROV <= '0';
  FCMAPUXERCA <= '0';
  FCMAPUCR <= "0000";
  FCMAPUEXCEPTION <= '0';
  
end architecture imp;
