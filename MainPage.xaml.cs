using Marvel_Scouter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Marvel_Scouter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Careful with its use. 3000 calls per day is the limit

        public ObservableCollection<Character> MarvelCharcters { get; set; }
        public ObservableCollection<Comic_s> MarvelComics { get; set; }
        private List<string> suggestions;

        public MainPage()
        {
            this.InitializeComponent();

            MarvelCharcters = new ObservableCollection<Character>();
            MarvelComics    = new ObservableCollection<Comic_s>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommandDictionary.xml"));
            await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(storageFile);
            try
            {
                MyProgressRing.IsActive = true;
                MyProgressRing.Visibility = Visibility.Visible;

                while(MarvelCharcters.Count < 20)
                {
                    Task t =  MarvelFacade.PopulateMarvelCharactersAsync(MarvelCharcters);
                    await t;
                }

                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;                
            }

            catch(Exception) { }
        }

        private void autoBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {           
            suggestions = MarvelCharcters.Where(p => p.name.StartsWith(sender.Text)).Select(p => p.name).ToList();
            autoBox.ItemsSource = suggestions;
        }

        private async void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                MyProgressRing.IsActive = true;
                MyProgressRing.Visibility = Visibility.Visible;
          
                var selectedCharacter = (Character)e.ClickedItem;

                DetailNameTextBlock.Text = selectedCharacter.name;
                DetailDescriptionTextBlock.Text = selectedCharacter.description;

                var larImage = new BitmapImage();
                Uri uri = new Uri(selectedCharacter.thumbnail.large, UriKind.Absolute);
                larImage.UriSource = uri;
                DetailImage.Source = larImage;

                MarvelComics.Clear(); // Clearing up last data (if any)

                await MarvelFacade.PopulateMarvelComicsAsync(selectedCharacter.id, MarvelComics);

                ComicNameTextBlock.Text = null;
                ComicDescriptionTextBlock.Text = null;
                ComicImage = null;


                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;
            }

            catch (Exception) { }
        }

        private void ComicsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                MyProgressRing.IsActive = true;
                MyProgressRing.Visibility = Visibility.Visible;

                var selectedComic = (Comic_s)e.ClickedItem;

                ComicNameTextBlock.Text = selectedComic.title;
                ComicDescriptionTextBlock.Text = selectedComic.description;

                var larImage = new BitmapImage();
                Uri uri = new Uri(selectedComic.thumbnail.large, UriKind.Absolute);
                larImage.UriSource = uri;

                ComicImage.Source = larImage;

                MarvelComics.Clear(); // Clearing up last data (if any)

               // await MarvelFacade.PopulateMarvelComicsAsync(selectedCharacter.id, MarvelComics);

                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;
            }

            catch (Exception) { }
        }

        public async void refresh()
        {
            try
            {
                MyProgressRing.IsActive = true;
                MyProgressRing.Visibility = Visibility.Visible;

                MarvelCharcters.Clear();
                while (MarvelCharcters.Count < 20)
                {
                    Task t = MarvelFacade.PopulateMarvelCharactersAsync(MarvelCharcters);
                    await t;
                }

                MyProgressRing.IsActive = false;
                MyProgressRing.Visibility = Visibility.Collapsed;
            }

            catch (Exception) { }
        }

        private async void autoBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try { await MarvelFacade.getCharactersByQuery(MarvelCharcters, autoBox.Text); }
            catch (Exception) { }         
        }
    }    
}
