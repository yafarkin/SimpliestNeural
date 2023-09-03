namespace MnistLearn;

public record DigitDto
{
    public byte Value { get; init; }

    public double[] Inputs { get; init; } = null!;
}