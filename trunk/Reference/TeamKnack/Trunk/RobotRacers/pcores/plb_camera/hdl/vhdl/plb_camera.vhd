------------------------------------------------------------------------------
-- plb_camera.vhd - entity/architecture pair
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

library plb_camera;
use plb_camera.all;

entity plb_camera is
  generic
  (
    C_IMAGE_WIDTH                  : integer              := 640;
    C_IMAGE_HEIGHT                 : integer              := 480;

    -- DO NOT EDIT BELOW THIS LINE ---------------------
    -- Bus protocol parameters, do not add to or delete
    C_BASEADDR                     : std_logic_vector     := X"00000000";
    C_HIGHADDR                     : std_logic_vector     := X"FFFFFFFF";
    C_PLB_AWIDTH                   : integer              := 32;
    C_PLB_DWIDTH                   : integer              := 64;
    C_PLB_NUM_MASTERS              : integer              := 8;
    C_PLB_MID_WIDTH                : integer              := 3;
    C_FAMILY                       : string               := "virtex4"
    -- DO NOT EDIT ABOVE THIS LINE ---------------------
  );
  port
  (
    -- ADD USER PORTS BELOW THIS LINE ------------------
    cam_data                       : in std_logic_vector(0 to 7);
    cam_frame_valid                : in std_logic;
    cam_line_valid                 : in std_logic;
    cam_pix_clk                    : in std_logic;
    cam_sdata_I                    : in std_logic;
    cam_sdata_O                    : out std_logic;
    cam_sdata_T                    : out std_logic;
    cam_sclk                       : out std_logic;
    Interrupt                      : out std_logic; 
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

end entity plb_camera;

------------------------------------------------------------------------------
-- Architecture section
------------------------------------------------------------------------------

architecture IMP of plb_camera is

  ------------------------------------------
  -- Constant: array of address range identifiers
  ------------------------------------------
  constant ARD_ID_ARRAY         : INTEGER_ARRAY_TYPE   := 
    (
      --IPIF_INTR,             -- ipif interrupt (pre-defined keyword)
      0 => USER_00             -- user ID (pre-defined keyword)
      --USER_01,               -- user ID (pre-defined keyword) 
      --USER_02,               -- user ID (pre-defined keyword) 
      --IPIF_DMA_SG,           -- ipif DMA/SG 
      --IPIF_RST,              -- ipif reset (pre-defined keyword)
      --IPIF_WRFIFO_REG,       -- ipif wrfifo registers (pre-defined keyword)
      --IPIF_WRFIFO_DATA,      -- ipif wrfifo data (pre-defined keyword)
      --IPIF_RDFIFO_REG,       -- ipif rdfifo registers (pre-defined keyword)
      --IPIF_RDFIFO_DATA,      -- ipif rdfifo data (pre-defined keyword)
      --IPIF_SESR_SEAR         -- IPIF SESR/SEAR Registers
      );
  
  ------------------------------------------
  -- Constant: array of address pairs for each address range
  ------------------------------------------
  constant ZERO_ADDR_PAD                  : std_logic_vector(0 to 64-C_PLB_AWIDTH-1) := (others => '0');

  constant USER_BASEADDR                  : std_logic_vector  := C_BASEADDR;
  constant USER_HIGHADDR                  : std_logic_vector  := C_HIGHADDR;

  constant ARD_ADDR_RANGE_ARRAY           : SLV64_ARRAY_TYPE     := 
    (
      ZERO_ADDR_PAD & USER_BASEADDR,    
      ZERO_ADDR_PAD & USER_HIGHADDR
    );

  ------------------------------------------
  -- Constant: array of data widths for each target address range
  ------------------------------------------
  constant USER_DWIDTH                    : integer              := C_PLB_DWIDTH;

  constant ARD_DWIDTH_ARRAY               : INTEGER_ARRAY_TYPE   :=
    -- This array specifies the data bus width of the memory address
    -- range specified for the cooresponding baseaddr pair.
    (
      0 => USER_DWIDTH   
    );
  
  ------------------------------------------
  -- Constant: array of desired number of chip enables for each address range
  ------------------------------------------
  constant USER_NUM_CE                    : integer              := 1;

  constant ARD_NUM_CE_ARRAY               : INTEGER_ARRAY_TYPE   :=
    -- This array spcifies the number of Chip Enables (CE) that is 
    -- required by the cooresponding baseaddr pair.
    (
      0 => USER_NUM_CE          
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


  constant DMA_CHAN_TYPE_ARRAY            : INTEGER_ARRAY_TYPE   :=       
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
      
       --C_ARD_DEPENDENT_PROPS_ARRAY    => ARD_DEPENDENT_PROPS_ARRAY, 
       --C_ARD_DTIME_READ_ARRAY         => ARD_DTIME_READ_ARRAY, 
       --C_ARD_DTIME_WRITE_ARRAY        => ARD_DTIME_WRITE_ARRAY, 
      
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
      
--      C_DMA_CHAN_TYPE_ARRAY          => DMA_CHAN_TYPE_ARRAY, 
--      C_DMA_LENGTH_WIDTH_ARRAY       =>  
--      C_DMA_PKT_LEN_FIFO_ADDR_ARRAY  => 
--      C_DMA_PKT_STAT_FIFO_ADDR_ARRAY => 
--      C_DMA_INTR_COALESCE_ARRAY      => 
--      C_DMA_ALLOW_BURST              => 
--      C_DMA_PACKET_WAIT_UNIT_NS      => 
      
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
      Bus2IP_Addr                    => iBus2IP_Addr,
      Bus2IP_Data                    => iBus2IP_Data,
      Bus2IP_RNW                     => open,
      Bus2IP_BE                      => iBus2IP_BE,
      Bus2IP_Burst                   => iBus2IP_Burst,
      Bus2IP_WrReq                   => iBus2IP_WrReq,
      Bus2IP_RdReq                   => iBus2IP_RdReq,
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
      IP2Bus_MstNum                  => iIP2Bus_MstNum,  
      Bus2IP_MstWrAck                => open,
      Bus2IP_MstRdAck                => open,
      Bus2IP_MstRetry                => open,
      Bus2IP_MstError                => open,
      Bus2IP_MstTimeOut              => open,
      Bus2IP_MstLastAck              => iBus2IP_MstLastAck,
      Bus2IP_IPMstTrans              => open,  
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
  USER_LOGIC_I : entity plb_camera.user_logic
    generic map
    (
      -- MAP USER GENERICS BELOW THIS LINE ---------------
      C_BASEADDR                     => C_BASEADDR,
      C_IMAGE_WIDTH                  => C_IMAGE_WIDTH,
      C_IMAGE_HEIGHT                 => C_IMAGE_HEIGHT,  
      -- MAP USER GENERICS ABOVE THIS LINE ---------------

      C_AWIDTH                       => C_PLB_AWIDTH,
      C_DWIDTH                       => USER_DWIDTH,
      C_NUM_CE                       => USER_NUM_CE
    )
    port map
    (
      -- MAP USER PORTS BELOW THIS LINE ------------------
      cam_data                       => cam_data,
      cam_frame_valid                => cam_frame_valid, 
      cam_line_valid                 => cam_line_valid, 
      cam_pix_clk                    => cam_pix_clk,
      cam_sdata_I                    => cam_sdata_I,
      cam_sdata_O                    => cam_sdata_O,
      cam_sdata_T                    => cam_sdata_T,
      cam_sclk                       => cam_sclk,
      Interrupt                      => Interrupt,
      
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
