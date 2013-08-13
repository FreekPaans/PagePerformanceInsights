set Configuration=Release

msbuild PagePerformanceInsights\PagePerformanceInsights.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
msbuild PagePerformanceInsights.MemoryStore\PagePerformanceInsights.MemoryStore.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
msbuild PagePerformanceInsights.SqlServerStore\PagePerformanceInsights.SqlServerStore.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
mkdir binaries\merged
packages\ilmerge.2.13.0307\ilmerge /internalize:exclude_internalize.txt /out:binaries\merged\PagePerformanceInsights.dll binaries\PagePerformanceInsights.dll binaries\PagePerformanceInsights.SqlServerStore.dll binaries\PagePerformanceInsights.MemoryStore.dll
copy binaries\merged\PagePerformanceInsights.dll nuget\lib\40