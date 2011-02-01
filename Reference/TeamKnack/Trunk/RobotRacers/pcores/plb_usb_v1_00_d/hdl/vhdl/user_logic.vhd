
-- user_logic.vhd - entity/architecture pair
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
-- Filename:          user_logic.vhd
-- Version:           1.00.a
-- Description:       User logic.
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

-- DO NOT EDIT BELOW THIS LINE --------------------
library ieee;
use ieee.std_logic_1164.all;
use ieee.std_logic_arith.all;
use ieee.std_logic_unsigned.all;
use ieee.numeric_std.all;

--library proc_common_v1_00_b;
--use proc_common_v1_00_b.proc_common_pkg.all;

--library plb_usb_v1_00_d;
--use plb_usb_v1_00_d.all;
-- DO NOT EDIT ABOVE THIS LINE --------------------

--USER libraries added here

------------------------------------------------------------------------------
-- Entity section
------------------------------------------------------------------------------
-- Definition of Generics:
--   C_AWIDTH                     -- User logic address bus width
--   C_DWIDTH                     -- User logic data bus width
--   C_NUM_CE                     -- User logic chip enable bus width
--
-- Definition of Ports:
--   Bus2IP_Clk                   -- Bus to IP clock
--   Bus2IP_Reset                 -- Bus to IP reset
--   Bus2IP_Addr                  -- Bus to IP address bus
--   Bus2IP_Data                  -- Bus to IP data bus for user logic
--   Bus2IP_BE                    -- Bus to IP byte enables for user logic
--   Bus2IP_RdCE                  -- Bus to IP read chip enable for user logic
--   Bus2IP_WrCE                  -- Bus to IP write chip enable for user logic
--   Bus2IP_RdReq                 -- Bus to IP read request
--   Bus2IP_WrReq                 -- Bus to IP write request
--   IP2Bus_Data                  -- IP to Bus data bus for user logic
--   IP2Bus_Retry                 -- IP to Bus retry response
--   IP2Bus_Error                 -- IP to Bus error response
--   IP2Bus_ToutSup               -- IP to Bus timeout suppress
--   IP2Bus_Busy                  -- IP to Bus busy response
--   IP2Bus_RdAck                 -- IP to Bus read transfer acknowledgement
--   IP2Bus_WrAck                 -- IP to Bus write transfer acknowledgement
------------------------------------------------------------------------------

entity user_logic is
  generic
  (
    -- ADD USER GENERICS BELOW THIS LINE ---------------
    C_USB_DATA_WIDTH               : integer              := 16;
    C_BASEADDR                     : std_logic_vector     := X"00000000";
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
    dcm_locked  : in  std_logic;
    dcm_reset   : out std_logic;
    
    if_clk      : in  std_logic;
    
    usb_full_n  : in  std_logic;      -- IN FIFO full flag
    usb_empty_n : in  std_logic;      -- OUT FIFO empty flag
    usb_alive   : in  std_logic;      -- USB chip GPIO, high when device active

    Interrupt   : out std_logic;
    sloe_n      : out std_logic;         -- USB FIFO slave operate enable
    slrd_n      : out std_logic;         -- USB FIFO slave read enable
    slwr_n      : out std_logic;         -- USB FIFO slave write enable

    pktend_n    : out std_logic;                        -- USB FIFO packet end
    fifoaddr    : out std_logic_vector(1 downto 0);     -- USB FIFO address
    fd_I        : in  std_logic_vector(C_USB_DATA_WIDTH-1 downto 0); -- USB FIFO data
    fd_O        : out std_logic_vector(C_USB_DATA_WIDTH-1 downto 0); -- USB FIFO data
    fd_T        : out std_logic_vector(C_USB_DATA_WIDTH-1 downto 0); -- USB FIFO data
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

  component plb_burst_fifo
    port (
      din       : IN std_logic_VECTOR(63 downto 0);
      rd_clk    : IN std_logic;
      rd_en     : IN std_logic;
      rst       : IN std_logic;
      wr_clk    : IN std_logic;
      wr_en     : IN std_logic;
      dout      : OUT std_logic_VECTOR(15 downto 0);
      empty     : OUT std_logic;
      full      : OUT std_logic);
  end component;        


  
  -- Bus decode signals
  signal addr_sel                       : std_logic_vector(3 downto 0);

  -- Core status signals
  signal status                         : std_logic_vector(7 downto 0);

  -- USB core signals
  signal recv_empty                     : std_logic;
  signal recv_rd_en                     : std_logic;
  signal recv_dout                      : std_logic_vector(C_USB_DATA_WIDTH-1 downto 0);
  signal send_full                      : std_logic;
  signal send_empty                     : std_logic;
  signal send_wr_en                     : std_logic;
  signal send_din                       : std_logic_vector(C_USB_DATA_WIDTH-1 downto 0);
  signal commit                         : std_logic;
  signal usb_ready                      : std_logic;

  signal single_send_wr_en              : std_logic;
    
  -- PLB Burst signals
  signal dma_start_addr                 : std_logic_vector(C_AWIDTH-1 downto 0);
  signal dma_start_addr_wr_en           : std_logic;
  signal dma_start_addr_counter         : std_logic_vector(C_AWIDTH-1 downto 0);

  
  signal dma_trans_size                 : std_logic_vector(31 downto 0);
  signal dma_trans_size_wr_en           : std_logic;
  signal dma_trans_size_remaining       : std_logic_vector(31 downto 0);
  
  


  -- PLB Burst fifo control signals
  signal pbf_rd_en                      : std_logic;
  signal pbf_wr_en                      : std_logic;
  signal pbf_out                        : std_logic_vector(15 downto 0);
  signal pbf_empty                      : std_logic;
  signal pbf_full                       : std_logic;

  signal pbf_rd_en_delayed              : std_logic;  -- this is the pbf_rd_en delayed
                                              -- by one clock cycle to be used
                                              -- as the input to the usb_core
                                              -- send_wr_en signal

  signal dma_session_active             : std_logic;
  signal initiate_dma_transfer          : std_logic;
  signal terminate_dma_transfer         : std_logic;
  
  type fg_state_type is                 ( DMA_SESSION_IDLE,
                                          DMA_SESSION_ASSERT,
                                          DMA_SESSION_WAIT);

  signal current_state, next_state      : fg_state_type;

  signal base_addr                      : std_logic_vector(31 downto 0);
  signal mstrdreq                       : std_logic;
  signal single_commit                  : std_logic;
  signal burst_size                     : std_logic_vector(4 downto 0);
  signal burst_size_wr_en               : std_logic;
  
begin
  base_addr <= C_BASEADDR;
  
  -- Extract portion of address to be used
  addr_sel <= Bus2IP_Addr(C_AWIDTH-7 to C_AWIDTH-4);

  -- Generate status signal
  status <= mstrdreq & dma_session_active & pbf_empty & usb_ready &       recv_empty & '0' & send_empty & send_full;
  Interrupt <= not recv_empty;                                                      
    
  -----------------------------------------------------------------------------
  -- Data In/Out Steering Process
  DATA_ROUTE: process (Bus2IP_RdReq,Bus2IP_WrReq,Bus2IP_Data,addr_sel,recv_dout)
  begin
    -- Default values
    IP2Bus_Data                 <= (others => '0');
    IP2Bus_RdAck                <= '0';
    IP2Bus_WrAck                <= '0';

    -- USB core signals
    single_commit               <= '0';
    recv_rd_en                  <= '0';
    single_send_wr_en           <= '0';
    pbf_wr_en                   <= '0';
    initiate_dma_transfer       <= '0';
    dma_start_addr_wr_en        <= '0';
    dma_trans_size_wr_en        <= '0';
    burst_size_wr_en            <= '0';
    
    -- Handle bus reads
    if Bus2IP_RdReq = '1' then
      Ip2Bus_RdAck <= '1';

      case addr_sel is
        when "1000" =>   -- Status read
          IP2Bus_Data(0 to 31) <=  conv_std_logic_vector(0, 32-status'length) & status;
                                
        when "1001" =>   -- Data read         
          recv_rd_en <= '1';
          IP2Bus_Data(0 to 15) <= recv_dout;

        when "1010" => -- DMA Start Address
          IP2Bus_Data(0 to 31) <= dma_start_addr;

        when "1011" => -- DMA Start Address
          IP2Bus_Data(0 to 31) <= dma_trans_size;

        when "1100" => -- DMA Start Address
          IP2Bus_Data(0 to 31) <= dma_start_addr;  

        when "1101" =>
          IP2Bus_Data(0 to 31) <= dma_start_addr_counter;  
          
        when others => null;                       
      end case;
    end if;

   
    -- Handle bus writes
    if Bus2IP_WrReq = '1' then
      IP2Bus_WrAck <= '1'; 

      if mstrdreq = '1' then
        pbf_wr_en <= '1';
      else
        
        case addr_sel is
          when "1000" => -- Control write
            if Bus2IP_Data(31) = '1' then
              single_commit       <= '1';
              single_send_wr_en   <=  '1';
            else
              initiate_dma_transfer <= '1';
            end if;

          when "1001" => -- Data write
            single_send_wr_en <= '1';         
            
          when "1010" => null;
                         burst_size_wr_en <= '1';

          when "1011" => 
            dma_trans_size_wr_en <= '1';

          when "1100" =>
            dma_start_addr_wr_en <= '1';
            
          when others => null;
        end case;
      end if;               
    end if;
  end process;


-------------------------------------------------------------------------------
-- DMA Transfer Registers
--
-- These registers store the start address and the number of 16 bit words to
-- transfer. 
--
-- Since the DATA_ROUTE process is not clocked, the signals need to be clocked
-- into a register which is done here
--
-- These are only loaded if we are not currently in an Active DMA Session.
-- 
-------------------------------------------------------------------------------
  process(Bus2IP_Clk,Bus2IP_Reset,dma_start_addr_wr_en)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        dma_start_addr <= (others => '0');
      else
        if dma_start_addr_wr_en = '1' and (dma_session_active = '0') then
          dma_start_addr <= Bus2IP_Data(0 to 31);
        end if;
      end if;
    end if;
  end process; 
  
  process(Bus2IP_Clk,Bus2IP_Reset,dma_trans_size_wr_en)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        dma_trans_size <= (others => '0');
      else
        if dma_trans_size_wr_en = '1' and (dma_session_active = '0') then
          dma_trans_size <= Bus2IP_Data(0 to 31);
        end if;
      end if;
    end if;
  end process; 


-------------------------------------------------------------------------------
-- DMA Transfer Size Remaining
--
-- The dma_trans_size_remaining signal stores the number of 16 bit words that
-- need to be send to the USB Core. 
-- 
-- The dma_trans_size_reamaing signal is loaded with the value stored in the
-- dma_trans_size register when an DMA Session is initiated.
-- 
-- Each time a 16 bit value is clocked out of the PLB Burst Fifo, the value in
-- the dma_trans_size_remaining is decremented by one. 
--   
-------------------------------------------------------------------------------  

  process(Bus2IP_Clk,Bus2IP_Reset,initiate_dma_transfer,pbf_rd_en)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        dma_trans_size_remaining <= (others => '0');
      else
        if initiate_dma_transfer = '1' and (dma_session_active = '0') then
          dma_trans_size_remaining <= dma_trans_size;
        else
          if pbf_rd_en = '1' then
            dma_trans_size_remaining <= dma_trans_size_remaining - 1;
          end if;
        end if;
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
 -- DMA Start Address Counter
 -- 
 -- The dma_start_addr_counter is used to assert the the IP2BUS_Addr signal which tells
 -- the PLB Bus where to read from memory
 --
 -- It is loaded with the intial value stored in the dma_start_addr register at
 -- the start if the Entire DMA Session
 --
 -- At the end of each individual DMA Sub-Session, there is a single clock
 -- cycle assertion of the Bus2IP_MstLastAck Signal which indicates that the
 -- burst transaction is completed. When that signal is asserted, we can safely
 -- increment the start address for the next burst 
 --  
 -------------------------------------------------------------------------------  

  process(Bus2IP_Clk,Bus2IP_Reset)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        dma_start_addr_counter <= (others => '0');
      else        
        if initiate_dma_transfer = '1' and (dma_session_active = '0') then
          dma_start_addr_counter <= dma_start_addr;
        else
          if dma_session_active = '1' and Bus2IP_MstLastAck = '1' then
            case burst_size is
                when "00001" => dma_start_addr_counter <= dma_start_addr_counter + 8;
                when "00010" => dma_start_addr_counter <= dma_start_addr_counter + 16;
                when "00100" => dma_start_addr_counter <= dma_start_addr_counter + 32;
                when "01000" => dma_start_addr_counter <= dma_start_addr_counter + 64;
                when "10000" => dma_start_addr_counter <= dma_start_addr_counter + 128;
                when others => null;
            end case;
          end if;          
        end if;
      end if;
    end if;
  end process; 

  
 -------------------------------------------------------------------------------
 -- DMA session encapsalator
 --
 -- When the dma_session_active signal is HIGH then we are currently
 -- conducting multiple DMA Sub-Sessions.
 --
 -- Each DMA Sub-Session is a single PLB Burst Read session in which the PLB
 -- will burst data from memory into the PLB_Burst_Fifo. 
 --
 -- With each DMA Sub-Session, the count of remaining bytes to be transfered
 -- will be reduced slowly toward zero.
 --
 -- The dma_session_active signal will stay high as long as the count of bytes
 -- remaing (dma_trans_size_remaining) is greater than zero and the
 -- PLB_Burst_Fifo is not empty. Since the dma_session_active signal will stay
 -- HIGH until the PLB_Burst_Fifo is empty, the dma_session_active will only go
 -- LOW when all the data has been transfered from memory to the USB chip. 
 --
 -- Since the dma_session_active signal indicates that there is still data to
 -- transfer, it is used to initiate individual DMA Sub-Sessions. After each
 -- DMA Sub-Session has completed it will check to see if the
 -- dma_session_active signal is HIGH. If it is HIGH, then it will begin
 -- another DMA Sub-Session.
 --
 --   
 -------------------------------------------------------------------------------
  process(dma_session_active,dma_trans_size_remaining,pbf_empty)
  begin
    terminate_dma_transfer <= '0';
    if (dma_session_active = '1') and (dma_trans_size_remaining = X"00000000") and (pbf_empty = '1') and (send_full = '0')then
      terminate_dma_transfer <= '1';      
    end if;
  end process;
    
  process(Bus2IP_Clk)
  begin
    if Bus2IP_CLK'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        dma_session_active <= '0';
      else
        if initiate_dma_transfer = '1' then
          dma_session_active <= '1';
        elsif terminate_dma_transfer = '1' then
          dma_session_active <= '0';
        end if;
      end if;
    end if;
  end process;
  

 -- VirtualAssist plugin for visual studio 
  
 ------------------------------------------------------------------------------
 -- DMA Sub-Session State Machine
 --
 -- STATES:
 --  1: Idle   - This state waits until the dma_session_active is 1
 --  2: Assert - This state waits until the Bus2IP_MstLastAck is 1
 --  3: Wait   - This state waits until the PLB Burst FIFO is empty
 --
 --
 --
 -- 
 -----------------------------------------------------------------------------
  
  process(Bus2IP_Clk,Bus2IP_Reset,next_state)
  begin
    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
      if Bus2IP_Reset = '1' then
        current_state <= DMA_SESSION_IDLE;
      else
        current_state <= next_state;
      end if;
    end if;
  end process; 

  process(Bus2IP_Clk,dma_session_active,Bus2IP_MstLastAck,pbf_empty)
  begin
    next_state <= current_state;
    
    IP2Bus_MstRdReq     <= '0';
    IP2Bus_MstWrReq     <= '0';
    IP2Bus_MstBurst     <= '0';
    IP2Bus_MstBusLock   <= '0';
    IP2Bus_MstNum       <= (others => '0');
    IP2Bus_Addr         <= (others => '0');
    IP2Bus_MstBE        <= (others => '0');
    IP2IP_Addr          <= (others => '0');
    IP2Bus_ToutSup      <= '0';

    mstrdreq        <= '0';
      
    case current_state is
      when DMA_SESSION_IDLE =>                  
               if dma_session_active = '1' and send_empty = '1'then
                 next_state <= DMA_SESSION_ASSERT;
               end if;
               
      when DMA_SESSION_ASSERT => 
               IP2Bus_MstRdReq          <= '1';
               IP2Bus_MstBurst          <= '1';
               IP2Bus_MstBusLock        <= '0';
               IP2Bus_MstNum            <= burst_size;
               IP2Bus_Addr              <= dma_start_addr_counter;
               IP2Bus_MstBE             <= "11111111";
               IP2IP_Addr               <= C_BASEADDR;
               IP2Bus_ToutSup           <= '0';
               
               mstrdreq                 <= '1';
               
               if Bus2IP_MstLastAck = '1' then
                 next_state <= DMA_SESSION_WAIT;
               end if;
               
      when DMA_SESSION_WAIT =>              
               if pbf_empty = '1' then
                 next_state <= DMA_SESSION_IDLE;
               end if;
               
      when others => null;
    end case;
  end process; 
  
 ------------------------------------------------------------------------------
 -- PLB Burst Fifo
 --
 -- The PLB Burst Fifo has a 64 bit input that is written to from the PLB Bus.
 -- 
 -- The output of the PLB Burst Fifo is connected to the USB Core chip. The
 -- fifo will clock out the 16 bit data whenever the FIFO is not empty and the
 -- USB Core chip is not have a full send FIFO
 --
 -- The FIFO has a 1 clock cycle delay from the time that the rd_en signal is
 -- asserted until the data out is valid. So a second process is used to delay
 -- the rd_en signal one clock cycle. That delayed signal is then used as the
 -- wr_en for the USB Core. The single_send_wr_en is to allow single 16 bit
 -- words to be written to the USB Core from software.
 --
 -- A third process is used to control what data is put onto the data bus that
 -- goes into the USB Core. This is needed because this user_logic core needs
 -- to be able to write single 16 bit words to the USB Core and Data from DMA
 -- Session transactions
 -- 
 ------------------------------------------------------------------------------
  
  PLB_BURST_FIFO1: plb_burst_fifo
    port map (
      din       => Bus2IP_Data,
      rd_clk    => Bus2IP_Clk,
      rd_en     => pbf_rd_en,
      rst       => Bus2IP_Reset,
      wr_clk    => Bus2IP_Clk,
      wr_en     => pbf_wr_en,
      dout      => pbf_out,
      empty     => pbf_empty,
      full      => pbf_full);          


  process(send_full,pbf_empty,pbf_out,Bus2IP_Data)
  begin
    send_din    <= Bus2IP_Data(0 to C_USB_DATA_WIDTH-1);
    pbf_rd_en   <= '0';

    if send_full = '0' and pbf_empty = '0' then
      send_din        <= pbf_out;
      pbf_rd_en       <= '1';      
    end if;        
  end process;
  send_wr_en    <= pbf_rd_en or single_send_wr_en or (terminate_dma_transfer);
  commit        <= single_commit or (terminate_dma_transfer);
  
--   process(send_full,pbf_empty,pbf_out,Bus2IP_Data,mstrdreq,Bus2IP_WrReq,Bus2IP_MstLastAck)
--   begin
--     send_din    <= Bus2IP_Data(0 to C_USB_DATA_WIDTH-1);
--     pbf_rd_en   <= '0';

--     if Bus2IP_MstLastAck = '1' then
--       send_din <= X"AB" & status;
--     else
--       if mstrdreq = '1' then
--         if Bus2IP_WrReq = '1' then
--           send_din <= X"56" & status;
--         else
--           send_din <= X"12" & status;
--         end if;
--       end if;
--     end if;
--   end process;
--   send_wr_en <= pbf_rd_en or single_send_wr_en or mstrdreq or Bus2IP_MstLastAck;  -- for debug
  -----------------------------------------------------------------------------
  -- 
  -----------------------------------------------------------------------------
  
  ------------------------------------------
  -- Example code to drive IP to Bus signals
  ------------------------------------------
  IP2Bus_Busy        <= '0';
  IP2Bus_Error       <= '0';
  IP2Bus_Retry       <= '0';
  


 USB_CORE_I: entity plb_usb_v1_00_d.usb_core
    generic map (
        C_IN_FIFOADDR  => "10",
        C_OUT_FIFOADDR => "00",
        C_BIG_ENDIAN   => TRUE,
        C_DATA_WIDTH   => C_USB_DATA_WIDTH,
        C_MAX_TRANSFER => 16,
        C_RST_HIGH     => TRUE)
    port map (
        clk         => Bus2IP_Clk,
        rst         => Bus2IP_Reset,
        dcm_locked  => dcm_locked,
        dcm_reset   => dcm_reset,
        recv_empty  => recv_empty,
        recv_rd_en  => recv_rd_en,
        recv_dout   => recv_dout,
        send_full   => send_full,
        send_empty  => send_empty,
        send_wr_en  => send_wr_en,
        send_din    => send_din,
        commit      => commit,
        usb_ready   => usb_ready,
        if_clk      => if_clk,
        usb_full_n  => usb_full_n,
        usb_empty_n => usb_empty_n,
        usb_alive   => usb_alive,
        sloe_n      => sloe_n,
        slrd_n      => slrd_n,
        slwr_n      => slwr_n,
        pktend_n    => pktend_n,
        fifoaddr    => fifoaddr,
        fd_I        => fd_I,
        fd_O        => fd_O,
        fd_T        => fd_T);

end IMP;




-- Template Clocked Process

-------------------------------------------------------------------------------
--  
-------------------------------------------------------------------------------
--  process(Bus2IP_Clk,Bus2IP_Reset)
--  begin
--    if Bus2IP_Clk'event and Bus2IP_Clk = '1' then
--      if Bus2IP_Reset = '1' then
--      else
--      end if;
--    end if;
--  end process; 

