#ifndef SIGNATURE_H 
#define SIGNATURE_H 

#define NUM_SIGS 36



typedef struct rangeentry
{
	unsigned short hmin;
	unsigned short hmax;
	unsigned short smin;
	unsigned short smax;
	unsigned short vmin;
	unsigned short vmax;
	unsigned short signature;

};


rangeentry orangetable[NUM_SIGS];
rangeentry greentable[NUM_SIGS];
rangeentry bluetable[NUM_SIGS];

unsigned short orangeranges[] =  
{ 
146, 15, 59, 155, 30, 113, 
158, 11, 87, 204, 54, 256, 
172, 17, 115, 200, 58, 256, 
172, 19, 118, 224, 187, 256, 
172, 17, 115, 200, 58, 256, 
163, 7, 100, 251, 82, 145, 
146, 19, 126, 212, 93, 256, 
174, 9, 82, 189, 85, 211, 
167, 20, 120, 179, 78, 226, 
173, 9, 93, 256, 121, 256, 
175, 11, 108, 256, 126, 256, 
140, 7, 74, 212, 178, 256, 
175, 6, 143, 256, 150, 256, 
175, 11, 108, 256, 126, 256, 
131, 5, 57, 130, 48, 96, 
146, 5, 45, 94, 45, 95, 
148, 10, 52, 191, 183, 256, 
153, 22, 84, 256, 152, 256, 
172, 17, 115, 200, 58, 256, 
172, 17, 115, 200, 58, 256, 
159, 7, 93, 214, 148, 256, 
162, 6, 89, 192, 76, 256, 
0, 0, 95, 234, 148, 256, 
126, 27, 81, 138, 23, 83, 
172, 10, 95, 234, 148, 256, 
174, 11, 95, 234, 148, 256, 
172, 10, 95, 234, 148, 256, 
172, 10, 95, 234, 148, 256, 
174, 11, 95, 234, 148, 256, 
174, 11, 95, 234, 148, 256, 
174, 11, 95, 234, 148, 256, 
0, 0, 0, 256, 0, 256, 
174, 5, 93, 167, 91, 197, 
0, 0, 95, 234, 148, 256, 
170, 6, 90, 169, 83, 128, 
149, 13, 73, 161, 44, 104 
};
unsigned short greenranges[] = 
{
41, 61, 42, 99, 80, 146, 
30, 72, 70, 175, 75, 256, 
0, 0, 0, 0, 0, 0, 
0, 0, 0, 161, 187, 256, 
36, 101, 0, 133, 193, 256, 
54, 64, 60, 91, 61, 122, 
0, 0, 0, 161, 187, 256, 
39, 57, 83, 128, 111, 256, 
38, 61, 68, 141, 84, 233, 
0, 96, 0, 71, 248, 256, 
0, 205, 0, 7, 249, 256, 
40, 70, 0, 161, 187, 256, 
0, 205, 0, 7, 249, 256, 
0, 205, 0, 7, 249, 256, 
49, 71, 42, 118, 63, 256, 
38, 69, 42, 91, 47, 131, 
0, 1, 0, 2, 251, 256, 
30, 96, 25, 148, 242, 256, 
135, 162, 6, 14, 246, 256, 
43, 69, 64, 125, 127, 256, 
30, 65, 46, 156, 126, 256, 
37, 64, 48, 141, 119, 256, 
30, 62, 50, 192, 135, 256, 
37, 64, 46, 82, 45, 80, 
30, 62, 50, 192, 135, 256, 
30, 62, 50, 192, 135, 256, 
30, 62, 50, 192, 135, 256, 
30, 62, 50, 192, 135, 256, 
30, 62, 50, 192, 135, 256, 
0, 0, 70, 173, 153, 256, 
0, 0, 70, 173, 153, 256, 
28, 58, 70, 173, 153, 256, 
39, 70, 64, 154, 107, 236, 
39, 70, 64, 154, 107, 236, 
37, 49, 54, 96, 81, 120, 
38, 57, 45, 85, 59, 90 
};
unsigned short blueranges[]=
{
108, 121, 90, 133, 69, 137, 
83, 119, 92, 215, 94, 256, 
77, 116, 89, 143, 240, 256, 
23, 125, 74, 176, 212, 256, 
0, 0, 0, 0, 0, 0, 
0, 0, 90, 133, 69, 137, 
0, 0, 74, 176, 212, 256, 
82, 124, 55, 158, 216, 256, 
0, 0, 70, 175, 75, 256, 
0, 0, 36, 101, 45, 83, 
82, 115, 7, 197, 255, 256, 
75, 154, 101, 230, 135, 256, 
0, 0, 55, 158, 216, 256, 
0, 0, 7, 197, 255, 256, 
0, 0, 90, 133, 69, 137, 
0, 0, 90, 133, 69, 137, 
87, 120, 134, 180, 149, 256, 
0, 0, 36, 101, 45, 83, 
0, 0, 0, 0, 0, 0, 
0, 0, 0, 0, 0, 0, 
81, 120, 72, 169, 135, 256, 
0, 0, 70, 175, 75, 256, 
105, 121, 70, 183, 132, 256, 
102, 137, 36, 101, 45, 83, 
0, 0, 60, 190, 160, 256, 
102, 127, 60, 190, 160, 256, 
0, 0, 60, 190, 160, 256, 
0, 0, 60, 190, 160, 256, 
0, 0, 0, 256, 0, 256, 
0, 0, 0, 256, 0, 256, 
0, 0, 0, 256, 0, 256, 
0, 0, 0, 256, 0, 256, 
100, 119, 92, 164, 86, 256, 
100, 119, 92, 164, 86, 256, 
0, 0, 92, 164, 86, 256, 
114, 127, 40, 77, 57, 80 
};
unsigned short signatures[] =  
{
7316, 
8350, 
8591, 
12920, 
12988, 
13890, 
14914, 
19038, 
19293, 
20482, 
23635, 
23777, 
24907, 
25209, 
27316, 
27981, 
28576, 
28717, 
29043, 
33411, 
36039, 
38297, 
39696, 
42649, 
43309, 
43776, 
43856, 
44073, 
44185, 
44274, 
44308, 
44745, 
50745, 
50812, 
54786, 
57823
};


void init_signatures()
{
	for(int i=0;i<NUM_SIGS; i++)
	{
		orangetable[i].hmin = *(orangeranges + i*6);
		orangetable[i].hmax = *(orangeranges + i*6 + 1);
		orangetable[i].smin = *(orangeranges + i*6 + 2);
		orangetable[i].smax = *(orangeranges + i*6 + 3);
		orangetable[i].vmin = *(orangeranges + i*6 + 4);
		orangetable[i].vmax = *(orangeranges + i*6 + 5);
		orangetable[i].signature = signatures[i];

		greentable[i].hmin = *(greenranges + i*6);
		greentable[i].hmax = *(greenranges + i*6 + 1);
		greentable[i].smin = *(greenranges + i*6 + 2);
		greentable[i].smax = *(greenranges + i*6 + 3);
		greentable[i].vmin = *(greenranges + i*6 + 4);
		greentable[i].vmax = *(greenranges + i*6 + 5);
		greentable[i].signature = signatures[i];

		bluetable[i].hmin = *(blueranges + i*6);
		bluetable[i].hmax = *(blueranges + i*6 + 1);
		bluetable[i].smin = *(blueranges + i*6 + 2);
		bluetable[i].smax = *(blueranges + i*6 + 3);
		bluetable[i].vmin = *(blueranges + i*6 + 4);
		bluetable[i].vmax = *(blueranges + i*6 + 5);
		bluetable[i].signature = signatures[i];
	}
}
int findSigIndex(int inputsig)
{
	unsigned short input = inputsig%(64*1024-1);

	for(int i=0; i< NUM_SIGS; i++)
	{
		if(input == signatures[i])
			return i;
	}
	return -1; //didn't find a match.

}

#endif