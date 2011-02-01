/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    fcm_fpu_light.h
AUTHOR:  Sean Clark Hess and Wade Fife
CREATED: 4/22/06

DESCRIPTION

Header file for the fcm_fpu_light core driver.

CHANGE LOG

04/22/06 WSF Created initial file.
05/09/06 WSF Added fneg() macro, to negate a float.

******************************************************************************/

#include <xbasic_types.h>



// FPU MACROS DEFINITIONS /////////////////////////////////////////////////////

#define fadd(OP_A, OP_B)                                              \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi0fcm %0,%1,%2"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)), "r"((Xfloat32)(OP_B))); \
        dest;                                                         \
    })

#define fsub(OP_A, OP_B)                                              \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi1fcm %0,%1,%2"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)), "r"((Xfloat32)(OP_B))); \
        dest;                                                         \
    })

#define fmul(OP_A, OP_B)                                              \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi2fcm %0,%1,%2"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)), "r"((Xfloat32)(OP_B))); \
        dest;                                                         \
    })

#define fdiv(OP_A, OP_B)                                              \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi3fcm %0,%1,%2"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)), "r"((Xfloat32)(OP_B))); \
        dest;                                                         \
    })

#define fsqrt(OP_A)                                                   \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi4fcm %0,%1, 0"                               \
                     : "=r"(dest)                                     \
                     :  "r"((Xfloat32)(OP_A)));                       \
        dest;                                                         \
    })

#define fftoi(OP_A)                                                   \
    ({                                                                \
        Xint32 dest;                                                  \
        asm volatile("udi5fcm %0,%1, 1"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)));                        \
        dest;                                                         \
    })

#define fftou(OP_A)                                                   \
    ({                                                                \
        Xuint32 dest;                                                 \
        asm volatile("udi5fcm %0,%1, 0"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)));                        \
        dest;                                                         \
    })

#define fitof(OP_A)                                                   \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi6fcm %0,%1, 1"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xint32)(OP_A)));                          \
        dest;                                                         \
    })

#define futof(OP_A)                                                   \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("udi6fcm %0,%1, 0"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xuint32)(OP_A)));                         \
        dest;                                                         \
    })

#define fcmp(OP_A, OP_B)                                              \
    ({                                                                \
        Xint32 dest;                                                  \
        asm volatile("udi7fcm %0,%1,%2"                               \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)), "r"((Xfloat32)(OP_B))); \
        dest;                                                         \
    })

#define fneg(OP_A)                                                    \
    ({                                                                \
        Xfloat32 dest;                                                \
        asm volatile("xoris %0,%1,0x8000"                             \
                     : "=r"(dest)                                     \
                     : "r"((Xfloat32)(OP_A)));                        \
        dest;                                                         \
    })



// CAMPERISON MACROS //////////////////////////////////////////////////////////

#define fgt(A, B) (fcmp(A,B) >  0)
#define fge(A, B) (fcmp(A,B) >= 0)
#define flt(A, B) (fcmp(A,B) <  0)
#define fle(A, B) (fcmp(A,B) <= 0)
#define feq(A, B) (fcmp(A,B) == 0)
#define fne(A, B) (fcmp(A,B) != 0)



// FUNCTIONS PROTOTYPES ///////////////////////////////////////////////////////
    
void FPULight_Init(void);
