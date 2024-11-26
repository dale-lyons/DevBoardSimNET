struct CmdParameter
{
	char param[PARAM_SIZE];
};
typedef void (*ptr)(void);

struct RegisterFile
{
	char	psw;
	char	a;
	char	c;
	char	b;
	char	e;
	char	d;
	char	l;
	char	h;
	char	spl;
	char	sph;
};

struct Ram
{
	char commandLine[32];
	struct CmdParameter cmdParameters[NUM_PARAMS];
	struct RegisterFile regFile;
};
