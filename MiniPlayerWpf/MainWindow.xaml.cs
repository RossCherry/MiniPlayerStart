using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MiniPlayerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MusicRepo musicRepo;
        private readonly MediaPlayer mediaPlayer;
        private readonly ObservableCollection<int> songIds;

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            try
            {
                musicRepo = new MusicRepo();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading file: " + e.Message, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            // Put the ids in an ObservableCollection, which has methods to add and remove items.
            // The UI will update itself automatically if any changes are made to this collection.
            songIds = new ObservableCollection<int>(musicRepo.SongIds);

            // Bind the song IDs to the combo box
            songIdComboBox.ItemsSource = songIds;

            // Select the first item
            if (songIdComboBox.Items.Count > 0)
            {
                songIdComboBox.SelectedIndex = 0;
            }
            else
            {
                artist.Content = "";
                album.Content = "";
                fName.Content = "";
                length.Content = "";
                genre.Content = "";
                songTitle.Content = "";
            }
        }

        private void songIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Display the selected song
            if (songIdComboBox.SelectedItem != null)
            {
                int songId = Convert.ToInt32(songIdComboBox.SelectedItem);
                Song? s = musicRepo.GetSong(songId);
                if (s is not null)
                {
                    songTitle.Content = s.Title;
                    if (s.Filename is not null)
                    {
                        mediaPlayer.Open(new Uri(s.Filename));
                        artist.Content = s.Artist;
                        album.Content = s.Album;
                        fName.Content = s.Filename;
                        length.Content = s.Length;
                        genre.Content = s.Genre;
                    }
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "",
                DefaultExt = "*.wma;*.wav;*mp3;*.m4a",
                Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                // Selected file is openFileDialog.FileName
                Song? s = MusicRepo.GetSongDetails(openFileDialog.FileName);
                int id = musicRepo.AddSong(s);
                musicRepo.Save();
                if (id != -1)
                {
                    ObservableCollection<int> newSongIds = new ObservableCollection<int>(musicRepo.SongIds);

                    // Bind the song IDs to the combo box
                    songIdComboBox.ItemsSource = newSongIds;
                    songIdComboBox.SelectedItem = id;
                }

            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (songIdComboBox.SelectedItem != null)
            {
                int id = (int)songIdComboBox.SelectedItem; 
                musicRepo.DeleteSong(id);
                musicRepo.Save();
                ObservableCollection<int> newSongIds = new ObservableCollection<int>(musicRepo.SongIds);
                // Bind the song IDs to the combo box
                songIdComboBox.ItemsSource = newSongIds;
                songIdComboBox.SelectedItem = id;
                if (songIdComboBox.Items.Count > 0)
                {
                    songIdComboBox.SelectedIndex = 0;
                }
                else
                {
                    artist.Content = "";
                    album.Content = "";
                    fName.Content = "";
                    length.Content = "";
                    genre.Content = "";
                    songTitle.Content = "";
                }
            }
        }
    }
}
