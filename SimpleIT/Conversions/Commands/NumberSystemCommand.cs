using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using SimpleIT.Conversions.Model.Enums;
using System.Text.RegularExpressions;

namespace SimpleIT.Conversions.Commands;

[Command("numbersystem-conversion", Description = "Converts number from one system to the other")]
public class NumberSystemCommand : ICommand
{
    [CommandParameter(0, Description = "Value to be converted")]
    public required string NumberValue { get; init; }    

    [CommandParameter(1, Description = "Source number system")]
    public required NumberSystems Source { get; init; }

    [CommandParameter(2, Description = "Target number system")]
    public required NumberSystems Target { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        if(Source == Target)
        {
            throw new CommandException($"No need to convert {Source} to {Target}.");
        }

        if (!IsValidNumberForBase(NumberValue))
        {
            throw new CommandException("The given number is not valid for the base number system");
        }

        var resultString = ConvertNumber(NumberValue, Target);

        resultString = resultString.TrimStart('0');

        console.Output.WriteLine(string.Concat("Old Value: ", NumberValue));
        console.Output.WriteLine(string.Concat("New Value: ", resultString));

        return default(ValueTask);

    }

    private string ConvertNumber(string number, NumberSystems target)
    {
        return target switch
        {
            NumberSystems.BINARY => ConvertToBinary(number),
            NumberSystems.DECIMAL => ConvertToDecimal(number),
            NumberSystems.HEXADECIMAL => ConvertToHexadecimal(number),
            _ => throw new CommandException($"Unsupported target number system: {target}")
        };
    }

    private string ConvertToBinary(string number)
    {
        var parts = GetNumberParts(number);
        var integerBinary = string.Concat(parts.Item1.Select(c => Convert.ToString(GetDigitValue(c), 2).PadLeft(4, '0')));
        var fractionalBinary = string.Concat(parts.Item2.Select(c => Convert.ToString(GetDigitValue(c), 2).PadLeft(4, '0')));
        return string.IsNullOrEmpty(fractionalBinary) ? integerBinary : $"{integerBinary}.{fractionalBinary}";
    }

    private string ConvertToDecimal(string number)
    {
        var parts = GetNumberParts(number);
        var integerPart = parts.Item1.Reverse().Select((c, i) => GetDigitValue(c) * (decimal)Math.Pow((int)Source, i)).Sum();
        var fractionPart = parts.Item2.Select((c, i) => GetDigitValue(c) * (decimal)Math.Pow((int)Source, -(i + 1))).Sum();
        return (integerPart + fractionPart).ToString();
    }

    private string ConvertToHexadecimal(string number)
    {
        var parts = GetNumberParts(number);
        var integerHex = string.Concat(parts.Item1.Select(c => GetDigitValue(c).ToString("X")));
        var fractionalHex = string.Concat(parts.Item2.Select(c => GetDigitValue(c).ToString("X")));
        return string.IsNullOrEmpty(fractionalHex) ? integerHex : $"{integerHex}.{fractionalHex}";
    }

    private int GetDigitValue(char digit) => char.IsDigit(digit) ? digit - '0' : char.ToUpper(digit) - 'A' + 10;

    private Tuple<string, string> GetNumberParts(string number)
    {
        var parts = number.Split('.');
        return new Tuple<string, string>(parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }

    private bool IsValidNumberForBase(string input)
    {
        if (Source == NumberSystems.BINARY)
            return Regex.IsMatch(input, "^[01]+$");

        if (Source == NumberSystems.HEXADECIMAL)
            return Regex.IsMatch(input, "^[0-9A-Fa-f]+$");

        if (Source == NumberSystems.DECIMAL)
            return decimal.TryParse(input,out decimal number);

        return false;
    }
}
