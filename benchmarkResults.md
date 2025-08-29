WorkloadResult  15: 131072 op, 762679200.00 ns, 5.8188 us/op
// GC:  34 0 0 1313865792 131072
// Threading:  0 0 131072

// AfterAll
// Benchmark Process 22708 has exited with code 0.

Mean = 5.852 us, StdErr = 0.008 us (0.13%), N = 15, StdDev = 0.029 us
Min = 5.802 us, Q1 = 5.827 us, Median = 5.854 us, Q3 = 5.877 us, Max = 5.899 us
IQR = 0.050 us, LowerFence = 5.752 us, UpperFence = 5.952 us
ConfidenceInterval = [5.821 us; 5.883 us] (CI 99.9%), Margin = 0.031 us (0.53% of Mean)
Skewness = -0.07, Kurtosis = 1.58, MValue = 2

// ** Remained 7 (87.5%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 2m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.UnmanagedArray_Byte: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1768 1296 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.UnmanagedArray_Byte --job ".NET 9.0" --benchmarkId 1 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256        
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 164700.00 ns, 164.7000 us/op
WorkloadJitting  1: 1 op, 1605400.00 ns, 1.6054 ms/op

OverheadJitting  2: 16 op, 172700.00 ns, 10.7937 us/op
WorkloadJitting  2: 16 op, 699800.00 ns, 43.7375 us/op

WorkloadPilot    1: 16 op, 278000.00 ns, 17.3750 us/op
WorkloadPilot    2: 32 op, 432400.00 ns, 13.5125 us/op
WorkloadPilot    3: 64 op, 834100.00 ns, 13.0328 us/op
WorkloadPilot    4: 128 op, 1704500.00 ns, 13.3164 us/op
WorkloadPilot    5: 256 op, 3170300.00 ns, 12.3840 us/op
WorkloadPilot    6: 512 op, 6391700.00 ns, 12.4838 us/op
WorkloadPilot    7: 1024 op, 12787400.00 ns, 12.4877 us/op
WorkloadPilot    8: 2048 op, 25545200.00 ns, 12.4732 us/op
WorkloadPilot    9: 4096 op, 50231700.00 ns, 12.2636 us/op
WorkloadPilot   10: 8192 op, 100890000.00 ns, 12.3157 us/op
WorkloadPilot   11: 16384 op, 153055600.00 ns, 9.3418 us/op
WorkloadPilot   12: 32768 op, 276040300.00 ns, 8.4241 us/op
WorkloadPilot   13: 65536 op, 553387800.00 ns, 8.4440 us/op

OverheadWarmup   1: 65536 op, 131500.00 ns, 2.0065 ns/op
OverheadWarmup   2: 65536 op, 130400.00 ns, 1.9897 ns/op
OverheadWarmup   3: 65536 op, 132000.00 ns, 2.0142 ns/op
OverheadWarmup   4: 65536 op, 133300.00 ns, 2.0340 ns/op
OverheadWarmup   5: 65536 op, 130300.00 ns, 1.9882 ns/op
OverheadWarmup   6: 65536 op, 130100.00 ns, 1.9852 ns/op
OverheadWarmup   7: 65536 op, 130000.00 ns, 1.9836 ns/op
OverheadWarmup   8: 65536 op, 130100.00 ns, 1.9852 ns/op
OverheadWarmup   9: 65536 op, 130200.00 ns, 1.9867 ns/op
OverheadWarmup  10: 65536 op, 130100.00 ns, 1.9852 ns/op

OverheadActual   1: 65536 op, 130500.00 ns, 1.9913 ns/op
OverheadActual   2: 65536 op, 130400.00 ns, 1.9897 ns/op
OverheadActual   3: 65536 op, 130100.00 ns, 1.9852 ns/op
OverheadActual   4: 65536 op, 131200.00 ns, 2.0020 ns/op
OverheadActual   5: 65536 op, 130400.00 ns, 1.9897 ns/op
OverheadActual   6: 65536 op, 130300.00 ns, 1.9882 ns/op
OverheadActual   7: 65536 op, 130100.00 ns, 1.9852 ns/op
OverheadActual   8: 65536 op, 130600.00 ns, 1.9928 ns/op
OverheadActual   9: 65536 op, 130400.00 ns, 1.9897 ns/op
OverheadActual  10: 65536 op, 130400.00 ns, 1.9897 ns/op
OverheadActual  11: 65536 op, 130500.00 ns, 1.9913 ns/op
OverheadActual  12: 65536 op, 130400.00 ns, 1.9897 ns/op
OverheadActual  13: 65536 op, 150300.00 ns, 2.2934 ns/op
OverheadActual  14: 65536 op, 130500.00 ns, 1.9913 ns/op
OverheadActual  15: 65536 op, 130300.00 ns, 1.9882 ns/op

WorkloadWarmup   1: 65536 op, 555608200.00 ns, 8.4779 us/op
WorkloadWarmup   2: 65536 op, 555785600.00 ns, 8.4806 us/op
WorkloadWarmup   3: 65536 op, 558880300.00 ns, 8.5278 us/op
WorkloadWarmup   4: 65536 op, 398358600.00 ns, 6.0785 us/op
WorkloadWarmup   5: 65536 op, 394858300.00 ns, 6.0251 us/op
WorkloadWarmup   6: 65536 op, 393369900.00 ns, 6.0023 us/op
WorkloadWarmup   7: 65536 op, 387044900.00 ns, 5.9058 us/op
WorkloadWarmup   8: 65536 op, 392307700.00 ns, 5.9861 us/op
WorkloadWarmup   9: 65536 op, 395475400.00 ns, 6.0345 us/op
WorkloadWarmup  10: 65536 op, 393295700.00 ns, 6.0012 us/op

// BeforeActualRun
WorkloadActual   1: 65536 op, 402239100.00 ns, 6.1377 us/op
WorkloadActual   2: 65536 op, 394488900.00 ns, 6.0194 us/op
WorkloadActual   3: 65536 op, 392069600.00 ns, 5.9825 us/op
WorkloadActual   4: 65536 op, 395326200.00 ns, 6.0322 us/op
WorkloadActual   5: 65536 op, 392185800.00 ns, 5.9843 us/op
WorkloadActual   6: 65536 op, 392027000.00 ns, 5.9819 us/op
WorkloadActual   7: 65536 op, 388346800.00 ns, 5.9257 us/op
WorkloadActual   8: 65536 op, 395085800.00 ns, 6.0285 us/op
WorkloadActual   9: 65536 op, 393933100.00 ns, 6.0109 us/op
WorkloadActual  10: 65536 op, 396937700.00 ns, 6.0568 us/op
WorkloadActual  11: 65536 op, 394403900.00 ns, 6.0181 us/op
WorkloadActual  12: 65536 op, 391791800.00 ns, 5.9783 us/op
WorkloadActual  13: 65536 op, 397690900.00 ns, 6.0683 us/op
WorkloadActual  14: 65536 op, 393525600.00 ns, 6.0047 us/op
WorkloadActual  15: 65536 op, 397434100.00 ns, 6.0644 us/op

// AfterActualRun
WorkloadResult   1: 65536 op, 394358500.00 ns, 6.0174 us/op
WorkloadResult   2: 65536 op, 391939200.00 ns, 5.9805 us/op
WorkloadResult   3: 65536 op, 395195800.00 ns, 6.0302 us/op
WorkloadResult   4: 65536 op, 392055400.00 ns, 5.9823 us/op
WorkloadResult   5: 65536 op, 391896600.00 ns, 5.9799 us/op
WorkloadResult   6: 65536 op, 388216400.00 ns, 5.9237 us/op
WorkloadResult   7: 65536 op, 394955400.00 ns, 6.0265 us/op
WorkloadResult   8: 65536 op, 393802700.00 ns, 6.0090 us/op
WorkloadResult   9: 65536 op, 396807300.00 ns, 6.0548 us/op
WorkloadResult  10: 65536 op, 394273500.00 ns, 6.0161 us/op
WorkloadResult  11: 65536 op, 391661400.00 ns, 5.9763 us/op
WorkloadResult  12: 65536 op, 397560500.00 ns, 6.0663 us/op
WorkloadResult  13: 65536 op, 393395200.00 ns, 6.0027 us/op
WorkloadResult  14: 65536 op, 397303700.00 ns, 6.0624 us/op
// GC:  0 0 0 400 65536
// Threading:  0 0 65536

// AfterAll
// Benchmark Process 6288 has exited with code 0.

Mean = 6.009 us, StdErr = 0.010 us (0.17%), N = 14, StdDev = 0.039 us
Min = 5.924 us, Q1 = 5.981 us, Median = 6.013 us, Q3 = 6.029 us, Max = 6.066 us
IQR = 0.048 us, LowerFence = 5.908 us, UpperFence = 6.102 us
ConfidenceInterval = [5.965 us; 6.053 us] (CI 99.9%), Margin = 0.044 us (0.73% of Mean)
Skewness = -0.33, Kurtosis = 2.44, MValue = 2

// ** Remained 6 (75.0%) benchmark(s) to run. Estimated finish 2025-08-29 17:49 (0h 1m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.ManagedArray_Int: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1876 1800 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.ManagedArray_Int --job ".NET 9.0" --benchmarkId 2 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256        
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 143300.00 ns, 143.3000 us/op
WorkloadJitting  1: 1 op, 392300.00 ns, 392.3000 us/op

OverheadJitting  2: 16 op, 151600.00 ns, 9.4750 us/op
WorkloadJitting  2: 16 op, 407600.00 ns, 25.4750 us/op

WorkloadPilot    1: 16 op, 238100.00 ns, 14.8812 us/op
WorkloadPilot    2: 32 op, 354300.00 ns, 11.0719 us/op
WorkloadPilot    3: 64 op, 788300.00 ns, 12.3172 us/op
WorkloadPilot    4: 128 op, 1239200.00 ns, 9.6813 us/op
WorkloadPilot    5: 256 op, 2294900.00 ns, 8.9645 us/op
WorkloadPilot    6: 512 op, 6391400.00 ns, 12.4832 us/op
WorkloadPilot    7: 1024 op, 10048700.00 ns, 9.8132 us/op
WorkloadPilot    8: 2048 op, 15679200.00 ns, 7.6559 us/op
WorkloadPilot    9: 4096 op, 35911400.00 ns, 8.7674 us/op
WorkloadPilot   10: 8192 op, 66833300.00 ns, 8.1584 us/op
WorkloadPilot   11: 16384 op, 116454000.00 ns, 7.1078 us/op
WorkloadPilot   12: 32768 op, 184832700.00 ns, 5.6406 us/op
WorkloadPilot   13: 65536 op, 369610100.00 ns, 5.6398 us/op
WorkloadPilot   14: 131072 op, 738138400.00 ns, 5.6315 us/op

OverheadWarmup   1: 131072 op, 270700.00 ns, 2.0653 ns/op
OverheadWarmup   2: 131072 op, 272400.00 ns, 2.0782 ns/op
OverheadWarmup   3: 131072 op, 271500.00 ns, 2.0714 ns/op
OverheadWarmup   4: 131072 op, 273000.00 ns, 2.0828 ns/op
OverheadWarmup   5: 131072 op, 272400.00 ns, 2.0782 ns/op

OverheadActual   1: 131072 op, 271600.00 ns, 2.0721 ns/op
OverheadActual   2: 131072 op, 272200.00 ns, 2.0767 ns/op
OverheadActual   3: 131072 op, 274900.00 ns, 2.0973 ns/op
OverheadActual   4: 131072 op, 271300.00 ns, 2.0699 ns/op
OverheadActual   5: 131072 op, 274600.00 ns, 2.0950 ns/op
OverheadActual   6: 131072 op, 272300.00 ns, 2.0775 ns/op
OverheadActual   7: 131072 op, 270400.00 ns, 2.0630 ns/op
OverheadActual   8: 131072 op, 275100.00 ns, 2.0988 ns/op
OverheadActual   9: 131072 op, 299000.00 ns, 2.2812 ns/op
OverheadActual  10: 131072 op, 291700.00 ns, 2.2255 ns/op
OverheadActual  11: 131072 op, 272000.00 ns, 2.0752 ns/op
OverheadActual  12: 131072 op, 272700.00 ns, 2.0805 ns/op
OverheadActual  13: 131072 op, 274800.00 ns, 2.0966 ns/op
OverheadActual  14: 131072 op, 272500.00 ns, 2.0790 ns/op
OverheadActual  15: 131072 op, 272500.00 ns, 2.0790 ns/op

WorkloadWarmup   1: 131072 op, 741441000.00 ns, 5.6567 us/op
WorkloadWarmup   2: 131072 op, 739716200.00 ns, 5.6436 us/op
WorkloadWarmup   3: 131072 op, 748981700.00 ns, 5.7143 us/op
WorkloadWarmup   4: 131072 op, 742766700.00 ns, 5.6669 us/op
WorkloadWarmup   5: 131072 op, 739461300.00 ns, 5.6416 us/op
WorkloadWarmup   6: 131072 op, 740956400.00 ns, 5.6530 us/op
WorkloadWarmup   7: 131072 op, 741817700.00 ns, 5.6596 us/op
WorkloadWarmup   8: 131072 op, 737366900.00 ns, 5.6257 us/op

// BeforeActualRun
WorkloadActual   1: 131072 op, 741248700.00 ns, 5.6553 us/op
WorkloadActual   2: 131072 op, 739065300.00 ns, 5.6386 us/op
WorkloadActual   3: 131072 op, 742721400.00 ns, 5.6665 us/op
WorkloadActual   4: 131072 op, 739825800.00 ns, 5.6444 us/op
WorkloadActual   5: 131072 op, 734140500.00 ns, 5.6010 us/op
WorkloadActual   6: 131072 op, 740204900.00 ns, 5.6473 us/op
WorkloadActual   7: 131072 op, 748324100.00 ns, 5.7093 us/op
WorkloadActual   8: 131072 op, 743128800.00 ns, 5.6696 us/op
WorkloadActual   9: 131072 op, 743940500.00 ns, 5.6758 us/op
WorkloadActual  10: 131072 op, 739901400.00 ns, 5.6450 us/op
WorkloadActual  11: 131072 op, 740096100.00 ns, 5.6465 us/op
WorkloadActual  12: 131072 op, 741310700.00 ns, 5.6558 us/op
WorkloadActual  13: 131072 op, 738563500.00 ns, 5.6348 us/op
WorkloadActual  14: 131072 op, 745470900.00 ns, 5.6875 us/op
WorkloadActual  15: 131072 op, 742038100.00 ns, 5.6613 us/op

// AfterActualRun
WorkloadResult   1: 131072 op, 740976200.00 ns, 5.6532 us/op
WorkloadResult   2: 131072 op, 738792800.00 ns, 5.6365 us/op
WorkloadResult   3: 131072 op, 742448900.00 ns, 5.6644 us/op
WorkloadResult   4: 131072 op, 739553300.00 ns, 5.6423 us/op
WorkloadResult   5: 131072 op, 733868000.00 ns, 5.5990 us/op
WorkloadResult   6: 131072 op, 739932400.00 ns, 5.6452 us/op
WorkloadResult   7: 131072 op, 742856300.00 ns, 5.6675 us/op
WorkloadResult   8: 131072 op, 743668000.00 ns, 5.6737 us/op
WorkloadResult   9: 131072 op, 739628900.00 ns, 5.6429 us/op
WorkloadResult  10: 131072 op, 739823600.00 ns, 5.6444 us/op
WorkloadResult  11: 131072 op, 741038200.00 ns, 5.6537 us/op
WorkloadResult  12: 131072 op, 738291000.00 ns, 5.6327 us/op
WorkloadResult  13: 131072 op, 745198400.00 ns, 5.6854 us/op
WorkloadResult  14: 131072 op, 741765600.00 ns, 5.6592 us/op
// GC:  138 0 0 5246025792 131072
// Threading:  0 0 131072

// AfterAll
// Benchmark Process 21160 has exited with code 0.

Mean = 5.650 us, StdErr = 0.006 us (0.10%), N = 14, StdDev = 0.021 us
Min = 5.599 us, Q1 = 5.642 us, Median = 5.649 us, Q3 = 5.663 us, Max = 5.685 us
IQR = 0.021 us, LowerFence = 5.612 us, UpperFence = 5.694 us
ConfidenceInterval = [5.626 us; 5.674 us] (CI 99.9%), Margin = 0.024 us (0.42% of Mean)
Skewness = -0.58, Kurtosis = 3.34, MValue = 2

// ** Remained 5 (62.5%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 1m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.UnmanagedArray_Int: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1644 1832 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.UnmanagedArray_Int --job ".NET 9.0" --benchmarkId 3 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 136700.00 ns, 136.7000 us/op
WorkloadJitting  1: 1 op, 1747200.00 ns, 1.7472 ms/op

OverheadJitting  2: 16 op, 137300.00 ns, 8.5813 us/op
WorkloadJitting  2: 16 op, 417700.00 ns, 26.1062 us/op

WorkloadPilot    1: 16 op, 184700.00 ns, 11.5437 us/op
WorkloadPilot    2: 32 op, 341300.00 ns, 10.6656 us/op
WorkloadPilot    3: 64 op, 644300.00 ns, 10.0672 us/op
WorkloadPilot    4: 128 op, 1309100.00 ns, 10.2273 us/op
WorkloadPilot    5: 256 op, 2367400.00 ns, 9.2477 us/op
WorkloadPilot    6: 512 op, 4891200.00 ns, 9.5531 us/op
WorkloadPilot    7: 1024 op, 9736000.00 ns, 9.5078 us/op
WorkloadPilot    8: 2048 op, 18987000.00 ns, 9.2710 us/op
WorkloadPilot    9: 4096 op, 40043900.00 ns, 9.7763 us/op
WorkloadPilot   10: 8192 op, 78144800.00 ns, 9.5392 us/op
WorkloadPilot   11: 16384 op, 117823900.00 ns, 7.1914 us/op
WorkloadPilot   12: 32768 op, 146349700.00 ns, 4.4662 us/op
WorkloadPilot   13: 65536 op, 297508300.00 ns, 4.5396 us/op
WorkloadPilot   14: 131072 op, 588330800.00 ns, 4.4886 us/op

OverheadWarmup   1: 131072 op, 260300.00 ns, 1.9859 ns/op
OverheadWarmup   2: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadWarmup   3: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadWarmup   4: 131072 op, 259500.00 ns, 1.9798 ns/op
OverheadWarmup   5: 131072 op, 259600.00 ns, 1.9806 ns/op
OverheadWarmup   6: 131072 op, 259300.00 ns, 1.9783 ns/op
OverheadWarmup   7: 131072 op, 259500.00 ns, 1.9798 ns/op
OverheadWarmup   8: 131072 op, 259400.00 ns, 1.9791 ns/op

OverheadActual   1: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadActual   2: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadActual   3: 131072 op, 260400.00 ns, 1.9867 ns/op
OverheadActual   4: 131072 op, 259600.00 ns, 1.9806 ns/op
OverheadActual   5: 131072 op, 260000.00 ns, 1.9836 ns/op
OverheadActual   6: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadActual   7: 131072 op, 259800.00 ns, 1.9821 ns/op
OverheadActual   8: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadActual   9: 131072 op, 260300.00 ns, 1.9859 ns/op
OverheadActual  10: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadActual  11: 131072 op, 259600.00 ns, 1.9806 ns/op
OverheadActual  12: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadActual  13: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadActual  14: 131072 op, 259600.00 ns, 1.9806 ns/op
OverheadActual  15: 131072 op, 259900.00 ns, 1.9829 ns/op

WorkloadWarmup   1: 131072 op, 581975100.00 ns, 4.4401 us/op
WorkloadWarmup   2: 131072 op, 580882200.00 ns, 4.4318 us/op
WorkloadWarmup   3: 131072 op, 580443700.00 ns, 4.4284 us/op
WorkloadWarmup   4: 131072 op, 580003100.00 ns, 4.4251 us/op
WorkloadWarmup   5: 131072 op, 1139415900.00 ns, 8.6931 us/op
WorkloadWarmup   6: 131072 op, 1143438400.00 ns, 8.7237 us/op
WorkloadWarmup   7: 131072 op, 1138494700.00 ns, 8.6860 us/op
WorkloadWarmup   8: 131072 op, 1141935500.00 ns, 8.7123 us/op
WorkloadWarmup   9: 131072 op, 1140610000.00 ns, 8.7022 us/op

// BeforeActualRun
WorkloadActual   1: 131072 op, 1140602300.00 ns, 8.7021 us/op
WorkloadActual   2: 131072 op, 1138988800.00 ns, 8.6898 us/op
WorkloadActual   3: 131072 op, 1144033000.00 ns, 8.7283 us/op
WorkloadActual   4: 131072 op, 1143850700.00 ns, 8.7269 us/op
WorkloadActual   5: 131072 op, 1141551800.00 ns, 8.7093 us/op
WorkloadActual   6: 131072 op, 1140275400.00 ns, 8.6996 us/op
WorkloadActual   7: 131072 op, 1140047700.00 ns, 8.6979 us/op
WorkloadActual   8: 131072 op, 1139731500.00 ns, 8.6955 us/op
WorkloadActual   9: 131072 op, 1139091100.00 ns, 8.6906 us/op
WorkloadActual  10: 131072 op, 1138630500.00 ns, 8.6871 us/op
WorkloadActual  11: 131072 op, 1146893200.00 ns, 8.7501 us/op
WorkloadActual  12: 131072 op, 1141212800.00 ns, 8.7068 us/op
WorkloadActual  13: 131072 op, 1137909700.00 ns, 8.6816 us/op
WorkloadActual  14: 131072 op, 1142351500.00 ns, 8.7155 us/op
WorkloadActual  15: 131072 op, 1145327400.00 ns, 8.7382 us/op

// AfterActualRun
WorkloadResult   1: 131072 op, 1140342500.00 ns, 8.7001 us/op
WorkloadResult   2: 131072 op, 1138729000.00 ns, 8.6878 us/op
WorkloadResult   3: 131072 op, 1143773200.00 ns, 8.7263 us/op
WorkloadResult   4: 131072 op, 1143590900.00 ns, 8.7249 us/op
WorkloadResult   5: 131072 op, 1141292000.00 ns, 8.7074 us/op
WorkloadResult   6: 131072 op, 1140015600.00 ns, 8.6976 us/op
WorkloadResult   7: 131072 op, 1139787900.00 ns, 8.6959 us/op
WorkloadResult   8: 131072 op, 1139471700.00 ns, 8.6935 us/op
WorkloadResult   9: 131072 op, 1138831300.00 ns, 8.6886 us/op
WorkloadResult  10: 131072 op, 1138370700.00 ns, 8.6851 us/op
WorkloadResult  11: 131072 op, 1146633400.00 ns, 8.7481 us/op
WorkloadResult  12: 131072 op, 1140953000.00 ns, 8.7048 us/op
WorkloadResult  13: 131072 op, 1137649900.00 ns, 8.6796 us/op
WorkloadResult  14: 131072 op, 1142091700.00 ns, 8.7135 us/op
WorkloadResult  15: 131072 op, 1145067600.00 ns, 8.7362 us/op
// GC:  0 0 0 400 131072
// Threading:  0 0 131072

// AfterAll
// Benchmark Process 8580 has exited with code 0.

Mean = 8.706 us, StdErr = 0.005 us (0.06%), N = 15, StdDev = 0.020 us
Min = 8.680 us, Q1 = 8.691 us, Median = 8.700 us, Q3 = 8.719 us, Max = 8.748 us
IQR = 0.028 us, LowerFence = 8.649 us, UpperFence = 8.761 us
ConfidenceInterval = [8.684 us; 8.727 us] (CI 99.9%), Margin = 0.021 us (0.25% of Mean)
Skewness = 0.62, Kurtosis = 2.14, MValue = 2

// ** Remained 4 (50.0%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 1m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.ManagedArray_Double: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1688 1572 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.ManagedArray_Double --job ".NET 9.0" --benchmarkId 4 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256        
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 149400.00 ns, 149.4000 us/op
WorkloadJitting  1: 1 op, 406400.00 ns, 406.4000 us/op

OverheadJitting  2: 16 op, 144500.00 ns, 9.0312 us/op
WorkloadJitting  2: 16 op, 701800.00 ns, 43.8625 us/op

WorkloadPilot    1: 16 op, 460100.00 ns, 28.7563 us/op
WorkloadPilot    2: 32 op, 652600.00 ns, 20.3938 us/op
WorkloadPilot    3: 64 op, 1049600.00 ns, 16.4000 us/op
WorkloadPilot    4: 128 op, 2919600.00 ns, 22.8094 us/op
WorkloadPilot    5: 256 op, 4078300.00 ns, 15.9309 us/op
WorkloadPilot    6: 512 op, 9797000.00 ns, 19.1348 us/op
WorkloadPilot    7: 1024 op, 11996200.00 ns, 11.7150 us/op
WorkloadPilot    8: 2048 op, 32132000.00 ns, 15.6895 us/op
WorkloadPilot    9: 4096 op, 49771100.00 ns, 12.1511 us/op
WorkloadPilot   10: 8192 op, 77692900.00 ns, 9.4840 us/op
WorkloadPilot   11: 16384 op, 156389300.00 ns, 9.5452 us/op
WorkloadPilot   12: 32768 op, 292448600.00 ns, 8.9248 us/op
WorkloadPilot   13: 65536 op, 569696300.00 ns, 8.6929 us/op

OverheadWarmup   1: 65536 op, 151600.00 ns, 2.3132 ns/op
OverheadWarmup   2: 65536 op, 156600.00 ns, 2.3895 ns/op
OverheadWarmup   3: 65536 op, 162100.00 ns, 2.4734 ns/op
OverheadWarmup   4: 65536 op, 161300.00 ns, 2.4612 ns/op
OverheadWarmup   5: 65536 op, 234900.00 ns, 3.5843 ns/op
OverheadWarmup   6: 65536 op, 152800.00 ns, 2.3315 ns/op

OverheadActual   1: 65536 op, 144800.00 ns, 2.2095 ns/op
OverheadActual   2: 65536 op, 161500.00 ns, 2.4643 ns/op
OverheadActual   3: 65536 op, 152900.00 ns, 2.3331 ns/op
OverheadActual   4: 65536 op, 140600.00 ns, 2.1454 ns/op
OverheadActual   5: 65536 op, 162700.00 ns, 2.4826 ns/op
OverheadActual   6: 65536 op, 156300.00 ns, 2.3849 ns/op
OverheadActual   7: 65536 op, 186800.00 ns, 2.8503 ns/op
OverheadActual   8: 65536 op, 157300.00 ns, 2.4002 ns/op
OverheadActual   9: 65536 op, 188300.00 ns, 2.8732 ns/op
OverheadActual  10: 65536 op, 144100.00 ns, 2.1988 ns/op
OverheadActual  11: 65536 op, 144300.00 ns, 2.2018 ns/op
OverheadActual  12: 65536 op, 191900.00 ns, 2.9282 ns/op
OverheadActual  13: 65536 op, 218100.00 ns, 3.3279 ns/op
OverheadActual  14: 65536 op, 189500.00 ns, 2.8915 ns/op
OverheadActual  15: 65536 op, 195800.00 ns, 2.9877 ns/op
OverheadActual  16: 65536 op, 141200.00 ns, 2.1545 ns/op
OverheadActual  17: 65536 op, 189100.00 ns, 2.8854 ns/op
OverheadActual  18: 65536 op, 191200.00 ns, 2.9175 ns/op
OverheadActual  19: 65536 op, 149200.00 ns, 2.2766 ns/op
OverheadActual  20: 65536 op, 160600.00 ns, 2.4506 ns/op

WorkloadWarmup   1: 65536 op, 613398200.00 ns, 9.3597 us/op
WorkloadWarmup   2: 65536 op, 613982200.00 ns, 9.3686 us/op
WorkloadWarmup   3: 65536 op, 610595500.00 ns, 9.3169 us/op
WorkloadWarmup   4: 65536 op, 618502200.00 ns, 9.4376 us/op
WorkloadWarmup   5: 65536 op, 621779100.00 ns, 9.4876 us/op
WorkloadWarmup   6: 65536 op, 617816000.00 ns, 9.4271 us/op

// BeforeActualRun
WorkloadActual   1: 65536 op, 618332300.00 ns, 9.4350 us/op
WorkloadActual   2: 65536 op, 611831100.00 ns, 9.3358 us/op
WorkloadActual   3: 65536 op, 615197400.00 ns, 9.3872 us/op
WorkloadActual   4: 65536 op, 621291800.00 ns, 9.4802 us/op
WorkloadActual   5: 65536 op, 619409400.00 ns, 9.4514 us/op
WorkloadActual   6: 65536 op, 605930300.00 ns, 9.2458 us/op
WorkloadActual   7: 65536 op, 616834300.00 ns, 9.4121 us/op
WorkloadActual   8: 65536 op, 625720200.00 ns, 9.5477 us/op
WorkloadActual   9: 65536 op, 612543700.00 ns, 9.3467 us/op
WorkloadActual  10: 65536 op, 618546900.00 ns, 9.4383 us/op
WorkloadActual  11: 65536 op, 610009800.00 ns, 9.3080 us/op
WorkloadActual  12: 65536 op, 615532300.00 ns, 9.3923 us/op
WorkloadActual  13: 65536 op, 612616200.00 ns, 9.3478 us/op
WorkloadActual  14: 65536 op, 617747700.00 ns, 9.4261 us/op
WorkloadActual  15: 65536 op, 620057900.00 ns, 9.4613 us/op

// AfterActualRun
WorkloadResult   1: 65536 op, 618171250.00 ns, 9.4325 us/op
WorkloadResult   2: 65536 op, 611670050.00 ns, 9.3333 us/op
WorkloadResult   3: 65536 op, 615036350.00 ns, 9.3847 us/op
WorkloadResult   4: 65536 op, 621130750.00 ns, 9.4777 us/op
WorkloadResult   5: 65536 op, 619248350.00 ns, 9.4490 us/op
WorkloadResult   6: 65536 op, 605769250.00 ns, 9.2433 us/op
WorkloadResult   7: 65536 op, 616673250.00 ns, 9.4097 us/op
WorkloadResult   8: 65536 op, 625559150.00 ns, 9.5453 us/op
WorkloadResult   9: 65536 op, 612382650.00 ns, 9.3442 us/op
WorkloadResult  10: 65536 op, 618385850.00 ns, 9.4358 us/op
WorkloadResult  11: 65536 op, 609848750.00 ns, 9.3056 us/op
WorkloadResult  12: 65536 op, 615371250.00 ns, 9.3898 us/op
WorkloadResult  13: 65536 op, 612455150.00 ns, 9.3453 us/op
WorkloadResult  14: 65536 op, 617586650.00 ns, 9.4236 us/op
WorkloadResult  15: 65536 op, 619896850.00 ns, 9.4589 us/op
// GC:  151 0 0 5244452928 65536
// Threading:  0 0 65536

// AfterAll
// Benchmark Process 5316 has exited with code 0.

Mean = 9.399 us, StdErr = 0.020 us (0.21%), N = 15, StdDev = 0.076 us
Min = 9.243 us, Q1 = 9.345 us, Median = 9.410 us, Q3 = 9.442 us, Max = 9.545 us
IQR = 0.098 us, LowerFence = 9.198 us, UpperFence = 9.589 us
ConfidenceInterval = [9.318 us; 9.480 us] (CI 99.9%), Margin = 0.081 us (0.86% of Mean)
Skewness = -0.16, Kurtosis = 2.46, MValue = 2

// ** Remained 3 (37.5%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 0m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.UnmanagedArray_Double: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1400 1664 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.UnmanagedArray_Double --job ".NET 9.0" --benchmarkId 5 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256        
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 140600.00 ns, 140.6000 us/op
WorkloadJitting  1: 1 op, 1906900.00 ns, 1.9069 ms/op

OverheadJitting  2: 16 op, 142900.00 ns, 8.9313 us/op
WorkloadJitting  2: 16 op, 419100.00 ns, 26.1938 us/op

WorkloadPilot    1: 16 op, 194300.00 ns, 12.1438 us/op
WorkloadPilot    2: 32 op, 463100.00 ns, 14.4719 us/op
WorkloadPilot    3: 64 op, 856200.00 ns, 13.3781 us/op
WorkloadPilot    4: 128 op, 1687900.00 ns, 13.1867 us/op
WorkloadPilot    5: 256 op, 3318600.00 ns, 12.9633 us/op
WorkloadPilot    6: 512 op, 6495600.00 ns, 12.6867 us/op
WorkloadPilot    7: 1024 op, 12011500.00 ns, 11.7300 us/op
WorkloadPilot    8: 2048 op, 20412400.00 ns, 9.9670 us/op
WorkloadPilot    9: 4096 op, 40279700.00 ns, 9.8339 us/op
WorkloadPilot   10: 8192 op, 86289700.00 ns, 10.5334 us/op
WorkloadPilot   11: 16384 op, 120189100.00 ns, 7.3358 us/op
WorkloadPilot   12: 32768 op, 183162100.00 ns, 5.5897 us/op
WorkloadPilot   13: 65536 op, 386039300.00 ns, 5.8905 us/op
WorkloadPilot   14: 131072 op, 731043700.00 ns, 5.5774 us/op

OverheadWarmup   1: 131072 op, 262100.00 ns, 1.9997 ns/op
OverheadWarmup   2: 131072 op, 261700.00 ns, 1.9966 ns/op
OverheadWarmup   3: 131072 op, 261400.00 ns, 1.9943 ns/op
OverheadWarmup   4: 131072 op, 261000.00 ns, 1.9913 ns/op
OverheadWarmup   5: 131072 op, 261400.00 ns, 1.9943 ns/op
OverheadWarmup   6: 131072 op, 261100.00 ns, 1.9920 ns/op
OverheadWarmup   7: 131072 op, 261000.00 ns, 1.9913 ns/op
OverheadWarmup   8: 131072 op, 261100.00 ns, 1.9920 ns/op
OverheadWarmup   9: 131072 op, 261600.00 ns, 1.9958 ns/op
OverheadWarmup  10: 131072 op, 261400.00 ns, 1.9943 ns/op

OverheadActual   1: 131072 op, 261600.00 ns, 1.9958 ns/op
OverheadActual   2: 131072 op, 261400.00 ns, 1.9943 ns/op
OverheadActual   3: 131072 op, 261700.00 ns, 1.9966 ns/op
OverheadActual   4: 131072 op, 262900.00 ns, 2.0058 ns/op
OverheadActual   5: 131072 op, 262300.00 ns, 2.0012 ns/op
OverheadActual   6: 131072 op, 261900.00 ns, 1.9981 ns/op
OverheadActual   7: 131072 op, 261000.00 ns, 1.9913 ns/op
OverheadActual   8: 131072 op, 260900.00 ns, 1.9905 ns/op
OverheadActual   9: 131072 op, 263100.00 ns, 2.0073 ns/op
OverheadActual  10: 131072 op, 261100.00 ns, 1.9920 ns/op
OverheadActual  11: 131072 op, 261600.00 ns, 1.9958 ns/op
OverheadActual  12: 131072 op, 261200.00 ns, 1.9928 ns/op
OverheadActual  13: 131072 op, 262100.00 ns, 1.9997 ns/op
OverheadActual  14: 131072 op, 261700.00 ns, 1.9966 ns/op
OverheadActual  15: 131072 op, 262800.00 ns, 2.0050 ns/op

WorkloadWarmup   1: 131072 op, 771369600.00 ns, 5.8851 us/op
WorkloadWarmup   2: 131072 op, 757568300.00 ns, 5.7798 us/op
WorkloadWarmup   3: 131072 op, 754439100.00 ns, 5.7559 us/op
WorkloadWarmup   4: 131072 op, 727974000.00 ns, 5.5540 us/op
WorkloadWarmup   5: 131072 op, 757990300.00 ns, 5.7830 us/op
WorkloadWarmup   6: 131072 op, 750081300.00 ns, 5.7227 us/op
WorkloadWarmup   7: 131072 op, 757118300.00 ns, 5.7764 us/op
WorkloadWarmup   8: 131072 op, 728642800.00 ns, 5.5591 us/op

// BeforeActualRun
WorkloadActual   1: 131072 op, 733395800.00 ns, 5.5954 us/op
WorkloadActual   2: 131072 op, 730993200.00 ns, 5.5770 us/op
WorkloadActual   3: 131072 op, 742166500.00 ns, 5.6623 us/op
WorkloadActual   4: 131072 op, 754401100.00 ns, 5.7556 us/op
WorkloadActual   5: 131072 op, 752178300.00 ns, 5.7387 us/op
WorkloadActual   6: 131072 op, 726539400.00 ns, 5.5431 us/op
WorkloadActual   7: 131072 op, 734082000.00 ns, 5.6006 us/op
WorkloadActual   8: 131072 op, 723318000.00 ns, 5.5185 us/op
WorkloadActual   9: 131072 op, 721947000.00 ns, 5.5080 us/op
WorkloadActual  10: 131072 op, 789684100.00 ns, 6.0248 us/op
WorkloadActual  11: 131072 op, 745438200.00 ns, 5.6872 us/op
WorkloadActual  12: 131072 op, 736413100.00 ns, 5.6184 us/op
WorkloadActual  13: 131072 op, 747596200.00 ns, 5.7037 us/op
WorkloadActual  14: 131072 op, 773435700.00 ns, 5.9008 us/op
WorkloadActual  15: 131072 op, 736702900.00 ns, 5.6206 us/op
WorkloadActual  16: 131072 op, 758253000.00 ns, 5.7850 us/op
WorkloadActual  17: 131072 op, 752597300.00 ns, 5.7419 us/op

// AfterActualRun
WorkloadResult   1: 131072 op, 733134100.00 ns, 5.5934 us/op
WorkloadResult   2: 131072 op, 730731500.00 ns, 5.5750 us/op
WorkloadResult   3: 131072 op, 741904800.00 ns, 5.6603 us/op
WorkloadResult   4: 131072 op, 754139400.00 ns, 5.7536 us/op
WorkloadResult   5: 131072 op, 751916600.00 ns, 5.7367 us/op
WorkloadResult   6: 131072 op, 726277700.00 ns, 5.5411 us/op
WorkloadResult   7: 131072 op, 733820300.00 ns, 5.5986 us/op
WorkloadResult   8: 131072 op, 723056300.00 ns, 5.5165 us/op
WorkloadResult   9: 131072 op, 721685300.00 ns, 5.5060 us/op
WorkloadResult  10: 131072 op, 745176500.00 ns, 5.6852 us/op
WorkloadResult  11: 131072 op, 736151400.00 ns, 5.6164 us/op
WorkloadResult  12: 131072 op, 747334500.00 ns, 5.7017 us/op
WorkloadResult  13: 131072 op, 773174000.00 ns, 5.8988 us/op
WorkloadResult  14: 131072 op, 736441200.00 ns, 5.6186 us/op
WorkloadResult  15: 131072 op, 757991300.00 ns, 5.7830 us/op
WorkloadResult  16: 131072 op, 752335600.00 ns, 5.7399 us/op
// GC:  0 0 0 400 131072
// Threading:  0 0 131072

// AfterAll
// Benchmark Process 8876 has exited with code 0.

Mean = 5.658 us, StdErr = 0.027 us (0.47%), N = 16, StdDev = 0.107 us
Min = 5.506 us, Q1 = 5.589 us, Median = 5.639 us, Q3 = 5.737 us, Max = 5.899 us
IQR = 0.149 us, LowerFence = 5.366 us, UpperFence = 5.960 us
ConfidenceInterval = [5.548 us; 5.767 us] (CI 99.9%), Margin = 0.109 us (1.93% of Mean)
Skewness = 0.45, Kurtosis = 2.35, MValue = 2

// ** Remained 2 (25.0%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 0m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.ManagedArray_Struct: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1740 1272 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.ManagedArray_Struct --job ".NET 9.0" --benchmarkId 6 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256        
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 136500.00 ns, 136.5000 us/op
WorkloadJitting  1: 1 op, 468900.00 ns, 468.9000 us/op

OverheadJitting  2: 16 op, 149200.00 ns, 9.3250 us/op
WorkloadJitting  2: 16 op, 653900.00 ns, 40.8687 us/op

WorkloadPilot    1: 16 op, 461500.00 ns, 28.8438 us/op
WorkloadPilot    2: 32 op, 739900.00 ns, 23.1219 us/op
WorkloadPilot    3: 64 op, 1356200.00 ns, 21.1906 us/op
WorkloadPilot    4: 128 op, 2484800.00 ns, 19.4125 us/op
WorkloadPilot    5: 256 op, 4107400.00 ns, 16.0445 us/op
WorkloadPilot    6: 512 op, 12836500.00 ns, 25.0713 us/op
WorkloadPilot    7: 1024 op, 17535100.00 ns, 17.1241 us/op
WorkloadPilot    8: 2048 op, 32923200.00 ns, 16.0758 us/op
WorkloadPilot    9: 4096 op, 62462200.00 ns, 15.2496 us/op
WorkloadPilot   10: 8192 op, 115075000.00 ns, 14.0472 us/op
WorkloadPilot   11: 16384 op, 158926300.00 ns, 9.7001 us/op
WorkloadPilot   12: 32768 op, 324396200.00 ns, 9.8998 us/op
WorkloadPilot   13: 65536 op, 646899800.00 ns, 9.8709 us/op

OverheadWarmup   1: 65536 op, 142900.00 ns, 2.1805 ns/op
OverheadWarmup   2: 65536 op, 143200.00 ns, 2.1851 ns/op
OverheadWarmup   3: 65536 op, 146000.00 ns, 2.2278 ns/op
OverheadWarmup   4: 65536 op, 146100.00 ns, 2.2293 ns/op
OverheadWarmup   5: 65536 op, 143000.00 ns, 2.1820 ns/op
OverheadWarmup   6: 65536 op, 144200.00 ns, 2.2003 ns/op
OverheadWarmup   7: 65536 op, 144800.00 ns, 2.2095 ns/op
OverheadWarmup   8: 65536 op, 147300.00 ns, 2.2476 ns/op
OverheadWarmup   9: 65536 op, 141600.00 ns, 2.1606 ns/op

OverheadActual   1: 65536 op, 142900.00 ns, 2.1805 ns/op
OverheadActual   2: 65536 op, 145900.00 ns, 2.2263 ns/op
OverheadActual   3: 65536 op, 142200.00 ns, 2.1698 ns/op
OverheadActual   4: 65536 op, 163100.00 ns, 2.4887 ns/op
OverheadActual   5: 65536 op, 143600.00 ns, 2.1912 ns/op
OverheadActual   6: 65536 op, 142900.00 ns, 2.1805 ns/op
OverheadActual   7: 65536 op, 143400.00 ns, 2.1881 ns/op
OverheadActual   8: 65536 op, 143100.00 ns, 2.1835 ns/op
OverheadActual   9: 65536 op, 156500.00 ns, 2.3880 ns/op
OverheadActual  10: 65536 op, 148400.00 ns, 2.2644 ns/op
OverheadActual  11: 65536 op, 154500.00 ns, 2.3575 ns/op
OverheadActual  12: 65536 op, 147000.00 ns, 2.2430 ns/op
OverheadActual  13: 65536 op, 148400.00 ns, 2.2644 ns/op
OverheadActual  14: 65536 op, 145100.00 ns, 2.2141 ns/op
OverheadActual  15: 65536 op, 147400.00 ns, 2.2491 ns/op

WorkloadWarmup   1: 65536 op, 651298600.00 ns, 9.9380 us/op
WorkloadWarmup   2: 65536 op, 649220600.00 ns, 9.9063 us/op
WorkloadWarmup   3: 65536 op, 648547000.00 ns, 9.8960 us/op
WorkloadWarmup   4: 65536 op, 646007100.00 ns, 9.8573 us/op
WorkloadWarmup   5: 65536 op, 647007600.00 ns, 9.8726 us/op
WorkloadWarmup   6: 65536 op, 656086600.00 ns, 10.0111 us/op
WorkloadWarmup   7: 65536 op, 649051300.00 ns, 9.9037 us/op
WorkloadWarmup   8: 65536 op, 657450100.00 ns, 10.0319 us/op
WorkloadWarmup   9: 65536 op, 645806200.00 ns, 9.8542 us/op

// BeforeActualRun
WorkloadActual   1: 65536 op, 652869100.00 ns, 9.9620 us/op
WorkloadActual   2: 65536 op, 644373500.00 ns, 9.8324 us/op
WorkloadActual   3: 65536 op, 650696500.00 ns, 9.9288 us/op
WorkloadActual   4: 65536 op, 654283200.00 ns, 9.9836 us/op
WorkloadActual   5: 65536 op, 647474400.00 ns, 9.8797 us/op
WorkloadActual   6: 65536 op, 647102200.00 ns, 9.8740 us/op
WorkloadActual   7: 65536 op, 649539100.00 ns, 9.9112 us/op
WorkloadActual   8: 65536 op, 642140300.00 ns, 9.7983 us/op
WorkloadActual   9: 65536 op, 644668600.00 ns, 9.8369 us/op
WorkloadActual  10: 65536 op, 647151900.00 ns, 9.8748 us/op
WorkloadActual  11: 65536 op, 643698300.00 ns, 9.8221 us/op
WorkloadActual  12: 65536 op, 648126800.00 ns, 9.8896 us/op
WorkloadActual  13: 65536 op, 645562100.00 ns, 9.8505 us/op
WorkloadActual  14: 65536 op, 646336500.00 ns, 9.8623 us/op
WorkloadActual  15: 65536 op, 620324200.00 ns, 9.4654 us/op

// AfterActualRun
WorkloadResult   1: 65536 op, 652723200.00 ns, 9.9598 us/op
WorkloadResult   2: 65536 op, 644227600.00 ns, 9.8301 us/op
WorkloadResult   3: 65536 op, 650550600.00 ns, 9.9266 us/op
WorkloadResult   4: 65536 op, 654137300.00 ns, 9.9813 us/op
WorkloadResult   5: 65536 op, 647328500.00 ns, 9.8774 us/op
WorkloadResult   6: 65536 op, 646956300.00 ns, 9.8718 us/op
WorkloadResult   7: 65536 op, 649393200.00 ns, 9.9090 us/op
WorkloadResult   8: 65536 op, 641994400.00 ns, 9.7961 us/op
WorkloadResult   9: 65536 op, 644522700.00 ns, 9.8346 us/op
WorkloadResult  10: 65536 op, 647006000.00 ns, 9.8725 us/op
WorkloadResult  11: 65536 op, 643552400.00 ns, 9.8198 us/op
WorkloadResult  12: 65536 op, 647980900.00 ns, 9.8874 us/op
WorkloadResult  13: 65536 op, 645416200.00 ns, 9.8483 us/op
WorkloadResult  14: 65536 op, 646190600.00 ns, 9.8601 us/op
WorkloadResult  15: 65536 op, 620178300.00 ns, 9.4632 us/op
// GC:  153 0 0 5244452928 65536
// Threading:  0 0 65536

// AfterAll
// Benchmark Process 26092 has exited with code 0.

Mean = 9.849 us, StdErr = 0.031 us (0.31%), N = 15, StdDev = 0.118 us
Min = 9.463 us, Q1 = 9.832 us, Median = 9.872 us, Q3 = 9.898 us, Max = 9.981 us
IQR = 0.066 us, LowerFence = 9.734 us, UpperFence = 9.997 us
ConfidenceInterval = [9.723 us; 9.976 us] (CI 99.9%), Margin = 0.126 us (1.28% of Mean)
Skewness = -2.14, Kurtosis = 7.73, MValue = 2

// ** Remained 1 (12.5%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 0m from now) **      
Setup power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// **************************
// Benchmark: DataTypeBenchmarks.UnmanagedArray_Struct: .NET 9.0(Runtime=.NET 9.0, Server=True)
// *** Execute ***
// Launch: 1 / 1
// Execute: dotnet 4988a665-b4f5-4e10-a3cb-954b97066821.dll --anonymousPipes 1576 1552 --benchmarkName ZiggyAlloc.Benchmarks.DataTypeBenchmarks.UnmanagedArray_Struct --job ".NET 9.0" --benchmarkId 7 in C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks\bin\Release\net9.0\4988a665-b4f5-4e10-a3cb-954b97066821\bin\Release\net9.0
// BeforeAnythingElse

// Benchmark Process Environment Information:
// BenchmarkDotNet v0.13.12
// Runtime=.NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
// GC=Concurrent Server
// HardwareIntrinsics=AVX2,AES,BMI1,BMI2,FMA,LZCNT,PCLMUL,POPCNT,AvxVnni,SERIALIZE VectorSize=256        
// Job: .NET 9.0(Server=True)

OverheadJitting  1: 1 op, 158500.00 ns, 158.5000 us/op
WorkloadJitting  1: 1 op, 1836300.00 ns, 1.8363 ms/op

OverheadJitting  2: 16 op, 127500.00 ns, 7.9688 us/op
WorkloadJitting  2: 16 op, 400600.00 ns, 25.0375 us/op

WorkloadPilot    1: 16 op, 307900.00 ns, 19.2437 us/op
WorkloadPilot    2: 32 op, 579100.00 ns, 18.0969 us/op
WorkloadPilot    3: 64 op, 1108000.00 ns, 17.3125 us/op
WorkloadPilot    4: 128 op, 2259400.00 ns, 17.6516 us/op
WorkloadPilot    5: 256 op, 4507800.00 ns, 17.6086 us/op
WorkloadPilot    6: 512 op, 8855300.00 ns, 17.2955 us/op
WorkloadPilot    7: 1024 op, 17904600.00 ns, 17.4850 us/op
WorkloadPilot    8: 2048 op, 35266300.00 ns, 17.2199 us/op
WorkloadPilot    9: 4096 op, 69528600.00 ns, 16.9748 us/op
WorkloadPilot   10: 8192 op, 53081700.00 ns, 6.4797 us/op
WorkloadPilot   11: 16384 op, 99637400.00 ns, 6.0814 us/op
WorkloadPilot   12: 32768 op, 195996500.00 ns, 5.9813 us/op
WorkloadPilot   13: 65536 op, 397103900.00 ns, 6.0593 us/op
WorkloadPilot   14: 131072 op, 802754300.00 ns, 6.1245 us/op

OverheadWarmup   1: 131072 op, 269500.00 ns, 2.0561 ns/op
OverheadWarmup   2: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadWarmup   3: 131072 op, 261300.00 ns, 1.9936 ns/op
OverheadWarmup   4: 131072 op, 260100.00 ns, 1.9844 ns/op
OverheadWarmup   5: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadWarmup   6: 131072 op, 282300.00 ns, 2.1538 ns/op
OverheadWarmup   7: 131072 op, 259600.00 ns, 1.9806 ns/op

OverheadActual   1: 131072 op, 260000.00 ns, 1.9836 ns/op
OverheadActual   2: 131072 op, 261700.00 ns, 1.9966 ns/op
OverheadActual   3: 131072 op, 259400.00 ns, 1.9791 ns/op
OverheadActual   4: 131072 op, 260500.00 ns, 1.9875 ns/op
OverheadActual   5: 131072 op, 260100.00 ns, 1.9844 ns/op
OverheadActual   6: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadActual   7: 131072 op, 259400.00 ns, 1.9791 ns/op
OverheadActual   8: 131072 op, 263500.00 ns, 2.0103 ns/op
OverheadActual   9: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadActual  10: 131072 op, 260100.00 ns, 1.9844 ns/op
OverheadActual  11: 131072 op, 260100.00 ns, 1.9844 ns/op
OverheadActual  12: 131072 op, 259900.00 ns, 1.9829 ns/op
OverheadActual  13: 131072 op, 259700.00 ns, 1.9814 ns/op
OverheadActual  14: 131072 op, 259500.00 ns, 1.9798 ns/op
OverheadActual  15: 131072 op, 259400.00 ns, 1.9791 ns/op

WorkloadWarmup   1: 131072 op, 782036100.00 ns, 5.9665 us/op
WorkloadWarmup   2: 131072 op, 821276400.00 ns, 6.2658 us/op
WorkloadWarmup   3: 131072 op, 806828800.00 ns, 6.1556 us/op
WorkloadWarmup   4: 131072 op, 774073400.00 ns, 5.9057 us/op
WorkloadWarmup   5: 131072 op, 807181800.00 ns, 6.1583 us/op
WorkloadWarmup   6: 131072 op, 774399900.00 ns, 5.9082 us/op

// BeforeActualRun
WorkloadActual   1: 131072 op, 787913700.00 ns, 6.0113 us/op
WorkloadActual   2: 131072 op, 809554300.00 ns, 6.1764 us/op
WorkloadActual   3: 131072 op, 798182800.00 ns, 6.0897 us/op
WorkloadActual   4: 131072 op, 820027200.00 ns, 6.2563 us/op
WorkloadActual   5: 131072 op, 798116100.00 ns, 6.0891 us/op
WorkloadActual   6: 131072 op, 814038500.00 ns, 6.2106 us/op
WorkloadActual   7: 131072 op, 785891500.00 ns, 5.9959 us/op
WorkloadActual   8: 131072 op, 795168700.00 ns, 6.0667 us/op
WorkloadActual   9: 131072 op, 800155700.00 ns, 6.1047 us/op
WorkloadActual  10: 131072 op, 798168800.00 ns, 6.0895 us/op
WorkloadActual  11: 131072 op, 820962500.00 ns, 6.2634 us/op
WorkloadActual  12: 131072 op, 809780600.00 ns, 6.1781 us/op
WorkloadActual  13: 131072 op, 819623100.00 ns, 6.2532 us/op
WorkloadActual  14: 131072 op, 782888300.00 ns, 5.9730 us/op
WorkloadActual  15: 131072 op, 813387100.00 ns, 6.2057 us/op

// AfterActualRun
WorkloadResult   1: 131072 op, 787653800.00 ns, 6.0093 us/op
WorkloadResult   2: 131072 op, 809294400.00 ns, 6.1744 us/op
WorkloadResult   3: 131072 op, 797922900.00 ns, 6.0877 us/op
WorkloadResult   4: 131072 op, 819767300.00 ns, 6.2543 us/op
WorkloadResult   5: 131072 op, 797856200.00 ns, 6.0872 us/op
WorkloadResult   6: 131072 op, 813778600.00 ns, 6.2086 us/op
WorkloadResult   7: 131072 op, 785631600.00 ns, 5.9939 us/op
WorkloadResult   8: 131072 op, 794908800.00 ns, 6.0647 us/op
WorkloadResult   9: 131072 op, 799895800.00 ns, 6.1027 us/op
WorkloadResult  10: 131072 op, 797908900.00 ns, 6.0876 us/op
WorkloadResult  11: 131072 op, 820702600.00 ns, 6.2615 us/op
WorkloadResult  12: 131072 op, 809520700.00 ns, 6.1762 us/op
WorkloadResult  13: 131072 op, 819363200.00 ns, 6.2512 us/op
WorkloadResult  14: 131072 op, 782628400.00 ns, 5.9710 us/op
WorkloadResult  15: 131072 op, 813127200.00 ns, 6.2037 us/op
// GC:  0 0 0 400 131072
// Threading:  0 0 131072

// AfterAll
// Benchmark Process 25272 has exited with code 0.

Mean = 6.129 us, StdErr = 0.025 us (0.41%), N = 15, StdDev = 0.097 us
Min = 5.971 us, Q1 = 6.076 us, Median = 6.103 us, Q3 = 6.206 us, Max = 6.261 us
IQR = 0.130 us, LowerFence = 5.881 us, UpperFence = 6.402 us
ConfidenceInterval = [6.025 us; 6.233 us] (CI 99.9%), Margin = 0.104 us (1.69% of Mean)
Skewness = -0.1, Kurtosis = 1.54, MValue = 2

// ** Remained 0 (0.0%) benchmark(s) to run. Estimated finish 2025-08-29 17:50 (0h 0m from now) **       
Successfully reverted power plan (GUID: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c FriendlyName: High performance)
// ***** BenchmarkRunner: Finish  *****

// * Export *
  BenchmarkDotNet.Artifacts\results\ZiggyAlloc.Benchmarks.DataTypeBenchmarks-report.csv
  BenchmarkDotNet.Artifacts\results\ZiggyAlloc.Benchmarks.DataTypeBenchmarks-report-github.md
  BenchmarkDotNet.Artifacts\results\ZiggyAlloc.Benchmarks.DataTypeBenchmarks-report.html

// * Detailed results *
DataTypeBenchmarks.ManagedArray_Byte: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 5.852 us, StdErr = 0.008 us (0.13%), N = 15, StdDev = 0.029 us
Min = 5.802 us, Q1 = 5.827 us, Median = 5.854 us, Q3 = 5.877 us, Max = 5.899 us
IQR = 0.050 us, LowerFence = 5.752 us, UpperFence = 5.952 us
ConfidenceInterval = [5.821 us; 5.883 us] (CI 99.9%), Margin = 0.031 us (0.53% of Mean)
Skewness = -0.07, Kurtosis = 1.58, MValue = 2
-------------------- Histogram --------------------
[5.787 us ; 5.914 us) | @@@@@@@@@@@@@@@
---------------------------------------------------

DataTypeBenchmarks.UnmanagedArray_Byte: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 6.009 us, StdErr = 0.010 us (0.17%), N = 14, StdDev = 0.039 us
Min = 5.924 us, Q1 = 5.981 us, Median = 6.013 us, Q3 = 6.029 us, Max = 6.066 us
IQR = 0.048 us, LowerFence = 5.908 us, UpperFence = 6.102 us
ConfidenceInterval = [5.965 us; 6.053 us] (CI 99.9%), Margin = 0.044 us (0.73% of Mean)
Skewness = -0.33, Kurtosis = 2.44, MValue = 2
-------------------- Histogram --------------------
[5.902 us ; 6.088 us) | @@@@@@@@@@@@@@
---------------------------------------------------

DataTypeBenchmarks.ManagedArray_Int: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 5.650 us, StdErr = 0.006 us (0.10%), N = 14, StdDev = 0.021 us
Min = 5.599 us, Q1 = 5.642 us, Median = 5.649 us, Q3 = 5.663 us, Max = 5.685 us
IQR = 0.021 us, LowerFence = 5.612 us, UpperFence = 5.694 us
ConfidenceInterval = [5.626 us; 5.674 us] (CI 99.9%), Margin = 0.024 us (0.42% of Mean)
Skewness = -0.58, Kurtosis = 3.34, MValue = 2
-------------------- Histogram --------------------
[5.588 us ; 5.697 us) | @@@@@@@@@@@@@@
---------------------------------------------------

DataTypeBenchmarks.UnmanagedArray_Int: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 8.706 us, StdErr = 0.005 us (0.06%), N = 15, StdDev = 0.020 us
Min = 8.680 us, Q1 = 8.691 us, Median = 8.700 us, Q3 = 8.719 us, Max = 8.748 us
IQR = 0.028 us, LowerFence = 8.649 us, UpperFence = 8.761 us
ConfidenceInterval = [8.684 us; 8.727 us] (CI 99.9%), Margin = 0.021 us (0.25% of Mean)
Skewness = 0.62, Kurtosis = 2.14, MValue = 2
-------------------- Histogram --------------------
[8.669 us ; 8.759 us) | @@@@@@@@@@@@@@@
---------------------------------------------------

DataTypeBenchmarks.ManagedArray_Double: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 9.399 us, StdErr = 0.020 us (0.21%), N = 15, StdDev = 0.076 us
Min = 9.243 us, Q1 = 9.345 us, Median = 9.410 us, Q3 = 9.442 us, Max = 9.545 us
IQR = 0.098 us, LowerFence = 9.198 us, UpperFence = 9.589 us
ConfidenceInterval = [9.318 us; 9.480 us] (CI 99.9%), Margin = 0.081 us (0.86% of Mean)
Skewness = -0.16, Kurtosis = 2.46, MValue = 2
-------------------- Histogram --------------------
[9.203 us ; 9.586 us) | @@@@@@@@@@@@@@@
---------------------------------------------------

DataTypeBenchmarks.UnmanagedArray_Double: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 5.658 us, StdErr = 0.027 us (0.47%), N = 16, StdDev = 0.107 us
Min = 5.506 us, Q1 = 5.589 us, Median = 5.639 us, Q3 = 5.737 us, Max = 5.899 us
IQR = 0.149 us, LowerFence = 5.366 us, UpperFence = 5.960 us
ConfidenceInterval = [5.548 us; 5.767 us] (CI 99.9%), Margin = 0.109 us (1.93% of Mean)
Skewness = 0.45, Kurtosis = 2.35, MValue = 2
-------------------- Histogram --------------------
[5.450 us ; 5.623 us) | @@@@@@@@
[5.623 us ; 5.764 us) | @@@@@@
[5.764 us ; 5.955 us) | @@
---------------------------------------------------

DataTypeBenchmarks.ManagedArray_Struct: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 9.849 us, StdErr = 0.031 us (0.31%), N = 15, StdDev = 0.118 us
Min = 9.463 us, Q1 = 9.832 us, Median = 9.872 us, Q3 = 9.898 us, Max = 9.981 us
IQR = 0.066 us, LowerFence = 9.734 us, UpperFence = 9.997 us
ConfidenceInterval = [9.723 us; 9.976 us] (CI 99.9%), Margin = 0.126 us (1.28% of Mean)
Skewness = -2.14, Kurtosis = 7.73, MValue = 2
-------------------- Histogram --------------------
[9.400 us ;  9.684 us) | @
[9.684 us ; 10.044 us) | @@@@@@@@@@@@@@
---------------------------------------------------

DataTypeBenchmarks.UnmanagedArray_Struct: .NET 9.0(Runtime=.NET 9.0, Server=True)
Runtime = .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2; GC = Concurrent Server
Mean = 6.129 us, StdErr = 0.025 us (0.41%), N = 15, StdDev = 0.097 us
Min = 5.971 us, Q1 = 6.076 us, Median = 6.103 us, Q3 = 6.206 us, Max = 6.261 us
IQR = 0.130 us, LowerFence = 5.881 us, UpperFence = 6.402 us
ConfidenceInterval = [6.025 us; 6.233 us] (CI 99.9%), Margin = 0.104 us (1.69% of Mean)
Skewness = -0.1, Kurtosis = 1.54, MValue = 2
-------------------- Histogram --------------------
[5.919 us ; 6.117 us) | @@@@@@@@
[6.117 us ; 6.280 us) | @@@@@@@
---------------------------------------------------

// * Summary *

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.4946)
Unknown processor
.NET SDK 9.0.304
  [Host]   : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
  .NET 9.0 : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=.NET 9.0  Runtime=.NET 9.0  Server=True

| Method                | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ManagedArray_Byte     | 5.852 us | 0.0312 us | 0.0292 us |  1.00 |    0.00 | 0.2594 |   10024 B |        1.00 |
| UnmanagedArray_Byte   | 6.009 us | 0.0441 us | 0.0391 us |  1.03 |    0.01 |      - |         - |        0.00 |
| ManagedArray_Int      | 5.650 us | 0.0236 us | 0.0209 us |  0.97 |    0.01 | 1.0529 |   40024 B |        3.99 |
| UnmanagedArray_Int    | 8.706 us | 0.0215 us | 0.0201 us |  1.49 |    0.01 |      - |         - |        0.00 |
| ManagedArray_Double   | 9.399 us | 0.0810 us | 0.0757 us |  1.61 |    0.01 | 2.3041 |   80024 B |        7.98 |
| UnmanagedArray_Double | 5.658 us | 0.1093 us | 0.1074 us |  0.97 |    0.02 |      - |         - |        0.00 |
| ManagedArray_Struct   | 9.849 us | 0.1265 us | 0.1183 us |  1.68 |    0.02 | 2.3346 |   80024 B |        7.98 |
| UnmanagedArray_Struct | 6.129 us | 0.1038 us | 0.0971 us |  1.05 |    0.02 |      - |         - |        0.00 |

// * Hints *
Outliers
  DataTypeBenchmarks.UnmanagedArray_Byte: .NET 9.0   -> 1 outlier  was  removed (6.14 us)
  DataTypeBenchmarks.ManagedArray_Int: .NET 9.0      -> 1 outlier  was  removed, 2 outliers were detected (5.60 us, 5.71 us)
  DataTypeBenchmarks.UnmanagedArray_Double: .NET 9.0 -> 1 outlier  was  removed (6.02 us)
  DataTypeBenchmarks.ManagedArray_Struct: .NET 9.0   -> 1 outlier  was  detected (9.47 us)

// * Legends *
  Mean        : Arithmetic mean of all measurements
  Error       : Half of 99.9% confidence interval
  StdDev      : Standard deviation of all measurements
  Ratio       : Mean of the ratio distribution ([Current]/[Baseline])
  RatioSD     : Standard deviation of the ratio distribution ([Current]/[Baseline])
  Gen0        : GC Generation 0 collects per 1000 operations
  Allocated   : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  Alloc Ratio : Allocated memory ratio distribution ([Current]/[Baseline])
  1 us        : 1 Microsecond (0.000001 sec)

// * Diagnostic Output - MemoryDiagnoser *


// ***** BenchmarkRunner: End *****
Run time: 00:02:36 (156.09 sec), executed benchmarks: 8

Global total time: 00:02:39 (159.75 sec), executed benchmarks: 8
// * Artifacts cleanup *
Artifacts cleanup is finished
Benchmarks completed!
PS C:\Users\alex3\Desktop\coding\ziggyalloc\benchmarks> 