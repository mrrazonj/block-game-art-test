using System;
using System.IO;
using UnityEngine;

namespace ArtTest.Utilities
{
    public class FileIO
    {
        private readonly string filePath = $"{Application.persistentDataPath}/Data/";
        private const string fileExtension = ".dat";

        public void SaveToFile(string fileName, string content)
        {
            try
            {
                if (!Directory.Exists(filePath))
                {
                    ValidateFilePath(filePath);
                }

                fileName += fileExtension;

                if (!File.Exists(filePath + fileName))
                {
                    File.Create(filePath + fileName).Dispose();
                }

                using (StreamWriter writer = new StreamWriter(filePath + fileName, false, System.Text.Encoding.UTF8))
                {
                    writer.Write(content);
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"File IO exception: {e.Message}");
            }
            catch (OperationCanceledException e)
            {
                Debug.Log($"Operation canceled/timeout: {e.Message}");
            }
        }

        public Result<string> LoadFromFile(string fileName)
        {
            Result<string> result = new Result<string>();
            fileName += fileExtension;

            try
            {
                if (!File.Exists(filePath + fileName))
                {
                    var errorMessage = $"File not found: {filePath + fileName}";
                    Debug.LogWarning(errorMessage);
                    result.Resolve(new Error(errorMessage));

                    return result;
                }

                using (StreamReader reader = new StreamReader(filePath + fileName, System.Text.Encoding.UTF8))
                {
                    string content = reader.ReadToEnd();
                    result.Resolve(content);
                }
            }
            catch (IOException e)
            {
                var errorMessage = $"File IO exception: {e.Message}";
                Debug.LogError(errorMessage);
                result.Resolve(new Error(errorMessage));
            }
            catch (OperationCanceledException e)
            {
                var errorMessage = $"Operation canceled/timeout: {e.Message}";
                Debug.Log(errorMessage);
                result.Resolve(new Error(errorMessage));
            }

            return result;
        }

        private void ValidateFilePath(string filePath)
        {
            var splitPath = filePath.Split('/');
            string currentPath = string.Empty;
            for (int i = 0; i < splitPath.Length - 1; i++)
            {
                currentPath += splitPath[i] + '/';
                if (Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                }
            }
        }
    }
}
