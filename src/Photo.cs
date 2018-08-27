using System.Collections.Generic;
using System.Linq;

namespace iPhotoExtractor
{
    class Photo
    {
        private readonly List<PhotoPath> _relativePaths;

        public int Id { get; }
        public string Caption { get; set; }
        public string AlbumName { get; set; }
        public IEnumerable<PhotoPath> RelativePaths => _relativePaths;

        public Photo(int id, string caption = null, string albumName = null)
        {
            Id = id;
            Caption = caption;
            AlbumName = albumName;
            _relativePaths = new List<PhotoPath>();
        }

        public void AddPath(string relativePath)
        {
            _relativePaths.Add(new PhotoPath(relativePath));
        }

        public void AddPath(PhotoPath path)
        {
            _relativePaths.Add(path);
        }

        public bool HasOriginalVersion()
        {
            return RelativePaths?.Any(p => p.PathType == PhotoPathType.Original) ?? false;
        }

        public bool HasModifiedVersion()
        {
            return RelativePaths?.Any(p => p.PathType == PhotoPathType.Modified) ?? false;
        }

        public List<string> GetUniquePaths(PhotoPathType pathType)
        {
            return RelativePaths?
                .Where(p => p.PathType == pathType)
                .Select(p => p.RelativePath)
                .Distinct()
                .ToList();
        }
    }
}