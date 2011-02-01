/******************************************************************************

FILE:    fcm_fpu_light.c
AUTHOR:  Wade Fife
CREATED: 4/22/06

DESCRIPTION

Includes initialization function for the fcm_fpu_light floating point driver.

DOCUMENTATION

Call the FPULight_Init() function to set up the PPC and APU to use the floating
point unit. The basic floating point operations are implemented as macros,
which use inline assembly, and are defined in the driver header file.

Be sure to enable the instruction and data caches to ensure proper operation of
the FPU instructions.

NOTES

There have been several problems when using the FPU instructions with caches
disabled. For proper operation, you must enable the instruction AND data caches
before using the FPU instructions.

The #defines below will need to be modified if the internal DCR base address is
changed in the ppc405 instance declaration in the MHS file. The #defines below
assume the default base address of 0b0100000000 (0x100).

This driver can NOT be used with other cores/drivers that use the APU. This
driver and the hardware for wich it was written assume exclusive use of the
APU.

CHANGE LOG

04/22/06 WSF Created initial file.

******************************************************************************/

#include <xpseudo_asm.h>
#include "fcm_fpu_light.h"



// DEFINITIONS ////////////////////////////////////////////////////////////////

/* 
 * WARNING: The IDCR base address defaults to 0x100 but can be changed
 * statically in the MHS file. If the base address is changed, update each
 * #define below to appropriately for new offset.
 */
#define IDCR_UDICFG_ADDR 0x104    // UDI configuration DCR address (offset 4)
#define IDCR_APUCFG_ADDR 0x105    // APU configuration DCR address (offset 5)



// FUNCTIONS //////////////////////////////////////////////////////////////////

// Initializes the UDI and APU configuration registers as well as the MSR for
// the APU-based FPU. These values canalso be set statically in the ppc405
// instance declaration in the MHS file.
void FPULight_Init(void)
{
	Xuint32 msr, msrOrg;

	// Disable interrupts, store original interrupt state in msrOrg
	msrOrg = mfmsr();
	asm volatile ("wrteei 0");

	// Setup UDI Instructions
	mtdcr(IDCR_UDICFG_ADDR, 0xC0770001);  // add:  UDI 0
	mtdcr(IDCR_UDICFG_ADDR, 0xC4770003);  // sub:  UDI 1
	mtdcr(IDCR_UDICFG_ADDR, 0xC8770005);  // mul:  UDI 2
	mtdcr(IDCR_UDICFG_ADDR, 0xCC770007);  // div:  UDI 3
	mtdcr(IDCR_UDICFG_ADDR, 0xD0750009);  // sqrt: UDI 4
	mtdcr(IDCR_UDICFG_ADDR, 0xD475000B);  // ftoi: UDI 5
	mtdcr(IDCR_UDICFG_ADDR, 0xD875000D);  // itof: UDI 6
	mtdcr(IDCR_UDICFG_ADDR, 0xDC77000F);  // cmp:  UDI 7

	// Setup APU configuration
	mtdcr(IDCR_APUCFG_ADDR, 0x04F00001);

	// Tell CPU that APU is present
	msr = mfmsr();
	msr = msr | XREG_MSR_APU_AVAILABLE;
	mtmsr(msr);

	// Restore interrupts
	asm volatile ("wrtee %0" :: "r" (msrOrg));
}
