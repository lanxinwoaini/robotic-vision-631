//TODO:
//--check !RDY before sending data to sound chip
//--SoundReset(mode) -> pin for resetting the chip
//--define struct for game parameters, talk to Collin, Andy, Jared
//--EEPROM.h and EEPROM.c
//--SaveAudioPhrase()
//--SaveGameParameters()
//--GetGameParameters()
//--Using Xmodem  http://www.columbia.edu/kermit/ek.html#source e-kermit
//figure out good way of initiating transfer of audio phrases
/////////!!!!!!!!!!test features
//spi.c, spi.h Modify funcitons to accept u08 slave select data, we have 3 devices on the slave
//--refer to global.h for different slave ss values.  Need to know pin names for these devices, as well as !RST pins

/*
******************************************
* FileName	:soundchip.h 
* Author	:Russell LeBaron
* Date		:1/14/10
* Version	:0.0
*
******************************************
*
* This code contains driver functions for the Keterex KX1400 sound chip.
*  This includes ability to play a tone, play a file saved on attached EEPROM, Pass thru access to the EEPROM
*
* An audio phrase is a sequence of PCM/ADPCM audio samples, tone-generator sequences, and control commands.
****EXAMPLES*******

*/
#include "soundchip.h"
#include "global.h"

#define mute()  sendSoundByte(KX_MUTE)
#define unmute()  sendSoundByte(KX_UN_MUTE)

//Global Variables
//static u16 phrasesInMem = 0; //this could go up to 4096, if this is the case change to u16
//static u16 audioSize = 0; // size of the audio file in memory, used to determine where to put global parameters
//extern gameParameters gp;

static u08 chipMode = KX_NORMAL;// current chip mode, 0 is normal, 1 is passthrough


//	soundInit - Initialize the KX1400 chip via the SPI bus
//
void soundInit(void)
{
	sbi(PORTC, KX_nRESET);
	sbi(DDRC, KX_nRESET); // Set reset to output
	cbi(DDRC, KX_BUSY); //set busy to input
	cbi(DDRC, KX_nRDY); //set nRDY to input
	chipMode = KX_PASS_THROUGH;
	soundReset(KX_NORMAL);
	EEPROMInit(); // initialize the EEPROM
}

//	soundReset - Reset the KX1400 chip via the SPI bus
// NOTE:  this will be needed when switching between EEPROM PassThrough and Sound Functionality modes
void soundReset(u08 mode)
{
	u08 command;
	volatile u08 wait;
	if((mode == KX_PASS_THROUGH) && (chipMode != KX_PASS_THROUGH))
	{
		command = (1<<5)|KX_RESET ;
		sendSoundByte(command);
		//rprintf("EEPROM");
		sbi(PORTC, SOUND_SPI_SS );
		while( (inb(PINC) & (1<<KX_nRDY)) );
		chipMode = KX_PASS_THROUGH;
	}
	else if((chipMode == KX_PASS_THROUGH) && (mode == KX_NORMAL) )
	{
		cbi(PORTC,KX_nRESET);//Set reset bit to 0
		chipMode = KX_NORMAL;
		//rprintf("HARD_RESET");
		for(wait = 0; wait < 0xff; wait++){//hold reset low at least 20 uS
			
		}
		sbi(PORTC,KX_nRESET);//Set reset bit to 1
		while( (inb(PINC) & (1<<KX_nRDY))  );
	}
	else
	{
		command = KX_RESET;
		//rprintf("SOFT_RESET");
		sendSoundByte(command);
		spiSetCS( OFF_SPI_SS );
		while( (inb(PINC) & (1<<KX_nRDY)) );
		chipMode = KX_NORMAL;
	}
	
}

// EEPROMMode - put the chip in EEPROMMode
// NOTE:  this will be needed when switching between EEPROM PassThrough and Sound Functionality modes
void EEPROMMode(void)
{
	soundReset(KX_PASS_THROUGH); //reset the chip and put in PASS_THROUGH mode
}

//	SoundMode - put the chip in Sound Chip mode if in PASS_THROUGH mode
// NOTE:  this will be needed when switching between EEPROM PassThrough and Sound Functionality modes
//this will do a hardware reset of the chip
void SoundMode(void)
{
	soundReset(KX_NORMAL);
}

s08 setVolume(u08 vol)
{
	//assert(volume<32);
	if(vol >  32)
		return 0;
	sendSoundByte( (vol << 4) | KX_SET_VOLUME);
	sbi(PORTC, SOUND_SPI_SS);
	return 1;
}

//	PlayTone - KX1400 can play an audio phrase(basically a sound clip, look for definition above) already stored in memory 
//	input parameters
// 		u16	freqIndex	Tone frequency to play.  The Frequency Index takes a value from 0 to 16383. It then generates a 
//			a tone at freq where freq =	freqIndex/4.096 Hz.  Therefore, frequencies can be generated in the range of 0.24414 Hz to 3.9997 KHz.
//			A 0 repeats previous tone, but mutes it, effectively a silence command.
//		u16 duration	The duration value represents time in milliseconds the tone will play and may be configured with a value from 0 to 4095	0 is infinity
//	return value
//		0 if freqIndex or duration are invalid
//		1 if a valid freqIndex and duration
//
s08 PlayTone(u16 freqIndex, u16 duration)
{
	if((freqIndex > 16383) || (duration > 4095))
	{
		return 0; //invalid frequency index or duration
	}
	unmute();
	sbi(PORTC, SOUND_SPI_SS);
	sendSoundWord( (duration << 4) | KX_SET_DURATION); //set the play duration
	sbi(PORTC, SOUND_SPI_SS);
	sendSoundWord( (freqIndex << 4) | KX_PLAY_TONE); //play frequency
	sbi(PORTC, SOUND_SPI_SS);
	mute();
	sbi(PORTC, SOUND_SPI_SS);
	return 1;
	
}

//	PlayAudioPhrase - KX1400 can play an audio phrase(basically a sound clip, look for definition above) already stored in memory 
//	input parameters
// 		phraseNum	phrase number to play.  This is defined as 0-4096. 
//	return value
//		0 if phrase number is invalid
//		1 with a valid phrase number
//
s08 PlayAudioPhrase(u16 phraseNum)
{
	//if(phraseNum > parameters.phrasesInMem){
	//	return 0;  // phrase does not exist
	//}
	sendSoundWord( (phraseNum << 4) | KX_PLAY_PHRASE); //playPhrase
	sbi(PORTC, SOUND_SPI_SS);
	return 1;
}
//	SaveAudioPhrase - Save Audio phrase to EEPROM 
//	input parameters
//		this could either keep track of what positions are taken, or more likely have specified which file location to save to
// 		u16	numPhrases 	number of phrases to save.  This is defined as 0-4096. 
//		u08* audioFile		pointer to beginning of audio file, what size?  this is either a PCM or a ADPCM file
//		u16 size	size of audio file in bytes
//		u08 audioType	defines whether file is PCM or ADPCM. PCM is defined as 0, ADPCM defined as 1
//	return value
//		0 if save failed
//		1 if save was succesfull
//
s08 SaveAudioPhrase(/*u16 numPhrases, u08 *audioFile, u16 size, u08 audioType*/void)
{
	//if( (numPhrases > parameters.phrasesInMem) || audioType > 1){
	//	return 0;
	//}
	//parameters.phrasesInMem = numPhrases;
	//TODO: implement save to EEPROM function
	EEPROMMode(); // enter pass-through mode
	
	xmodemInit(uartSendByte,uartGetByte);
	xmodemReceive();	//recieves file from hyperterm in Xmodem protocol, writes it to EEPROM
	//SaveGameParameters(); //save the new number of phrases
	
	SoundMode(); // return to normal chip mode
	return 1;
}
//may have to format the file on the fly

//	SaveGameParameters - Save Game Parameters to EEPROM 
//	input parameters
//		this might happen at the same time that the sound is saved to the chip?
// 		u08* parameter	name of parameter being saved
//		u08* value	value of the parameter
//	return value
//		0 if save failed
//		1 if save was succesfull
//
s08 SaveGameParameters(void)//u08* parameter, u08 *value)
{
//may have to format the file on the fly
	EEPROMMode(); // enter pass-through mode
	EEPROMWritePage(GAME_PARAMETERS_ADDR, (u08*)(&gp), GAME_PARAMETERS_SIZE);
	SoundMode(); // return to normal chip mode
	return 1;
}

u08 ReadGameParameters(void)
{
	EEPROMMode(); // enter pass-through mode
	EEPROMRead(GAME_PARAMETERS_ADDR, (u08*)(&gp), GAME_PARAMETERS_SIZE);
	SoundMode(); // return to normal chip mode
	return 1;
}

void sendSoundByte(u08 data)
{
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	//rprintf("%x ",inb(PORTC));
	while( (inb(PINC) & (1<<KX_nRDY))  || (inb(PINC) & (1<<KX_BUSY))){
			//rprintf("Waiting...");
		}//wait for kx1400 rdy
	spiSetCS( SOUND_SPI_SS );
	spiSendByte(data);
	SREG = sreg;//restore interrupts
}

void sendSoundWord(u16 data)
{
	u08 sreg;
	sreg = SREG;
	cli();//disable interrupts
	while( (inb(PINC) & (1<<KX_nRDY)) || (inb(PINC) & (1<<KX_BUSY))){
		//	rprintf("Waiting...");
		}//wait for kx1400 rdy
	spiSetCS( SOUND_SPI_SS );
	spiSendByte((data >> 8)&0x00FF);
	//while(inb(PORTC) & KX_nRDY);//wait for kx1400 rdy
	spiSendByte(data &(0xff));
	
	SREG = sreg;//restore interrupts
}

#ifdef TESTING_ON
s08 SoundTest(void)
{
	//SoundMode();
	//setVolume(15);
	//PlayTone(1000,1000);
	//setVolume(8);
	//PlayTone(10000,1000);
	PlayAudioPhrase(0);
	PlayAudioPhrase(0x0001);
	PlayAudioPhrase(0x0002);
	PlayAudioPhrase(0x0003);
	PlayAudioPhrase(0x0003);
	PlayAudioPhrase(0x0003);
	PlayAudioPhrase(0x0003);
	PlayAudioPhrase(0x0004);
	PlayAudioPhrase(0x0005);
	PlayAudioPhrase(0x0006);
	return TRUE;
}
#endif
