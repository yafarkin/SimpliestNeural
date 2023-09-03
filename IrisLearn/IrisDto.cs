using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace IrisLearn;

public record IrisDto
{
    [Name("sepal_length")]
    public double SepalLength { get; init; }

    [Name("sepal_width")]
    public double SepalWidth { get; init; }

    [Name("petal_length")]
    public double PetalLength { get; init; }

    [Name("petal_width")]
    public double PetalWidth { get; init; }

    [Name("species")]
    public string Species { get; init; } = null!;

    private int _value;

    [Ignore]
    public int Value
    {
        get
        {
            if (0 == _value)
            {
                switch (Species)
                {
                    case "setosa":
                        _value = 1;
                        break;
                    case "versicolor":
                        _value = 2;
                        break;
                    case "virginica":
                        _value = 3;
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid 'species' value: {Species}");
                }
            }

            return _value;
        }
    }
}