using System;
using ExxerAI.Application.DTOs;
using ExxerAI.Application.Enums;
using ExxerAI.Application.Patterns;

class TestOCRPattern
{
    static void Main()
    {
        Console.WriteLine("Testing OCRRegionPattern fixes...\n");
        
        var context = new ExtractionContext();
        
        // Test currency symbols
        Console.WriteLine("=== Currency Symbol Tests ===");
        TestCurrencyExtraction("Price: $1,500.00", "Price:", "1,500.00", context);
        TestCurrencyExtraction("Cost: €2,750.50", "Cost:", "2,750.50", context);
        TestCurrencyExtraction("Amount: £999.99", "Amount:", "999.99", context);
        TestCurrencyExtraction("Total: ¥10,000", "Total:", "10,000", context);
        
        // Test negative numbers
        Console.WriteLine("\n=== Negative Number Tests ===");
        TestCurrencyExtraction("Balance: -500.00", "Balance:", "500.00", context);
        TestCurrencyExtraction("Loss: -1,250.75", "Loss:", "1,250.75", context);
        TestCurrencyExtraction("Deficit: -10,000", "Deficit:", "10,000", context);
        
        // Test percentage values
        Console.WriteLine("\n=== Percentage Tests ===");
        TestCurrencyExtraction("Rate: 5.25%", "Rate:", "5.25", context);
        TestCurrencyExtraction("Interest: 10%", "Interest:", "10", context);
        TestCurrencyExtraction("Discount: 15.5%", "Discount:", "15.5", context);
        
        Console.WriteLine("\nTest completed!");
    }
    
    static void TestCurrencyExtraction(string inputText, string referenceText, string expectedValue, ExtractionContext context)
    {
        var pattern = new OCRRegionPattern
        {
            ReferenceText = referenceText,
            SearchStrategy = SearchStrategy.NextToken
        };
        
        var result = pattern.ExtractValue(inputText, context);
        
        var status = result == expectedValue ? "✅ PASS" : "❌ FAIL";
        Console.WriteLine($"{status} Input: \"{inputText}\" | Expected: \"{expectedValue}\" | Got: \"{result}\"");
        
        if (result != expectedValue)
        {
            Console.WriteLine($"      Expected: '{expectedValue}' but got: '{result}'");
        }
    }
} 