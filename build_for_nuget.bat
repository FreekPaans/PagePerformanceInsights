set Configuration=Release

msbuild PagePerformanceInsights\PagePerformanceInsights.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
msbuild PagePerformanceInsights.MemoryStore\PagePerformanceInsights.MemoryStore.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
msbuild PagePerformanceInsights.SqlServerStore\PagePerformanceInsights.SqlServerStore.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
msbuild PagePerformanceInsights.Auth\PagePerformanceInsights.Auth.csproj /p:Configuration=%Configuration%,outputPath=..\binaries /t:Clean,Build
mkdir binaries\merged
packages\ilmerge.2.13.0307\ilmerge /internalize:exclude_internalize.txt /out:binaries\merged\PagePerformanceInsights.dll binaries\PagePerformanceInsights.dll binaries\PagePerformanceInsights.SqlServerStore.dll binaries\PagePerformanceInsights.MemoryStore.dll binaries\RazorGenerator.Templating.dll binaries\PagePerformanceInsights.Auth.dll
mkdir nuget\lib\40
copy binaries\merged\PagePerformanceInsights.dll nuget\lib\40