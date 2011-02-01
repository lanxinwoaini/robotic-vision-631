proc generate {drv_handle} {
  xdefine_include_file $drv_handle "xparameters.h" "XPLB_CAMERA" "NUM_INSTANCES" "C_BASEADDR" "C_HIGHADDR"
}
