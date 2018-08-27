using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace iPhotoExtractor
{
    class PhotoStore
    {
        private readonly string _sqliteConnectionString;

        public PhotoStore(string pathToDbFile)
        {
            _sqliteConnectionString = $"Data Source={pathToDbFile};";
        }

        public List<Photo> GetAllPhotos()
        {
            var photos = new Dictionary<int, Photo>();

            var query =
@"select
    spi.primaryKey,
    spi.caption,
    se.name,
    sfi.relativePath
from SqPhotoInfo spi
left outer join SqEvent se on se.primaryKey = spi.event
left outer join SqFileImage sfim on sfim.photoKey = spi.primaryKey
left outer join SqFileInfo sfi on sfi.primaryKey = sfim.sqFileInfo
where lower(sfi.relativePath) like 'modified%' or lower(sfi.relativePath) like 'originals%'
";

            using (var connection = new SqliteConnection(_sqliteConnectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(query, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt32(reader["primaryKey"]);
                        var photoPath = new PhotoPath(reader["relativePath"].ToString());

                        if (!photos.ContainsKey(id))
                        {
                            var caption = reader["caption"].ToString();
                            var albumName = reader["name"].ToString();
                            var photo = new Photo(id, caption, albumName);

                            photos.Add(id, photo);
                        }

                        photos[id].AddPath(photoPath);
                    }
                }
            }

            return photos.Values.ToList();
        }
    }
}