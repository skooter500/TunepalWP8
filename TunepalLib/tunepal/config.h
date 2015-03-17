#pragma once

class Config
{
public:
	static int getFundamental() { return _instance._fundamental; }
    static void setFundamental(int value) { _instance._fundamental = value; }

private:
	Config()
	{
		_fundamental = 3;
	}

	int _fundamental;

	static Config _instance;
};
