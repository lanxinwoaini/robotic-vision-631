------------------------------------------------------------------------------
-- plb_usb.vhd - entity/architecture pair
------------------------------------------------------------------------------
-- IMPORTANT:
-- DO NOT MODIFY THIS FILE EXCEPT IN THE DESIGNATED SECTIONS.
--
-- SEARCH FOR --USER TO DETERMINE WHERE CHANGES ARE ALLOWED.
--
-- TYPICALLY, THE ONLY ACCEPTABLE CHANGES INVOLVE ADDING NEW
-- PORTS AND GENERICS THAT GET PASSED THROUGH TO THE INSTANTIATION
-- OF THE USER_LOGIC ENTITY.
------------------------------------------------------------------------------
--
-- ***************************************************************************
-- ** Copyright (c) 1995-2005 Xilinx, Inc.  All rights reserved.            **
-- **                                                                       **
-- ** Xilinx, Inc.                                                          **
-- ** XILINX IS PROVIDING THIS DESIGN, CODE, OR INFORMATION "AS IS"         **
-- ** AS A COURTESY TO YOU, SOLELY FOR USE IN DEVELOPING PROGRAMS AND       **
-- ** SOLUTIONS FOR XILINX DEVICES.  BY PROVIDING THIS DESIGN, CODE,        **
-- ** OR INFORMATION AS ONE POSSIBLE IMPLEMENTATION OF THIS FEATURE,        **
-- ** APPLICATION OR STANDARD, XILINX IS MAKING NO REPRESENTATION           **
-- ** THAT THIS IMPLEMENTATION IS FREE FROM ANY CLAIMS OF INFRINGEMENT,     **
-- ** AND YOU ARE RESPONSIBLE FOR OBTAINING ANY RIGHTS YOU MAY REQUIRE      **
-- ** FOR YOUR IMPLEMENTATION.  XILINX EXPRESSLY DISCLAIMS ANY              **
-- ** WARRANTY WHATSOEVER WITH RESPECT TO THE ADEQUACY OF THE               **
-- ** IMPLEMENTATION, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OR        **
-- ** REPRESENTATIONS THAT THIS IMPLEMENTATION IS FREE FROM CLAIMS OF       **
-- ** INFRINGEMENT, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS       **
-- ** FOR A PARTICULAR PURPOSE.                                             **
-- **                                                                       **
-- ** YOU MAY COPY AND MODIFY THESE FILES FOR YOUR OWN INTERNAL USE SOLELY  **
-- ** WITH XILINX PROGRAMMABLE LOGIC DEVICES AND XILINX EDK SYSTEM OR       **
-- ** CREATE IP MODULES SOLELY FOR XILINX PROGRAMMABLE LOGIC DEVICES AND    **
-- ** XILINX EDK SYSTEM. NO RIGHTS ARE GRANTED TO DISTRIBUTE ANY FILES      **
-- ** UNLESS THEY ARE DISTRIBUTED IN XILINX PROGRAMMABLE LOGIC DEVICES.     **
-- **                                                                       **
-- ***************************************************************************
--
------------------------------------------------------------------------------
-- Filename:          plb_usb.vhd
-- Version:           1.00.a
-- Description:       Top level design, instantiates IPIF and user logic.
-- Date:              Mon Feb 27 08:07:33 2006 (by Create and Import Peripheral Wizard)
-- VHDL Standard:     VHDL'93
------------------------------------------------------------------------------
-- Naming Conventions:
--   active low signals:                    "*_n"
--   clock signals:                         "clk", "clk_div#", "clk_#x"
--   reset signals:                         "rst", "rst_n"
--   generics:                              "C_*"
--   user defined types:                    "*_TYPE"
--   state machine next state:              "*_ns"
--   state machine current state:           "*_cs"
--   combinatorial signals:                 "*_com"
--   pipelined or register delay signals:   "*_d#"
--   counter signals:                       "*cnt*"
--   clock enable signals:                  "*_ce"
--   internal version of output port:       "*_i"
--   device pins:                           "*_pin"
--   ports:                                 "- Names begin with Uppercase"
--   processes:                             "*_PROCESS"
--   component instantiations:              "<ENTITY_>I_<#|FUNC>"
------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;

library proc_common_v1_00_b;
use proc_common_v1_00_b.proc_common_pkg.all;

library ipif_common_v1_00_e;
use ipif_common_v1_00_e.ipif_pkg.all;

library plb_ipif_v2_01_a;
use plb_ipif_v2_01_a.all;

library plb_usb_v1_00_d;
use plb_usb_v1_00_d.all;

------------------------------------------------------------------------------
-- Entity section
------------------------------------------------------------------------------
-- Definition of Generics:
--   C_BASEADDR                   -- User logic base address
--   C_HIGHADDR                   -- User logic high address
--   C_PLB_AWIDTH                 -- PLB address bus width
--   C_PLB_DWIDTH                 -- PLB address data width
--   C_PLB_NUM_MASTERS            -- Number of PLB masters
--   C_PLB_MID_WIDTH              -- log2(C_PLB_NUM_MASTERS)
--   C_FAMILY                     -- Target FPGA architecture
--
-- Definition of Ports:
--   PLB_Clk                      -- PLB Clock
--   PLB_Rst                      -- PLB Reset
--   Sl_addrAck                   -- Slave address acknowledge
--   Sl_MBusy                     -- Slave busy indicator
--   Sl_MErr                      -- Slave error indicator
--   Sl_rdBTerm                   -- Slave terminate read burst transfer
--   Sl_rdComp                    -- Slave read transfer complete indicator
--   Sl_rdDAck                    -- Slave read data acknowledge
--   Sl_rdDBus                    -- Slave read data bus
--   Sl_rdWdAddr                  -- Slave read word address
--   Sl_rearbitrate               -- Slave re-arbitrate bus indicator
--   Sl_SSize                     -- Slave data bus size
--   Sl_wait                      -- Slave wait indicator
--   Sl_wrBTerm                   -- Slave terminate write burst transfer
--   Sl_wrComp                    -- Slave write transfer complete indicator
--   Sl_wrDAck                    -- Slave write data acknowledge
--   PLB_abort                    -- PLB abort request indicator
--   PLB_ABus                     -- PLB address bus
--   PLB_BE                       -- PLB byte enables
--   PLB_busLock                  -- PLB bus lock
--   PLB_compress                 -- PLB compressed data transfer indicator
--   PLB_guarded                  -- PLB guarded transfer indicator
--   PLB_lockErr                  -- PLB lock error indicator
--   PLB_masterID                 -- PLB current master identifier
--   PLB_MSize                    -- PLB master data bus size
--   PLB_ordered                  -- PLB synchronize transfer indicator
--   PLB_PAValid                  -- PLB primary address valid indicator
--   PLB_pendPri                  -- PLB pending request priority
--   PLB_pendReq                  -- PLB pending bus request indicator
--   PLB_rdBurst                  -- PLB burst read transfer indicator
--   PLB_rdPrim                   -- PLB secondary to primary read request indicator
--   PLB_reqPri                   -- PLB current request priority
--   PLB_RNW                      -- PLB read/not write
--   PLB_SAValid                  -- PLB secondary address valid indicator
--   PLB_size                     -- PLB transfer size
--   PLB_type                     -- PLB transfer type
--   PLB_wrBurst                  -- PLB burst write transfer indicator
--   PLB_wrDBus                   -- PLB write data bus
--   PLB_wrPrim                   -- PLB secondary to primary write request indicator
------------------------------------------------------------------------------

entity plb_usb is
  generic
  (
    -- ADD USER GENERICS BELOW THIS LINE --------------- 
    C_USB_DATA_WIDTH               : integer              := 16;
    -- ADD USER GENERICS ABOVE THIS LINE ---------------

    -- DO NOT EDIT BELOW THIS LINE ---------------------
    -- Bus protocol parameters, do not add to or delete
    C_BASEADDR                     : std_logic_vector     := X"00000000";
    C_HIGHADDR                     : std_logic_vector     := X"FFFFFFFF";
    C_PLB_AWIDTH                   : integer              := 32;
    C_PLB_DWIDTH                   : integer              := 64;
    C_PLB_NUM_MASTERS              : integer              := 8;
    C_PLB_MID_WIDTH                : integer              := 3;
    C_FAMILY                       : string               := "virtex2p"
    -- DO NOT EDIT ABOVE THIS LINE ---------------------
  );
  port
  (
    -- ADD USER PORTS BELOW THIS LINE ------------------
    dcm_locked  : in  std_logic;
    dcm_reset   : out std_logic;
    
    if_clk      : in  std_logic;
    
    usb_full_n  : in  std_logic;      -- IN FIFO full flag
    usb_empty_n : in  std_logic;      -- OUT FIFO empty flag
    usb_alive   : in  std_logic;      -- USB chip GPIO, high when device active

    Interrupt : out std_logic; 
    sloe_n   : out std_logic;         -- USB FIFO slave operate enable
    slrd_n   : out std_logic;         -- USB FIFO slave read enable
    slwr_n   : out std_logic;         -- USB FIFO slave write enable

    pktend_n : out std_logic;                        -- USB FIFO packet end
    fifoaddr : out std_logic_vector(1 downto 0);     -- USB FIFO address

    fd_I : in  std_logic_vector(C_USB_DATA_WIDTH-1 downto 0); -- USB FIFO data
    fd_O : out std_logic_vector(C_USB_DATA_WIDTH-1 downto 0); -- USB FIFO data
    fd_T : out std_logic_vector(C_USB_DATA_WIDTH-1 downto 0); -- USB FIFO data
    -- ADD USER PORTS ABOVE THIS LINE ------------------

    -- DO NOT EDIT BELOW THIS LINE ---------------------
    -- Bus protocol ports, do not add to or delete
    PLB_Clk                        : in  std_logic;
    PLB_Rst                        : in  std_logic;
    Sl_addrAck                     : out std_logic;
    Sl_MBusy                       : out std_logic_vector(0 to C_PLB_NUM_MASTERS-1);
    Sl_MErr                        : out std_logic_vector(0 to C_PLB_NUM_MASTERS-1);
    Sl_rdBTerm                     : out std_logic;
    Sl_rdComp                      : out std_logic;
    Sl_rdDAck                      : out std_logic;
    Sl_rdDBus                      : out std_logic_vector(0 to C_PLB_DWIDTH-1);
    Sl_rdWdAddr                    : out std_logic_vector(0 to 3);
    Sl_rearbitrate                 : out std_logic;
    Sl_SSize                       : out std_logic_vector(0 to 1);
    Sl_wait                        : out std_logic;
    Sl_wrBTerm                     : out std_logic;
    Sl_wrComp                      : out std_logic;
    Sl_wrDAck                      : out std_logic;
    PLB_abort                      : in  std_logic;
    PLB_ABus                       : in  std_logic_vector(0 to C_PLB_AWIDTH-1);
    PLB_BE                         : in  std_logic_vector(0 to C_PLB_DWIDTH/8-1);
    PLB_busLock                    : in  std_logic;
    PLB_compress                   : in  std_logic;
    PLB_guarded                    : in  std_logic;
    PLB_lockErr                    : in  std_logic;
    PLB_masterID                   : in  std_logic_vector(0 to C_PLB_MID_WIDTH-1);
    PLB_MSize                      : in  std_logic_vector(0 to 1);
    PLB_ordered                    : in  std_logic;
    PLB_PAValid                    : in  std_logic;
    PLB_pendPri                    : in  std_logic_vector(0 to 1);
    PLB_pendReq                    : in  std_logic;
    PLB_rdBurst                    : in  std_logic;
    PLB_rdPrim                     : in  std_logic;
    PLB_reqPri                     : in  std_logic_vector(0 to 1);
    PLB_RNW                        : in  std_logic;
    PLB_SAValid                    : in  std_logic;
    PLB_size                       : in  std_logic_vector(0 to 3);
    PLB_type                       : in  std_logic_vector(0 to 2);
    PLB_wrBurst                    : in  std_logic;
    PLB_wrDBus                     : in  std_logic_vector(0 to C_PLB_DWIDTH-1);
    PLB_wrPrim                     : in  std_logic;

    -- below signals are added from other core
    M_abort                        : out std_logic;
    M_ABus                         : out std_logic_vector(0 to C_PLB_AWIDTH-1);
    M_BE                           : out std_logic_vector(0 to C_PLB_DWIDTH/8-1);
    M_busLock                      : out std_logic;
    M_compress                     : out std_logic;
    M_guarded                      : out std_logic;
    M_lockErr                      : out std_logic;
    M_MSize                        : out std_logic_vector(0 to 1);
    M_ordered                      : out std_logic;
    M_priority                     : out std_logic_vector(0 to 1);
    M_rdBurst                      : out std_logic;
    M_request                      : out std_logic;
    M_RNW                          : out std_logic;
    M_size                         : out std_logic_vector(0 to 3);
    M_type                         : out std_logic_vector(0 to 2);
    M_wrBurst                      : out std_logic;
    M_wrDBus                       : out std_logic_vector(0 to C_PLB_DWIDTH-1);
    PLB_MBusy                      : in  std_logic;
    PLB_MErr                       : in  std_logic;
    PLB_MWrBTerm                   : in  std_logic;
    PLB_MWrDAck                    : in  std_logic;
    PLB_MAddrAck                   : in  std_logic;
    PLB_MRdBTerm                   : in  std_logic;
    PLB_MRdDAck                    : in  std_logic;
    PLB_MRdDBus                    : in  std_logic_vector(0 to (C_PLB_DWIDTH-1));
    PLB_MRdWdAddr                  : in  std_logic_vector(0 to 3);
    PLB_MRearbitrate               : in  std_logic;
    PLB_MSSize                     : in  std_logic_vector(0 to 1)
    -- DO NOT EDIT ABOVE THIS LINE ---------------------
  );

  attribute SIGIS : string;
  attribute SIGIS of PLB_Clk       : signal is "Clk";
  attribute SIGIS of PLB_Rst       : signal is "Rst";

end entity plb_usb;

------------------------------------------------------------------------------
-- Architecture section
------------------------------------------------------------------------------

architecture IMP of plb_usb is

    ------------------------------------------
  -- Constant: array of address range identifiers
  ------------------------------------------
--Wade
--   constant ARD_ID_ARRAY                   : INTEGER_ARRAY_TYPE   := 
--     (
--       0  => USER_00                           -- user logic address space
--     );
  constant ARD_ID_ARRAY         : INTEGER_ARRAY_TYPE   := 
    (
      --IPIF_INTR,         -- ipif interrupt (pre-defined keyword)
      0 => USER_00             -- user ID (pre-defined keyword)
      --USER_01,           -- user ID (pre-defined keyword) 
      --USER_02,           -- user ID (pre-defined keyword) 
      --IPIF_DMA_SG,          -- ipif DMA/SG 
      --IPIF_RST,          -- ipif reset (pre-defined keyword)
      --IPIF_WRFIFO_REG,   -- ipif wrfifo registers (pre-defined keyword)
      --IPIF_WRFIFO_DATA,  -- ipif wrfifo data (pre-defined keyword)
      --IPIF_RDFIFO_REG,   -- ipif rdfifo registers (pre-defined keyword)
      --IPIF_RDFIFO_DATA,  -- ipif rdfifo data (pre-defined keyword)
      --IPIF_SESR_SEAR     -- IPIF SESR/SEAR Registers
      );
  
  ------------------------------------------
  -- Constant: array of address pairs for each address range
  ------------------------------------------
  constant ZERO_ADDR_PAD                  : std_logic_vector(0 to 64-C_PLB_AWIDTH-1) := (others => '0');

  constant USER_BASEADDR  : std_logic_vector  := C_BASEADDR;
  constant USER_HIGHADDR  : std_logic_vector  := C_HIGHADDR;

  constant ARD_ADDR_RANGE_ARRAY           : SLV64_ARRAY_TYPE     := 
    (
      ZERO_ADDR_PAD & USER_BASEADDR,              -- user logic base address
      ZERO_ADDR_PAD & USER_HIGHADDR
    );

  ------------------------------------------
  -- Constant: array of data widths for each target address range
  ------------------------------------------
  constant USER_DWIDTH                    : integer              := C_PLB_DWIDTH;

--   constant ARD_DWIDTH_ARRAY               : INTEGER_ARRAY_TYPE   := 
--     (
--       0  => USER_DWIDTH                       -- user logic data width
--     );
  constant ARD_DWIDTH_ARRAY     : INTEGER_ARRAY_TYPE :=
    -- This array specifies the data bus width of the memory address
    -- range specified for the cooresponding baseaddr pair.
    (
      0 => 64   -- User0 data width
    );
  
  ------------------------------------------
  -- Constant: array of desired number of chip enables for each address range
  ------------------------------------------
  constant USER_NUM_CE                    : integer              := 2;

 --  constant ARD_NUM_CE_ARRAY               : INTEGER_ARRAY_TYPE   := 
--     (
--       0  => pad_power2(USER_NUM_CE)           -- user logic number of CEs
--     );
  constant ARD_NUM_CE_ARRAY   : INTEGER_ARRAY_TYPE :=
    -- This array spcifies the number of Chip Enables (CE) that is 
    -- required by the cooresponding baseaddr pair.
    (
      0 => 1    -- User0 CE Number      
      );
  

  ------------------------------------------
  -- Constant: user core ID code
  ------------------------------------------
  constant DEV_BLK_ID                     : integer              := 0;

  ------------------------------------------
  -- Constant: enable MIR/Reset register
  ------------------------------------------
  constant DEV_MIR_ENABLE                 : integer              := 1;

  ------------------------------------------
  -- Constant: enable burst support
  ------------------------------------------
  constant DEV_BURST_ENABLE               : integer              := 1;

  ------------------------------------------
  -- Constant: max burst size in bytes - reserved
  ------------------------------------------
  constant DEV_MAX_BURST_SIZE             : integer              := 128;

  ------------------------------------------
  -- Constant: size of the largest target burstable memory space in bytes - reserved
  ------------------------------------------
  constant DEV_BURST_PAGE_SIZE            : integer              := 1024;

  ------------------------------------------
  -- Constant: dataphase timeout value for IPIF
  ------------------------------------------
  constant DEV_DPHASE_TIMEOUT             : integer              := 64;

  ------------------------------------------
  -- Constant: include device interrupt source controller
  ------------------------------------------
  constant INCLUDE_DEV_ISC                : integer              := 0;

  ------------------------------------------
  -- Constant: include IPIF ISC priority encoder
  ------------------------------------------
  constant INCLUDE_DEV_PENCODER           : integer              := 0;

  ------------------------------------------
  -- Constant: array of IP interrupt mode
  -- 1 = Level Pass through (non-inverted)
  -- 2 = Level Pass through (invert input)
  -- 3 = Registered Event (non-inverted)
  -- 4 = Registered Event (inverted input)
  -- 5 = Rising Edge Detect
  -- 6 = Falling Edge Detect
  ------------------------------------------
  constant IP_INTR_MODE_ARRAY             : INTEGER_ARRAY_TYPE   := 
    (
      0  => 0  -- not used
    );

  ------------------------------------------
  -- Constant: include PLB master service - reserved
  ------------------------------------------
  constant IP_MASTER_PRESENT              : integer              := 1;


  constant DMA_CHAN_TYPE_ARRAY : INTEGER_ARRAY_TYPE :=       
    -- One entry in the array for each channel, encoded as
    --    0 = simple DMA,  1 = simple sg,  2 = pkt tx SG,  3 = pkt rx SG
    (
      0 => 0 
    );
  
  ------------------------------------------
  -- Constant: write FIFO depth
  ------------------------------------------
  constant WRFIFO_DEPTH                   : integer              := 512;

  ------------------------------------------
  -- Constant: include write FIFO packet mode service
  ------------------------------------------
  constant WRFIFO_INCLUDE_PACKET_MODE     : boolean              := false;

  ------------------------------------------
  -- Constant: include write FIFO vacancy status
  ------------------------------------------
  constant WRFIFO_INCLUDE_VACANCY         : boolean              := true;

  ------------------------------------------
  -- Constant: read FIFO depth
  ------------------------------------------
  constant RDFIFO_DEPTH                   : integer              := 512;

  ------------------------------------------
  -- Constant: include read FIFO packet mode service
  ------------------------------------------
  constant RDFIFO_INCLUDE_PACKET_MODE     : boolean              := false;

  ------------------------------------------
  -- Constant: include read FIFO vacancy status
  ------------------------------------------
  constant RDFIFO_INCLUDE_VACANCY         : boolean              := true;

  ------------------------------------------
  -- Constant: PLB clock period in ps - reserved
  ------------------------------------------
  constant PLB_CLK_PERIOD_PS              : integer              := 10000;

  ------------------------------------------
  -- Constant: index for CS/CE
  ------------------------------------------
  --constant USER00_CS_INDEX                : integer              := get_id_index(ARD_ID_ARRAY, USER_00);

  --constant USER00_CE_INDEX                : integer              := calc_start_ce_index(ARD_NUM_CE_ARRAY, USER00_CS_INDEX);

 
  ------------------------------------------
  -- IP Interconnect (IPIC) signal declarations -- do not delete
  -- prefix 'i' stands for IPIF while prefix 'u' stands for user logic
  -- typically user logic will be hooked up to IPIF directly via i<sig>
  -- unless signal slicing and muxing are needed via u<sig>
  ------------------------------------------
  signal ZERO_PLB_MSSize        : std_logic_vector(0 to 1)   := (others => '0'); -- work around for XST not taking (others => '0') in port mapping
  signal ZERO_PLB_MRdDBus       : std_logic_vector(0 to 63)  := (others => '0'); -- work around for XST not taking (others => '0') 
  signal ZERO_PLB_MRdWdAddr     : std_logic_vector(0 to 3)   := (others => '0'); -- work around for XST not taking (others => '0') in port mapping

  signal iIP2Bus_MstRdReq       : std_logic;
  signal iIP2Bus_MstWrReq       : std_logic;
  signal iIP2Bus_MstBurst       : std_logic;
  signal iIP2Bus_MstBusLock     : std_logic;
  signal iIP2Bus_Addr           : std_logic_vector(0 to 31);
  signal iIP2Bus_MstBE          : std_logic_vector(0 to 7);
  signal iIP2IP_Addr            : std_logic_vector(0 to 31);
  signal iBus2IP_MstLastAck     : std_logic;
  signal iBus2IP_Burst          : std_logic;
  signal iIP2Bus_MstNum         : std_logic_vector(0 to 4);
  

  signal iBus2IP_Clk            : std_logic;
  signal iBus2IP_Reset          : std_logic;
  signal ZERO_IP2Bus_IntrEvent  : std_logic_vector(0 to IP_INTR_MODE_ARRAY'length - 1)   := (others => '0'); -- work around for XST not taking (others =>
  signal iIP2Bus_Data           : std_logic_vector(0 to 63)  := (others => '0');
  signal iIP2Bus_WrAck          : std_logic   := '0';
  signal iIP2Bus_RdAck          : std_logic   := '0';
  signal iIP2Bus_Retry          : std_logic   := '0';
  signal iIP2Bus_Error          : std_logic   := '0';
  signal iIP2Bus_ToutSup        : std_logic   := '0';
  signal iIP2Bus_Busy           : std_logic   := '0';
  signal iBus2IP_Addr           : std_logic_vector(0 to 31);
  signal iBus2IP_Data           : std_logic_vector(0 to 63);
  signal iBus2IP_BE             : std_logic_vector(0 to 7);
  signal iBus2IP_WrReq          : std_logic;
  signal iBus2IP_RdReq          : std_logic;
--signal iBus2IP_RdCE           : std_logic_vector(0 to calc_num_ce(ARD_NUM_CE_ARRAY)-1);
--signal iBus2IP_WrCE           : std_logic_vector(0 to calc_num_ce(ARD_NUM_CE_ARRAY)-1);
--signal ZERO_IP2RFIFO_Data     : std_logic_vector(0 to find_id_dwidth(ARD_ID_ARRAY,ARD_DWIDTH_ARRAY,IPIF_RDFIFO_DATA,C_PLB_DWIDTH)-1):= (others => '0');
  signal iBus2IP_RdCE           : std_logic_vector(0 to USER_NUM_CE-1);
  signal iBus2IP_WrCE           : std_logic_vector(0 to USER_NUM_CE-1);  
  signal ZERO_IP2RFIFO_Data     : std_logic_vector(0 to 63) := (others => '0');
  signal uBus2IP_Data           : std_logic_vector(0 to 63);
  signal uBus2IP_BE             : std_logic_vector(0 to 7);
  signal uBus2IP_RdCE           : std_logic_vector(0 to USER_NUM_CE-1);
  signal uBus2IP_WrCE           : std_logic_vector(0 to USER_NUM_CE-1);
  signal uIP2Bus_Data           : std_logic_vector(0 to 63);


begin

  ------------------------------------------
  -- instantiate the PLB IPIF
  ------------------------------------------


  
  PLB_IPIF_I : entity plb_ipif_v2_01_a.plb_ipif
    generic map
    (
      C_ARD_ID_ARRAY                 => ARD_ID_ARRAY,
      C_ARD_ADDR_RANGE_ARRAY         => ARD_ADDR_RANGE_ARRAY,
      C_ARD_DWIDTH_ARRAY             => ARD_DWIDTH_ARRAY,
      C_ARD_NUM_CE_ARRAY             => ARD_NUM_CE_ARRAY,
      
       --C_ARD_DEPENDENT_PROPS_ARRAY    => ARD_DEPENDENT_PROPS_ARRAY, -- added with Version 2.01a
       --C_ARD_DTIME_READ_ARRAY         => ARD_DTIME_READ_ARRAY, -- added with Version 2.01a
       --C_ARD_DTIME_WRITE_ARRAY        => ARD_DTIME_WRITE_ARRAY, -- added with Version 2.01a
      
      C_DEV_BLK_ID                   => DEV_BLK_ID,
      C_DEV_MIR_ENABLE               => DEV_MIR_ENABLE,
      C_DEV_BURST_ENABLE             => DEV_BURST_ENABLE,
      C_DEV_MAX_BURST_SIZE           => DEV_MAX_BURST_SIZE,
      C_DEV_BURST_PAGE_SIZE          => DEV_BURST_PAGE_SIZE,
      C_DEV_DPHASE_TIMEOUT           => DEV_DPHASE_TIMEOUT,
      
      C_INCLUDE_DEV_ISC              => INCLUDE_DEV_ISC,
      C_INCLUDE_DEV_PENCODER         => INCLUDE_DEV_PENCODER,
      C_IP_INTR_MODE_ARRAY           => IP_INTR_MODE_ARRAY,
      C_IP_MASTER_PRESENT            => IP_MASTER_PRESENT,
      
--      C_DMA_CHAN_TYPE_ARRAY          => DMA_CHAN_TYPE_ARRAY, -- added with Version 2.01a
--      C_DMA_LENGTH_WIDTH_ARRAY       => -- added with Version 2.01a 
--      C_DMA_PKT_LEN_FIFO_ADDR_ARRAY  => -- added with Version 2.01a
--      C_DMA_PKT_STAT_FIFO_ADDR_ARRAY => -- added with Version 2.01a
--      C_DMA_INTR_COALESCE_ARRAY      => -- added with Version 2.01a
--      C_DMA_ALLOW_BURST              => -- added with Version 2.01a
--      C_DMA_PACKET_WAIT_UNIT_NS      => -- added with Version 2.01a
      
      C_PLB_MID_WIDTH                => C_PLB_MID_WIDTH,
      C_PLB_NUM_MASTERS              => C_PLB_NUM_MASTERS,
      C_PLB_AWIDTH                   => C_PLB_AWIDTH,
      C_PLB_DWIDTH                   => C_PLB_DWIDTH,
      C_PLB_CLK_PERIOD_PS            => PLB_CLK_PERIOD_PS,
      C_IPIF_DWIDTH                  => C_PLB_DWIDTH,
      C_IPIF_AWIDTH                  => C_PLB_AWIDTH,
      C_FAMILY                       => C_FAMILY
    )
    port map
    (
      PLB_clk                        => PLB_Clk,
      Reset                          => PLB_Rst,
      Freeze                         => '0',
      IP2INTC_Irpt                   => open,
      PLB_ABus                       => PLB_ABus,
      PLB_PAValid                    => PLB_PAValid,
      PLB_SAValid                    => PLB_SAValid,
      PLB_rdPrim                     => PLB_rdPrim,
      PLB_wrPrim                     => PLB_wrPrim,
      PLB_masterID                   => PLB_masterID,
      PLB_abort                      => PLB_abort,
      PLB_busLock                    => PLB_busLock,
      PLB_RNW                        => PLB_RNW,
      PLB_BE                         => PLB_BE,
      PLB_MSize                      => PLB_MSize,
      PLB_size                       => PLB_size,
      PLB_type                       => PLB_type,
      PLB_compress                   => PLB_compress,
      PLB_guarded                    => PLB_guarded,
      PLB_ordered                    => PLB_ordered,
      PLB_lockErr                    => PLB_lockErr,
      PLB_wrDBus                     => PLB_wrDBus,
      PLB_wrBurst                    => PLB_wrBurst,
      PLB_rdBurst                    => PLB_rdBurst,
      PLB_pendReq                    => PLB_pendReq,
      PLB_pendPri                    => PLB_pendPri,
      PLB_reqPri                     => PLB_reqPri,
      Sl_addrAck                     => Sl_addrAck,
      Sl_SSize                       => Sl_SSize,
      Sl_wait                        => Sl_wait,
      Sl_rearbitrate                 => Sl_rearbitrate,
      Sl_wrDAck                      => Sl_wrDAck,
      Sl_wrComp                      => Sl_wrComp,
      Sl_wrBTerm                     => Sl_wrBTerm,
      Sl_rdDBus                      => Sl_rdDBus,
      Sl_rdWdAddr                    => Sl_rdWdAddr,
      Sl_rdDAck                      => Sl_rdDAck,
      Sl_rdComp                      => Sl_rdComp,
      Sl_rdBTerm                     => Sl_rdBTerm,
      Sl_MBusy                       => Sl_MBusy,
      Sl_MErr                        => Sl_MErr,
      PLB_MAddrAck                   => PLB_MAddrAck,
      PLB_MSSize                     => PLB_MSSize,
      PLB_MRearbitrate               => PLB_MRearbitrate,
      PLB_MBusy                      => PLB_MBusy,
      PLB_MErr                       => PLB_MErr,
      PLB_MWrDAck                    => PLB_MWrDAck,
      PLB_MRdDBus                    => PLB_MRdDBus,
      PLB_MRdWdAddr                  => PLB_MRdWdAddr,
      PLB_MRdDAck                    => PLB_MRdDAck,
      PLB_MRdBTerm                   => PLB_MRdBTerm,
      PLB_MWrBTerm                   => PLB_MWrBTerm,
      M_request                      => M_request,
      M_priority                     => M_priority,
      M_busLock                      => M_busLock,
      M_RNW                          => M_RNW,
      M_BE                           => M_BE,
      M_MSize                        => M_MSize,
      M_size                         => M_size,
      M_type                         => M_type,
      M_compress                     => M_compress,
      M_guarded                      => M_guarded,
      M_ordered                      => M_ordered,
      M_lockErr                      => M_lockErr,
      M_abort                        => M_abort,
      M_ABus                         => M_ABus,
      M_wrDBus                       => M_wrDBus,
      M_wrBurst                      => M_wrBurst,
      M_rdBurst                      => M_rdBurst,
      IP2Bus_Clk                     => '0',
      Bus2IP_Clk                     => iBus2IP_Clk,
      Bus2IP_Reset                   => iBus2IP_Reset,
      Bus2IP_Freeze                  => open,
      IP2Bus_IntrEvent               => ZERO_IP2Bus_IntrEvent,
      IP2Bus_Data                    => iIP2Bus_Data,
      IP2Bus_WrAck                   => iIP2Bus_WrAck,
      IP2Bus_RdAck                   => iIP2Bus_RdAck,
      IP2Bus_Retry                   => iIP2Bus_Retry,
      IP2Bus_Error                   => iIP2Bus_Error,
      IP2Bus_ToutSup                 => iIP2Bus_ToutSup,
      IP2Bus_PostedWrInh             => '0',
      --IP2Bus_Busy                    => iIP2Bus_Busy, --removed from V2.01a
      --IP2Bus_AddrAck                 => '0',          --removed from V2.01a
      --IP2Bus_BTerm                   => '0',          --removed from V2.01a
      Bus2IP_Addr                    => iBus2IP_Addr,
      Bus2IP_Data                    => iBus2IP_Data,
      Bus2IP_RNW                     => open,
      Bus2IP_BE                      => iBus2IP_BE,
      Bus2IP_Burst                   => iBus2IP_Burst,
      --Bus2IP_IBurst                  => open,         --removed from V2.01a
      Bus2IP_WrReq                   => iBus2IP_WrReq,
      Bus2IP_RdReq                   => iBus2IP_RdReq,

      --Bus2IP_RNW_Early               => open,         --removed from V2.01a
      --Bus2IP_PselHit                 => open,         --removed from V2.01a
      Bus2IP_CS                      => open,
      Bus2IP_CE                      => open,
      Bus2IP_RdCE                    => iBus2IP_RdCE,
      Bus2IP_WrCE                    => iBus2IP_WrCE,
      IP2DMA_RxLength_Empty          => '0',
      IP2DMA_RxStatus_Empty          => '0',
      IP2DMA_TxLength_Full           => '0',
      IP2DMA_TxStatus_Empty          => '0',

      IP2Bus_Addr                    => iIP2Bus_Addr,
      IP2Bus_MstBE                   => iIP2Bus_MstBE,
      IP2IP_Addr                     => iIP2IP_Addr,
      IP2Bus_MstWrReq                => iIP2Bus_MstWrReq,
      IP2Bus_MstRdReq                => iIP2Bus_MstRdReq,
      IP2Bus_MstBurst                => iIP2Bus_MstBurst,
      IP2Bus_MstBusLock              => iIP2Bus_MstBusLock,
      IP2Bus_MstNum                  => iIP2Bus_MstNum,  -- Added with version 2.01a
      Bus2IP_MstWrAck                => open,
      Bus2IP_MstRdAck                => open,
      Bus2IP_MstRetry                => open,
      Bus2IP_MstError                => open,
      Bus2IP_MstTimeOut              => open,
      Bus2IP_MstLastAck              => iBus2IP_MstLastAck,
      Bus2IP_IPMstTrans              => open,  -- added with Ver 2.01a

      IP2RFIFO_WrReq                 => '0',
      IP2RFIFO_Data                  => ZERO_IP2RFIFO_Data,
      IP2RFIFO_WrMark                => '0',
      IP2RFIFO_WrRelease             => '0',
      IP2RFIFO_WrRestore             => '0',
      RFIFO2IP_WrAck                 => open,
      RFIFO2IP_AlmostFull            => open,
      RFIFO2IP_Full                  => open,
      RFIFO2IP_Vacancy               => open,
      IP2WFIFO_RdReq                 => '0',
      IP2WFIFO_RdMark                => '0',
      IP2WFIFO_RdRelease             => '0',
      IP2WFIFO_RdRestore             => '0',
      WFIFO2IP_Data                  => open,
      WFIFO2IP_RdAck                 => open,
      WFIFO2IP_AlmostEmpty           => open,
      WFIFO2IP_Empty                 => open,
      WFIFO2IP_Occupancy             => open,

      IP2Bus_DMA_Req                 => '0',
      Bus2IP_DMA_Ack                 => open
    );

  
  
  ------------------------------------------
  -- instantiate the User Logic
  ------------------------------------------
  USER_LOGIC_I : entity plb_usb_v1_00_d.user_logic
    generic map
    (
      -- MAP USER GENERICS BELOW THIS LINE ---------------
      C_USB_DATA_WIDTH               => C_USB_DATA_WIDTH,
      C_BASEADDR                     => C_BASEADDR,
      -- MAP USER GENERICS ABOVE THIS LINE ---------------

      C_DWIDTH                       => USER_DWIDTH,
      C_NUM_CE                       => USER_NUM_CE,
      C_AWIDTH                       => C_PLB_AWIDTH
    )
    port map
    (
      -- MAP USER PORTS BELOW THIS LINE ------------------
      dcm_locked        => dcm_locked,
      dcm_reset         => dcm_reset,
      if_clk            => if_clk,
      usb_full_n        => usb_full_n,
      usb_empty_n       => usb_empty_n,
      usb_alive         => usb_alive,
      Interrupt         => Interrupt,
      sloe_n            => sloe_n,
      slrd_n            => slrd_n,
      slwr_n            => slwr_n,
      pktend_n          => pktend_n,
      fifoaddr          => fifoaddr,
      fd_I              => fd_I,
      fd_O              => fd_O,
      fd_T              => fd_T,
      -- MAP USER PORTS ABOVE THIS LINE ------------------
      IP2Bus_MstRdReq                => iIP2Bus_MstRdReq,
      IP2Bus_MstWrReq                => iIP2Bus_MstWrReq,
      IP2Bus_MstBurst                => iIP2Bus_MstBurst,
      IP2Bus_MstBusLock              => iIP2Bus_MstBusLock,   
      IP2Bus_Addr                    => iIP2Bus_Addr,
      IP2Bus_MstBE                   => iIP2Bus_MstBE,
      IP2IP_Addr                     => iIP2IP_Addr,
      Bus2IP_MstLastAck              => iBus2IP_MstLastAck,
      Bus2IP_Burst                   => iBus2IP_Burst,
      IP2Bus_MstNum                  => iIP2Bus_MstNum,   
      
      Bus2IP_Clk                     => iBus2IP_Clk,
      Bus2IP_Reset                   => iBus2IP_Reset,
      Bus2IP_Addr                    => iBus2IP_Addr,
      Bus2IP_Data                    => uBus2IP_Data,
      Bus2IP_BE                      => uBus2IP_BE,
      Bus2IP_RdCE                    => uBus2IP_RdCE,
      Bus2IP_WrCE                    => uBus2IP_WrCE,
      Bus2IP_RdReq                   => iBus2IP_RdReq,
      Bus2IP_WrReq                   => iBus2IP_WrReq,
      IP2Bus_Data                    => uIP2Bus_Data,
      IP2Bus_Retry                   => iIP2Bus_Retry,
      IP2Bus_Error                   => iIP2Bus_Error,
      IP2Bus_ToutSup                 => iIP2Bus_ToutSup,
      IP2Bus_Busy                    => iIP2Bus_Busy,
      IP2Bus_RdAck                   => iIP2Bus_RdAck,
      IP2Bus_WrAck                   => iIP2Bus_WrAck
    );


  ------------------------------------------
  -- hooking up signal slicing
  ------------------------------------------
  uBus2IP_BE <= iBus2IP_BE(0 to USER_DWIDTH/8-1);
  uBus2IP_Data <= iBus2IP_Data(0 to USER_DWIDTH-1);
--   uBus2IP_RdCE <= iBus2IP_RdCE(USER00_CE_INDEX to USER00_CE_INDEX+USER_NUM_CE-1);
--   uBus2IP_WrCE <= iBus2IP_WrCE(USER00_CE_INDEX to USER00_CE_INDEX+USER_NUM_CE-1);
  uBus2IP_RdCE <= iBus2IP_RdCE(0 to USER_NUM_CE-1);
  uBus2IP_WrCE <= iBus2IP_WrCE(0 to USER_NUM_CE-1);
  iIP2Bus_Data(0 to USER_DWIDTH-1) <= uIP2Bus_Data;

  
end IMP;
