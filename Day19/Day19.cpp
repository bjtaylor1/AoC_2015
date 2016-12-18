// Day19.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

using namespace std;

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


class iteration
{
public:
	string desc;
	vector<string> expansions;
	int depth;
	int length;
	bool visited;
	bool is_target;
	int reducability;

	static vector<string> get_replacements(const string& input, const reduction& r)
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


	static int get_reducability(const vector<string>& expansions, const int length)
	{
		//minus is good
		int reducability = 0;
		for (vector<string>::const_iterator it = expansions.begin(); it != expansions.end(); it++)
		{
			reducability += it->size() - length;
		}
		return reducability;
	}

	
	iteration(const string& _desc, const int _depth, const bool _visited) : desc(_desc), depth(_depth), visited(_visited), 
		length(desc.size()), expansions(get_expansions(_desc)), reducability(get_reducability(expansions, length)), is_target(_desc == "e") {}

	iteration(const string& _desc, const int _depth) : iteration(_desc, _depth, false) {}
	vector<iteration> expand()
	{
		visited = true;
		vector<iteration> its;
		for (vector<string>::const_iterator it = expansions.begin(); it != expansions.end(); it++)
		{
			iteration i(*it, depth + 1);
			its.push_back(i);
		}
		return its;
	}

	
	bool operator<(const iteration& rhs) //return true if lhs is better
	{
		if (is_target != rhs.is_target)
		{
			return is_target > rhs.is_target;
		}
		if (visited != rhs.visited)
		{
			return visited < rhs.visited;
		}
		if (length != rhs.length)
		{
			return length < rhs.length;
		}
		return reducability < rhs.reducability;
	}

	void print_answer()
	{
		cout << desc << ", Depth: " << depth << endl;
	}

};

ostream& operator<<(ostream& os, const iteration& i)
{
	os << i.length << ": " << i.desc.substr(0, 50);
	return os;
}


int main()
{
	iteration initial("CRnCaSiRnBSiRnFArTiBPTiTiBFArPBCaSiThSiRnTiBPBPMgArCaSiRnTiMgArCaSiThCaSiRnFArRnSiRnFArTiTiBFArCaCaSiRnSiThCaCaSiRnMgArFYSiRnFYCaFArSiThCaSiThPBPTiMgArCaPRnSiAlArPBCaCaSiRnFYSiThCaRnFArArCaCaSiRnPBSiRnFArMgYCaCaCaCaSiThCaCaSiAlArCaCaSiRnPBSiAlArBCaCaCaCaSiThCaPBSiThPBPBCaSiRnFYFArSiThCaSiRnFArBCaCaSiRnFYFArSiThCaPBSiThCaSiRnPMgArRnFArPTiBCaPRnFArCaCaCaCaSiRnCaCaSiRnFYFArFArBCaSiThFArThSiThSiRnTiRnPMgArFArCaSiThCaPBCaSiRnBFArCaCaPRnCaCaPMgArSiRnFYFArCaSiThRnPBPMgAr"
		, 0);
	puzzle_iterator<iteration> puzzle(initial);
	iteration target = puzzle.get_best();
	target.print_answer();
	return 0;
}

