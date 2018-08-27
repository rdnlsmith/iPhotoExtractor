using System.IO;

namespace iPhotoExtractor
{
    enum PhotoPathType
    {
        Original,
        Modified,
        Other
    }

    class PhotoPath
    {
        public PhotoPathType PathType { get; }
        public string RelativePath { get; }
        
        public PhotoPath(string relativePath)
        {
            RelativePath = relativePath;
            string[] parts = relativePath.Split(Path.DirectorySeparatorChar);

            switch(parts[0].ToLower())
            {
                case "originals":
                    PathType = PhotoPathType.Original;
                    break;

                case "modified":
                    PathType = PhotoPathType.Modified;
                    break;

                default:
                    PathType = PhotoPathType.Other;
                    break;
            }
        }
    }
}