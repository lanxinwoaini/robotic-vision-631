-------------------------------------------------------------------------------
--
-- File: fpu_ligh_norm.vhd
-- Author: Wade S. Fife
-- Date: February 22, 2005
--
--                        Copyright (c) 2005 Wade Fife
--
-- DESCRIPTION
--
-- This module contains entities used by FPULight
-- (fpu_light.vhd). They consist of hardware used to normalize the
-- numbers as the flow through the FPU entities.
--
-------------------------------------------------------------------------------



-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

entity leading_zeros_25 is
  port (
    input  : in  unsigned(24 downto 0);
    output : out unsigned(4 downto 0));
end leading_zeros_25;

architecture imp of leading_zeros_25 is
begin
  COUNT_ZEROS: process (input)
  begin
    if input(24) = '1' then
      output <= "00000";
    elsif input(24 downto 23) = "01" then
      output <= "00001";
    elsif input(24 downto 22) = "001" then
      output <= "00010";
    elsif input(24 downto 21) = "0001" then
      output <= "00011";
    elsif input(24 downto 20) = "00001" then
      output <= "00100";
    elsif input(24 downto 19) = "000001" then
      output <= "00101";
    elsif input(24 downto 18) = "0000001" then
      output <= "00110";
    elsif input(24 downto 17) = "00000001" then
      output <= "00111";
    elsif input(24 downto 16) = "000000001" then
      output <= "01000";
    elsif input(24 downto 15) = "0000000001" then
      output <= "01001";
    elsif input(24 downto 14) = "00000000001" then
      output <= "01010";
    elsif input(24 downto 13) = "000000000001" then
      output <= "01011";
    elsif input(24 downto 12) = "0000000000001" then
      output <= "01100";
    elsif input(24 downto 11) = "00000000000001" then
      output <= "01101";
    elsif input(24 downto 10) = "000000000000001" then
      output <= "01110";
    elsif input(24 downto 9)  = "0000000000000001" then
      output <= "01111";
    elsif input(24 downto 8)  = "00000000000000001" then
      output <= "10000";
    elsif input(24 downto 7)  = "000000000000000001" then
      output <= "10001";
    elsif input(24 downto 6)  = "0000000000000000001" then
      output <= "10010";
    elsif input(24 downto 5)  = "00000000000000000001" then
      output <= "10011";
    elsif input(24 downto 4)  = "000000000000000000001" then
      output <= "10100";
    elsif input(24 downto 3)  = "0000000000000000000001" then
      output <= "10101";
    elsif input(24 downto 2)  = "00000000000000000000001" then
      output <= "10110";
    elsif input(24 downto 1)  = "000000000000000000000001" then
      output <= "10111";
    elsif input(24 downto 0)  = "0000000000000000000000001" then
      output <= "11000";
    else
      output <= "11001";
    end if;    
  end process COUNT_ZEROS;
end imp;


-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

entity leading_zeros_31 is
  port (
    input  : in  unsigned(30 downto 0);
    output : out unsigned(4 downto 0));
end leading_zeros_31;

architecture imp of leading_zeros_31 is
begin
  COUNT_ZEROS: process (input)
  begin
    if input(30) = '1' then
      output <= "00000";
    elsif input(30 downto 29) = "01" then
      output <= "00001";
    elsif input(30 downto 28) = "001" then
      output <= "00010";
    elsif input(30 downto 27) = "0001" then
      output <= "00011";
    elsif input(30 downto 26) = "00001" then
      output <= "00100";
    elsif input(30 downto 25) = "000001" then
      output <= "00101";
    elsif input(30 downto 24) = "0000001" then
      output <= "00110";
    elsif input(30 downto 23) = "00000001" then
      output <= "00111";
    elsif input(30 downto 22) = "000000001" then
      output <= "01000";
    elsif input(30 downto 21) = "0000000001" then
      output <= "01001";
    elsif input(30 downto 20) = "00000000001" then
      output <= "01010";
    elsif input(30 downto 19) = "000000000001" then
      output <= "01011";
    elsif input(30 downto 18) = "0000000000001" then
      output <= "01100";
    elsif input(30 downto 17) = "00000000000001" then
      output <= "01101";
    elsif input(30 downto 16) = "000000000000001" then
      output <= "01110";
    elsif input(30 downto 15) = "0000000000000001" then
      output <= "01111";
    elsif input(30 downto 14) = "00000000000000001" then
      output <= "10000";
    elsif input(30 downto 13) = "000000000000000001" then
      output <= "10001";
    elsif input(30 downto 12) = "0000000000000000001" then
      output <= "10010";
    elsif input(30 downto 11) = "00000000000000000001" then
      output <= "10011";
    elsif input(30 downto 10) = "000000000000000000001" then
      output <= "10100";
    elsif input(30 downto 9)  = "0000000000000000000001" then
      output <= "10101";
    elsif input(30 downto 8)  = "00000000000000000000001" then
      output <= "10110";
    elsif input(30 downto 7)  = "000000000000000000000001" then
      output <= "10111";
    elsif input(30 downto 6)  = "0000000000000000000000001" then
      output <= "11000";
    elsif input(30 downto 5)  = "00000000000000000000000001" then
      output <= "11001";
    elsif input(30 downto 4)  = "000000000000000000000000001" then
      output <= "11010";
    elsif input(30 downto 3)  = "0000000000000000000000000001" then
      output <= "11011";
    elsif input(30 downto 2)  = "00000000000000000000000000001" then
      output <= "11100";
    elsif input(30 downto 1)  = "000000000000000000000000000001" then
      output <= "11101";
    elsif input(30 downto 0)  = "0000000000000000000000000000001" then
      output <= "11110";
    else
      output <= "11111";
    end if;    
  end process COUNT_ZEROS;
end imp;


-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

entity pre_norm_mult is
  port (
    product : in unsigned(26 downto 0);
    exp_adj : out std_logic;
    output  : out unsigned(25 downto 0));
end pre_norm_mult;

architecture imp of pre_norm_mult is
  signal msb : std_logic;
  signal sticky : std_logic;
begin
  msb <= product(26);
  sticky <= product(1) or product(0);

  with msb select
    output <=
    product(25 downto 0)          when '0',
    product(26 downto 2) & sticky when others;

  exp_adj <= msb;
end imp;



-------------------------------------------------------------------------------

library IEEE;
use IEEE.std_logic_1164.all;
use IEEE.numeric_std.all;

entity post_norm is
  port (
    value : in  unsigned(24 downto 0);
    exp_adj : out std_logic;
    output  : out unsigned(22 downto 0));
end post_norm;

architecture imp of post_norm is
  signal msb : std_logic;
begin
  msb <= value(24);
  
  with msb select
    output <=
    value(22 downto 0) when '0',
    value(23 downto 1) when others;

  exp_adj <= msb;
end imp;



-------------------------------------------------------------------------------

