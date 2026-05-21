namespace GWO.Optimizer;

public class EliteWolves
{
    private Wolf[] _wolves = new Wolf[3];

    public EliteWolves()
    {
        for (int i = 0; i < _wolves.Length; ++i)
        {
            _wolves[i] = Wolf.DefaultWolf();
        }
    }
    
    public Wolf[] Get()
    {
        return _wolves;
    }

    public void Insert(Wolf wolf)
    {
        for (int i = 0; i < 3; ++i)
        {
            if (wolf.Fitness < _wolves[i].Fitness)
            {
                for (int j = 2; j >= i + 1; --j)
                {
                    _wolves[j] = _wolves[j - 1];
                }

                _wolves[i] = wolf;
                break;
            }
        }
    }

    public void Reset()
    {
        _wolves[0] = Wolf.DefaultWolf();
        _wolves[1] = Wolf.DefaultWolf();
        _wolves[2] = Wolf.DefaultWolf();
    }
    public Wolf Alpha()
    {
        return _wolves[0];
    }
    public Wolf Beta()
    {
        return _wolves[1];
    }
    public Wolf Delta()
    {
        return _wolves[2];
    }
}