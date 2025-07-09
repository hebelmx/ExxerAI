using System;
using System.Text.RegularExpressions;

class TestOCRPattern
{
    static void Main()
    {
        Console.WriteLine("Testing OCR Pattern Regex Fixes...\n");
        
        // Test the specific extraction patterns we fixed
        TestRegexPattern("Price: $1,500.00", "1,500.00");
        TestRegexPattern("Cost: €2,750.50", "2,750.50");
        TestRegexPattern("Amount: £999.99", "999.99");
        TestRegexPattern("Total: ¥10,000", "10,000");
        TestRegexPattern("Balance: -500.00", "500.00");
        TestRegexPattern("Loss: -1,250.75", "1,250.75");
        TestRegexPattern("Rate: 5.25%", "5.25");
        TestRegexPattern("Interest: 10%", "10");
        TestRegexPattern("Discount: 15.5%", "15.5");
        
        Console.WriteLine("\nTest completed!");
    }
    
    static void TestRegexPattern(string input, string expectedValue)
    {
        // Use the same patterns from OCRRegionPattern
        var patterns = new[]
        {
            @"^([\d,]+\.?\d*)",                    // Pattern 1: Numbers at start (simplified)
            @"\s+([\d,]+\.?\d*)",                  // Pattern 2: Numbers after whitespace
            @"([\d,]+\.?\d*)"                      // Pattern 3: Any numbers (fallback)
        };
        
        string? result = null;
        
        // Get text after the colon
        var colonIndex = input.IndexOf(':');
        if (colonIndex >= 0)
        {
            var afterColon = input.Substring(colonIndex + 1).Trim();
            
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(afterColon, pattern);
                if (match.Success)
                {
                    var value = match.Groups[1].Value;
                    
                    // Clean the value (remove currency symbols, negative signs, percentages)
                    value = Regex.Replace(value, @"[^\d,.]", ""); // Remove non-numeric chars except comma and dot
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        result = value;
                        break;
                    }
                }
            }
        }
        
        var status = result == expectedValue ? "✅ PASS" : "❌ FAIL";
        Console.WriteLine($"{status} Input: \"{input}\" | Expected: \"{expectedValue}\" | Got: \"{result}\"");
        
        if (result != expectedValue)
        {
            Console.WriteLine($"      Expected: '{expectedValue}' but got: '{result}'");
        }
    }
} 