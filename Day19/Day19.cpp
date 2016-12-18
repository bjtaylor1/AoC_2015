// Day19.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

using namespace std;

class c
{
public:
	const string  s;
	const int i;
	c(const int _i, const string _s) : i(_i), s(_s) {}
	c& operator=(const c& rhs)
	{
		return c(rhs.i, rhs.s);
	}

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


ostream& operator<<(ostream& os, const c& c)
{
	os << c.i << "; " << c.s;
	return os;
}

class reduction
{
public:
	const string find;
	const string replacement;
	reduction(const string& f, const string& r) : find(f), replacement(r) {}
	reduction& operator=(const reduction& rhs) { return reduction(rhs.find, rhs.replacement); }
};

const vector<reduction> get_reductions()
{
	vector<reduction> r;
	r.push_back(reduction("ThF", "Al"));
	r.push_back(reduction("ThRnFAr", "Al"));
	r.push_back(reduction("BCa", "B"));
	r.push_back(reduction("TiB", "B"));
	r.push_back(reduction("TiRnFAr", "B"));
	r.push_back(reduction("CaCa", "Ca"));
	r.push_back(reduction("PB", "Ca"));
	r.push_back(reduction("PRnFAr", "Ca"));
	r.push_back(reduction("SiRnFYFAr", "Ca"));
	r.push_back(reduction("SiRnMgAr", "Ca"));
	r.push_back(reduction("SiTh", "Ca"));
	r.push_back(reduction("CaF", "F"));
	r.push_back(reduction("PMg", "F"));
	r.push_back(reduction("SiAl", "F"));
	r.push_back(reduction("CRnAlAr", "H"));
	r.push_back(reduction("CRnFYFYFAr", "H"));
	r.push_back(reduction("CRnFYMgAr", "H"));
	r.push_back(reduction("CRnMgYFAr", "H"));
	r.push_back(reduction("HCa", "H"));
	r.push_back(reduction("NRnFYFAr", "H"));
	r.push_back(reduction("NRnMgAr", "H"));
	r.push_back(reduction("NTh", "H"));
	r.push_back(reduction("OB", "H"));
	r.push_back(reduction("ORnFAr", "H"));
	r.push_back(reduction("BF", "Mg"));
	r.push_back(reduction("TiMg", "Mg"));
	r.push_back(reduction("CRnFAr", "N"));
	r.push_back(reduction("HSi", "N"));
	r.push_back(reduction("CRnFYFAr", "O"));
	r.push_back(reduction("CRnMgAr", "O"));
	r.push_back(reduction("HP", "O"));
	r.push_back(reduction("NRnFAr", "O"));
	r.push_back(reduction("OTi", "O"));
	r.push_back(reduction("CaP", "P"));
	r.push_back(reduction("PTi", "P"));
	r.push_back(reduction("SiRnFAr", "P"));
	r.push_back(reduction("CaSi", "Si"));
	r.push_back(reduction("ThCa", "Th"));
	r.push_back(reduction("BP", "Ti"));
	r.push_back(reduction("TiTi", "Ti"));
	r.push_back(reduction("HF", "e"));
	r.push_back(reduction("NAl", "e"));
	r.push_back(reduction("OMg", "e"));
	return r;
}

static vector<reduction> reductions = get_reductions();

const vector<string> get_replacements(const string& input, const reduction& r)
{
	vector<string> replacements;
	size_t start = 0;
	const size_t findsize = r.find.size();
	for (size_t pos = input.find(r.find); pos < input.size(); pos = input.find(r.find, pos + 1))
	{
		string replacement = input;
		replacement.replace(pos, findsize, r.replacement);
		replacements.push_back(replacement);
	}
	return replacements;
}

const int get_reducability(const string& input, const reduction& r)
{
	int reducability = 0;
	size_t start = 0;
	const size_t findsize = r.find.size();
	for (size_t pos = input.find(r.find); pos < input.size(); pos = input.find(r.find, pos + 1))
	{
		reducability++;
	}
	return reducability;
}

class iteration
{
public:
	const string desc;
	const vector<string> expansions;
	const int depth;
	const int length;
	bool visited;
	const int reducability;

	static vector<string> get_expansions(const string& desc)
	{
		vector<string> retval;
		vector<iteration> its;
		for (vector<reduction>::const_iterator it = reductions.begin(); it != reductions.end(); it++)
		{
			const vector<string> replacements = get_replacements(desc, *it);
			for (vector<string>::const_iterator r_it = replacements.begin(); r_it != replacements.end(); r_it++)
			{
				retval.push_back(*r_it);
			}
		}
		return retval;
	}


	static int get_reducability_of_expansions(const vector<string>& expansions)
	{
		return 0;
	}

	iteration(const string& _desc, const int _depth, const bool _visited) : desc(_desc), depth(_depth), visited(_visited), 
		length(desc.size()), expansions(get_expansions(_desc)), reducability(get_reducability_of_expansions(expansions)) {}

	iteration(const string& _desc, const int _depth) : iteration(_desc, _depth, false) {}
	const vector<iteration> expand()
	{
		vector<iteration> its;
		for (vector<string>::const_iterator it = expansions.begin(); it != expansions.end(); it++)
		{
			iteration i(*it, depth + 1);
			its.push_back(i);
		}
		return its;
	}

	const int get_my_reducability() const
	{
		int reducability = 0;
		for (vector<reduction>::const_iterator it = reductions.begin(); it != reductions.end(); it++)
		{
			reducability += get_reducability(desc, *it);
		}
		return reducability;
	}

};

bool compare(const iteration& lhs, const iteration& rhs) //return true if lhs is better
{
	if (lhs.length != rhs.length) return lhs.length < rhs.length;
	int lhsreducability = lhs.get_my_reducability();
	int rhsreducability = rhs.get_my_reducability();
	return lhsreducability > rhsreducability;
}

int main()
{
	string floor("^^..^^");
	char c = floor[2];
	string blah;
	blah += c;
	if (c == '.')
	{
		cout << "yes: " << c << endl;
		cout << blah << endl;
	}
	vector<iteration> its;
	its.push_back(iteration("CRnCaSiRnBSiRnFArTiBPTiTiBFArPBCaSiThSiRnTiBPBPMgArCaSiRnTiMgArCaSiThCaSiRnFArRnSiRnFArTiTiBFArCaCaSiRnSiThCaCaSiRnMgArFYSiRnFYCaFArSiThCaSiThPBPTiMgArCaPRnSiAlArPBCaCaSiRnFYSiThCaRnFArArCaCaSiRnPBSiRnFArMgYCaCaCaCaSiThCaCaSiAlArCaCaSiRnPBSiAlArBCaCaCaCaSiThCaPBSiThPBPBCaSiRnFYFArSiThCaSiRnFArBCaCaSiRnFYFArSiThCaPBSiThCaSiRnPMgArRnFArPTiBCaPRnFArCaCaCaCaSiRnCaCaSiRnFYFArFArBCaSiThFArThSiThSiRnTiRnPMgArFArCaSiThCaPBCaSiRnBFArCaCaPRnCaCaPMgArSiRnFYFArCaSiThRnPBPMgAr"
		, 0));

	string s("ababab");
	reduction r("ab", "cd");
	//vector<string> r = get_replacements(s, &r);

	/*
	vector<c> vec;
	vec.push_back(c(42 , "def"));
	vec.push_back(c(42 , "abc"));
	vec.push_back(c(715 , "abc"));
	sort(vec.begin(), vec.end(), iThenS);
	for (vector<c>::iterator it = vec.begin(); it != vec.end(); it++)
	{
		cout << (*it) << endl;
	}
	cout << vec.size() << endl;
	*/
	return 0;
}

