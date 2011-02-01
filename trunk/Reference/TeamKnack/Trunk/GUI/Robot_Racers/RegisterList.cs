using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace Robot_Racers
{
    public class Register
    {
        public const int INT_FLOAT_ID_DIVIDER = 100;

        public enum RegisterIds : ushort
        {
            //Any register ids below INT_FLOAT_ID_DIVIDER will be ints
            ENCODER_VALUE_INT = 1,
            NUM_MILLISECONDS_TO_RUN_INT = 2,
            DESIRED_ANGLE_INT = 3,
            DESIRED_ENCODER_VALUE_INT = 4,
            PID_DISTANCE_MODE_INT = 5,
            
            H_HIGH_ORANGE = 6,
            H_LOW_ORANGE = 7,
            S_HIGH_ORANGE = 8,
            S_LOW_ORANGE = 9,
            V_HIGH_ORANGE = 10,
            V_LOW_ORANGE = 11,

            H_HIGH_GREEN = 12,
            H_LOW_GREEN = 13,
            S_HIGH_GREEN = 14,
            S_LOW_GREEN = 15,
            V_HIGH_GREEN = 16,
            V_LOW_GREEN = 17,

            CONV_THRESHOLD_GREEN = 18,
            CONV_THRESHOLD_ORANGE = 19,

            PIT_INT = 24,
            FPS_SW_INT = 25,
            FPS_HW_INT = 28,

            TURN_ANGLE_INT = 26,
            FIND_RANGE_INT = 27,

            //Any register ids above INT_FLOAT_ID_DIVIDER will be floats
            KP_REG_FLOAT = 101,
            KD_REG_FLOAT = 102,
            KI_REG_FLOAT = 103,
            I_MAX_FLOAT = 104,
            I_MIN_FLOAT = 105,
            OUTPUT_PID_FLOAT = 106,
            DESIRED_VELOCITY_FLOAT = 107,
            NUM_METERS_TO_RUN_FLOAT = 108,
            TRANSMIT_STATE_VELOCITY_FLOAT = 109,
            DELTA_DISTANCE_FLOAT = 110,

            BLIND_TURN_FLOAT = 111,
            BLIND_STRAIGHT_FLOAT = 112,
            STRAIGHT_SPEED_FLOAT = 113,
            TURN_RADIUS_FLOAT = 114
        }

        KeyValuePair<RegisterIds, string> register;

        public Register(RegisterIds regId, string tag)
        {
            register = new KeyValuePair<RegisterIds, string>(regId, tag);
        }
        public string Tag
        {
            get { return register.Value; }
        }

        public ushort Id
        {
            get { return (ushort)register.Key; }
        }

    }

    public class FullRegisterList : List<String>
    {
        SortedList<string, Register> regs;
        List<Register> list;

        public FullRegisterList(RegisterCollection coll) 
        {
            list = new List<Register>();
            regs = new SortedList<string, Register>();

            if (coll == null)
                coll = new RegisterCollection(false);


            /* Instructions to add a register */
            // 1. Add to the registerId enum above with a unique variable name for the register
            // 2. Add a line below to add to the list with the string in quotes being the tag name
            // 3. Add a corresponding register ID to Registers.c and Registers.h
            // 4. Make sure that there MAX_NUM_REGISTERS is big enough for the number of registers added in Registers.c
            /* Add registers here */

            list.Add(new Register(Register.RegisterIds.ENCODER_VALUE_INT, "Encoder Value"));
            list.Add(new Register(Register.RegisterIds.NUM_MILLISECONDS_TO_RUN_INT, "Num Milliseconds to Run"));
            list.Add(new Register(Register.RegisterIds.DESIRED_ANGLE_INT, "Desired Angle"));


            list.Add(new Register(Register.RegisterIds.KP_REG_FLOAT, "K_P Float"));
            list.Add(new Register(Register.RegisterIds.KD_REG_FLOAT, "K_D Float"));
            list.Add(new Register(Register.RegisterIds.KI_REG_FLOAT, "K_I Float"));
            list.Add(new Register(Register.RegisterIds.I_MAX_FLOAT, "Integrator Max"));
            list.Add(new Register(Register.RegisterIds.I_MIN_FLOAT, "Integrator Min"));
            list.Add(new Register(Register.RegisterIds.OUTPUT_PID_FLOAT, "PID Output"));
            list.Add(new Register(Register.RegisterIds.DESIRED_VELOCITY_FLOAT, "Desired Velocity"));

            list.Add(new Register(Register.RegisterIds.NUM_METERS_TO_RUN_FLOAT, "Num Meters to Run"));
            list.Add(new Register(Register.RegisterIds.DESIRED_ENCODER_VALUE_INT, "Desired Encoder"));
            list.Add(new Register(Register.RegisterIds.PID_DISTANCE_MODE_INT, "PID Distance Mode"));
            list.Add(new Register(Register.RegisterIds.TRANSMIT_STATE_VELOCITY_FLOAT, "Transmit State Velocity"));

            list.Add(new Register(Register.RegisterIds.PIT_INT, "Actual PIT Frequency"));
            list.Add(new Register(Register.RegisterIds.FPS_SW_INT, "Software FramesPerSecond"));
            list.Add(new Register(Register.RegisterIds.FPS_HW_INT, "Hardware FramesPerSecond"));


            /* End adding registers to the drop-down boxes */


            for(int i = 0; i < list.Count; i ++)
            {
                regs.Add(list[i].Tag, list[i]);
            }

            for (int i = 0; i < list.Count; i++)
            {
                this.Add(regs.ElementAt(i).Value.Tag);
            }
        }

        public ushort getRegisterId(string tag)
        {
            Register r = regs[tag];
            if (r != null)
            {
                return r.Id;
            }
            return 0;
        }

        public Register getRegister(string tag)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Tag.Equals(tag))
                    return list[i];
            }
            return null;
        }

        public string getTag(int regId)
        {
            for(int i = 0; i < list.Count; i ++){
                if(list[i].Id == regId){
                    return list[i].Tag;
                }
            }
            return "";
        }
    }    
}
