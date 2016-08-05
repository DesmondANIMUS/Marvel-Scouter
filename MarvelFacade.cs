using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Runtime.Serialization.Json;
using Windows.Security.Cryptography.Core;
using Marvel_Scouter.Models;
using System.Collections.ObjectModel;

namespace Marvel_Scouter
{
    public class MarvelFacade
    {
        private const int randHash = 1500;
    

        #region Comic Characters
        public static async Task PopulateMarvelCharactersAsync(ObservableCollection<Character> marvelCharacters)
        {
            var characterDataWrapper = await GetCharacterDataWrapperAsync();
            var characters = characterDataWrapper.data.results;

            foreach (var chars in characters)
            {
                // filtering characters that are missing thumbnail images

                if ((chars.thumbnail != null) && (chars.thumbnail.path != "") && (chars.thumbnail.path != imageNotAvail))
                {
                    chars.thumbnail.small = String.Format("{0}/standard_small.{1}", chars.thumbnail.path, chars.thumbnail.extension);
                    chars.thumbnail.large = String.Format("{0}/standard_xlarge.{1}", chars.thumbnail.path, chars.thumbnail.extension);

                    marvelCharacters.Add(chars);
                }
            }
        }


        internal static async Task getCharactersByQuery(ObservableCollection<Character> characters, string name)
        {
            await PopulateMarvelCharactersAsync(characters);
            var filter = characters.Where(p => p.name == name).ToList();
            characters.Clear();
            filter.ForEach(p => characters.Add(p));
        }


        private static async Task<CharacterDataWrapper> GetCharacterDataWrapperAsync()
        {
            Random random = new Random();
            var offset = random.Next(1500);

            string url = String.Format("http://gateway.marvel.com:80/v1/public/characters?limit=30&offset={0}", offset);

            // Call out to Marvel, then we get a response in a string 

            var jsonMessage = await CallMarvelAsync(url);

            // That string will represent JSON
            // Then we de-serialize that 

            var sereializer = new DataContractJsonSerializer(typeof(CharacterDataWrapper));
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (CharacterDataWrapper)sereializer.ReadObject(memoryStream);

            return result;
        }
        #endregion

        #region Hash and Stuff
        private static string CreateHash(string timeStamp)
        {
            var toBeHashed = timeStamp + privateKey + publicKey;
            var hashedMessage = ComputeMD5(toBeHashed);

            return hashedMessage;
        }

        private static string ComputeMD5(string s)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(s, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);

            return res;
        }
        #endregion

        #region Comics
        private static async Task<ComicDataWrapper> GetComicDataWrapperAsync(int characterId)
        {
            var url = String.Format("http://gateway.marvel.com:80/v1/public/comics?characters={0}&limit=10", characterId);
            var jsonMessage = await CallMarvelAsync(url);

            // That string will represent JSON
            // Then we de-serialize that 

            var sereializer = new DataContractJsonSerializer(typeof(ComicDataWrapper));
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (ComicDataWrapper)sereializer.ReadObject(memoryStream);

            return result;
        }

        #region Call Marvel
        private async static Task<string> CallMarvelAsync(string url)
        {
            // Get MD5 Hash
            var timeStamp = DateTime.Now.Ticks.ToString(); // timeStamp can be any string that can change frm time to time request basis
            var hash = CreateHash(timeStamp);


            string completeUrl = String.Format("{0}&apikey={1}&ts={2}&hash={3}", url, publicKey, timeStamp, hash);

            // Call out to Marvel, then we get a response in a string 

            HttpClient http = new HttpClient();
            var response = await http.GetAsync(completeUrl); // This point we have response in form of HttpClient

            return await response.Content.ReadAsStringAsync();
        }
        #endregion

        public static async Task PopulateMarvelComicsAsync(int characterId, ObservableCollection<Comic_s> marvelComics)
        {
            var comicDataWrapper = await GetComicDataWrapperAsync(characterId);
            var comics = comicDataWrapper.data.results;

            foreach (var comic in comics)
            {
                // filtering characters that are missing thumbnail images

                if ((comic.thumbnail != null) && (comic.thumbnail.path != "") && (comic.thumbnail.path != imageNotAvail))
                {
                    comic.thumbnail.small = String.Format("{0}/portrait_medium.{1}", comic.thumbnail.path, comic.thumbnail.extension);
                    comic.thumbnail.large = String.Format("{0}/portrait_xlarge.{1}", comic.thumbnail.path, comic.thumbnail.extension);

                    marvelComics.Add(comic);
                }
            }
        }

        #endregion
    }
}
