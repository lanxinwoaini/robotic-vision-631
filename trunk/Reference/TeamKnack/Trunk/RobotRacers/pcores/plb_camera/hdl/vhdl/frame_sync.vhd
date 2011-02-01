-------------------------------------------------------------------------------
-- Frame Grabber for Micron Cameras
--
-- FILE: frame_sync.vhd
-- AUTHOR: Wade S. Fife
-- DATE: June 9, 2004
-- MODIFIED: March 11, 2005
--
-- DESCRIPTION
-- 
-- Provides interface to frame data output on Micron MT9V111 digital
-- camera. When start_capture is asserted the entity waits for the next frame
-- to begin by monitoring the cam_frame_valid_in input (FRAME_VALID on the
-- camera). Bytes are transfered one at a time using the data_out and
-- data_valid outputs.
--
-- The cam_bytes input indicates which bytes of the camera data are transfered.
-- All camera color modes output two bytes per pixel. The cam_bytes can be
-- configured to transfer all bytes bytes (cam_bytes = "00"), only odd bytes
-- (byte_ comp = "01", and only even bytes (cam_bytes = "10"). This is useful,
-- for example, if you only want the Y component (intensity) transfered for
-- grayscale images. To be clear, the first byte (byte 0) is considered an even
-- byte.
--
-------------------------------------------------------------------------------

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

entity frame_sync is
  
  port (
    clk                : in std_logic;
    reset              : in std_logic;

    -- Frame capture interface
    start_capture      : in std_logic;
    capture_done       : out std_logic;
    data_out           : out std_logic_vector(0 to 7);
    data_valid         : out std_logic;

    -- Control interface
    cam_bytes          : in std_logic_vector(0 to 1);
    
    -- Camera interface
    cam_data_in        : in std_logic_vector(0 to 7);
    cam_frame_valid_in : in std_logic;
    cam_line_valid_in  : in std_logic;
    cam_pix_clk_in     : in std_logic);

end frame_sync;

architecture imp of frame_sync is
  -- States
  type fg_state_type is ( GRAB_IDLE,
                          GRAB_WAIT_FRAME_END,
                          GRAB_WAIT_FRAME_START,
                          GRAB_WAIT_FIRST_LINE,
                          GRAB_WAIT_NEXT_LINE,
                          GRAB_WAIT_PIXEL,
                          GRAB_WAIT_PIXEL_END,
                          GRAB_WRITE);

  signal current_state, next_state : fg_state_type;

  signal pixel_comp_reg : std_logic;    -- Tracks odd/even bytes (the Y comp)
  signal pixel_comp_inc : std_logic;    -- Signal to increment pixel_comp_reg
  signal pixel_comp_clear : std_logic;  -- Signal to clear pixel_comp_reg
  
  signal data_out_load : std_logic;     -- Signal to latch pixel data

  signal cam_data : std_logic_vector(0 to 7);
  signal cam_frame_valid : std_logic;
  signal cam_line_valid : std_logic;
  signal cam_pix_clk : std_logic;
  
begin

  REGISTER_CAM_INPUTS : process (clk, reset, cam_data_in, cam_frame_valid_in,
                                 cam_line_valid_in, cam_pix_clk_in)
  begin
    if reset = '1' then
      cam_data <= (others => '0');
      cam_frame_valid <= '0';
      cam_line_valid <= '0';
      cam_pix_clk <= '0';
    elsif clk'event and clk='1' then
      cam_data <= cam_data_in;
      cam_frame_valid <= cam_frame_valid_in;
      cam_line_valid <= cam_line_valid_in;
      cam_pix_clk <= cam_pix_clk_in;    
    end if;
  end process REGISTER_CAM_INPUTS;

  
  FRAME_GRAB_REG: process (clk, reset, pixel_comp_clear, pixel_comp_inc,
                           data_out_load, cam_data)
  begin
    if reset = '1' then                 -- asynchronous reset (active low)
      current_state <= GRAB_IDLE;
    elsif clk'event and clk = '1' then  -- rising clock edge
      current_state <= next_state;

      -- Toggle pixel component count
      if pixel_comp_clear = '1' then
        pixel_comp_reg <= '0';
      elsif pixel_comp_inc = '1' then
        pixel_comp_reg <= not pixel_comp_reg;
      end if;

      -- Latch pixel data
      if data_out_load = '1' then
        data_out <= cam_data;
      end if;
    end if;
  end process FRAME_GRAB_REG;

  
  FRAME_GRAB_COMB: process (current_state, start_capture, cam_frame_valid,
                            cam_line_valid, cam_pix_clk, pixel_comp_reg, cam_bytes)
  begin
    -- Default signal values
    next_state <= current_state;
    pixel_comp_clear <= '0';
    pixel_comp_inc <= '0';
    data_out_load <= '0';
    data_valid <= '0';
    capture_done <= '0';

    case current_state is
      when GRAB_IDLE =>               -- Wait for grab frame command
        capture_done <= '1';
        if start_capture = '1' then
          pixel_comp_clear <= '1';
          next_state <= GRAB_WAIT_FRAME_END;
        end if;

      when GRAB_WAIT_FRAME_END =>      -- Wait for frame_valid to go low
        if cam_frame_valid='0' then 
          next_state <= GRAB_WAIT_FRAME_START;
        end if;

      when GRAB_WAIT_FRAME_START =>   -- Wait for start of frame
        if cam_frame_valid='1' then
          next_state <= GRAB_WAIT_FIRST_LINE;
        end if;

      -- Note: States WAIT_FIRST_LINE and WAIT_NEXT_LINE are made
      -- distinct to avoid an apparent bounce problem in signal
      -- cam_frame_valid when it goes from low to high. Accordingly,
      -- WAIT_FIRST_LINE doesn't check if cam_frame_valid has gone low.
      when GRAB_WAIT_FIRST_LINE =>     -- Wait for start of the first line
        if cam_line_valid='1' then
          next_state <= GRAB_WAIT_PIXEL;
        end if;

      when GRAB_WAIT_NEXT_LINE =>     -- Wait for start of next line
        -- Are we done with frame?
        if cam_frame_valid='0' then
          next_state <= GRAB_IDLE;
        elsif cam_line_valid='1' then
          -- Could make it go to GRAB4 to make sure pix_clk is low first
          next_state <= GRAB_WAIT_PIXEL;
        end if;

      when GRAB_WAIT_PIXEL =>         -- Wait for pix clk edge
        -- Are we done with line?
        if cam_line_valid='0' then
          next_state <= GRAB_WAIT_NEXT_LINE;

        -- Deal with new pixel data
        elsif cam_pix_clk='1' then
          pixel_comp_inc <= '1';
          next_state <= GRAB_WAIT_PIXEL_END;
          -- If pixel data is correct component (both, odd, or even)
          if cam_bytes = "00" or cam_bytes(1) = pixel_comp_reg then
            data_out_load <= '1';
            next_state <= GRAB_WRITE;
          end if;
        end if;

      when GRAB_WAIT_PIXEL_END =>     -- Wait for pix clk to go low
        if cam_pix_clk='0' then
          next_state <= GRAB_WAIT_PIXEL;
        end if;

      when GRAB_WRITE =>              -- Assert pixel valid for 1 cycle
        data_valid <= '1';
        next_state <= GRAB_WAIT_PIXEL_END;

      when others =>
        next_state <= current_state;
        
    end case;

  end process FRAME_GRAB_COMB;

end imp;
