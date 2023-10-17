﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace MiniPlayerWpf
{
    public class MusicRepo
    {
        private readonly DataSet musicDataSet;

        public const string XML_MUSICFILE = "music.xml";
        public const string XSD_MUSICFILE = "music.xsd";

        /// <summary>
        /// The list of all song IDs in sorted order
        /// </summary>
        public IEnumerable<int> SongIds
        {
            get
            {
                return from row in musicDataSet?.Tables["song"]?.AsEnumerable()
                          orderby row["id"]
                          select Convert.ToInt32(row["id"]);
            }
        }

        public MusicRepo()
        {
            musicDataSet = new DataSet();
            musicDataSet.ReadXmlSchema(XSD_MUSICFILE);
            musicDataSet.ReadXml(XML_MUSICFILE);
        }

        /// <summary>
        /// Adds a song to the music library and returns the song's ID. The Song's ID
        /// is also updated to reflect the song's auto-assigned ID.
        /// </summary>
        /// <param name="s">Song to add</param>
        /// <returns>The song's ID</returns>
        public int AddSong(Song s)
        {
            DataTable? table = musicDataSet.Tables["song"];
            if (table != null)
            {
                DataRow row = table.NewRow();

                row["title"] = s.Title;
                row["artist"] = s.Artist;
                row["album"] = s.Album;
                row["filename"] = s.Filename;
                row["length"] = s.Length;
                row["genre"] = s.Genre;
                table.Rows.Add(row);

                // Update this song's ID
                s.Id = Convert.ToInt32(row["id"]);

                return s.Id;
            }

            return -1;
        }

        /// <summary>
        /// Return a Song for the given song ID. Returns null if the song was not found.
        /// </summary>
        /// <param name="songId">ID of song to search for</param>
        /// <returns>The song matching the songId or null if the song wasn't found</returns>
        public Song? GetSong(int songId)
        {
            DataTable? table = musicDataSet?.Tables["song"];
            if (table != null)
            {
                // Only one row should be selected
                foreach (DataRow row in table.Select("id=" + songId))
                {
                    Song song = new()
                    {
                        Id = songId,
                        Title = row["title"].ToString(),
                        Artist = row["artist"].ToString(),
                        Album = row["album"].ToString(),
                        Genre = row["genre"].ToString(),
                        Length = row["length"].ToString(),
                        Filename = row["filename"].ToString()
                    };

                    return song;
                }
            }

            // Must not have found this song ID
            return null;
        }

        /// <summary>
        /// Reads song metadata from the given filename. Uses a third-party library
        /// called TagLib# https://github.com/mono/taglib-sharp
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Song from given filename or null if an error occured reading the file</returns>
        public static Song? GetSongDetails(string filename)
        {
            Song? s = null;
            try
            {
                // PM> Install-Package TagLibSharp
                TagLib.File file = TagLib.File.Create(filename);

                s = new Song
                {
                    Title = file.Tag.Title,
                    Artist = file.Tag.AlbumArtists.Length > 0 ? file.Tag.AlbumArtists[0] : "",
                    Album = file.Tag.Album,
                    Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "",
                    Length = file.Properties.Duration.Minutes + ":" + file.Properties.Duration.Seconds,
                    Filename = filename
                };

                return s;
            }
            catch (Exception)
            {
                // Problem reading file
            }

            return s;
        }

        /// <summary>
        /// Update the given song with the given song ID. Returns true if the song was 
        /// updated, false if it could not because the song ID was not found.
        /// </summary>
        /// <param name="songId">Song ID to search for</param>
        /// <param name="song">Song data that has possibly changed</param>
        /// <returns>true if the song with the song ID was found, false otherwise</returns>
        public bool UpdateSong(int songId, Song song)
        {
            DataTable? table = musicDataSet.Tables["song"];
            if (table != null)
            { 
                // Only one row should be selected
                foreach (DataRow row in table.Select("id=" + songId))
                {
                    row["title"] = song.Title;
                    row["artist"] = song.Artist;
                    row["album"] = song.Album;
                    row["genre"] = song.Genre;
                    row["length"] = song.Length;
                    row["filename"] = song.Filename;

                    return true;
                }
            }

            // Must not have found the song ID
            return false;
        }

        /// <summary>
        /// Delete a song given the song's ID. Return true if the song was
        /// successfully deleted, false otherwise.
        /// </summary>
        /// <param name="songId">Song ID to delete</param>
        /// <returns>true if the song ID was found and deleted, false otherwise</returns>
        public bool DeleteSong(int songId)
        {
            // Search the primary key for the selected song and delete it from 
            // the song table
            DataTable? table = musicDataSet.Tables["song"];
            if (table != null)
            {
                DataRow? songRow = table.Rows.Find(songId);
                if (songRow == null)
                {
                    return false;
                }

                table.Rows.Remove(songRow);
            }

            // Remove from playlist_song every occurance of songId            
            table = musicDataSet.Tables["playlist_song"];
            if (table != null)
            {
                // Add rows to a separate list before deleting because we'll get an exception
                // if we try to delete more than one row while looping through table.Rows
                List<DataRow> rows = new();
                foreach (DataRow row in table.Rows)
                {
                    if (Convert.ToInt32(row["song_id"]) == songId)
                    {
                        rows.Add(row);
                    }
                }

                foreach (DataRow row in rows)
                {
                    row.Delete();
                }
            }

            return true;
        }

        /// <summary>
        /// Save the song database to file.
        /// </summary>
        public void Save()
        {
            // Save music.xml 
            Console.WriteLine("Saving " + XML_MUSICFILE);
            musicDataSet.WriteXml(XML_MUSICFILE);
        }

        /// <summary>
        /// Debug information displaying all the table data in the console.
        /// </summary>
        public void PrintAllTables()
        {
            foreach (DataTable table in musicDataSet.Tables)
            {
                Console.WriteLine("Table name = " + table.TableName);
                foreach (DataRow row in table.Rows)
                {
                    Console.WriteLine("Row:");
                    int i = 0;
                    foreach (object? item in row.ItemArray)
                    {
                        Console.WriteLine(" " + table.Columns[i].Caption + "=" + item);
                        i++;
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
