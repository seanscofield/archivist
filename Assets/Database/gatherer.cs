using System;
using System.Diagnostics;
using System.IO;

// Source code :: https://github.com/heltonx/CsharpPShell__PCinfoAPI?tab=readme-ov-file

class Program
{
    static void Main ( )
    {
        // Gather system health information (for example, memory and processor details):
        var memoryUsage = GetMemoryUsage ( ) ;
        var processorUsage = GetProcessorUsage ( ) ;

        // Prepare the data as a JSON-like string
        var systemHealthData = "{\n  \"MemoryUsage\": \"" + memoryUsage + "\",\n  \"ProcessorUsage\": \"" + processorUsage + "\"\n}";

        // Save the data to data.json file in the current directory
        var fileName = "data.json" ;
        var filePath = Path.Combine( Environment.CurrentDirectory , fileName ) ;
        SaveDataToJsonFile ( filePath, systemHealthData ) ;
    }

    static string GetMemoryUsage ( )
    {
        // Code to get memory usage details from the system
        PerformanceCounter ramCounter = new PerformanceCounter ( "Memory" , "Available MBytes" ) ;
        float availableMemoryInMB = ramCounter.NextValue ( ) ;
        return availableMemoryInMB + " MB" ;
    }

    static string GetProcessorUsage ( )
    {
        // Code to get processor usage details from the system
        PerformanceCounter cpuCounter = new PerformanceCounter( "Processor" , "% Processor Time" , "_Total" ) ;
        cpuCounter.NextValue() ;
        System.Threading.Thread.Sleep(1000) ; // Wait for a second to get a valid reading
        float processorUsage = cpuCounter.NextValue() ;
        return processorUsage + "%" ;
    }

    static void SaveDataToJsonFile (string filePath , string jsonData )
    {
        // Save the data to data.json file in the specified path
        File.WriteAllText ( filePath , jsonData ) ;
        Console.WriteLine( "System health data saved to " + filePath + "." ) ;
    }
}
