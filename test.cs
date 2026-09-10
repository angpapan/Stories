using System;
using System.Text.Json;

class Program {
    static void Main() {
        var x = new { Id = "123", Text = "abc", Media = new string[0] };
        Console.WriteLine(JsonSerializer.Serialize(x));
    }
}
