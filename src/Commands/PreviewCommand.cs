using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace iPhotoExtractor.Commands
{
    public class PreviewCommand : ICommand
    {
        private const char TEE = '\u251c';
        private const char ELBOW = '\u2514';
        private const char V_BAR = '\u2502';
        private const char H_BAR = '\u2500';

        public static void Configure(CommandLineApplication command)
        {
            command.HelpOption("-h|--help");
            command.Description = "Display the directory structure that would result from " +
                "running 'iPhotoExtractor extract'.";

            var libPathArgument = command.Argument(
                "library_dir",
                "Path to the iPhoto library directory.");

            var outputDirArgument = command.Argument(
                "[output_dir]",
                "Path to the directory where extracted photos will be copied.");

            command.OnExecute(() =>
            {
                (new PreviewCommand(libPathArgument.Value, outputDirArgument.Value)).Run();
                return 0;
            });
        }

        private readonly string _libraryDir;
        private readonly string _outputDir;

        public PreviewCommand(string libraryDir, string outputDir)
        {
            _libraryDir = libraryDir;
            _outputDir = outputDir;
        }

        public void Run()
        {
            var libraryDir = String.IsNullOrWhiteSpace(_libraryDir) ?
                Directory.GetCurrentDirectory() :
                _libraryDir;

            var outputDir = String.IsNullOrWhiteSpace(_outputDir) ?
                Directory.GetCurrentDirectory() :
                _outputDir;

            var dbPath = Path.Combine(libraryDir, "iPhotoMain.db");

            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"File '{dbPath}' not found.");
                return;
            }

            var photoStore = new PhotoStore(dbPath);
            List<Photo> photos = photoStore.GetAllPhotos();

            Dictionary<string, List<Photo>> albums = photos
                .GroupBy(p => p.AlbumName)
                .ToDictionary(g => g.Key, g => g.ToList());

            Console.WriteLine(_outputDir);
            string album;

            for (int i = 0; i < albums.Keys.Count - 1; i++)
            {
                album = albums.Keys.ElementAt(i);
                DrawAlbumTree(album, albums[album], false);
            }

            album = albums.Keys.Last();
            DrawAlbumTree(album, albums[album], true);
        }

        private void DrawAlbumTree(string albumName, List<Photo> photos, bool isLastChild)
        {
            var modified = photos
                .Where(p => p.HasModifiedVersion() && p.HasOriginalVersion())
                .ToList();

            var hasModified = modified.Any();
            string prefix = isLastChild ? "    " : $"{V_BAR}   ";

            Console.WriteLine($"{GetCurrLevelTreeString(isLastChild)}{albumName}");
            DrawPhotos(photos, prefix, hasModified);

            if (!hasModified)
                return;

            Console.WriteLine($"{prefix}{GetCurrLevelTreeString(true)}Originals");
            DrawPhotos(modified, prefix + "    ", false);
        }

        private void DrawPhotos(List<Photo> photos, string prefix, bool hasModified)
        {
            for (int i = 0; i < photos.Count; i++)
            {
                var photo = photos.ElementAt(i);
                var isLastChild = i == photos.Count - 1 && !hasModified;

                List<string> paths = new List<string>();

                if (hasModified)
                    paths.AddRange(photo.GetUniquePaths(PhotoPathType.Modified));

                if (!paths.Any())
                    paths.AddRange(photo.GetUniquePaths(PhotoPathType.Original));

                if (!paths.Any())
                    paths.Add("");

                for (int j = 0; j < paths.Count; j++)
                {
                    var path = paths.ElementAt(j);

                    if (j < paths.Count - 1)
                        DrawPhoto(path, prefix, false);
                    else
                        DrawPhoto(path, prefix, isLastChild);
                }
            }
        }

        private void DrawPhoto(string relativePath, string prefix, bool isLastChild)
        {
            string fileName = Path.GetFileName(relativePath);
            string currLevel = GetCurrLevelTreeString(isLastChild);

            Console.WriteLine($"{prefix}{currLevel}{fileName} ({relativePath})");
        }

        private string GetCurrLevelTreeString(bool isLastChild)
        {
            if (isLastChild)
                return $"{ELBOW}{H_BAR}{H_BAR} ";
            else
                return $"{TEE}{H_BAR}{H_BAR} ";
        }
    }
}