<Query Kind="Program">
  <NuGetReference>System.Memory</NuGetReference>
</Query>

void Main()
{
	var testString = string.Concat(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKL", 1000));
	
	var iterations = 100;
	var sliceLength = 20;
	
	Profile(nameof(SpanSlices), iterations, () => SpanSlices(testString, sliceLength));
	Profile(nameof(MemorySlices), iterations, () => MemorySlices(testString, sliceLength));
	Profile(nameof(StringSubstring), iterations, () => StringSubstring(testString, sliceLength));
}

// Define other methods and classes here
public static string[] SpanSlices(string numbers, int sliceLength)
{
	if (sliceLength > numbers.Length || sliceLength <= 0)
		throw new ArgumentException($"Length must be greater than 0 and less than the size of {nameof(numbers)}", nameof(sliceLength));

	var span = numbers.AsSpan();
	var slices = new string[(numbers.Length - sliceLength) + 1];

	for (int i = 0; i <= (numbers.Length - sliceLength); i++)
	{
		slices[i] = span.Slice(i, sliceLength).ToString();
	}

	return slices;
}

public static string[] MemorySlices(string numbers, int sliceLength)
{
	if (sliceLength > numbers.Length || sliceLength <= 0)
		throw new ArgumentException($"Length must be greater than 0 and less than the size of {nameof(numbers)}", nameof(sliceLength));

	var memory = numbers.AsMemory();
	var slices = new string[(numbers.Length - sliceLength) + 1];

	for (int i = 0; i <= (numbers.Length - sliceLength); i++)
	{
		slices[i] = memory.Slice(i, sliceLength).ToString();
	}

	return slices;
}

public static string[] StringSubstring(string numbers, int sliceLength)
{
	if (sliceLength > numbers.Length || sliceLength <= 0)
		throw new ArgumentException($"Length must be greater than 0 and less than the size of {nameof(numbers)}", nameof(sliceLength));

	var slices = new string[(numbers.Length - sliceLength) + 1];

	for (int i = 0; i <= (numbers.Length - sliceLength); i++)
	{
		slices[i] = numbers.Substring(i, sliceLength);
	}

	return slices;
}

// https://stackoverflow.com/a/1048708/1574232
static void Profile(string description, int iterations, Action func)
{
	//Run at highest priority to minimize fluctuations caused by other processes/threads
	Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
	Thread.CurrentThread.Priority = ThreadPriority.Highest;

	// warm up 
	func();

	var watch = new Stopwatch();

	// clean up
	GC.Collect();
	GC.WaitForPendingFinalizers();
	GC.Collect();
	GC.WaitForFullGCComplete();
	
	description.Dump();
	var BeforeMemory = GC.GetTotalMemory(true);
	$" GC.TotalMemory Before: {BeforeMemory:N}".Dump();
	watch.Start();
	for (int i = 0; i < iterations; i++)
	{
		func();
	}
	watch.Stop();
	var AfterMemory = GC.GetTotalMemory(false);
	$" GC.TotalMemory After: {AfterMemory:N}".Dump();
	$" Memory difference (After - Before): {(AfterMemory - BeforeMemory):N}".Dump();
	$" Time Elapsed {watch.Elapsed.TotalMilliseconds} ms".Dump();
	$" Average time {watch.Elapsed.TotalMilliseconds / iterations} ms".Dump();
}