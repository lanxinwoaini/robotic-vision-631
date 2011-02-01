
-- user_logic_full.vhd - entity/architecture pair
------------------------------------------------------------------------------

-- DO NOT EDIT BELOW THIS LINE --------------------
library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;

-- DO NOT EDIT ABOVE THIS LINE --------------------

--USER libraries added here

------------------------------------------------------------------------------
-- Entity section
------------------------------------------------------------------------------

entity user_logic is
  generic
  (
    -- ADD USER GENERICS BELOW THIS LINE ---------------
    C_BASEADDR                     : std_logic_vector     := X"00000000";
    C_IMAGE_WIDTH                  : integer              := 640;
    C_IMAGE_HEIGHT                 : integer              := 480;
    -- ADD USER GENERICS ABOVE THIS LINE ---------------

    -- DO NOT EDIT BELOW THIS LINE ---------------------
    -- Bus protocol parameters, do not add to or delete
    C_AWIDTH                       : integer              := 32;
    C_DWIDTH                       : integer              := 64;
    C_NUM_CE                       : integer              := 1
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

    IP2Bus_MstRdReq                : out std_logic;
    IP2Bus_MstWrReq                : out std_logic;
    IP2Bus_MstBurst                : out std_logic;
    IP2Bus_MstBusLock              : out std_logic;
    IP2Bus_Addr                    : out std_logic_vector(0 to C_AWIDTH-1);
    IP2Bus_MstBE                   : out std_logic_vector(0 to 7);
    IP2IP_Addr                     : out std_logic_vector(0 to C_AWIDTH-1);
    Bus2IP_MstLastAck              : in  std_logic;
    Bus2IP_Burst                   : in  std_logic;
    IP2Bus_MstNum                  : out std_logic_vector(0 to 4);

    Bus2IP_Clk                     : in  std_logic;
    Bus2IP_Reset                   : in  std_logic;
    Bus2IP_Addr                    : in  std_logic_vector(0 to C_AWIDTH-1);
    Bus2IP_Data                    : in  std_logic_vector(0 to C_DWIDTH-1);
    Bus2IP_BE                      : in  std_logic_vector(0 to C_DWIDTH/8-1);
    Bus2IP_RdCE                    : in  std_logic_vector(0 to C_NUM_CE-1);
    Bus2IP_WrCE                    : in  std_logic_vector(0 to C_NUM_CE-1);
    Bus2IP_RdReq                   : in  std_logic;
    Bus2IP_WrReq                   : in  std_logic;

    IP2Bus_Data                    : out std_logic_vector(0 to C_DWIDTH-1);
    IP2Bus_Retry                   : out std_logic;
    IP2Bus_Error                   : out std_logic;
    IP2Bus_ToutSup                 : out std_logic;
    IP2Bus_Busy                    : out std_logic;
    IP2Bus_RdAck                   : out std_logic;
    IP2Bus_WrAck                   : out std_logic
    -- DO NOT EDIT ABOVE THIS LINE ---------------------
  );
end entity user_logic;

------------------------------------------------------------------------------
-- Architecture section
------------------------------------------------------------------------------

architecture IMP of user_logic is

-------------------------------------------------------------------------------
-- ----------------------------------------------------------------------------
-- Section 1: Component Declarations
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
  -- For grabbing frames from the camera
	component frame_sync
	port (
		clk                : in  std_logic;
		reset              : in  std_logic;
		start_capture      : in  std_logic;
		capture_done       : out std_logic;
		data_out           : out std_logic_vector(0 to 7);
		data_valid         : out std_logic;
		cam_bytes          : in  std_logic_vector(0 to 1);
		cam_data_in        : in  std_logic_vector(0 to 7);
		cam_frame_valid_in : in  std_logic;
		cam_line_valid_in  : in  std_logic;
		cam_pix_clk_in     : in  std_logic);
	end component;

  -- For communicating over the camera's serial interface
	component cam_ser
	generic (
		C_CLK_DIV : integer := 112);
	port (
		clk               : in    std_logic;
		reset             : in    std_logic;
		wr_en             : in    std_logic;
		rd_en             : in    std_logic;
		dev_addr          : in    std_logic_vector(0 to 7);
		reg_addr          : in    std_logic_vector(0 to 7);
		data_in           : in    std_logic_vector(0 to 15);
		data_out          : out   std_logic_vector(0 to 15);
		ready             : out   std_logic;
		sdata_I           : in    std_logic;
		sdata_O           : out   std_logic;
		sdata_T           : out   std_logic;
		sclk              : out   std_logic);
	end component;

	--for storing frames temporarily before being sent to the SDRAM
	component org_frame_fifo
		port (
		din: IN std_logic_VECTOR(15 downto 0);
		prog_full_thresh: IN std_logic_VECTOR(9 downto 0);
		rd_clk: IN std_logic;
		rd_en: IN std_logic;
		rst: IN std_logic;
		wr_clk: IN std_logic;
		wr_en: IN std_logic;
		dout: OUT std_logic_VECTOR(63 downto 0);
		empty: OUT std_logic;
		full: OUT std_logic;
		prog_full: OUT std_logic);
	end component;

	--converts RGB to HSV
	component hsv_buffer is
		generic
		(
			C_IN_WIDTH  : integer := 8;
			C_OUT_WIDTH : integer := 8
		);
		port
		(
			clk          : in  std_logic;
			reset        : in  std_logic;
			in_valid     : in  std_logic;
			data_in      : in  std_logic_vector(C_IN_WIDTH-1 downto 0);
			out_valid    : out std_logic;
			data_out     : out std_logic_vector(C_OUT_WIDTH-1 downto 0)
		);
	end component;
	
	--packs smaller bit widths into larger (like packing the output of segment_buffer (1 bit) to the input of the FIFO (8 bits))
	component size_buffer
		generic (
			C_IN_WIDTH        : integer;
			C_OUT_WIDTH       : integer
		);
		port 
		(
			clk               : in  std_logic;
			reset             : in  std_logic;
			in_valid          : in  std_logic;
			data_in           : in  std_logic_vector(C_IN_WIDTH-1 downto 0);
			out_valid         : out std_logic;
			data_out          : out std_logic_vector(C_OUT_WIDTH-1 downto 0)
		);
	end component;
	
	component size_buffer_onebit
		generic (
			C_OUT_WIDTH       : integer
		);
		port 
		(
			clk               : in  std_logic;
			reset             : in  std_logic;
			in_valid          : in  std_logic;
			data_in           : in  std_logic;
			out_valid         : out std_logic;
			data_out          : out std_logic_vector(C_OUT_WIDTH-1 downto 0)
		);
	end component;
	

	--segments HSV to a mask
	component segment_buffer is
		port
		(
			clk                : in  std_logic;
			reset              : in  std_logic;

			in_valid           : in  std_logic;
			data_in            : in  std_logic_vector(23 downto 0);

			out_valid          : out std_logic;
			data_out           : out std_logic;

			thresh_we          : in  std_logic;
			thresh_type_hue_we : in  std_logic;
			thresh_type_sat_we : in  std_logic;
			thresh_type_val_we : in  std_logic;
			thresh_max         : in  std_logic_vector(7 downto 0);
			thresh_min         : in  std_logic_vector(7 downto 0);
			
			thresh_hue_min_out : out std_logic_vector(7 downto 0);
			thresh_hue_max_out : out std_logic_vector(7 downto 0);
			thresh_sat_min_out : out std_logic_vector(7 downto 0);
			thresh_sat_max_out : out std_logic_vector(7 downto 0);
			thresh_val_min_out : out std_logic_vector(7 downto 0);
			thresh_val_max_out : out std_logic_vector(7 downto 0)
		);
	end component;
	
	component conv_buffer is 
		generic
		(
			C_DATA_WIDTH	: integer := 8
		);
		port(
			clk, reset 		: in std_logic;
			frame_reset		: in std_logic;
			conv_valid_in 	: in std_logic;
			conv_din 		: in std_logic;
			conv_data_out	: out std_logic_vector(7 downto 0); --this output will output the total pixel count in each convolution frame. This output should be attached to a threshold block to produce a single pixel denoting whether the count satisfies the designated threshold value.
			conv_valid_out	: out std_logic;
	  
			reg_val_in				: in std_logic_vector(15 downto 0);
			col_init_we				: in std_logic;
			row_init_we				: in std_logic;
			between_frames_init_we	: in std_logic;
			initial_invalid_init_we	: in std_logic;
			initial_zero_init_we	: in std_logic;
			zeros_val_we			: in std_logic;
	  
			min_valid		: in std_logic;
			min_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
			max_valid		: in std_logic;
			max_in			: in	std_logic_vector(C_DATA_WIDTH-1 downto 0);
			min_out			: out	std_logic_vector(C_DATA_WIDTH-1 downto 0); --always output the min threshold value
			max_out			: out	std_logic_vector(C_DATA_WIDTH-1 downto 0); --always output the max threshold value

			thresh_out_valid	: out	std_logic;
			thresh_data_out		: out	std_logic --binary threshold (1 or 0)
		);
	end component;



-------------------------------------------------------------------------------
-- ----------------------------------------------------------------------------
-- Section 2: Signal Declarations
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

	--used for debug purposes:

	signal status                         : std_logic_vector(18 downto 0);

	signal addr_sel                       : std_logic_vector(4 downto 0);

	signal cam_ser_we                     : std_logic;
	signal cam_ser_re                     : std_logic;
	signal cam_ser_addr                   : std_logic_vector(7 downto 0);
	signal cam_ser_data_in                : std_logic_vector(15 downto 0);
	signal cam_ser_data_out               : std_logic_vector(15 downto 0);
	signal cam_ser_ready                  : std_logic;


	-- Signals for registers
	signal burst_size_wr_en					: std_logic;
	signal save_options_wr_en				: std_logic;
	constant C_ORG_SAVE_INDEX				: integer := 0;
	constant C_HSV_SAVE_INDEX				: integer := 1;
	constant C_MASK_SAVE_INDEX				: integer := 2;
	constant C_GREY_SAVE_INDEX				: integer := 3;
	constant C_COL_COUNT_SAVE_INDEX			: integer := 4;

	signal org_frm_mem_addr_wr_en         : std_logic;
	signal hsv_frm_mem_addr_wr_en         : std_logic;
	signal mask_orange_frm_mem_addr_wr_en : std_logic;
	signal mask_green_frm_mem_addr_wr_en  : std_logic;
	signal conv_orange_frm_mem_addr_wr_en : std_logic;
	signal conv_green_frm_mem_addr_wr_en  : std_logic;
	signal colcount_orange_frm_mem_addr_wr_en : std_logic;
	signal colcount_green_frm_mem_addr_wr_en  : std_logic;
	signal core_soft_reset                : std_logic;
	--signal reg_sel_we                     : std_logic;

	signal burst_size                     : std_logic_vector(4 downto 0);

	-- "00000": None   "00001": Save Origianl  "00010": save HSV   "00100" save Mask  "01000" save Grey  "10000" save ColCount
	signal save_options                   : std_logic_vector(4 downto 0);

	signal org_frm_mem_addr               : std_logic_vector(31 downto 0);
	signal hsv_frm_mem_addr               : std_logic_vector(31 downto 0);
	signal mask_orange_frm_mem_addr       : std_logic_vector(31 downto 0);
	signal mask_green_frm_mem_addr        : std_logic_vector(31 downto 0);
	signal conv_orange_frm_mem_addr       : std_logic_vector(31 downto 0);
	signal conv_green_frm_mem_addr        : std_logic_vector(31 downto 0);
	signal colcount_orange_frm_mem_addr   : std_logic_vector(31 downto 0);
	signal colcount_green_frm_mem_addr    : std_logic_vector(31 downto 0);

	signal bursting_org_frame             : std_logic;
	signal bursting_hsv_frame             : std_logic;
	signal bursting_mask_orange_frame     : std_logic;
	signal bursting_mask_green_frame      : std_logic;
	signal bursting_conv_orange_frame     : std_logic;
	signal bursting_conv_green_frame      : std_logic;
	signal bursting_colcount_orange_frame : std_logic;
	signal bursting_colcount_green_frame  : std_logic;

	-- State Machine control
	type   fg_state_type is	(			IDLE,
										ASSERT_ORG_FRAME,
										ASSERT_HSV_FRAME,
										ASSERT_MASK_ORANGE_FRAME,
										ASSERT_MASK_GREEN_FRAME,
										ASSERT_CONV_ORANGE_FRAME,
										ASSERT_CONV_GREEN_FRAME,
										ASSERT_COLCOUNT_ORANGE_FRAME,
										ASSERT_COLCOUNT_GREEN_FRAME,
										PAUSE);
	signal current_state, next_state      : fg_state_type;
	--signal burst_state                    : std_logic_vector(1 downto 0);


	-- Component Instantiation
	signal start_capture                  : std_logic;
	signal capture_done                   : std_logic;

	signal pixel                          : std_logic_vector(7 downto 0);
	signal pixel_valid                    : std_logic;

	signal pix_word                       : std_logic_vector(15 downto 0);
	signal pix_word_valid                 : std_logic;
	signal hsv_word                       : std_logic_vector(23 downto 0); --8bits H, 8bits S, 8bits V
	signal hsv_partialword                : std_logic_vector(15 downto 0); --this is just the H and S
	signal hsv_word_valid                 : std_logic;


	signal binary_orange_onebit			  : std_logic;
	signal binary_orange_onebit_valid	  : std_logic;
	signal binary_green_onebit			  : std_logic;
	signal binary_green_onebit_valid	  : std_logic;
	signal binary_orange_sel              : std_logic; --this choses whether to output mask_orange_onebit or conv_orange_onebit
	signal binary_green_sel               : std_logic;
	signal binary_orange_sel_next         : std_logic; --this choses whether to output mask_orange_onebit or conv_orange_onebit
	signal binary_green_sel_next          : std_logic;
	
	signal mask_orange_onebit			  : std_logic;
	signal mask_orange_onebit_valid	      : std_logic;
	signal binary_orange_word               : std_logic_vector(15 downto 0);
	signal binary_orange_word_valid         : std_logic;
	signal mask_green_onebit			  : std_logic;
	signal mask_green_onebit_valid	      : std_logic;
	signal binary_green_word                : std_logic_vector(15 downto 0);
	signal binary_green_word_valid          : std_logic;
	signal conv_orange_onebit             : std_logic;
	signal conv_orange_onebit_valid       : std_logic;
	signal conv_orange_word               : std_logic_vector(7 downto 0);
	signal conv_orange_word_valid         : std_logic;
	signal conv_orange_fullword               : std_logic_vector(15 downto 0);
	signal conv_orange_fullword_valid         : std_logic;
	signal conv_green_onebit              : std_logic;
	signal conv_green_onebit_valid        : std_logic;
	signal conv_green_word                : std_logic_vector(7 downto 0);
	signal conv_green_word_valid          : std_logic;
	signal conv_green_fullword                : std_logic_vector(15 downto 0);
	signal conv_green_fullword_valid          : std_logic;
	signal colcount_orange_word           : std_logic_vector(7 downto 0);
	signal colcount_orange_word_valid     : std_logic;
	signal colcount_orange_fullword           : std_logic_vector(15 downto 0);
	signal colcount_orange_fullword_valid     : std_logic;
	signal colcount_green_word            : std_logic_vector(7 downto 0);
	signal colcount_green_word_valid      : std_logic;
	signal colcount_green_fullword            : std_logic_vector(15 downto 0);
	signal colcount_green_fullword_valid      : std_logic;
	-----signals for FIFOs writing image data to the SDRAM:
	signal fifo_prog_full                 : std_logic_vector(9 downto 0);

	signal org_frame_fifo_out             : std_logic_vector(63 downto 0);
	signal hsv_frame_fifo_out             : std_logic_vector(63 downto 0);
	signal mask_orange_frame_fifo_out     : std_logic_vector(63 downto 0);
	signal mask_green_frame_fifo_out      : std_logic_vector(63 downto 0);
	signal conv_orange_frame_fifo_out     : std_logic_vector(63 downto 0);
	signal conv_green_frame_fifo_out      : std_logic_vector(63 downto 0);
	signal colcount_orange_frame_fifo_out : std_logic_vector(63 downto 0);
	signal colcount_green_frame_fifo_out  : std_logic_vector(63 downto 0);

	signal org_frame_fifo_wr_en           : std_logic;
	signal hsv_frame_fifo_wr_en           : std_logic;
	signal mask_orange_frame_fifo_wr_en   : std_logic;
	signal mask_green_frame_fifo_wr_en    : std_logic;
	signal conv_orange_frame_fifo_wr_en   : std_logic;
	signal conv_green_frame_fifo_wr_en    : std_logic;
	signal colcount_orange_frame_fifo_wr_en   : std_logic;
	signal colcount_green_frame_fifo_wr_en    : std_logic;

	signal org_frame_fifo_rd_en           : std_logic;
	signal hsv_frame_fifo_rd_en           : std_logic;
	signal mask_orange_frame_fifo_rd_en   : std_logic;
	signal mask_green_frame_fifo_rd_en    : std_logic;
	signal conv_orange_frame_fifo_rd_en   : std_logic;
	signal conv_green_frame_fifo_rd_en    : std_logic;
	signal colcount_orange_frame_fifo_rd_en   : std_logic;
	signal colcount_green_frame_fifo_rd_en    : std_logic;

	signal org_frame_fifo_empty           : std_logic;
	signal hsv_frame_fifo_empty           : std_logic;
	signal mask_orange_frame_fifo_empty   : std_logic;
	signal mask_green_frame_fifo_empty    : std_logic;
	signal conv_orange_frame_fifo_empty   : std_logic;
	signal conv_green_frame_fifo_empty    : std_logic;
	signal colcount_orange_frame_fifo_empty   : std_logic;
	signal colcount_green_frame_fifo_empty    : std_logic;

	signal org_frame_fifo_ready           : std_logic;
	signal hsv_frame_fifo_ready           : std_logic;
	signal mask_orange_frame_fifo_ready   : std_logic;
	signal mask_green_frame_fifo_ready    : std_logic;
	signal conv_green_frame_fifo_ready    : std_logic;
	signal conv_orange_frame_fifo_ready   : std_logic;
	signal colcount_green_frame_fifo_ready    : std_logic;
	signal colcount_orange_frame_fifo_ready   : std_logic;

	
	-----threshold get/set signals:
	signal thresh_min                     : std_logic_vector (7 downto 0);
	signal thresh_max                     : std_logic_vector (7 downto 0);

	signal thresh_seg_orange_we					: std_logic;
	signal thresh_seg_green_we					: std_logic;
	signal thresh_seg_type_hue_we				: std_logic;
	signal thresh_seg_type_sat_we				: std_logic;
	signal thresh_seg_type_val_we				: std_logic;
	signal thresh_postconvolution_orange_we		: std_logic;
	signal thresh_postconvolution_green_we		: std_logic;
	
	signal thresh_seg_orange_hue_min_out		: std_logic_vector (7 downto 0);
	signal thresh_seg_orange_hue_max_out		: std_logic_vector (7 downto 0);
	signal thresh_seg_orange_sat_min_out		: std_logic_vector (7 downto 0);
	signal thresh_seg_orange_sat_max_out		: std_logic_vector (7 downto 0);
	signal thresh_seg_orange_val_min_out		: std_logic_vector (7 downto 0);
	signal thresh_seg_orange_val_max_out		: std_logic_vector (7 downto 0);
	signal thresh_seg_green_hue_min_out			: std_logic_vector (7 downto 0);
	signal thresh_seg_green_hue_max_out			: std_logic_vector (7 downto 0);
	signal thresh_seg_green_sat_min_out			: std_logic_vector (7 downto 0);
	signal thresh_seg_green_sat_max_out			: std_logic_vector (7 downto 0);
	signal thresh_seg_green_val_min_out			: std_logic_vector (7 downto 0);
	signal thresh_seg_green_val_max_out			: std_logic_vector (7 downto 0);
	signal orange_convolution_seg_thresh_min_out	: std_logic_vector (7 downto 0);
	signal green_convolution_seg_thresh_min_out		: std_logic_vector (7 downto 0);
	signal convolution_seg_thresh_max           : std_logic_vector (7 downto 0);
	signal convolution_seg_thresh_max_valid     : std_logic;
	
	--signals for debugging the convolution kernel
	signal frame_reset							: std_logic;
	signal conv_reg_val							: std_logic_vector(15 downto 0);
	signal conv_col_init_we						: std_logic;
	signal conv_row_init_we						: std_logic;
	signal conv_between_frames_init_we			: std_logic;
	signal conv_initial_invalid_init_we			: std_logic;
	signal conv_initial_zero_init_we			: std_logic;
	signal conv_zeros_val_we					: std_logic;
	signal pixel_count_next, pixel_count_reg	: unsigned(31 downto 0);
	signal pixel_count_sel, pixel_count_sel_next: std_logic_vector(3 downto 0);
	
	signal conv_green_8bit_valid				: std_logic;
	signal conv_green_8bit						: std_logic_vector(7 downto 0);
	signal conv_orange_8bit_valid				: std_logic;
	signal conv_orange_8bit						: std_logic_vector(7 downto 0);


	
	
	signal interrupt_reg, interrupt_next  : std_logic;
	signal interrupt_acknowledged         : std_logic;
	signal allow_frame_reg, allow_frame_next : std_logic;
	signal image_written_out              : std_logic;
	signal pixel_valid_and_allowed        : std_logic;
	
begin
  -----------------------------------------------------------------------------
  -- --------------------------------------------------------------------------
  -- Section 3: Combinational signal routing (non-process)
  -----------------------------------------------------------------------------
  -----------------------------------------------------------------------------
  Interrupt <= interrupt_reg;
  status    <= capture_done & interrupt_reg & allow_frame_reg
               & org_frame_fifo_ready & hsv_frame_fifo_ready & mask_orange_frame_fifo_ready & mask_green_frame_fifo_ready & conv_green_frame_fifo_ready & conv_orange_frame_fifo_ready & colcount_green_frame_fifo_ready & colcount_orange_frame_fifo_ready
               & org_frame_fifo_empty & hsv_frame_fifo_empty & mask_orange_frame_fifo_empty & mask_green_frame_fifo_empty & conv_orange_frame_fifo_empty & conv_green_frame_fifo_empty & colcount_orange_frame_fifo_empty & colcount_green_frame_fifo_empty;



  -----------------------------------------------------------------------------
  -- --------------------------------------------------------------------------
  -- Section 4: Data In/Out Steering Process
  -----------------------------------------------------------------------------
  -----------------------------------------------------------------------------
  DATA_ROUTE: process (Bus2IP_Reset,Bus2IP_RdReq,Bus2IP_WrReq,Bus2IP_Addr, bursting_org_frame, org_frame_fifo_out, bursting_hsv_frame, hsv_frame_fifo_out, bursting_mask_orange_frame, bursting_mask_green_frame,bursting_conv_orange_frame,bursting_conv_green_frame,bursting_colcount_orange_frame,bursting_colcount_green_frame, mask_orange_frame_fifo_out,conv_orange_frame_fifo_out,conv_green_frame_fifo_out,colcount_orange_frame_fifo_out,colcount_green_frame_fifo_out,
                       mask_green_frame_fifo_out, status, cam_ser_ready, cam_ser_data_out, burst_size, save_options, org_frm_mem_addr, hsv_frm_mem_addr, mask_green_frm_mem_addr,conv_orange_frm_mem_addr,conv_green_frm_mem_addr,colcount_orange_frm_mem_addr,colcount_green_frm_mem_addr, mask_orange_frm_mem_addr, Bus2IP_Data,
                       thresh_seg_orange_hue_min_out,thresh_seg_orange_hue_max_out,thresh_seg_orange_sat_min_out,thresh_seg_orange_sat_max_out,thresh_seg_orange_val_min_out,thresh_seg_orange_val_max_out,thresh_seg_green_hue_min_out,thresh_seg_green_hue_max_out,thresh_seg_green_sat_min_out,thresh_seg_green_sat_max_out,thresh_seg_green_val_min_out,thresh_seg_green_val_max_out,orange_convolution_seg_thresh_min_out,green_convolution_seg_thresh_min_out,
							  binary_orange_sel, binary_green_sel, pixel_count_sel, pixel_count_reg)
  begin
    -- Write Request Default values
    start_capture                 <= '0';
    cam_ser_we                    <= '0';
    cam_ser_re                    <= '0';
    burst_size_wr_en              <= '0';
    save_options_wr_en            <= '0';
    org_frm_mem_addr_wr_en        <= '0';
    hsv_frm_mem_addr_wr_en        <= '0';
    mask_orange_frm_mem_addr_wr_en<= '0';
	mask_green_frm_mem_addr_wr_en <= '0';
    conv_orange_frm_mem_addr_wr_en<= '0';
	conv_green_frm_mem_addr_wr_en <= '0';
    colcount_orange_frm_mem_addr_wr_en  <= '0';
	colcount_green_frm_mem_addr_wr_en   <= '0';
    core_soft_reset               <= '0';

    cam_ser_addr                  <= (others => '0');
    cam_ser_data_in               <= (others => '0');

	interrupt_acknowledged <= '0';

    -- Read Request default values
    org_frame_fifo_rd_en          <= '0';
    hsv_frame_fifo_rd_en          <= '0';
    mask_orange_frame_fifo_rd_en  <= '0';
    mask_green_frame_fifo_rd_en   <= '0';
    conv_orange_frame_fifo_rd_en  <= '0';
    conv_green_frame_fifo_rd_en   <= '0';
    colcount_orange_frame_fifo_rd_en  <= '0';
    colcount_green_frame_fifo_rd_en   <= '0';

	thresh_seg_orange_we				<= '0';
	thresh_seg_green_we					<= '0';
	thresh_seg_type_hue_we				<= '0';
	thresh_seg_type_sat_we				<= '0';
	thresh_seg_type_val_we				<= '0';
	thresh_postconvolution_orange_we	<= '0';
	thresh_postconvolution_green_we		<= '0';
	frame_reset							<= '0';
	conv_col_init_we					<= '0';
	conv_row_init_we					<= '0';
	conv_between_frames_init_we			<= '0';
	conv_initial_invalid_init_we		<= '0';
	conv_initial_zero_init_we			<= '0';
	conv_zeros_val_we					<= '0';
	binary_orange_sel_next				<= binary_orange_sel;
	binary_green_sel_next				<= binary_green_sel;
	pixel_count_sel_next				<= pixel_count_sel;


    addr_sel                      <= Bus2IP_Addr(C_AWIDTH-8 to C_AWIDTH-4);

	IP2Bus_Data <= (others=>'0');

    if Bus2IP_Reset = '1' then
      core_soft_reset <= '1';
	  frame_reset <= '1';

    else

      -- Handle bus reads
      if Bus2IP_RdReq = '1' then
        Ip2Bus_RdAck <= '1';

        if bursting_org_frame = '1' then
          org_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= org_frame_fifo_out;

        elsif bursting_hsv_frame = '1' then
          hsv_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= hsv_frame_fifo_out;

        elsif bursting_mask_orange_frame = '1' then
          mask_orange_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= mask_orange_frame_fifo_out;

        elsif bursting_mask_green_frame = '1' then
          mask_green_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= mask_green_frame_fifo_out;

        elsif bursting_conv_orange_frame = '1' then
          conv_orange_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= conv_orange_frame_fifo_out;

        elsif bursting_conv_green_frame = '1' then
          conv_green_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= conv_green_frame_fifo_out;

        elsif bursting_colcount_orange_frame = '1' then
          colcount_orange_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= colcount_orange_frame_fifo_out;

        elsif bursting_colcount_green_frame = '1' then
          colcount_green_frame_fifo_rd_en    <= '1';
          IP2Bus_Data             <= colcount_green_frame_fifo_out;

        else
          case addr_sel is
            when "00000" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',32-status'length)                  & status;
            when "00001" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',31)                                & cam_ser_ready;
            when "00010" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & cam_ser_data_out;
            when "00011" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',32-burst_size'length)              & burst_size;
            when "00100" => IP2Bus_Data          <= (others => '0');
            when "00101" => IP2Bus_Data          <= (others => '0');
            when "00110" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',32-save_options'length)            & save_options;
            when "00111" => IP2Bus_Data(0 to 31) <=                                                                org_frm_mem_addr;
            when "01000" => IP2Bus_Data(0 to 31) <=                                                                hsv_frm_mem_addr;
            when "01001" => IP2Bus_Data(0 to 31) <=                                                                mask_orange_frm_mem_addr;
            when "01010" => IP2Bus_Data(0 to 31) <=                                                                mask_green_frm_mem_addr;
            when "01011" => IP2Bus_Data(0 to 31) <=                                                                conv_orange_frm_mem_addr;
            when "01100" => IP2Bus_Data(0 to 31) <=                                                                conv_green_frm_mem_addr;
            when "01101" => IP2Bus_Data(0 to 31) <=                                                                colcount_orange_frm_mem_addr;
            when "01110" => IP2Bus_Data(0 to 31) <=                                                                colcount_green_frm_mem_addr;
            when "01111" => IP2Bus_Data(0 to 31) <= (others => '0');
            when "10000" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & thresh_seg_orange_hue_max_out & thresh_seg_orange_hue_min_out;
            when "10001" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & thresh_seg_orange_sat_max_out & thresh_seg_orange_sat_min_out;
            when "10010" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & thresh_seg_orange_val_max_out & thresh_seg_orange_val_min_out;
            when "10011" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & thresh_seg_green_hue_max_out & thresh_seg_green_hue_min_out;
            when "10100" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & thresh_seg_green_sat_max_out & thresh_seg_green_sat_min_out;
            when "10101" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',16)                                & thresh_seg_green_val_max_out & thresh_seg_green_val_min_out;
            when "10110" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',24)                                & orange_convolution_seg_thresh_min_out;
            when "10111" => IP2Bus_Data(0 to 31) <= conv_std_logic_vector('0',24)                                & green_convolution_seg_thresh_min_out;
            when "11000" => IP2Bus_Data(0 to 31) <=                                                                std_logic_vector(pixel_count_reg);
            when "11001" => IP2Bus_Data(0 to 31) <= (others => '0');
            when "11010" => IP2Bus_Data          <= (others => '0');
            when "11011" => IP2Bus_Data          <= (others => '0');
            when "11100" => IP2Bus_Data          <= (others => '0');
            when "11101" => IP2Bus_Data          <= (others => '0');
            when "11110" => IP2Bus_Data          <= (others => '0');
            when "11111" => IP2Bus_Data          <= (others => '0');
            when others => IP2Bus_Data        <= (others => '0');
          end case;

        end if;
      end if;  -- End: Bus2IP_RdReq = '1'

      -- Handle bus writes
      if Bus2IP_WrReq = '1' then
        IP2Bus_WrAck <= '1';

        case addr_sel is

          when "00000" =>                  -- Start frame grab, update capture addr
            start_capture                 <= '1';

          when "00001" =>                  -- Write to camera register
            cam_ser_we                    <= '1';
            cam_ser_addr                  <= Bus2IP_Data(8 to 15);
            cam_ser_data_in               <= Bus2IP_Data(16 to 31);

          when "00010" =>                  -- Start read from camera register by writing to camera register
            cam_ser_re                    <= '1';
            cam_ser_addr                  <= Bus2IP_Data(24 to 31);

          when "00011" =>
            burst_size_wr_en              <= '1';

          when "00100" =>
            core_soft_reset               <= '1';
			frame_reset                   <= '1';

          when "00101" =>
            interrupt_acknowledged        <= '1';

          when "00110" =>                  -- Set frame save options
            save_options_wr_en            <= '1';

          when "00111" =>                  -- Set memory address for original frame
            org_frm_mem_addr_wr_en        <= '1';

          when "01000" =>                  -- Set memory address for hsv frame
            hsv_frm_mem_addr_wr_en        <= '1';

          when "01001" =>                  -- Set memory address for seg frame
            mask_orange_frm_mem_addr_wr_en        <= '1';

          when "01010" =>
            mask_green_frm_mem_addr_wr_en         <= '1';

          when "01011" =>
            conv_orange_frm_mem_addr_wr_en        <= '1';

          when "01100" =>
            conv_green_frm_mem_addr_wr_en         <= '1';

          when "01101" =>
            colcount_orange_frm_mem_addr_wr_en    <= '1';

          when "01110" =>
            colcount_green_frm_mem_addr_wr_en     <= '1';
			
          when "01111" =>              

		--the next cases are for setting the segment threshold values
          when "10000" => 
			thresh_seg_orange_we <= '1';
			thresh_seg_type_hue_we <= '1';
          when "10001" => 
			thresh_seg_orange_we <= '1';
			thresh_seg_type_sat_we <= '1';
          when "10010" => 
			thresh_seg_orange_we <= '1';
			thresh_seg_type_val_we <= '1';
          when "10011" => 
			thresh_seg_green_we <= '1';
			thresh_seg_type_hue_we <= '1';
          when "10100" => 
			thresh_seg_green_we <= '1';
			thresh_seg_type_sat_we <= '1';
          when "10101" => 
			thresh_seg_green_we <= '1';
			thresh_seg_type_val_we <= '1';
          when "10110" => 
			thresh_postconvolution_orange_we <= '1';
          when "10111" => 
			thresh_postconvolution_green_we <= '1';
          when "11000" => 
			binary_orange_sel_next <= Bus2IP_Data(27);
			binary_green_sel_next  <= Bus2IP_Data(26);
			pixel_count_sel_next   <= Bus2IP_Data(28 to 31);
          when "11001" => 
			conv_zeros_val_we <= '1';
          when "11010" => 
			conv_col_init_we <= '1';
          when "11011" => 
			conv_row_init_we <= '1';
          when "11100" => 
			conv_between_frames_init_we <= '1';
          when "11101" => 
			conv_initial_invalid_init_we <= '1';
          when "11110" => 
			conv_initial_zero_init_we <= '1';
          when "11111" => 
			frame_reset <= '1';
          when others => null;
        end case;
      end if;

    end if;
  end process;
  

-------------------------------------------------------------------------------
-- Store the PLB Burst Size in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset,burst_size_wr_en)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        burst_size <= "10000";          -- default to 128
      else
        if burst_size_wr_en = '1' then
          burst_size <= Bus2IP_Data(27 to 31);
        end if;
      end if;
    end if;
  end process;



-------------------------------------------------------------------------------
-- Store the save options in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset,save_options_wr_en)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
		binary_orange_sel <= '0';		--default to outputting the convolution result
		binary_green_sel <= '0';		--default to outputting the convolution result
        save_options <= (others => '0');
      else
        if save_options_wr_en = '1' then
          save_options <= Bus2IP_Data(32-save_options'length to 32-1);
		  binary_orange_sel <= binary_orange_sel_next;
		  binary_green_sel <= binary_green_sel_next;
        end if;
      end if;
    end if;
  end process;


-------------------------------------------------------------------------------
-- Store the memory address for the original frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset,org_frm_mem_addr_wr_en,bursting_org_frame)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        org_frm_mem_addr <= (others => '0');
      else
        if org_frm_mem_addr_wr_en = '1' then
          org_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_org_frame = '1' and Bus2IP_MstLastAck = '1' then
              case burst_size is
                when "00001" => org_frm_mem_addr <= org_frm_mem_addr + 8;
                when "00010" => org_frm_mem_addr <= org_frm_mem_addr + 16;
                when "00100" => org_frm_mem_addr <= org_frm_mem_addr + 32;
                when "01000" => org_frm_mem_addr <= org_frm_mem_addr + 64;
                when "10000" => org_frm_mem_addr <= org_frm_mem_addr + 128;
                when others => null;
              end case;
          end if;
        end if;
      end if;
    end if;
  end process;


-------------------------------------------------------------------------------
-- Store the memory address for the hsv frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        hsv_frm_mem_addr <= (others => '0');
      else
        if hsv_frm_mem_addr_wr_en = '1' then
          hsv_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_hsv_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => hsv_frm_mem_addr <= hsv_frm_mem_addr + 8;
                when "00010" => hsv_frm_mem_addr <= hsv_frm_mem_addr + 16;
                when "00100" => hsv_frm_mem_addr <= hsv_frm_mem_addr + 32;
                when "01000" => hsv_frm_mem_addr <= hsv_frm_mem_addr + 64;
                when "10000" => hsv_frm_mem_addr <= hsv_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;



-------------------------------------------------------------------------------
-- Store the memory address for the orange mask frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        mask_orange_frm_mem_addr <= (others => '0');
      else
        if mask_orange_frm_mem_addr_wr_en = '1' then
          mask_orange_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_mask_orange_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => mask_orange_frm_mem_addr <= mask_orange_frm_mem_addr + 8;
                when "00010" => mask_orange_frm_mem_addr <= mask_orange_frm_mem_addr + 16;
                when "00100" => mask_orange_frm_mem_addr <= mask_orange_frm_mem_addr + 32;
                when "01000" => mask_orange_frm_mem_addr <= mask_orange_frm_mem_addr + 64;
                when "10000" => mask_orange_frm_mem_addr <= mask_orange_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;

-------------------------------------------------------------------------------
-- Store the memory address for the green mask frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        mask_green_frm_mem_addr <= (others => '0');
      else
        if mask_green_frm_mem_addr_wr_en = '1' then
          mask_green_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_mask_green_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => mask_green_frm_mem_addr <= mask_green_frm_mem_addr + 8;
                when "00010" => mask_green_frm_mem_addr <= mask_green_frm_mem_addr + 16;
                when "00100" => mask_green_frm_mem_addr <= mask_green_frm_mem_addr + 32;
                when "01000" => mask_green_frm_mem_addr <= mask_green_frm_mem_addr + 64;
                when "10000" => mask_green_frm_mem_addr <= mask_green_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;

-------------------------------------------------------------------------------
-- Store the memory address for the orange conv frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        conv_orange_frm_mem_addr <= (others => '0');
      else
        if conv_orange_frm_mem_addr_wr_en = '1' then
          conv_orange_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_conv_orange_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => conv_orange_frm_mem_addr <= conv_orange_frm_mem_addr + 8;
                when "00010" => conv_orange_frm_mem_addr <= conv_orange_frm_mem_addr + 16;
                when "00100" => conv_orange_frm_mem_addr <= conv_orange_frm_mem_addr + 32;
                when "01000" => conv_orange_frm_mem_addr <= conv_orange_frm_mem_addr + 64;
                when "10000" => conv_orange_frm_mem_addr <= conv_orange_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;

-------------------------------------------------------------------------------
-- Store the memory address for the green conv frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        conv_green_frm_mem_addr <= (others => '0');
      else
        if conv_green_frm_mem_addr_wr_en = '1' then
          conv_green_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_conv_green_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => conv_green_frm_mem_addr <= conv_green_frm_mem_addr + 8;
                when "00010" => conv_green_frm_mem_addr <= conv_green_frm_mem_addr + 16;
                when "00100" => conv_green_frm_mem_addr <= conv_green_frm_mem_addr + 32;
                when "01000" => conv_green_frm_mem_addr <= conv_green_frm_mem_addr + 64;
                when "10000" => conv_green_frm_mem_addr <= conv_green_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;

-------------------------------------------------------------------------------
-- Store the memory address for the orange colcount frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        colcount_orange_frm_mem_addr <= (others => '0');
      else
        if colcount_orange_frm_mem_addr_wr_en = '1' then
          colcount_orange_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_colcount_orange_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => colcount_orange_frm_mem_addr <= colcount_orange_frm_mem_addr + 8;
                when "00010" => colcount_orange_frm_mem_addr <= colcount_orange_frm_mem_addr + 16;
                when "00100" => colcount_orange_frm_mem_addr <= colcount_orange_frm_mem_addr + 32;
                when "01000" => colcount_orange_frm_mem_addr <= colcount_orange_frm_mem_addr + 64;
                when "10000" => colcount_orange_frm_mem_addr <= colcount_orange_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;

-------------------------------------------------------------------------------
-- Store the memory address for the green colcount frame in a register
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        colcount_green_frm_mem_addr <= (others => '0');
      else
        if colcount_green_frm_mem_addr_wr_en = '1' then
          colcount_green_frm_mem_addr <= Bus2IP_Data(0 to 31);
        else
          if bursting_colcount_green_frame = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => colcount_green_frm_mem_addr <= colcount_green_frm_mem_addr + 8;
                when "00010" => colcount_green_frm_mem_addr <= colcount_green_frm_mem_addr + 16;
                when "00100" => colcount_green_frm_mem_addr <= colcount_green_frm_mem_addr + 32;
                when "01000" => colcount_green_frm_mem_addr <= colcount_green_frm_mem_addr + 64;
                when "10000" => colcount_green_frm_mem_addr <= colcount_green_frm_mem_addr + 128;
                when others => null;
            end case;
          end if;
        end if;
      end if;
    end if;
  end process;





  process(Bus2IP_Clk,core_soft_reset,next_state)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if core_soft_reset = '1' then
        pixel_count_reg <= (others => '0');
		pixel_count_sel <= (others => '0');
      else
        pixel_count_reg <= pixel_count_next;
        pixel_count_sel <= pixel_count_sel_next;
      end if;
    end if;
  end process;

	--increment the counter to count pixels
	process(pix_word_valid,hsv_word_valid,mask_orange_onebit_valid,mask_green_onebit_valid,conv_orange_onebit_valid,conv_green_onebit_valid,conv_orange_fullword_valid,conv_green_fullword_valid,colcount_orange_fullword_valid,colcount_green_fullword_valid,pixel_count_reg,pixel_count_sel)
	begin
		pixel_count_next <= pixel_count_reg;
		case pixel_count_sel is
			when "0000" =>
				if pix_word_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0001" =>
				if hsv_word_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0010" =>
				if mask_orange_onebit_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0011" =>
				if mask_green_onebit_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0100" =>
				if conv_orange_onebit_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0101" =>
				if conv_green_onebit_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0110" =>
				if conv_orange_fullword_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "0111" =>
				if conv_green_fullword_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "1000" =>
				if colcount_orange_fullword_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when "1001" =>
				if colcount_green_fullword_valid = '1' then
				pixel_count_next <= pixel_count_reg+1;
				end if;
			when others => null;
		end case;
	end process;



 ------------------------------------------------------------------------------
 -- ---------------------------------------------------------------------------
 -- Section 7: PLB Burst State Machine
 --
 -- STATES:
 --  1: Idle   - This state waits until the dma_session_active is 1
 --  2: Assert_OFrame - This state waits until the Bus2IP_MstLastAck is 1
 --  3: Assert_HSVFrame
 --  4: Assert_SegFrame
 --
 --
 --
 --
 -- ---------------------------------------------------------------------------
 ------------------------------------------------------------------------------

  process(Bus2IP_Clk,core_soft_reset,next_state)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if core_soft_reset = '1' then
        current_state <= IDLE;
      else
        current_state <= next_state;
      end if;
    end if;
  end process;

  process(current_state,Bus2IP_MstLastAck,org_frame_fifo_ready,mask_orange_frame_fifo_ready,mask_green_frame_fifo_ready,conv_green_frame_fifo_ready,conv_orange_frame_fifo_ready,colcount_green_frame_fifo_ready,colcount_orange_frame_fifo_ready,save_options, hsv_frame_fifo_ready, burst_size, org_frm_mem_addr, hsv_frm_mem_addr,
          mask_green_frm_mem_addr, mask_orange_frm_mem_addr, conv_orange_frm_mem_addr, conv_green_frm_mem_addr, colcount_orange_frm_mem_addr, colcount_green_frm_mem_addr)
  begin
    next_state          <= current_state;

    IP2Bus_MstRdReq     <= '0';
    IP2Bus_MstWrReq     <= '0';
    IP2Bus_MstBurst     <= '0';
    IP2Bus_MstBusLock   <= '0';
    IP2Bus_MstNum       <= (others => '0');
    IP2Bus_Addr         <= (others => '0');
    IP2Bus_MstBE        <= (others => '0');
    IP2IP_Addr          <= (others => '0');
    IP2Bus_ToutSup      <= '0';

    bursting_org_frame  <= '0';
    bursting_hsv_frame  <= '0';
    bursting_mask_orange_frame  <= '0';
    bursting_mask_green_frame   <= '0';
    bursting_conv_orange_frame  <= '0';
    bursting_conv_green_frame   <= '0';
    bursting_colcount_orange_frame  <= '0';
    bursting_colcount_green_frame   <= '0';


    case current_state is
      when IDLE =>
        if org_frame_fifo_ready = '1' and save_options(C_ORG_SAVE_INDEX) = '1' then
          next_state <= ASSERT_ORG_FRAME;

        elsif hsv_frame_fifo_ready = '1' and save_options(C_HSV_SAVE_INDEX) = '1' then
          next_state <= ASSERT_HSV_FRAME;

        elsif mask_orange_frame_fifo_ready = '1' and save_options(C_MASK_SAVE_INDEX) = '1' then
          next_state <= ASSERT_MASK_ORANGE_FRAME;

        elsif mask_green_frame_fifo_ready = '1' and save_options(C_MASK_SAVE_INDEX) = '1' then
          next_state <= ASSERT_MASK_GREEN_FRAME;

        elsif conv_green_frame_fifo_ready = '1' and save_options(C_GREY_SAVE_INDEX) = '1' then
          next_state <= ASSERT_CONV_GREEN_FRAME;

        elsif conv_orange_frame_fifo_ready = '1' and save_options(C_GREY_SAVE_INDEX) = '1' then
          next_state <= ASSERT_CONV_ORANGE_FRAME;

        elsif colcount_green_frame_fifo_ready = '1' and save_options(C_COL_COUNT_SAVE_INDEX) = '1' then
          next_state <= ASSERT_COLCOUNT_ORANGE_FRAME;

        elsif colcount_orange_frame_fifo_ready = '1' and save_options(C_COL_COUNT_SAVE_INDEX) = '1' then
          next_state <= ASSERT_COLCOUNT_GREEN_FRAME;

        end if;


      when ASSERT_ORG_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= org_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_org_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_HSV_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= hsv_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_hsv_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_MASK_ORANGE_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= mask_orange_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_mask_orange_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_MASK_GREEN_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= mask_green_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_mask_green_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_CONV_ORANGE_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= conv_orange_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_conv_orange_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_CONV_GREEN_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= conv_green_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_conv_green_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_COLCOUNT_ORANGE_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= colcount_orange_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_colcount_orange_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;

      when ASSERT_COLCOUNT_GREEN_FRAME =>
        IP2Bus_MstWrReq         <= '1';
        IP2Bus_MstBurst         <= '1';
        IP2Bus_MstBusLock       <= '0';
        IP2Bus_MstNum           <= burst_size;
        IP2Bus_Addr             <= colcount_green_frm_mem_addr;
        IP2Bus_MstBE            <= "11111111";
        IP2IP_Addr              <= C_BASEADDR;
        --IP2Bus_ToutSup          <= '1';

        bursting_colcount_green_frame      <= '1';

        if Bus2IP_MstLastAck = '1' then
          next_state <= PAUSE;
        end if;


      when PAUSE =>
        next_state <= IDLE;

      when others => null;
    end case;
  end process;

  IP2Bus_Retry          <= '0';
  IP2Bus_Error          <= '0';
  IP2Bus_Busy           <= '0';

-------------------------------------------------------------------------------
-- ----------------------------------------------------------------------------
-- Section 8: Component Instantiations
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
	process(Bus2IP_Clk,Bus2IP_Reset,burst_size_wr_en)
	begin
		if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
			if Bus2IP_Reset = '1' then
				interrupt_reg <= '0';
				allow_frame_reg <= '1';
			else
				interrupt_reg <= interrupt_next;
				allow_frame_reg <= allow_frame_next;
			end if;
		end if;
	end process;

	--image_written_out <= capture_done and org_frame_fifo_empty and mask_orange_frame_fifo_empty and mask_green_frame_fifo_empty and hsv_frame_fifo_empty and conv_orange_frame_fifo_empty and conv_green_frame_fifo_empty and colcount_orange_frame_fifo_empty and colcount_green_frame_fifo_empty;
	image_written_out <= capture_done and org_frame_fifo_empty and hsv_frame_fifo_empty;
	interrupt_next <=
		'1' when image_written_out = '1' else
		'0' when interrupt_acknowledged = '1' else
		interrupt_reg;
	allow_frame_next <=
		'0' when interrupt_reg = '1' else
		'1' when capture_done = '0' else
		allow_frame_reg;

	pixel_valid_and_allowed <= '1' when pixel_valid = '1' and allow_frame_reg = '1' else '0';

	FRAME_SYNC_I: frame_sync
	port map (
		clk                       => Bus2IP_Clk,
		reset                     => Bus2IP_Reset,
		start_capture             => start_capture,
		capture_done              => capture_done,
		data_out                  => pixel,
		data_valid                => pixel_valid,
		cam_bytes                 => "00", -- was "10" to just get intensity and not full color
		cam_data_in               => cam_data,
		cam_frame_valid_in        => cam_frame_valid,
		cam_line_valid_in         => cam_line_valid,
		cam_pix_clk_in            => cam_pix_clk);

	CAM_SER_I: cam_ser
	port map (
		clk                       => Bus2IP_Clk,
		reset                     => Bus2IP_Reset,
		wr_en                     => cam_ser_we,
		rd_en                     => cam_ser_re,
		dev_addr                  => x"B8",
		reg_addr                  => cam_ser_addr,
		data_in                   => cam_ser_data_in,
		data_out                  => cam_ser_data_out,
		ready                     => cam_ser_ready,
		sdata_I                   => cam_sdata_I,--
		sdata_O                   => cam_sdata_O,--
		sdata_T                   => cam_sdata_T,--
		sclk                      => cam_sclk);--




  fifo_prog_full <= "000" & burst_size & "00";

  ORIGINAL_FRAME_FIFO: org_frame_fifo
    port map (
        din               => pix_word,
        prog_full_thresh  => fifo_prog_full,
        rd_clk            => Bus2IP_Clk,
        rd_en             => org_frame_fifo_rd_en,
        rst               => core_soft_reset,
        wr_clk            => Bus2IP_Clk,
        wr_en             => org_frame_fifo_wr_en,
        dout              => org_frame_fifo_out,
        empty             => org_frame_fifo_empty,
        full              => open,
        prog_full         => org_frame_fifo_ready);
  org_frame_fifo_wr_en   <= pix_word_valid and save_options(C_ORG_SAVE_INDEX);


  COLOR_CONVERT_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => hsv_partialword,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => hsv_frame_fifo_rd_en,
        rst                    => core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => hsv_frame_fifo_wr_en,
        dout                   => hsv_frame_fifo_out,
        empty                  => hsv_frame_fifo_empty,
        full                   => open,
        prog_full              => hsv_frame_fifo_ready);
  hsv_frame_fifo_wr_en  <= hsv_word_valid and save_options(C_HSV_SAVE_INDEX);
  hsv_partialword <= hsv_word(23 downto 8);


  BINARY_ORANGE_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => binary_orange_word,--binary_orange_word,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => mask_orange_frame_fifo_rd_en,
        rst                    => frame_reset,--core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => mask_orange_frame_fifo_wr_en,
        dout                   => mask_orange_frame_fifo_out,
        empty                  => mask_orange_frame_fifo_empty,
        full                   => open,
        prog_full              => mask_orange_frame_fifo_ready);
  mask_orange_frame_fifo_wr_en  <= binary_orange_word_valid and save_options(C_MASK_SAVE_INDEX);

  BINARY_GREEN_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => binary_green_word,--binary_green_word,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => mask_green_frame_fifo_rd_en,
        rst                    => frame_reset,--core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => mask_green_frame_fifo_wr_en,
        dout                   => mask_green_frame_fifo_out,
        empty                  => mask_green_frame_fifo_empty,
        full                   => open,
        prog_full              => mask_green_frame_fifo_ready);
  mask_green_frame_fifo_wr_en  <= binary_green_word_valid and save_options(C_MASK_SAVE_INDEX);

  CONV_ORANGE_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => conv_orange_fullword,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => conv_orange_frame_fifo_rd_en,
        rst                    => frame_reset,--core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => conv_orange_frame_fifo_wr_en,
        dout                   => conv_orange_frame_fifo_out,
        empty                  => conv_orange_frame_fifo_empty,
        full                   => open,
        prog_full              => conv_orange_frame_fifo_ready);
  conv_orange_frame_fifo_wr_en  <= conv_orange_fullword_valid and save_options(C_GREY_SAVE_INDEX);

  CONV_GREEN_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => conv_green_fullword,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => conv_green_frame_fifo_rd_en,
        rst                    => frame_reset,--core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => conv_green_frame_fifo_wr_en,
        dout                   => conv_green_frame_fifo_out,
        empty                  => conv_green_frame_fifo_empty,
        full                   => open,
        prog_full              => conv_green_frame_fifo_ready);
  conv_green_frame_fifo_wr_en  <= conv_green_fullword_valid and save_options(C_GREY_SAVE_INDEX);

  COLCOUNT_ORANGE_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => colcount_orange_fullword,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => colcount_orange_frame_fifo_rd_en,
        rst                    => core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => colcount_orange_frame_fifo_wr_en,
        dout                   => colcount_orange_frame_fifo_out,
        empty                  => colcount_orange_frame_fifo_empty,
        full                   => open,
        prog_full              => colcount_orange_frame_fifo_ready);
  colcount_orange_frame_fifo_wr_en  <= colcount_orange_fullword_valid and save_options(C_COL_COUNT_SAVE_INDEX);

  COLCOUNT_GREEN_FRAME_FIFO: org_frame_fifo
    port map (
        din                    => colcount_green_fullword,
        prog_full_thresh       => fifo_prog_full,
        rd_clk                 => Bus2IP_Clk,
        rd_en                  => colcount_green_frame_fifo_rd_en,
        rst                    => core_soft_reset,
        wr_clk                 => Bus2IP_Clk,
        wr_en                  => colcount_green_frame_fifo_wr_en,
        dout                   => colcount_green_frame_fifo_out,
        empty                  => colcount_green_frame_fifo_empty,
        full                   => open,
        prog_full              => colcount_green_frame_fifo_ready);
  colcount_green_frame_fifo_wr_en  <= colcount_green_fullword_valid and save_options(C_COL_COUNT_SAVE_INDEX);

  -- 1 byte to 2 byte converter
  -- Martial single bytes into full machine words
  RGBPIXEL_BYTES_TO_WORD: size_buffer
   generic map (
       C_IN_WIDTH  => 8,
       C_OUT_WIDTH => 16)
   port map (
       clk                      => Bus2IP_Clk,
       reset                    => core_soft_reset,
       in_valid                 => pixel_valid_and_allowed,
       data_in                  => pixel,
       out_valid                => pix_word_valid,
       data_out                 => pix_word);
  CONV_ORANGE_BYTES_TO_WORD: size_buffer
   generic map (
       C_IN_WIDTH  => 8,
       C_OUT_WIDTH => 16)
   port map (
       clk                      => Bus2IP_Clk,
       reset                    => frame_reset,
       in_valid                 => conv_orange_8bit_valid,
       data_in                  => conv_orange_8bit,
       out_valid                => conv_orange_fullword_valid,
       data_out                 => conv_orange_fullword);
  CONV_GREEN_BYTES_TO_WORD: size_buffer
   generic map (
       C_IN_WIDTH  => 8,
       C_OUT_WIDTH => 16)
   port map (
       clk                      => Bus2IP_Clk,
       reset                    => frame_reset,
       in_valid                 => conv_green_8bit_valid,
       data_in                  => conv_green_8bit,
       out_valid                => conv_green_fullword_valid,
       data_out                 => conv_green_fullword);
  COLCOUNT_ORANGE_BYTES_TO_WORD: size_buffer
   generic map (
       C_IN_WIDTH  => 8,
       C_OUT_WIDTH => 16)
   port map (
       clk                      => Bus2IP_Clk,
       reset                    => frame_reset,
       in_valid                 => colcount_orange_word_valid,
       data_in                  => colcount_orange_word,
       out_valid                => colcount_orange_fullword_valid,
       data_out                 => colcount_orange_fullword);
  COLCOUNT_GREEN_BYTES_TO_WORD: size_buffer
   generic map (
       C_IN_WIDTH  => 8,
       C_OUT_WIDTH => 16)
   port map (
       clk                      => Bus2IP_Clk,
       reset                    => frame_reset,
       in_valid                 => colcount_green_word_valid,
       data_in                  => colcount_green_word,
       out_valid                => colcount_green_fullword_valid,
       data_out                 => colcount_green_fullword);



-------------------------------------
--This entity converts data (sent from camera in 8-bit chunks)
--into HSV, and outputs the complete 24-bit value (HSV888)
	RGB_TO_HSV: hsv_buffer
		generic map
		(
			C_IN_WIDTH=>8,
			C_OUT_WIDTH=>24 --HSV 888 = 24 bits
		)
		port map
		(
			clk=>Bus2IP_Clk,
			reset=>core_soft_reset,
			in_valid=>pixel_valid_and_allowed,
			data_in=>pixel,
			out_valid=>hsv_word_valid,
			data_out=>hsv_word
		);


---------------------------------------
--Take the data from the HSV conversion and segment it by Hue, Sat, and Val
--Output '1' or '0' per pixel if the HSVs were in range
		GREEN_PYLON_SEGMENT: segment_buffer
		port map
		(
			clk=>Bus2IP_Clk,
			reset=>core_soft_reset,

			in_valid=>hsv_word_valid,
			data_in=>hsv_word,

			out_valid=>mask_green_onebit_valid,
			data_out=>mask_green_onebit,

			thresh_we=>thresh_seg_green_we,
			thresh_type_hue_we=>thresh_seg_type_hue_we,
			thresh_type_sat_we=>thresh_seg_type_sat_we,
			thresh_type_val_we=>thresh_seg_type_val_we,
			thresh_max=>thresh_max,
			thresh_min=>thresh_min,

			thresh_hue_min_out=>thresh_seg_green_hue_min_out,
			thresh_hue_max_out=>thresh_seg_green_hue_max_out,
			thresh_sat_min_out=>thresh_seg_green_sat_min_out,
			thresh_sat_max_out=>thresh_seg_green_sat_max_out,
			thresh_val_min_out=>thresh_seg_green_val_min_out,
			thresh_val_max_out=>thresh_seg_green_val_max_out
		);
		


	ORANGE_PYLON_SEGMENT: segment_buffer
		port map
		(
			clk=>Bus2IP_Clk,
			reset=>core_soft_reset,

			in_valid=>hsv_word_valid,
			data_in=>hsv_word,

			out_valid=>mask_orange_onebit_valid,
			data_out=>mask_orange_onebit,

			thresh_we=>thresh_seg_orange_we,
			thresh_type_hue_we=>thresh_seg_type_hue_we,
			thresh_type_sat_we=>thresh_seg_type_sat_we,
			thresh_type_val_we=>thresh_seg_type_val_we,
			thresh_max=>thresh_max,
			thresh_min=>thresh_min,

			thresh_hue_min_out=>thresh_seg_orange_hue_min_out,
			thresh_hue_max_out=>thresh_seg_orange_hue_max_out,
			thresh_sat_min_out=>thresh_seg_orange_sat_min_out,
			thresh_sat_max_out=>thresh_seg_orange_sat_max_out,
			thresh_val_min_out=>thresh_seg_orange_val_min_out,
			thresh_val_max_out=>thresh_seg_orange_val_max_out
		);
		
	GREEN_BINARY_TO_BYTE: size_buffer_onebit --convert the 1-bit to an 8-bit
		generic map (
			C_OUT_WIDTH => 16)
		port map (
			clk						=> Bus2IP_Clk,
			reset					=> frame_reset,
			in_valid				=> binary_green_onebit_valid,--mask_green_onebit_valid,--conv_green_onebit_valid,
			data_in					=> binary_green_onebit,--mask_green_onebit,--conv_green_onebit,
			out_valid				=> binary_green_word_valid,
			data_out				=> binary_green_word);

	ORANGE_BINARY_TO_BYTE: size_buffer_onebit --convert the 1-bit to an 8-bit
		generic map (
			C_OUT_WIDTH => 16)
		port map (
			clk						=> Bus2IP_Clk,
			reset					=> frame_reset,
			in_valid				=> binary_orange_onebit_valid,--mask_orange_onebit_valid,--conv_orange_onebit_valid,
			data_in					=> binary_orange_onebit,--mask_orange_onebit,--conv_orange_onebit,
			out_valid				=> binary_orange_word_valid,
			data_out				=> binary_orange_word);

	--choose which to use when outputting a binary channel: segment mask or convolution mask
	binary_green_onebit_valid  <= conv_green_onebit_valid;
	binary_green_onebit        <= conv_green_onebit;
	binary_orange_onebit_valid <= conv_orange_onebit_valid;
	binary_orange_onebit       <= conv_orange_onebit;

	thresh_max <= Bus2IP_Data(31-15 to 31-8);
	thresh_min <= Bus2IP_Data(31-7 to 31);

	
	CONV_ORANGE: conv_buffer
		generic map		(
			C_DATA_WIDTH			=> 8
		)
		port map	(
			clk						=> Bus2IP_Clk,
			reset					=> core_soft_reset,--core_soft_reset,
			frame_reset				=> frame_reset,--core_soft_reset,
			conv_valid_in 			=> mask_orange_onebit_valid,
			conv_din 				=> mask_orange_onebit,
			conv_data_out			=> conv_orange_word,
			conv_valid_out			=> conv_orange_word_valid,
		  
			reg_val_in				=> conv_reg_val,
			col_init_we				=> conv_col_init_we,
			row_init_we				=> conv_row_init_we,
			between_frames_init_we	=> conv_between_frames_init_we,
			initial_invalid_init_we	=> conv_initial_invalid_init_we,
			initial_zero_init_we	=> conv_initial_zero_init_we,
			zeros_val_we			=> conv_zeros_val_we,
	  
			min_valid				=> thresh_postconvolution_orange_we,
			min_in					=> thresh_min,
			max_valid				=> convolution_seg_thresh_max_valid,
			max_in					=> convolution_seg_thresh_max,
			min_out					=> orange_convolution_seg_thresh_min_out,
			max_out					=> open,

			thresh_out_valid		=> conv_orange_onebit_valid,
			thresh_data_out			=> conv_orange_onebit);

	CONV_GREEN: conv_buffer
		generic map		(
			C_DATA_WIDTH			=> 8
		)
		port map	(
			clk						=> Bus2IP_Clk,
			reset					=> core_soft_reset,--core_soft_reset,
			frame_reset				=> frame_reset,--core_soft_reset,
			conv_valid_in 			=> mask_green_onebit_valid,
			conv_din 				=> mask_green_onebit,
			conv_data_out			=> conv_green_word,
			conv_valid_out			=> conv_green_word_valid,
		  
			reg_val_in				=> conv_reg_val,
			col_init_we				=> conv_col_init_we,
			row_init_we				=> conv_row_init_we,
			between_frames_init_we	=> conv_between_frames_init_we,
			initial_invalid_init_we	=> conv_initial_invalid_init_we,
			initial_zero_init_we	=> conv_initial_zero_init_we,
			zeros_val_we			=> conv_zeros_val_we,
	  
			min_valid				=> thresh_postconvolution_green_we,
			min_in					=> thresh_min,
			max_valid				=> convolution_seg_thresh_max_valid,
			max_in					=> convolution_seg_thresh_max,
			min_out					=> green_convolution_seg_thresh_min_out,
			max_out					=> open,

			thresh_out_valid		=> conv_green_onebit_valid,
			thresh_data_out			=> conv_green_onebit);

	conv_green_8bit_valid <= conv_green_onebit_valid;
	conv_green_8bit <= conv_green_onebit & "0000000";
	conv_orange_8bit_valid <= conv_orange_onebit_valid;
	conv_orange_8bit <= conv_orange_onebit & "0000000";
	
	conv_reg_val <= Bus2IP_Data(31-15 to 31-0);
	convolution_seg_thresh_max_valid <= '1';
	convolution_seg_thresh_max <= std_logic_vector(to_unsigned(255,8));
end IMP;
