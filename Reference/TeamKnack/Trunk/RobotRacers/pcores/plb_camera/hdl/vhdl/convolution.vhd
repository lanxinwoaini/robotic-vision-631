	      library ieee;
    use ieee.std_logic_1164.all;
    use ieee.numeric_std.all;
    entity convolution is 
    generic (
        C_ROWS : integer := 480;
        C_COLS: integer := 640;
		CONV_HEIGHT : integer := 33;
		CONV_WIDTH : integer := 3;
        C_WIDTH : integer := 1);
    port(
		clk, rst 				: in std_logic;
		frame_reset				: in std_logic;
		valid_in 				: in std_logic;
		din 					: in std_logic;

		reg_val_in				: in std_logic_vector(15 downto 0);
		col_init_we				: in std_logic;
		row_init_we				: in std_logic;
		between_frames_init_we	: in std_logic;
		initial_invalid_init_we	: in std_logic;
		initial_zero_init_we	: in std_logic;
		zeros_val_we			: in std_logic;

		data_out				: out std_logic_vector(7 downto 0); --this output will output the total pixel count in each convolution frame. This output should be attached to a threshold block to produce a single pixel denoting whether the count satisfies the designated threshold value.
		valid_out				: out std_logic);
    end convolution;
    
    architecture conv_arch of convolution is
		
	component line_buffer 
	  generic (
		C_DEPTH : integer := C_COLS-1;
		C_WIDTH : integer := 16);
	  port (
		clk   : in  std_logic;
		rst   : in  std_logic;
		din   : in  std_logic_vector(C_WIDTH-1 downto 0);
		wr_en : in  std_logic;
		dout  : out std_logic_vector(C_WIDTH-1 downto 0);
		valid : out std_logic);
	end component;	
    
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
      
  function vectorize(s: std_logic) return std_logic_vector is
	     variable v: std_logic_vector(0 downto 0);
	     begin
	     v(0) := s;
	     return v;
  end function vectorize;
    
   type states is 
      (first_frame_invalid, first_frame_zero, produce_zeroes, produce_pixel, skip_edges1, skip_edges2);
   type intarray33 is array (32 downto 0) of integer range 0 to 255;
   
   type intarray11 is array (10 downto 0) of integer range 0 to 255;
   
   type intarray4 is array (3 downto 0) of integer range 0 to 255;

   type intarray2 is array (1 downto 0) of integer range 0 to 255;
   
   
   --last destination is 'sum'
   
	  signal state_reg, state_next: states;
	  signal left_FFs_reg,  middle_FFs_reg,  right_FFs_reg, right_FFs_next  : std_logic_vector(CONV_HEIGHT-1 downto 0); -- signals to store the 3 FFs associated with each line buffer.
      
	  
	  
      signal adder_s1 : intarray33;
	  signal adder_s2 : intarray11;
	  signal adder_s3 : intarray4;
	  signal adder_s4 : intarray2;
	  
      signal col_count_next, col_count_reg: unsigned(15 downto 0); --keeps track of which column we're currently on. 
      signal col_init: unsigned(15 downto 0); --when we reset the column count, this is the value that we reset it to.
      signal row_count_next, row_count_reg: unsigned(15 downto 0); --row count
      signal row_init: unsigned(row_count_next'length-1 downto 0); --row count initialization value.
      signal between_frames_count, between_frames_reg: unsigned(15 downto 0);
      signal between_frames_init: unsigned(15 downto 0);
	  signal initial_invalid_count, initial_invalid_reg : unsigned(between_frames_count'range);
	  signal initial_invalid_init: unsigned(between_frames_count'range);
	  signal initial_zero_count, initial_zero_reg : unsigned(between_frames_count'range);
	  signal initial_zero_init: unsigned(between_frames_count'range);
	 
      signal zero : unsigned(between_frames_count'length-1 downto 0);
	  signal sum : integer range 0 to 255; -- 8 bit integer
	  
	  signal pixel_count: integer range 0 to 307200;
	  signal pixel_count_reg: integer range 0 to 307200;
	  signal pixel_count_incr1 :	std_logic;
	  signal pixel_count_incr2 : 	std_logic;
	  
	  signal produce_zeroes_valid : std_logic;
	  signal line_buffer_reset : std_logic;
	  signal in_produce_zeroes : std_logic;
	  
	  
	  signal col_init_reg, col_init_next							: std_logic_vector(15 downto 0);
	  signal row_init_reg, row_init_next							: std_logic_vector(15 downto 0);
	  signal between_frames_init_reg, between_frames_init_next		: std_logic_vector(15 downto 0);
	  signal initial_invalid_init_reg, initial_invalid_init_next	: std_logic_vector(15 downto 0);
	  signal initial_zero_init_reg, initial_zero_init_next			: std_logic_vector(15 downto 0);
	  signal initial_zero_val_reg, initial_zero_val_next			: std_logic_vector(7 downto 0);
	  signal final_zero_val_reg, final_zero_val_next				: std_logic_vector(7 downto 0);
	  
	  signal data_out_next			: std_logic_vector (7 downto 0);
	  signal valid_out_next			: std_logic;
	  
	  
      begin
	  
	  
        process(clk, rst)
          begin
         if(rst = '1') then 
		   col_init_reg <= std_logic_vector(to_signed(C_COLS-2, col_init_reg'length));
		   row_init_reg <= std_logic_vector(to_signed(C_ROWS-33, row_init_reg'length));
		   between_frames_init_reg <= std_logic_vector(to_signed(C_COLS*16 -256, between_frames_init_reg'length));
		   initial_invalid_init_reg <= std_logic_vector(to_signed(C_COLS*15 +1, initial_invalid_init_reg'length));
		   initial_zero_init_reg <= std_logic_vector(to_signed(C_COLS*16 +1, initial_zero_init_reg'length));
		   initial_zero_val_reg <= (others => '0');
		   final_zero_val_reg <= (others => '0');
		   data_out <= (others => '0');
		   valid_out <= '0';
         elsif (clk'event and clk = '1') then  --register signals on the clock edge
           col_init_reg <= col_init_next;
		   row_init_reg <= row_init_next;
		   between_frames_init_reg <= between_frames_init_next;
		   initial_invalid_init_reg <= initial_invalid_init_next;
		   initial_zero_init_reg <= initial_zero_init_next;
		   initial_zero_val_reg <= initial_zero_val_next;
		   final_zero_val_reg <= final_zero_val_next;
		   data_out <= data_out_next;
		   valid_out <= valid_out_next;
		 end if;
        end process;
       
	   col_init_next				<= reg_val_in when col_init_we = '1' else col_init_reg;
	   row_init_next				<= reg_val_in when row_init_we = '1' else row_init_reg;
	   between_frames_init_next		<= reg_val_in when between_frames_init_we = '1' else between_frames_init_reg;
	   initial_invalid_init_next	<= reg_val_in when initial_invalid_init_we = '1' else initial_invalid_init_reg;
	   initial_zero_init_next		<= reg_val_in when initial_zero_init_we = '1' else initial_zero_init_reg;
	   initial_zero_val_next		<= reg_val_in(7 downto 0) when zeros_val_we = '1' else initial_zero_val_reg;
	   final_zero_val_next			<= reg_val_in(15 downto 8) when zeros_val_we = '1' else final_zero_val_reg;
	   
	  --C_ROWS : integer := 40;
      --C_COLS: integer := 6;
	  --CONV_HEIGHT : integer := 33;
	  --CONV_WIDTH : integer := 3;
        
        zero 				 <= unsigned(to_signed(0, between_frames_count'length));
		
		
        col_init 			 <= unsigned(col_init_reg);
        row_init 		     <= unsigned(row_init_reg);
        between_frames_init  <= unsigned(between_frames_init_reg); -- the extra one is to allow the last pixel to propogate. --between frames there are many rows of invalid neighborhoods of pixels.
		initial_invalid_init <= unsigned(initial_invalid_init_reg);
		initial_zero_init    <= unsigned(initial_zero_init_reg);

		right_FFs_next(0) <= din when rst = '0' else '0';
		line_buffer_reset <= rst or in_produce_zeroes or frame_reset;
		
		
		
        process(clk, frame_reset,col_init,row_init,between_frames_init,initial_invalid_init,initial_zero_init)
          begin
		 
         if(frame_reset = '1') then 
           state_reg <= first_frame_invalid;		--initialize default state when reset = 1;
		   --right_FFs_next(31 downto 1) <= (others => '0');
		   col_count_reg <= col_init;
		   row_count_reg <= row_init;
		   between_frames_reg <= between_frames_init;
		   initial_invalid_reg <= initial_invalid_init;
		   initial_zero_reg <= initial_zero_init;
		   pixel_count_reg <= 0;

         elsif (clk'event and clk = '1') then  --register signals on the clock edge
           state_reg <= state_next;
		   between_frames_reg <= between_frames_count;
		   if ((valid_in = '1') or (in_produce_zeroes = '1')) then
				pixel_count_reg <= pixel_count;
			    col_count_reg <= col_count_next;
			    row_count_reg <= row_count_next;
			    initial_invalid_reg <= initial_invalid_count;
			   initial_zero_reg <= initial_zero_count;
		   end if;
		 end if;
        end process;
        
		--pixel_count <= pixel_count_reg + 1 when (pixel_count_incr1 = '1' or pixel_count_incr2 = '1') else pixel_count_reg;	
		--next state logic, states  (first_frame_invalid, first_frame_zero, produce_zeroes, produce_pixel, skip_edges1, skip_edges2);
        process(state_reg, valid_in, initial_invalid_reg, initial_zero_reg, between_frames_reg, col_count_reg, row_count_reg )
          begin
			pixel_count_incr1 <= '0';
            case state_reg is
			  when first_frame_invalid =>
			    if(initial_invalid_reg = zero) then
				  state_next <= first_frame_zero;
				else
				  state_next <=first_frame_invalid;
				end if;
				
			  when first_frame_zero =>
			    if(initial_zero_reg = zero) then
				  state_next <= produce_pixel;
				else
				  state_next <= first_frame_zero;
				end if;

              when produce_zeroes =>
                if(between_frames_reg = zero) then
                  state_next <= first_frame_invalid;
                else
                  state_next <= produce_zeroes;
                end if;
				
              when produce_pixel =>
                if(col_count_reg = zero) then
					if(row_count_reg = zero) then
						state_next <=produce_zeroes;
					else
						state_next <= skip_edges1;
					end if;
                else
                  state_next <= produce_pixel;
                end if;
				
              when skip_edges1 =>
			   if(valid_in = '1') then
                  state_next <= skip_edges2; --skip two pixels, simulating skipping the very right and very left columns. (invalid because we can't get a neighborhood on edge pixels)
			  else
			      state_next <= skip_edges1;
			   end if;
			   
			  when skip_edges2 =>
			    if(valid_in = '1') then
                  state_next <= produce_pixel; --skip two pixels, simulating skipping the very right and very left columns. (invalid because we can't get a neighborhood on edge pixels)
				  pixel_count_incr1 <= '1';
  			    else
			      state_next <= skip_edges2;
			    end if;
				
            end case;
        end process;
        
        --Moore output logic
		--states  (first_frame_invalid, first_frame_zero, produce_zeroes, produce_pixel, skip_edges1, skip_edges2);
		process(state_reg, in_produce_zeroes, produce_zeroes_valid, valid_in, initial_invalid_reg, initial_zero_reg, between_frames_reg, col_count_reg, row_count_reg ,sum, pixel_count_reg,
				col_init,row_init,between_frames_init,initial_invalid_init,initial_zero_init,initial_zero_val_reg, final_zero_val_reg)
          begin
            
            --default assignments
			in_produce_zeroes <= '0';
            produce_zeroes_valid <= '0';
			col_count_next <= col_count_reg;
			row_count_next <= row_count_reg;
			between_frames_count <= between_frames_reg;
			initial_invalid_count <= initial_invalid_reg;
			initial_zero_count <= initial_zero_reg;
			pixel_count <= pixel_count_reg;
			
			valid_out_next <= valid_in or produce_zeroes_valid;
			data_out_next <= std_logic_vector(to_unsigned(0, data_out'length));

			pixel_count_incr2 <= '0';

          case state_reg is
          when first_frame_invalid => 
  		   -- col_count_next <= col_init;
		   -- row_count_next <= row_init;
		   -- between_frames_count <= between_frames_init;
		   --initial_zero_reg <= initial_zero_init;
		    pixel_count <= 0;
			valid_out_next <= '0';
			 if(valid_in = '1') then
			     initial_invalid_count <= initial_invalid_reg -1;
			 end if;
			
     	  when first_frame_zero =>
		  initial_zero_count <= initial_zero_reg -1;
		  pixel_count <= pixel_count_reg +1;
     	     if(valid_in = '1') then
		          data_out_next <= initial_zero_val_reg;
				  pixel_count_incr2 <= '1';
		       end if;
		    
          when produce_zeroes =>
		   in_produce_zeroes <= '1';
		   produce_zeroes_valid <= '1';
		   initial_invalid_count <= initial_invalid_init;
		   initial_zero_count <= initial_zero_init;
		   data_out_next <= final_zero_val_reg;
		  
		   between_frames_count <= between_frames_reg -1;
		   pixel_count <= pixel_count_reg +1;
		   pixel_count_incr2 <= '1';
			col_count_next <= col_init;
			row_count_next <= row_init;
			
			
          when produce_pixel=>
		  between_frames_count <= between_frames_init;
		  pixel_count <= pixel_count_reg +1;
		  col_count_next <= col_count_reg-1;
		    if(valid_in = '1') then
              
			  data_out_next <= std_logic_vector(to_unsigned(sum, data_out_next'length));
				  pixel_count_incr2 <= '1';
			end if;
			
          when skip_edges1 =>
		    row_count_next <= row_count_reg-1;
			pixel_count <= pixel_count_reg +1;
		    if(valid_in = '1') then
		      
				  pixel_count_incr2 <= '1';
			end if;
			col_count_next <= col_init;
			
          when skip_edges2 =>
		  pixel_count <= pixel_count_reg +1;
			
        end case;
       end process;
	   
	   process(clk, valid_in, right_FFs_reg, middle_FFs_reg, left_FFs_reg)
	   begin
		if(clk'event and clk = '1' and valid_in = '1') then
		left_FFs_reg <= middle_FFs_reg;
		middle_FFs_reg <= right_FFs_reg;
		right_FFs_reg <= right_FFs_next;
		end if;
	  end process;
	  
	  linebuffers0to15 : line_buffer
		  generic map (C_DEPTH => C_COLS-1, 
					   C_WIDTH => 16)
		  port map (
			clk => clk,
			rst => line_buffer_reset, 
			din => right_FFs_next(15 downto 0), 
			wr_en => valid_in,
			dout => right_FFs_next(16 downto 1),
			valid => open);
			
	  linebuffers16to31 : line_buffer
		  generic map (C_DEPTH => C_COLS-1, 
					   C_WIDTH => 16)
		  port map (
			clk => clk,
			rst => line_buffer_reset, 
			din => right_FFs_next(31 downto 16), 
			wr_en => valid_in,
			dout => right_FFs_next(32 downto 17),
			valid => open);
	   
	   ADD_FIRST_STAGE: FOR N in 0 to 32 GENERATE
			adder_s1(N) <= to_integer(unsigned(vectorize(right_FFs_reg(N)))) + to_integer(unsigned(vectorize(middle_FFs_reg(N)))) + to_integer(unsigned(vectorize(left_FFs_reg(N))));
	   END GENERATE ADD_FIRST_STAGE;

	   ADD_SECOND_STAGE: FOR N in 0 to 10 GENERATE
			adder_s2(N) <= adder_s1(N)+adder_s1(N+11)+adder_s1(N+22);
	   END GENERATE ADD_SECOND_STAGE;	   

	   ADD_THIRD_STAGE: FOR N in 0 to 3 GENERATE
			SPECIAL_CASE1: if (N = 3) GENERATE
				adder_s3(N) <= adder_s2(N)+adder_s2(N+4) + 0;
			END GENERATE SPECIAL_CASE1;
			NORMAL_CASE1: if (N <3) GENERATE
				adder_s3(N) <= adder_s2(N)+adder_s2(N+4)+adder_s2(N+8);
			END GENERATE NORMAL_CASE1;
	   END GENERATE ADD_THIRD_STAGE;	   
	   
	   ADD_FOURTH_STAGE: FOR N in 0 to 1 GENERATE
			adder_s4(N) <= adder_s3(N) + adder_s3(N+2);
	   END GENERATE ADD_FOURTH_STAGE;
	   
	   sum <= adder_s4(0) + adder_s4(1) when middle_FFs_reg(16) = '1' else 0;
	   
	   
end conv_arch;