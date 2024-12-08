using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using SimpleIT.Conversions.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIT.Conversions.Commands
{
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
            var resultString = string.Empty;
            switch(Source)
            {
                case NumberSystems.BINARY:
                    switch (Target)
                    {
                        case NumberSystems.BINARY:
                            throw new CommandException("No need to convert binary to binary.");
                        case NumberSystems.DECIMAL:
                            resultString = this.BinaryToDecimal(NumberValue).ToString();
                            break;
                        case NumberSystems.HEXADECIMAL:
                            break;
                        default:
                            break;
                    }
                    break;
                case NumberSystems.DECIMAL:
                    break;
                case NumberSystems.HEXADECIMAL:
                    break;
                default:
                   throw new NotImplementedException();
            }


            console.Output.WriteLine(string.Concat("Old Value: ", this.NumberValue));
            console.Output.WriteLine(string.Concat("New Value: ", resultString));

            return default(ValueTask);

        }

        private string DecimalToBinary(decimal number)
        {
            return number.ToString("B");
        }

        private string DecimalToHexadecimal(decimal number)
        {
            return number.ToString("X");
        }

        private decimal BinaryToDecimal(string binary)
        {
            var binaryParts = binary.Split(".");
            var integerPart = binaryParts[0];
            var fractionPart = binaryParts[1];

            var integerArray = integerPart.ToCharArray().Reverse();
            decimal integerValue = 0;
            for(int i = 0; i < integerArray.Count(); i++)
            {
                integerValue += integerArray.ElementAt(i) == '1' ? (decimal)Math.Pow(2, i) : 0;
            }

            var fractionArray = fractionPart.ToCharArray();
            for (int i = 1; i < fractionPart.Count()+1; i++)
            {
                var val = (decimal)Math.Pow(2, -i);
                integerValue += fractionArray.ElementAt(i-1) == '1' ? val : 0;
            }

            return integerValue;
        }
    }
}
