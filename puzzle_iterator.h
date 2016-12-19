template <class TIteration>
class puzzle_iterator
{
public:
	TIteration start;
	puzzle_iterator(const TIteration& _start) : start(_start) {}

	TIteration get_best()
	{
		vector<TIteration> its;
		its.push_back(start);
		while (!its.begin()->is_target)
		{
			cout << *its.begin() << endl;
			const vector<TIteration> newitems = its.begin()->expand();
			if (!is_cyclic())
			{
				its.erase(its.begin());
			}
			its.insert(its.end(), newitems.begin(), newitems.end());
			sort(its.begin(), its.end());
		}
		return *its.begin();
	}

	bool is_cyclic()
	{
		return true;
	}
};
