// Day19.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

class c
{
public:
	std::string s;
	int i;
	c(const int _i , const std::string _s) : i(_i) , s(_s) {}
	bool operator<(const  c& other) //(1)
	{
		if (i != other.i) return i < other.i;
		return s < other.s;
	}
};


bool iThenS(const c& c1, const c& c2)
{
	if (c1.i != c2.i) return c1.i < c2.i;
	return c1.s < c2.s;
}


std::ostream& operator<<(std::ostream& os, const c& c)
{
	os << c.i << "; " << c.s;
	return os;
}

int main()
{
	std::vector<c> vec;
	vec.push_back(c(42 , "def"));
	vec.push_back(c(42 , "abc"));
	vec.push_back(c(715 , "abc"));
	std::sort(vec.begin(), vec.end(), iThenS);
	for (std::vector<c>::iterator it = vec.begin(); it != vec.end(); it++)
	{
		std::cout << (*it) << std::endl;
	}
	std::cout << vec.size() << std::endl;
	return 0;
}

