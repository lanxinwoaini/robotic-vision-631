proc generate {drv_handle} {
  xdefine_include_file $drv_handle "xparameters.h" "fcm_fpu_light" "NUM_INSTANCES" "C_BASEADDR" "C_HIGHADDR"
}

