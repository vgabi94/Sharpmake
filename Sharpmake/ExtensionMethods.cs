﻿// Copyright (c) 2017 Ubisoft Entertainment
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sharpmake
{
    public static partial class ExtensionMethods
    {
        public static bool IsPC(this Platform platform)
        {
            return platform == Platform.win32 || platform == Platform.win64;
        }

        public static bool IsMicrosoft(this Platform platform)
        {
            return PlatformRegistry.Get<IPlatformDescriptor>(platform).IsMicrosoftPlatform;
        }

        public static bool IsUsingClang(this Platform platform)
        {
            return PlatformRegistry.Get<IPlatformDescriptor>(platform).IsUsingClang;
        }

        public static string ToVersionString(this DotNetFramework framework)
        {
            switch (framework)
            {
                case DotNetFramework.v2:
                    return "2.0";
                case DotNetFramework.v3:
                    return "3.0";
                case DotNetFramework.v3_5:
                    return "3.5";
                case DotNetFramework.v3_5clientprofile:
                    return "3.5";
                case DotNetFramework.v4_0:
                    return "4.0";
                case DotNetFramework.v4_5:
                    return "4.5";
                case DotNetFramework.v4_5_1:
                    return "4.5.1";
                case DotNetFramework.v4_5_2:
                    return "4.5.2";
                case DotNetFramework.v4_5clientprofile:
                    return "4.5";
                case DotNetFramework.v4_6:
                    return "4.6";
                case DotNetFramework.v4_6_1:
                    return "4.6.1";
                case DotNetFramework.v4_6_2:
                    return "4.6.2";
                case DotNetFramework.v4_7:
                    return "4.7";
                case DotNetFramework.v4_7_1:
                    return "4.7.1";
                default:
                    throw new ArgumentOutOfRangeException("framework");
            }
        }

        public static string ToFolderName(this DotNetFramework framework)
        {
            switch (framework)
            {
                case DotNetFramework.v2:
                    return "net20";
                case DotNetFramework.v3_5:
                    return "net35";
                case DotNetFramework.v4_0:
                    return "net40";
                case DotNetFramework.v4_5:
                    return "net45";
                case DotNetFramework.v4_5_1:
                    return "net451";
                case DotNetFramework.v4_5_2:
                    return "net452";
                case DotNetFramework.v4_6:
                    return "net46";
                case DotNetFramework.v4_6_1:
                    return "net461";
                case DotNetFramework.v4_6_2:
                    return "net462";
                case DotNetFramework.v4_7:
                    return "net47";
                case DotNetFramework.v4_7_1:
                    return "net471";
                default:
                    throw new ArgumentOutOfRangeException("framework");
            }
        }

        public static string GetVisualProjectToolsVersionString(this DevEnv visualVersion)
        {
            switch (visualVersion)
            {
                case DevEnv.vs2010:
                    return "4.0";
                case DevEnv.vs2012:
                    return "4.0";
                case DevEnv.vs2013:
                    return "12.0";
                case DevEnv.vs2015:
                    return "14.0";
                case DevEnv.vs2017:
                    return "15.0";
                default:
                    throw new Error("DevEnv " + visualVersion + " not recognized!");
            }
        }

        public static string GetVisualVersionString(this DevEnv visualVersion)
        {
            switch (visualVersion)
            {
                case DevEnv.vs2010:
                    return "10.0";
                case DevEnv.vs2012:
                    return "11.0";
                case DevEnv.vs2013:
                    return "12.0";
                case DevEnv.vs2015:
                    return "14.0";
                case DevEnv.vs2017:
                    return "15.0";
                default:
                    throw new NotImplementedException("DevEnv " + visualVersion + " not recognized!");
            }
        }

        public static string GetDefaultPlatformToolset(this DevEnv visualVersion)
        {
            switch (visualVersion)
            {
                case DevEnv.vs2010:
                    return "v100";
                case DevEnv.vs2012:
                    return "v110";
                case DevEnv.vs2013:
                    return "v120";
                case DevEnv.vs2015:
                    return "v140";
                case DevEnv.vs2017:
                    return "v141";
                default:
                    throw new Error("DevEnv " + visualVersion + " not recognized!");
            }
        }

        public static string GetVSYear(this DevEnv visualVersion)
        {
            switch (visualVersion)
            {
                case DevEnv.vs2010: return "2010";
                case DevEnv.vs2012: return "2012";
                case DevEnv.vs2013: return "2013";
                case DevEnv.vs2015: return "2015";
                case DevEnv.vs2017: return "2017";
                default:
                    throw new Error("DevEnv " + visualVersion + " not recognized!");
            }
        }

        private static readonly ConcurrentDictionary<DevEnv, string> s_visualStudioDirOverrides = new ConcurrentDictionary<DevEnv, string>();
        public static void SetVisualStudioDirOverride(this DevEnv visualVersion, string path)
        {
            bool result = s_visualStudioDirOverrides.TryAdd(visualVersion, path);
            if (!result)
                throw new Error("Can't override a specific Visual Studio version directory more than once. Version: " + visualVersion);
        }

        public static bool OverridenVisualStudioDir(this DevEnv visualVersion)
        {
            return s_visualStudioDirOverrides.ContainsKey(visualVersion);
        }

        public static string GetVisualStudioDir(this DevEnv visualVersion)
        {
            // First check if the visual studio path is overriden from default value.
            string pathOverride;
            if (s_visualStudioDirOverrides.TryGetValue(visualVersion, out pathOverride))
                return pathOverride;

            string registryKeyString = string.Format(
                            @"SOFTWARE{0}\Microsoft\VisualStudio\SxS\VS7",
                            Environment.Is64BitProcess ? @"\Wow6432Node" : string.Empty);

            string fallback = visualVersion == DevEnv.vs2017 ? @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional"
                                                             : @"C:\Program Files (x86)\Microsoft Visual Studio " + visualVersion.GetVisualVersionString();

            string installDir = Util.GetRegistryLocalMachineSubKeyValue(registryKeyString, visualVersion.GetVisualVersionString(), fallback);
            return Util.SimplifyPath(installDir);
        }

        private static readonly ConcurrentDictionary<DevEnv,string> s_visualStudioVCRootPathCache = new ConcurrentDictionary<DevEnv, string>();
        public static string GetVisualStudioVCRootPath(this DevEnv visualVersion)
        {
            string visualStudioVCRootPath = s_visualStudioVCRootPathCache.GetOrAdd(visualVersion, devEnv =>
            {
                string vsDir = visualVersion.GetVisualStudioDir();
                switch (visualVersion)
                {
                    case DevEnv.vs2010:
                    case DevEnv.vs2012:
                    case DevEnv.vs2013:
                    case DevEnv.vs2015:
                        return Path.Combine(vsDir, "VC");

                    case DevEnv.vs2017:
                        string compilerVersion = "14.10.25017"; // default fallback
                        try
                        {
                            string toolchainFile = Path.Combine(vsDir, "VC", "Auxiliary", "Build", "Microsoft.VCToolsVersion.default.txt");
                            if (File.Exists(toolchainFile))
                            {
                                using (StreamReader file = new StreamReader(toolchainFile))
                                    compilerVersion = file.ReadLine().Trim();
                            }
                        }
                        catch { }

                        return Path.Combine(vsDir, @"VC\Tools\MSVC", compilerVersion);
                }
                throw new ArgumentOutOfRangeException("VS version not recognized " + visualVersion);
            });

            return visualStudioVCRootPath;
        }

        public static string GetVisualStudioBinPath(this DevEnv visualVersion, Platform platform)
        {
            switch (visualVersion)
            {
                case DevEnv.vs2010:
                case DevEnv.vs2012:
                case DevEnv.vs2013:
                case DevEnv.vs2015:
                    {
                        string targetPlatform = (platform == Platform.win64) ? "amd64" : "";
                        return Path.Combine(visualVersion.GetVisualStudioVCRootPath(), "bin", targetPlatform);
                    }
                case DevEnv.vs2017:
                    {
                        string targetPlatform = (platform == Platform.win64) ? "x64" : "x86";
                        string compilerHost = Environment.Is64BitOperatingSystem ? "HostX64" : "HostX86";
                        return Path.Combine(visualVersion.GetVisualStudioVCRootPath(), "bin", compilerHost, targetPlatform);
                    }
            }
            throw new ArgumentOutOfRangeException("VS version not recognized " + visualVersion);
        }

        public static bool OverridenWindowsPath(this DevEnv visualVersion)
        {
            return !KitsRootPaths.IsDefaultKitRootPath(visualVersion);
        }

        public static string GetWindowsExecutablePath(this DevEnv visualVersion, Platform platform)
        {
            KitsRootEnum kitsRoot = KitsRootPaths.GetUseKitsRootForDevEnv(visualVersion);

            string targetPlatform = (platform == Platform.win64) ? "x64" : "x86";

            var paths = new Strings();
            paths.Add(visualVersion.GetVisualStudioBinPath(platform));

            switch (kitsRoot)
            {
                case KitsRootEnum.KitsRoot:
                    paths.Add(Path.Combine(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot), "bin", targetPlatform));
                    break;
                case KitsRootEnum.KitsRoot81:
                    paths.Add(Path.Combine(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81), "bin", targetPlatform));
                    break;
                case KitsRootEnum.KitsRoot10:
                    {
                        Options.Vc.General.WindowsTargetPlatformVersion windowsTargetPlatformVersion = KitsRootPaths.GetWindowsTargetPlatformVersionForDevEnv(visualVersion);

                        string kitsRoot10Path = KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot10);
                        string platformVersion = windowsTargetPlatformVersion.ToVersionString();

                        // Use WindowsSdkVerBinPath (the version specific folder), if it exists
                        string candidateWindowsSdkVerBinPath = Path.Combine(kitsRoot10Path, "bin", platformVersion, targetPlatform);
                        if (Util.DirectoryExists(candidateWindowsSdkVerBinPath))
                            paths.Add(candidateWindowsSdkVerBinPath);
                        else
                            paths.Add(Path.Combine(kitsRoot10Path, "bin", targetPlatform));

                        if (windowsTargetPlatformVersion <= Options.Vc.General.WindowsTargetPlatformVersion.v10_0_10240_0)
                            paths.Add(Path.Combine(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81), "bin", targetPlatform));
                    }
                    break;
                default:
                    throw new NotImplementedException("No GetWindowsExecutablePath associated with " + kitsRoot);
            }

            paths.Add("$(PATH)");
            return string.Join(";", paths);
        }

        public static string GetWindowsResourceCompiler(this DevEnv visualVersion, Platform platform)
        {
            KitsRootEnum kitsRoot = KitsRootPaths.GetUseKitsRootForDevEnv(visualVersion);

            string targetPlatform = (platform == Platform.win64) ? "x64" : "x86";

            switch (kitsRoot)
            {
                case KitsRootEnum.KitsRoot:
                    return Path.Combine(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot), "bin", targetPlatform, "rc.exe");
                case KitsRootEnum.KitsRoot81:
                    return Path.Combine(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81), "bin", targetPlatform, "rc.exe");
                case KitsRootEnum.KitsRoot10:
                    {
                        Options.Vc.General.WindowsTargetPlatformVersion windowsTargetPlatformVersion = KitsRootPaths.GetWindowsTargetPlatformVersionForDevEnv(visualVersion);
                        if (windowsTargetPlatformVersion <= Options.Vc.General.WindowsTargetPlatformVersion.v10_0_10240_0)
                        {
                            string kitsRoot81Path = KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81);
                            return Path.Combine(kitsRoot81Path, "bin", targetPlatform, "rc.exe");
                        }

                        string kitsRoot10Path = KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot10);
                        string platformVersion = windowsTargetPlatformVersion.ToVersionString();

                        // First, try WindowsSdkVerBinPath
                        string candidateWindowsSdkVerBinPath = Path.Combine(kitsRoot10Path, "bin", platformVersion, targetPlatform, "rc.exe");
                        if (File.Exists(candidateWindowsSdkVerBinPath))
                            return candidateWindowsSdkVerBinPath;

                        // If it didn't contain rc.exe, fallback to WindowsSdkBinPath
                        return Path.Combine(kitsRoot10Path, "bin", targetPlatform, "rc.exe");
                    }
                default:
                    throw new NotImplementedException("No WindowsResourceCompiler associated with " + kitsRoot);
            }
        }

        public static string GetWindowsIncludePath(this DevEnv visualVersion)
        {
            string visualStudioDir = Util.EnsureTrailingSeparator(visualVersion.GetVisualStudioVCRootPath());
            string visualStudioInclude = string.Format(@"{0}include;{0}atlmfc\include", visualStudioDir);

            if (visualVersion == DevEnv.vs2010)
            {
                return visualStudioInclude;
            }
            else
            {
                KitsRootEnum useKitsRoot = KitsRootPaths.GetUseKitsRootForDevEnv(visualVersion);

                switch (useKitsRoot)
                {
                    case KitsRootEnum.KitsRoot:
                        {
                            string kitsRoot = Util.EnsureTrailingSeparator(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot));
                            return String.Format(@"{0};{1}Include\shared;{1}Include\um;{1}Include\WinRT;", visualStudioInclude, kitsRoot);
                        }
                    case KitsRootEnum.KitsRoot81:
                        {
                            string kitsRoot = Util.EnsureTrailingSeparator(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81));
                            return String.Format(@"{0};{1}Include\shared;{1}Include\um;{1}Include\WinRT;", visualStudioInclude, kitsRoot);
                        }
                    case KitsRootEnum.KitsRoot10:
                        {
                            string kitsRoot10 = Util.EnsureTrailingSeparator(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot10));
                            Options.Vc.General.WindowsTargetPlatformVersion windowsTargetPlatformVersion = KitsRootPaths.GetWindowsTargetPlatformVersionForDevEnv(visualVersion);
                            string platformVersion = windowsTargetPlatformVersion.ToVersionString();
                            var paths = new List<string> {
                                $@"{visualStudioInclude}",
                                $@"{kitsRoot10}Include\{platformVersion}\um",     // $(UM_IncludePath)
                                $@"{kitsRoot10}Include\{platformVersion}\shared", // $(KIT_SHARED_IncludePath)
                                $@"{kitsRoot10}Include\{platformVersion}\winrt",  // $(WinRT_IncludePath)
                                $@"{kitsRoot10}Include\{platformVersion}\ucrt",   // $(UniversalCRT_IncludePath)
                            };

                            if (windowsTargetPlatformVersion <= Options.Vc.General.WindowsTargetPlatformVersion.v10_0_10240_0)
                            {
                                //
                                // Version 10.0.10240.0 and below only contain the UCRT libraries
                                // and headers, not the usual Win32 stuff. So if we are using
                                // version 10240 or older, also include the Windows 8.1 paths so we
                                // have a complete Win32 support.
                                //

                                string kitsRoot81 = Util.EnsureTrailingSeparator(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81));
                                paths.AddRange(new[]
                                {
                                    $@"{kitsRoot81}Include\um",
                                    $@"{kitsRoot81}Include\shared"
                                });
                            }

                            return string.Join(";", paths);

                        }
                    default:
                        throw new NotImplementedException("No WindowsResourceCompiler associated with " + visualVersion);
                }
            }
        }

        public static string GetWindowsLibraryPath(this DevEnv visualVersion, Platform platform, DotNetFramework? dotNetFramework = null)
        {
            string visualStudioVCDir = Util.EnsureTrailingSeparator(visualVersion.GetVisualStudioVCRootPath());
            string subDir = platform == Platform.win64 ? @"\amd64" : "";
            if (visualVersion == DevEnv.vs2017)
                subDir = platform == Platform.win64 ? @"\x64" : @"\x86";

            string visualStudioLib = string.Format(@"{0}lib{1};{0}atlmfc\lib{1};", visualStudioVCDir, subDir);

            if (visualVersion == DevEnv.vs2010)
            {
                return visualStudioLib;
            }
            else
            {
                KitsRootEnum useKitsRoot = KitsRootPaths.GetUseKitsRootForDevEnv(visualVersion);
                string targetPlatform = platform == Platform.win64 ? "x64" : "x86";

                switch (useKitsRoot)
                {
                    case KitsRootEnum.KitsRoot:
                        {
                            string kitsRoot = Util.EnsureTrailingSeparator(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot));
                            return string.Format(@"{0};{1}lib\win8\um\{2};{1}References\CommonConfiguration\Neutral;", visualStudioLib, kitsRoot, targetPlatform);
                        }
                    case KitsRootEnum.KitsRoot81:
                        {
                            string kitsRoot = Util.EnsureTrailingSeparator(KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81));
                            return string.Format(@"{0};{1}lib\winv6.3\um\{2};{1}References\CommonConfiguration\Neutral;", visualStudioLib, kitsRoot, targetPlatform);
                        }
                    case KitsRootEnum.KitsRoot10:
                        {
                            string netFxPath = string.Empty;
                            if (dotNetFramework.HasValue && visualVersion >= DevEnv.vs2015)
                            {
                                string netFXKitsDir = Util.EnsureTrailingSeparator(KitsRootPaths.GetNETFXKitsDir(dotNetFramework.Value < DotNetFramework.v4_6 ? DotNetFramework.v4_6 : dotNetFramework.Value));
                                netFxPath = Path.Combine(netFXKitsDir, "Lib", "um", targetPlatform);
                            }

                            string kitsRoot10 = KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot10);
                            Options.Vc.General.WindowsTargetPlatformVersion windowsTargetPlatformVersion = KitsRootPaths.GetWindowsTargetPlatformVersionForDevEnv(visualVersion);
                            string platformVersion = windowsTargetPlatformVersion.ToVersionString();
                            var paths = new[]
                            {
                                visualStudioLib,
                                Path.Combine(kitsRoot10, "Lib", platformVersion, "ucrt", targetPlatform),   // $(UniversalCRT_LibraryPath_x86) or $(UniversalCRT_LibraryPath_x64)
                                Path.Combine(kitsRoot10, "Lib", platformVersion, "um", targetPlatform),     // $(WindowsSDK_LibraryPath_x86) or $(WindowsSDK_LibraryPath_x64)
                                netFxPath
                            }.ToList();

                            if (windowsTargetPlatformVersion <= Options.Vc.General.WindowsTargetPlatformVersion.v10_0_10240_0)
                            {
                                string kitsRoot81 = KitsRootPaths.GetRoot(KitsRootEnum.KitsRoot81);
                                paths.AddRange(new[] {
                                    Path.Combine(kitsRoot81, "lib", "winv6.3", "um", targetPlatform),
                                    Path.Combine(kitsRoot81, "References", "CommonConfiguration", "Neutral")
                                });
                            }

                            return string.Join(";", paths);
                        }
                    default:
                        throw new NotImplementedException("No WindowsResourceCompiler associated with " + visualVersion);
                }
            }
        }

        public static string GetWindowsAdditionalDependencies(this DevEnv visualVersion)
        {
            return @"kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib";
        }

        public static string GetCommonToolsPath(this DevEnv visualVersion)
        {
            return Path.Combine(GetVisualStudioDir(visualVersion), "Common7\\Tools");
        }

        public static string ToVersionString(this Options.Vc.General.WindowsTargetPlatformVersion windowsTargetPlatformVersion)
        {
            switch (windowsTargetPlatformVersion)
            {
                case Options.Vc.General.WindowsTargetPlatformVersion.v8_1: return "8.1";
                case Options.Vc.General.WindowsTargetPlatformVersion.v10_0_10240_0: return "10.0.10240.0";
                case Options.Vc.General.WindowsTargetPlatformVersion.v10_0_10586_0: return "10.0.10586.0";
                case Options.Vc.General.WindowsTargetPlatformVersion.v10_0_14393_0: return "10.0.14393.0";
                case Options.Vc.General.WindowsTargetPlatformVersion.v10_0_15063_0: return "10.0.15063.0";
                case Options.Vc.General.WindowsTargetPlatformVersion.v10_0_16299_0: return "10.0.16299.0";
                case Options.Vc.General.WindowsTargetPlatformVersion.v10_0_17134_0: return "10.0.17134.0";
                default:
                    throw new ArgumentOutOfRangeException(windowsTargetPlatformVersion.ToString());
            }
        }

        /// <summary>
        /// Gets whether a <see cref="DevEnv"/> is a Visual Studio version.
        /// </summary>
        /// <param name="devEnv">The <see cref="DevEnv"/> to check.</param>
        /// <returns>`true` if <paramref name="devEnv"/> is a Visual Studio version, `false` otherwise.</returns>
        public static bool IsVisualStudio(this DevEnv devEnv)
        {
            return (0 != (devEnv & DevEnv.VisualStudio));
        }

        /// <summary>
        /// Gets whether two <see cref="DevEnv"/> values generate ABI-compatible binaries with
        /// their respective C++ compiler.
        /// </summary>
        /// <param name="devEnv">The <see cref="DevEnv"/> to check for ABI-compatibility.</param>
        /// <param name="other">The other <see cref="DevEnv"/> to check for ABI-compatibility with.</param>
        /// <returns>`true` if ABI-compatible, `false` otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="devEnv"/> is not a Visual Studio version.</exception>
        /// <remarks>
        /// Only works for Visual Studio versions because other DevEnvs (such as Eclipse) are not
        /// shipped with a compiler version.
        /// </remarks>
        public static bool IsAbiCompatibleWith(this DevEnv devEnv, DevEnv other)
        {
            if (!devEnv.IsVisualStudio())
                throw new ArgumentException($"{devEnv} is not a Visual Studio DevEnv.");

            // a VS version is obviously compatible with itself (identity check)
            if (devEnv == other)
                return true;

            // VS2017 is guaranteed by Microsoft to be ABI-compatible with VS2015 for C++.
            if ((devEnv == DevEnv.vs2015 && other == DevEnv.vs2017) || (devEnv == DevEnv.vs2017 && other == DevEnv.vs2015))
                return true;

            return false;
        }
    }
}
