using ExxerAI.Domain.Operations;
using System;

class Program
{
    static void Main()
    {
        // Test 1: Success with null value
        var result1 = Result<string>.Success(null!);
        Console.WriteLine($"Test 1 - Success with null:");
        Console.WriteLine($"  IsSuccessMayBeNull: {result1.IsSuccessMayBeNull}");
        Console.WriteLine($"  IsSuccess: {result1.IsSuccess}");
        Console.WriteLine($"  IsSuccessValueNull: {result1.IsSuccessValueNull}");
        Console.WriteLine($"  Value: {result1.Value}");
        Console.WriteLine();

        // Test 2: Success with non-null value
        var result2 = Result<string>.Success("hello");
        Console.WriteLine($"Test 2 - Success with value:");
        Console.WriteLine($"  IsSuccessMayBeNull: {result2.IsSuccessMayBeNull}");
        Console.WriteLine($"  IsSuccess: {result2.IsSuccess}");
        Console.WriteLine($"  IsSuccessValueNull: {result2.IsSuccessValueNull}");
        Console.WriteLine($"  Value: {result2.Value}");
        Console.WriteLine();

        // Test 3: OnSuccess behavior with null
        var actionExecuted = false;
        result1.OnSuccess(value => 
        {
            actionExecuted = true;
            Console.WriteLine($"  OnSuccess executed with value: {value}");
        });
        Console.WriteLine($"Test 3 - OnSuccess with null:");
        Console.WriteLine($"  Action executed: {actionExecuted}");
    }
}