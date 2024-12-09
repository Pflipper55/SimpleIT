using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using SimpleIT.Conversions.Model.Enums;

namespace SimpleIT.Conversions.Commands;

[Command("memoryunit-conversion", Description = "Convert from one unit to the other")]
public class MemoryUnitConversions : ICommand
{
    [CommandParameter(0, Description = "Value to be converted")]
    public required decimal NumberValue { get; init; }

    [CommandParameter(1, Description = "Source unit")]
    public required MemoryUnit Source { get; init; }

    [CommandParameter(2, Description = "Target unit")]
    public required MemoryUnit Target { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        if(Source == Target)
        {
            // ToDo: CommandException -> Logging
            throw new CommandException($"No need to convert {NumberValue} because target and source unit are the same");
        }

        var byteValue = this.ToBytes(NumberValue);
        var unitValue = this.ByteToUnit(byteValue);

        console.Output.WriteLine(string.Concat("Old Value: ", NumberValue));
        console.Output.WriteLine(string.Concat("New Value: ", unitValue));

        return default(ValueTask);
    }

    private decimal ByteToUnit(decimal bytesValue)
    {
        if(Target == MemoryUnit.BYTE)
        {
            return bytesValue;
        }
        else
        {
            return bytesValue / (ulong) Target;
        }
    }

    private decimal ToBytes(decimal originalValue)
    {
        if(Source == MemoryUnit.BITS)
        {
            return originalValue / 8;
        }
        else if (Source != MemoryUnit.BYTE) 
        {
            return originalValue * (ulong)Source;
        }
        else
        {
            return originalValue;
        }
    }
}
