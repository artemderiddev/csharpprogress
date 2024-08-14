﻿using BenchmarkDotNet.Running;
using FileServiceBenchmarksRunner;

BenchmarkRunner.Run<FileServiceBenchmarks>();

/* 2024-08-14 results
| Method               | SizeInMb | BufferSize | Mean       | Error    | StdDev    |
|--------------------- |--------- |----------- |-----------:|---------:|----------:|
| RunWithCurrentParams | 100      | 100        |   359.3 ms |  2.41 ms |   1.88 ms |
| RunWithCurrentParams | 100      | 256        |   339.7 ms |  4.45 ms |   3.95 ms |
| RunWithCurrentParams | 100      | 1024       |   335.1 ms |  6.30 ms |   7.49 ms |
| RunWithCurrentParams | 256      | 100        |   911.4 ms | 10.65 ms |   8.31 ms |
| RunWithCurrentParams | 256      | 256        |   880.7 ms | 10.95 ms |   8.55 ms |
| RunWithCurrentParams | 256      | 1024       |   892.7 ms | 17.59 ms |  28.41 ms |
| RunWithCurrentParams | 1000     | 100        | 3,654.3 ms | 71.32 ms |  92.73 ms |
| RunWithCurrentParams | 1000     | 256        | 3,572.2 ms | 71.18 ms | 126.52 ms |
| RunWithCurrentParams | 1000     | 1024       | 3,471.9 ms | 65.56 ms |  94.03 ms |
*/