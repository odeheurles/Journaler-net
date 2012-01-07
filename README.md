# High performance .NET Journaler

http://odeheurles.com/2011/12/high-performance-journaler/

## File format
|(long)sequence|(int)length|(binary)data|(long)sequence|(int)length|(binary)data|(long)sequence|(int)length|(binary)data|...

## Research
I do not have much experience with file IO so I have made some research before starting:
 - An interesting paper from Microsoft Research with benchmarks and sample source code: http://research.microsoft.com/apps/pubs/default.aspx?id=64538
 - SQLIO is a tool provided by Microsoft which can also be used to determine the I/O capacity of a given configuration: http://www.microsoft.com/download/en/details.aspx?id=20163

