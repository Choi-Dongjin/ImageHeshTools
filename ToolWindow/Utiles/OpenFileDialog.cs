using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageHeshTools.ToolWindow.Utiles
{
    internal class OpenFileDialog
    {
        /// <summary>
        /// 파일 또는 폴더 선택 대화상자 열기
        /// </summary>
        /// <typeparam name="T"> 선택된 파일 또는 폴더 경로의 타입 </typeparam>
        /// <param name="filter"> 파일 필터 </param>
        /// <param name="folderSelection"> true = 폴더 선택, false = 파일 선택 </param>
        /// <param name="allowMultiSelect"> true = 다중 파일 선택 허용, false = 단일 파일 선택 </param>
        /// <returns> 선택된 파일 또는 폴더 경로 </returns>
        public static T? COpenFileDialog<T>(string[]? filter = null, bool folderSelection = false, bool allowMultiSelect = false)
        {
            T? selectedPath = default;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) // Windows 운영 체제
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    ValidateNames = false,
                    CheckFileExists = false,
                    CheckPathExists = true
                };

                var combinedFilters = new List<string>();

                if (filter is not null && filter.Length > 0)
                {
                    foreach (var item in filter)
                    {
                        var extensions = item.Split(',').Select(ext => ext.Trim()).ToArray();
                        string filterString = string.Join(";", extensions.Select(ext => $"*{ext}"));
                        string displayName = string.Join(", ", extensions.Select(ext => $"*{ext}"));
                        combinedFilters.Add($"{displayName} files ({filterString})|{filterString}");
                    }
                }

                combinedFilters.Add("All files (*.*)|*.*");

                openFileDialog.Filter = string.Join("|", combinedFilters);
                openFileDialog.DefaultExt = filter?.FirstOrDefault()?.TrimStart('.'); // 첫 번째 확장자를 DefaultExt로 설정

                openFileDialog.Multiselect = allowMultiSelect;

                if (folderSelection)
                    openFileDialog.FileName = "Folder Selection.";

                bool? result = openFileDialog.ShowDialog();

                if (result != null && result == true)
                {
                    if (allowMultiSelect)
                        selectedPath = (T?)(object?)openFileDialog.FileNames;
                    else
                        selectedPath = (T?)(object?)(folderSelection ? Path.GetDirectoryName(openFileDialog.FileName) : openFileDialog.FileName);
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix) // Linux 또는 MacOS 운영 체제
            {
                string dialogCommand = folderSelection ? "zenity --file-selection --directory" : "zenity --file-selection";

                if (allowMultiSelect)
                    dialogCommand += " --multiple";

                if (filter is not null && filter.Length > 0)
                {
                    string filterString = string.Join(" ", filter.Select(ext => $"*{ext}"));
                    dialogCommand += $" --file-filter=\"{string.Join(" ", filter)}|{filterString}\"";
                }

                using var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{dialogCommand}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(result))
                {
                    selectedPath = (T?)(object?)result.Trim();
                }
            }

            return selectedPath;
        }

        public static T? CSaveFileDialog<T>(string[]? filter = null)
        {
            T? savedFilePath = default;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) // Windows 운영 체제
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.ValidateNames = true;
                saveFileDialog.CheckPathExists = true;

                if (filter is not null && filter.Length > 0)
                {
                    var combinedFilters = new List<string>();
                    foreach (var item in filter)
                    {
                        if (item.Contains(","))
                        {
                            var extensions = item.Split(',').Select(ext => ext.Trim()).ToArray();
                            string filterString = string.Join(";", extensions.Select(ext => $"*{ext}"));
                            string displayName = string.Join(", ", extensions.Select(ext => $"*{ext}"));
                            combinedFilters.Add($"{displayName} files ({filterString})|{filterString}");
                        }
                        else
                        {
                            string filterString = $"*{item}";
                            combinedFilters.Add($"{item} files ({filterString})|{filterString}");
                        }
                    }

                    combinedFilters.Add("All files (*.*)|*.*");

                    saveFileDialog.Filter = string.Join("|", combinedFilters);
                    saveFileDialog.DefaultExt = filter[0].TrimStart('.'); // 첫 번째 확장자를 DefaultExt로 설정
                }
                else
                {
                    saveFileDialog.Filter = "All files (*.*)|*.*";
                    saveFileDialog.DefaultExt = "*"; // 기본적으로 모든 파일을 선택할 수 있도록 DefaultExt 설정
                }

                bool? result = saveFileDialog.ShowDialog();

                if (result != null && result == true)
                {
                    savedFilePath = (T?)(object?)saveFileDialog.FileName;
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) // Linux 또는 MacOS 운영 체제
            {
                string dialogCommand = "zenity --file-selection --save";

                if (filter is not null && filter.Length > 0)
                {
                    string filterString = string.Join(" ", filter.Select(ext => $"*{ext}"));
                    dialogCommand += $" --file-filter=\"{string.Join(" ", filter)}|{filterString}\"";
                }

                using var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{dialogCommand}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(result))
                {
                    savedFilePath = (T?)(object?)result.Trim();
                }
            }
            else
            {
                // 다른 운영 체제에서의 처리를 추가합니다.
                // 해당 운영 체제에서 지원하는 저장 대화상자 도구를 사용하여 처리합니다.
                // 필요한 코드를 작성해야 합니다.
            }

            return savedFilePath;
        }
    }
}
