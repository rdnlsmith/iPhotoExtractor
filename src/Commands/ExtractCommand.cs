using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace iPhotoExtractor.Commands
{
    public class ExtractCommand : ICommand
    {
        public static void Configure(CommandLineApplication command)
        {
            command.HelpOption("-h|--help");
            command.Description = "Copy photos from their original locations to a directory " +
                "structure mirroring the event albums within iPhoto.";

            var libPathArgument = command.Argument(
                "library_dir",
                "Path to the iPhoto library directory.");

            var outputDirArgument = command.Argument(
                "[output_dir]",
                "Path to the directory where the extracted photos will be copied.");

            command.OnExecute(() =>
            {
                (new ExtractCommand(libPathArgument.Value, outputDirArgument.Value)).Run();
                return 0;
            });
        }

        private readonly string _libraryDir;
        private readonly string _outputDir;

        public ExtractCommand(string libraryDir, string outputDir)
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
                Path.Combine(Directory.GetCurrentDirectory(), "Extracted Photos") :
                _outputDir;

            var dbPath = Path.Combine(libraryDir, "iPhotoMain.db");

            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"File '{dbPath}' not found.");
                return;
            }

            if (Directory.Exists(outputDir))
            {
                Console.Write($"Warning: directory '{outputDir}' already exists. Continue (y/n)? ");
                var response = Console.ReadLine().Trim().ToLower();

                if (response != "y" && response != "yes")
                    return;
            }

            Directory.CreateDirectory(outputDir);

            var photoStore = new PhotoStore(dbPath);
            List<Photo> photos = photoStore.GetAllPhotos();

            Dictionary<string, List<Photo>> albums = photos
                .GroupBy(p => p.AlbumName)
                .ToDictionary(g => g.Key, g => g.ToList());

            Console.WriteLine($"Found {albums.Keys.Count} albums.");
            var counter = 1;

            foreach (string album in albums.Keys)
            {
                var albumPhotos = albums[album];
                var s = albumPhotos.Count == 1 ? "" : "s";

                Console.WriteLine($"[{counter}/{albums.Keys.Count}] Extracting album '{album}' " +
                    $"({albumPhotos.Count} photo{s})...");

                ExtractAlbum(libraryDir, outputDir, album, albumPhotos);
                counter++;
            }

            Console.WriteLine("Done.");
        }

        private void ExtractAlbum(
            string libraryDir,
            string outputDir,
            string albumName,
            List<Photo> photos)
        {
            string albumDir = String.IsNullOrWhiteSpace(albumName) ?
                Path.Combine(outputDir, "Untitled Album") :
                Path.Combine(outputDir, albumName);

            if (!Directory.Exists(albumDir))
                Directory.CreateDirectory(albumDir);

            var hasModified = photos.Any(p => p.HasModifiedVersion());
            string originalsDir = Path.Combine(albumDir, "Originals");

            if (hasModified && !Directory.Exists(originalsDir))
                Directory.CreateDirectory(originalsDir);

            foreach (var photo in photos)
            {
                List<string> paths = photo.GetUniquePaths(PhotoPathType.Modified);
                bool isModified = false;

                if (paths.Any())
                {
                    isModified = true;

                    foreach (string path in paths)
                    {
                        CopyPhoto(libraryDir, albumDir, path);
                    }
                }

                paths = photo.GetUniquePaths(PhotoPathType.Original);
                string destDir = isModified ? originalsDir : albumDir;

                foreach (string path in paths)
                {
                    CopyPhoto(libraryDir, destDir, path);
                }
            }
        }

        private void CopyPhoto(string libraryDir, string destDir, string relativePhotoPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(relativePhotoPath);
            string extension = Path.GetExtension(relativePhotoPath);
            string destFileName = $"{fileName}{extension}";
            var suffix = 2;

            while (File.Exists(Path.Combine(destDir, destFileName)))
            {
                destFileName = $"{fileName} ({suffix}){extension}";
                suffix++;
            }

            string sourcePath = Path.Combine(libraryDir, relativePhotoPath);
            string destPath = Path.Combine(destDir, destFileName);

            File.Copy(sourcePath, destPath);
        }
    }
}