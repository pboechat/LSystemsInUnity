public class Production
{
    private string _predecessor;
    private string _successor;
    private float _probability;

    public string predecessor
    {
        get
        {
            return this._predecessor;
        }
    }

    public float probability
    {
        get
        {
            return this._probability;
        }
    }

    public string successor
    {
        get
        {
            return this._successor;
        }
    }

    public Production(string predecessor, string sucessor, float probability)
    {
        _predecessor = predecessor;
        _successor = sucessor;
        _probability = probability;
    }
    
}