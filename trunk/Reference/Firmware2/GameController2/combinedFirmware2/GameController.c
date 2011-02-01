// http://search.digikey.com/scripts/DkSearch/dksus.dll?Detail&name=ATTINY2313-20SU-ND
// ATtiny2313, 2K flash, $2.15 Qty 1, $1.35 Qty 25
// http://search.digikey.com/scripts/DkSearch/dksus.dll?Detail&name=ATMEGA168-20PU-ND
// ATmega168, 16K flash, $4.11 Qty 1, $2.58 Qty 25, 28-pin DIP

#include <string.h>
#include <stdlib.h>
#include "uart.h"
#include "global.h"
#include "rprintf.h"
#include "avrlibdefs.h"
#include "avrlibtypes.h"
#include "gameState.h"
#include "UnitTest.h"
#include "soundchip.h"
#include "eeprom.h"
#include "pwmled.h"
#include "uart2.h"
#include "interrupts.h"
#include "timer.h"
#include "packets.h"
#include "shots.h"

#define DEBUG //comment this out to remove debug rprintfstatements

//#define TRUE 1
//#define FALSE 0

typedef enum _test{
	ALL,
	SOUND,
	EEPROM,
	PWM,
	BIUART
}test;

// Global Variables
gameParameters gp;
volatile u08 truckCount = 0;
u08 truckNames[6][16];
// console output
void putChar(int);
void GameParametersInit(void);
void SetHandlers(void);
char *getNum(char *, u08 *);
char *getNum16(char *, u16 *);
s08 LoadParameters(void);
void gameInit();
//void putString(char *);
//void putDecimal(int);
//void putHex(int,int);

// monitor global variables
static char version[] = "0.9 (3/10/2010)";	// version string 
static int echo=1;		// echo chars to stdout
static void cmdChar(char);	// process chars from console
static void command(char *);	// process command string
static char *scanString(char *, char *, int);
static void TestGame();
#ifdef TESTING_ON
static void TestDevice(test deviceID);
#endif

//-----------------------------------------------------------------------------------------

int main()
{	
	u08 ledCounter2 = 0;
	u16 ledCounter = 0;
	//local variables
	s08 c;//temp variable for pulling characters from the uart buffer
	//setup uart and rprintf functionality
	uartInit();					// initialize UART (serial port)
	uartSetBaudRate(9600);		// baud rate set to default of 9600 in UART Init
	rprintfInit(uartSendByte); 		//this sets the output stream for rprintf
	spiInit();
	timerInit();
	
	soundInit();
	pwmledInit();
	uart2Init();
	sbi(PORTD, DEBUG_LED);
	sbi(DDRD, DEBUG_LED);
	
	SetHandlers();
	uart2EnableLaserInt();
	uart2EnableXbeeInt();
	uart2EnableGPIOInt();
	
	/* use this only if you want receiving characters to be an interrupt driven process
	//uartSetRxHandler(cmdChar);  // set the UART rx interrupt handler to cmdChar function, automatically grabs char and processes it
	*/
	 

	if( !LoadParameters() )//load parameters from eeprom, if no parameters are saved init parameters to defaults
	{
		GameParametersInit(); //initializes GameParameters to defaults in EEPROMs
	}
	colorUpdate();
	rprintf("\nGame System - version ");	// 
	rprintfStr(version);
	rprintf("\n\n$");
	

	while(1)
	{
		if(ledCounter == 0x1FFF){
			ledCounter2++;
			ledCounter = 0;
		}
		if(ledCounter2 & 0x10){
			if(inb(PORTD) & (1<<DEBUG_LED)){//LED is on
				cbi(PORTD, DEBUG_LED); //turn off LED
			}
			else{
				sbi(PORTD, DEBUG_LED); //turn on LED
			}
			ledCounter2 = 0;
		}
		c=uartGetByte();
		if(c>=0){
			cmdChar(c);
		}
		ledCounter++;
	}
	return 0;
}

void SetHandlers()
{
// /* Sets the GPIO Handler for the IOs */
// void uart2SetGPIOHandler(void (*rx_func)(u08 c));

// /* Sets the RX Handler for the Xbee */
// void uart2SetXBeeHandler(void (*rx_func)(void * c));

// /* Sets the RX Handler for the Laser/Photodiode */
// void uart2SetLaserHandler(void (*rx_func)(u08 c));

	uart2SetXBeeHandler(handleXbeeInterrupt);
	uart2SetGPIOHandler(handleRobotInterrupt);
	uart2SetLaserHandler(handleLaserInterrupt);

}

void putChar(int c)
{
	uartSendByte(c);  // command to uart for printing to console
}

// characters received by console are processed here
// some filtering and preprocessing of command line are 
void cmdChar(char c)
{
	static char buf[128];
	static int i=0;
	if(echo) //echo char back to terminal if echo is set to 1
	{
		putChar(c); 
	}
	if ((c>='a' && c<='z') || (c>='A' && c<='Z'))	//alphabetic char
	{
		buf[i++] = c;
	}
	else if (c>'\040' && c<'\177')	// numbers and other printable characters
	{
		if (i>0)	// can't use as first character of command
		{
			buf[i++] = c;
		}
	}
	else if (c=='\011' || c==' ')	//tab or space, whitespace
	{
		if (i>0) // ignore whitespace at beginning of command string.
		{
			buf[i++] = ' ';
		}
	}
	else if (c=='\012' || c=='\015')	//linefeed or carriage return
	{
		if (i>0)				// process non null command
		{
			buf[i] = '\000';	// null terminate command string
			//saveCommand(buf);		// do command
			command(buf);
			i = 0;				// init for next command
		}
		rprintf("$ ");
	}
	// ignore all other characters (like non-printable characters
}

//	set [ <parameter> <value> | *all ]
///		given parameter to given value,
///		or set all parameters to their default values
//	display [ <parameter> | *all ]
///		display the value of a given parameter, or all parameters
//	reset [ uart | led | sound | biuart | *all ]
///		reset different peripherals
//	out [ laser | xbee ] <value>
///		send out messages
///		could also be made to send out periodic messages
///		for testing photo diodes, etc.
//	play [ <index> | <tone> <freq> <time> ]
///		play a sound, either a clip (index) or a tone
//  save [ parameters | audiofiles ]
///		parameters - save game parameters that have been set during this console period
///		audiofiles - save a preformatted string of audio files via xmodem protocol
//	eeprom [ enable | disable | upload <hex_file> | check <hex_file> ]
///		turn write enable on or off
///		upload code into eeprom, or check eeprom contents
//	game [ start | stop | pause | reset | init]
///		begin the game play, pause game, stop and reset game 
///		init game--pings all players and requests truck names, then waits to set up teams, sends team data, rule set, timercounts
//	test [ eeprom | sound | biuart | PWM | *all ]
///		runs test cases for devices specified
 
void command(char *s)
{
	char *p;	// pointer into string
	char *v;
	u16 tmp1;
	u16 tmp2;
	u08 tmp3;
	u08 i = 0;
	u16 x = 0;
	// convert command to lower case
	if (s[0]>='A' && s[0]<='Z') s[0] += 32;
	rprintfCRLF();
	switch (s[0])
	{
		case 'd':	// display [param | *<all>]
			if (!(p = scanString(s, "display", 1))) goto unknownCommand;
			if (!*p){
				rprintf("Display All");
				// display all
				break;
			}
			// convert command to lower case
			if ( *p>='A' && *p<='Z' ) *p += 32;
			switch (*p)
			{
				case 'b':
					if ((v = scanString(p, "base", 2)))
					{
						if(gp.isBase){
							rprintf("This controller is set to play as a base\n\r");
							}
						else
							rprintf("This controller is set to play as a robot\n\r");
					}
					else goto unknownSetParam;
					break;
				case 'r':
					if ((v = scanString(p, "robotid", 6)))
					{
						rprintf("My ID is:\n\r%d\n\r",gp.myTruckID);
					}
					else if ((v = scanString(p, "robotname", 6)))
					{
						rprintf("My Name is:\t");
						rprintfStr(gp.myTruckName);
						rprintf("\n\r");
					}
					else goto unknownDisplayParam;
					break;
				case 's':
					if ((v = scanString(p, "state", 2)))
					{
						rprintf("My Game State: %d\n", gp.myGameState);
					}
					else if ((v = scanString(p, "statusenabled", 7)))
					{
						//rprintf("Status Enabled color is set to\n\r\tRed: %d\tGreen: %d\tBlue: %d\n\r",gp.statusEnabled[0],gp.statusEnabled[1],gp.statusEnabled[2]);
					}
					else if ((v = scanString(p, "statusdisabled", 7)))
					{
						//rprintf("Status Disabled is set to\n\r\tRed: %d\tGreen: %d\tBlue: %d\n\r",gp.statusDisabled[0],gp.statusDisabled[1],gp.statusDisabled[2]);
					}
					else if ((v = scanString(p, "statushaveflag", 7)))
					{
						//rprintf("Status Have Flag color is set to\n\r\tRed: %d\tGreen: %d\tBlue: %d\n\r",gp.statusHaveFlag[0],gp.statusHaveFlag[1],gp.statusHaveFlag[2]);					
					}
					else goto unknownDisplayParam;
					break;
				case 't':
					if ((v = scanString(p, "teamid", 5)))
					{
						rprintf("Team ID is \t%d\n\r",gp.myTeamID);
						// TODO send teamSelect packet
					}
					else if ((v = scanString(p, "teamcolor", 5)))
					{
						//rprintf("Team color is set to\n\r\tRed: %d\tGreen: %d\tBlue: %d\n\r",gp.myColor[0],gp.myColor[1],gp.myColor[2]);
					}
					else goto unknownDisplayParam;
					break;
				case 'v':
					if ((v = scanString(p, "volume", 3)))
					{
						rprintf("Volume is set to:\t%d\n\r",gp.volume);
					}
					else if ((v = scanString(p, "version", 3)))
					{
						
					}
					else goto unknownDisplayParam;
					break;
				case 'x':
					if ((v = scanString(p, "xbeeid", 5)))
					{
						rprintf("XBEE ID is %d\n\r", gp.xbeeID);
					}
					else if ((v = scanString(p, "xbeechannel", 5)))
					{
						rprintf("XBEE ID is %d\n\r", gp.xbeeChannel);
					}
					else goto unknownSetParam;
					break;
				default:
				unknownDisplayParam:
					rprintf("Unknown parameter\n");//putString("Unknown parameter\n");
			}
			break;

		case 'e':	// eeprom [enable | disable | upload | check ]
			if (!(p = scanString(s, "eeprom", 1))) goto unknownCommand;
			if ((v = scanString(p, "check", 3)))
			{
				tmp3=0;
				EEPROMMode();
				for(x = 0; x<1600; x++){
					EEPROMRead((x),&tmp3,1);
					rprintf("%d:%x ",x,tmp3);
				}
				SoundMode();
			}
			else if ((v = scanString(p, "upload", 2))){
				for( i=0; i < 15; i++){
					timerPause(255);//255);
				}
				//rprintf("testing\n\r");
				SaveAudioPhrase();
			}
			else rprintf("Invalid parameter\n");
			break;

		case 'g':	// game [start | stop | reset | init]
			if (!(p = scanString(s, "game", 1))) goto unknownCommand;
			if ((v = scanString(p, "stop", 3)))
			{
				game.running = 0;
			}
			else if ((v = scanString(p, "start", 3)))
			{
				game.running = 1;
				if(gp.myTruckID == 0){
					//sendWireless(GAME_START)
					//uart2SendXBeePacket();
				}
			}
			else if ((v = scanString(p, "init", 1)))
			{
				if(gp.myTruckID == 0){
					gameInit();
				}
				else
					rprintf("Cant initialize game!");
			}
			else if ((v = scanString(p, "reset", 1)))
			{
				//reset functionality
			}
			else rprintf("Invalid parameter\n");
			break;

		case 'h':	// help
			if (!(p = scanString(s, "help", 1))) goto unknownCommand;
			rprintf(" display [ <parameter> | *all ]\n");
			rprintf(" set [ <parameter> <value> | *all ]\n");
			rprintf(" reset [ uart | led | sound | biuart | *all ]\n");
			rprintf(" out [ laser | xbee ] <value>\n");
			rprintf(" play [ <index> | tone <freq> <time> ]\n");
			rprintf(" save [ parameters | audiofiles ]\n");
			rprintf(" eeprom [ enable | disable | upload <hex_file> | check <hex_file> ]\n");
			rprintf(" game [ start | stop | pause | reset | init]\n");
			rprintf(" test [ eeprom | sound | biuart | PWM | *all ]\n");
			break;

		case 'o':	// out [laser | xbee] value
			if (!(p = scanString(s, "output", 1))) goto unknownCommand;
			//out functionality
			if ((v = scanString(p, "laser", 3))){
				uart2LaserSendString(v);
			}
			else if ((v = scanString(p, "xbee", 3))){
				uart2XbeeSendString(v);
			}
			else if ((v = scanString(p, "gpio", 3))){
				v = getNum(v, &i);
				uart2GPIOSetState(i);
			}
			break;

		case 'p':	// play [value | tone <freq> <time ms>]
			if (!(p = scanString(s, "play", 1))) goto unknownCommand;
			//play functionality
				//setVolume(15);
			if ((v = scanString(p, "tone", 1))){
			//convert the character representation of decimals to decimals
				p = getNum16( v, &tmp1); 
				v = getNum16( p, &tmp2);
				PlayTone(tmp1, tmp2);
			}
			else{
			//convert the character representation of decimals to decimals
				getNum(p, &i);
				PlayAudioPhrase(i);
			}
			break;

		case 'r':	// reset [uart | led | sound | biuart | *<all>]
			if (!(p = scanString(s, "reset", 1))) goto unknownCommand;
			if (!*p){
				rprintf("Reset All");
				// reset all
				soundReset(KX_NORMAL);
				//other reset functions
				break;
			}
			if ( *p>='A' && *p<='Z' ) *p += 32;
			switch (*p)
			{
				case 'l':
					break;
				case 's':
					soundReset(KX_NORMAL);
					break;
				case 'u':
					break;
				case 'b':
					break;
				default:
					rprintf("Unknown Parameter");
			}
			break;
		
		
		case 's':	// set [<param> <value> | *<defaults>]
			if ((p = scanString(s, "save", 3)))
			{
				SaveGameParameters();
				break;
			}
			else if (!(p = scanString(s, "set", 1))) goto unknownCommand;
			// convert parameter to lower case
			if ( *p>='A' && *p<='Z' ) *p += 32;
			switch (*p)
			{
				case 'b':
					if ((v = scanString(p, "base", 2)))
					{
						v = getNum(v, &(gp.isBase));
						if(!v)
						{
							goto unknownSetParam;
						}
					}
					else goto unknownSetParam;
					break;
				case 'r':
					if ((v = scanString(p, "robotid", 6)))
					{
						v = getNum(v, &(gp.myTruckID));
						if(!v)
						{
							goto unknownSetParam;
						}
					}
					else if ((v = scanString(p, "robotname", 6)))
					{
						if(v) // if v is not null
						{
							strcpy(gp.myTruckName, v);
						}
						else
							goto unknownSetParam;
					}
					else goto unknownSetParam;
					break;
				case 's':
					if ((v = scanString(p, "state", 2)))
					{
						v = getNum(v, &(gp.myGameState));
						colorUpdate();
						break;
					}
					else if ((v = scanString(p, "statusenabled", 7)))
					{
						for(i = 0; i < 3; i++){
							v = getNum(v, &(gp.statusEnabled[i]));
							if(!v)
							{
								goto unknownSetParam;
							}
						}
						/* Sets an LEDSet to a color */
						/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
						pwmSetLEDColor(STATUS_LEDSET, gp.statusEnabled);
					}
					else if ((v = scanString(p, "statusdisabled", 7)))
					{
						for(i = 0; i < 3; i++){
							v = getNum(v, &(gp.statusDisabled[i]));
							if(!v)
							{
								goto unknownSetParam;
							}
						}
						/* Sets an LEDSet to a color */
						/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
						pwmSetLEDColor(STATUS_LEDSET, gp.statusDisabled);
					}
					else if ((v = scanString(p, "statushaveflag", 7)))
					{
						for(i = 0; i < 3; i++){
							v = getNum(v, &(gp.statusHaveFlag[i]));
							if(!v)
							{
								goto unknownSetParam;
							}
						}
						/* Sets an LEDSet to a color */
						/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
						pwmSetLEDColor(STATUS_LEDSET, gp.statusHaveFlag);
					}
					else goto unknownSetParam;
					break;
				case 't':
					if ((v = scanString(p, "teamid", 5)))
					{
						v = getNum(v, &(gp.myTeamID) );
						if(!v)
						{
							goto unknownSetParam;
						}
					}
					else if ((v = scanString(p, "team1color", 5)))
					{
						for(i = 0; i < 3; i++){
							v = getNum(v, &(gp.team1Color[i]));
							if(!v)
							{
								goto unknownSetParam;
							}
						}
						/* Sets an LEDSet to a color */
						/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
						pwmSetLEDColor(TEAMCOLOR_LEDSET, gp.team1Color);
					}
					else if ((v = scanString(p, "team2color", 5)))
					{
						for(i = 0; i < 3; i++){
							v = getNum(v, &(gp.team2Color[i]));
							if(!v)
							{
								goto unknownSetParam;
							}
						}
						/* Sets an LEDSet to a color */
						/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
						pwmSetLEDColor(TEAMCOLOR_LEDSET, gp.myColor);
					}
					else if ((v = scanString(p, "teamname", 5)))
					{
					}
					else goto unknownSetParam;
					break;
				case 'v':
					if ((v = scanString(p, "volume", 3)))
					{
						v = getNum16(v, &(gp.volume));
						if(!v)
						{
							goto unknownSetParam;
						}
					}
					else if ((v = scanString(p, "version", 3)))
					{
					}
					else goto unknownSetParam;
					break;
				case 'x':
					if ((v = scanString(p, "xbeeid", 5)))
					{
						if(*v){
							gp.xbeeID= atoi(v);
						}
						else goto unknownSetParam;
					}
					else if ((v = scanString(p, "xbeechannel", 5)))
					{
						v = getNum(v, &(gp.xbeeChannel));
						if(!v)
						{
							goto unknownSetParam;
						}
					}
					else goto unknownSetParam;
					break;
				default:
				unknownSetParam:
					rprintf("Unknown parameter\n");
			}
			break;
			case 't':	// test [ eeprom | sound | biuart | PWM | *all ]
			if (!(p = scanString(s, "test", 1))) goto unknownCommand;
			if (!*p){//test all
				rprintf("Test All\n");
				TestGame();
				break;
			}
			else break;
#ifdef TESTING_ON			
		case 't':	// test [ eeprom | sound | biuart | PWM | *all ]
			if (!(p = scanString(s, "test", 1))) goto unknownCommand;
			if (!*p){//test all
				rprintf("Test All\n");
				TestDevice(ALL);
				break;
			}
			// convert parameter to lower case
			if ( *p>='A' && *p<='Z' ) *p += 32;
			switch (*p)
			{
				case 'e':
					if ((v = scanString(p, "eeprom", 2)))
					{
						TestDevice(EEPROM);
					}
					else goto unknownTestParam;
					break;
				case 'p':
					if ((v = scanString(p, "pwm", 2)))
					{
						TestDevice(PWM);
					}
					else goto unknownTestParam;
					break;
				case 's':
					if ((v = scanString(p, "sound", 2)))
					{
						TestDevice(SOUND);
					}
					else goto unknownTestParam;
					break;
				case 'b':
					if ((v = scanString(p, "biuart", 2)))
					{
						TestDevice(BIUART);
					}
					else goto unknownTestParam;
					break;
				default:
				unknownTestParam:
					rprintf("Unknown parameter\n");
			}
			break;
#endif
		default:
		unknownCommand:
				rprintf("Unknown parameter\n");//putString("Unknown command\n");
			break;
	}
	//commandReceived = FALSE;
}


//	scanString - scan a command string for a reference string
//	input parameters
// 		s1		command line string
//		s2		reference string
// 		exact	minimum number of characters that must match
//	return value
//		0 if strings are different
//		pointer to first non-blank character after command if strings are the same
//
char *scanString(char *s1, char *s2, int exact)
{
	char *p1, *p2;
	if (!s1 || !s2) return 0; //check for empty string
	for (p1=s1, p2=s2; *p1 && *p2;p1++, p2++)
	{
		if (*p1==' ')
			break;		// abbreviated command is OK
		if ('A'<=*p1 && 'Z'>=*p1) *p1 += 32;	// convert to lower case
		if (*p1 != *p2) return 0;	// not the same
	}
	if ((p1-s1)<exact) return 0;	// not enough matching characters
	if (!*p1) return p1;			// end of command
	if (*p1!=' ') return 0;			// extra command characters before whitespace
	while (*(++p1) && *p1==' ') ;	// scan past whitespace
	return p1;
}

// getNum - scan a command string and parse out the number at the begging of the command string
// inputs
// 		s1	command line string
//	  s08   pointer to the value you want to save the pointer to
// return value
//		0  if string does not begin with a number or if string is empty
//		pointer to next non whitespace character
char *getNum(char *s1, u08 *value){
	if(*s1){
		//rprintfStr(v);
		*value = atoi(s1);
		//v = p;
	}
	else return 0;
	while (*(++s1) && *s1!=' ') ;	//find whitespace
	while (*(++s1) && *s1==' ') ;   //scan past whitespace
	return s1;
}

char *getNum16(char *s1, u16 *value){
	if(*s1){
		//rprintfStr(v);
		*value = atoi(s1);
		//v = p;
	}
	else return 0;
	while (*(++s1) && *s1!=' ') ;	//find whitespace
	while (*(++s1) && *s1==' ') ;   //scan past whitespace
	return s1;
}

#ifdef TESTING_ON
static void TestDevice(test deviceID){
	s08 success = TRUE;
	rprintf("---------Start Test---------\n");
	switch(deviceID){
		case ALL:
			if( (success = SoundTest()) )
				rprintf("Sound test passed!\r\n");
			EEPROMMode();
			if( (success = EEPROMTest()) )
				rprintf("EEPROM test passed!\r\n");
			SoundMode();
			break;
		case SOUND:
			if( (success = SoundTest()) )
				rprintf("Sound test passed!\r\n");
			break;
		case EEPROM:
			EEPROMMode();
			if( (success = EEPROMTest()) )
				rprintf("EEPROM test passed!\r\n");
			SoundMode();
			break;
		case PWM:
			if( (success = PWMTest()) )
				rprintf("PWM test passed (maybe did you see the lights blink?)!\r\n");
			break;
		case BIUART:
			//if( (success = UART2Test()) )
				//rprintf("DUAL UART test passed!\r\n");
			break;
		default:
			rprintf("No deviceID specified");
	}
	if(!success)
		rprintf("Test Failed\n");
	else
		rprintf("Test Passed\n");
	rprintf("----------End Test----------\n");
}
#endif



void colorInit(void){
	//team1
	gp.team1Color[0] = 0;
	gp.team1Color[1] = 250;
	gp.team1Color[2] = 0;
	//team2
	gp.team2Color[0] = 250;
	gp.team2Color[1] = 0;
	gp.team2Color[2] = 0;
	//Enabled without flag
	gp.statusEnabled[0] = 0;
	gp.statusEnabled[1] = 250;
	gp.statusEnabled[2] = 250;
	//Disabled
	gp.statusDisabled[0] = 250;
	gp.statusDisabled[1] = 250;
	gp.statusDisabled[2] = 0;
	//Enabled With Flag
	gp.statusHaveFlag[0] = 250;
	gp.statusHaveFlag[1] = 50;
	gp.statusHaveFlag[2] = 0;
	colorUpdate();
}

static void TestGame()
{
	// rprintf("My Truck Name:\t", gp.myTruckName);
	// rprintfStr(gp.myTruckName);
	// rprintf("\nMy Truck ID:\t%d\n", gp.myTruckID);
	// rprintf("My Team ID:\t%d\n", gp.myTeamID);
	// rprintf("my state = %d\n", gp.myGameState);
		
	// colorInit();
	
	// fireLaser(KILL);
	// colorUpdate();
	//rprintf("Testing Laser\n");
	u08 laserPacket;
	laserPacket = (0xc0 & (KILL << 6)) | (0x30 & (gp.myTeamID << 4)) | (0x0F & gp.myTruckID);
	uart2LaserSendString(&laserPacket);
	//rprintf("Testing Complete\n");
}





s08 LoadParameters(void){
	u08 i;
	u08 *x = (u08*)&gp;
	//read from EEPROM into buffer
	if( !ReadGameParameters() ){
			return FALSE;
	}
	for(i = 0; i < GAME_PARAMETERS_SIZE; i++){
			//rprintf("%x ", x[i]);
			if(x[i] != 0xFF){
				return TRUE;
			}
	}
	return FALSE;
}

void GameParametersInit(void){
	s08 i = 0;
	gp.ruleFlags = 0;
	gp.isBase = 0;
	gp.myTeamID = 0;
	gp.myTruckID = 0;
	gp.myGameState = 0;
	u08 green[3] = {0, 250, 0}; //team1
	u08 red[3] = {250, 0, 0}; //team2
	u08 cyan[3] = {0, 250, 250}; //enabled_without_flag
	u08 yellow[3] = {250, 250, 0}; //disabled
	u08 orange[3] = {250, 50, 0}; //enabled_with_flag
	
	for(i = 0;i < 16; i++){
		gp.myTruckName[i] = 0;
		if(i<3){
			gp.team1Color[i] = green[i]; // Set by default to green
			gp.team2Color[i] = red[i]; // Set by default to red
			gp.statusEnabled[i] = cyan[i]; //sets statusEnabled color to cyan
			gp.statusDisabled[i] = yellow[i];
			gp.statusHaveFlag[i] = orange[i];
		}
	}
	
	gp.hlIntensity = 0;
	gp.tlIntensity = 0;
	gp.phrasesInMem = 0;
 	gp.volume = 0;
	gp.xbeeID = 0;
	gp.xbeeChannel = 0;
	
}

void gameInit()
{
	u08 trucksPlaying = 0;
	u08 teams = 0;
	u08 teamOne[3];
	u08 teamTwo[3];
	u08 i = 0;
	char c = 0;
	//init fuctionality
	//scanf("%d",&trucksPlaying);
	//rprintf("%d",trucksPlaying);
	rprintf("How many trucks?\n");
	
	c = uartGetByte();
	trucksPlaying = atoi( &c );
	
	//how many teams
	rprintf("How many teams?\n");
	c = uartGetByte();
	teams = atoi( &c );

	// Send Wireless to initialize game
	struct gameInitPacket init;
	init.packetType = GAME_INIT;
	init.ruleFlags = gp.ruleFlags;
	uart2SendXBeePacket(&init, GAME_INIT_SIZE);
	
	while(truckCount < trucksPlaying);
	
	//rprintf("#: truck ID, which robots are on team 1?");
	rprintf("List trucks that will be on team 1 by number (starting with the base): ");
	
	for(i = 0; i < truckCount; i++)
	{
		rprintf("%d:\t", i);
		rprintfStr(truckNames[i]);
		rprintf("\n");
	}
	// Russell --> get teams for trucks teamOne[]
	c = uartGetByte();
	teamOne[0] = atoi( &c );
	c = uartGetByte();
	teamOne[1] = atoi( &c );
	if(trucksPlaying >2){
		c = uartGetByte();
		teamOne[2] = atoi( &c );
	}
	// set the others to teamTwo[]
	c = uartGetByte();
	teamTwo[0] = atoi( &c );
	c = uartGetByte();
	teamTwo[1] = atoi( &c );
	if(trucksPlaying >2){
		c = uartGetByte();
		teamTwo[2] = atoi( &c );
	}
	// Tell team one trucks they are on team one.
	for(i = 0; i < 3; i++)
	{
		struct teamSelectPacket select;
		select.packetType = TEAM_SELECT;
		strcpy(select.truckName, truckNames[teamOne[i]]);
		select.teamID = TEAM1; 
		select.truckID = teamOne[i];
		// select.teamColor = color;
		// select.statusColor = color;
		uart2SendXBeePacket(&select, TEAM_SELECT_SIZE);
	}	
	
	// Tell team two trucks they are on team two.
	for(i = 0; i < 3; i++)
	{
		struct teamSelectPacket select;
		select.packetType = TEAM_SELECT;
		strcpy(select.truckName, truckNames[teamTwo[i]]);
		select.teamID = TEAM2; 
		select.truckID = teamTwo[i];
		// select.teamColor = color;
		// select.statusColor = color;
		uart2SendXBeePacket(&select, TEAM_SELECT_SIZE);
	}	
}
