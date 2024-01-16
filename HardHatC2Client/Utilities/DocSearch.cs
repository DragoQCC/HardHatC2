using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using MudBlazor.Interfaces;

namespace HardHatCore.HardHatC2Client.Utilities
{
    public class DocSearchUtil : INotifyPropertyChanged
    {
        private string _globalSearch;
        public string GlobalSearch
        {
            get => _globalSearch;
            set => SetField(ref _globalSearch,value, "GlobalSearch");
        }

        private static string DocFolderPath = HelperFunctions.GetBaseFolderLocation() + HelperFunctions.PathingTraverseUpString + "Docs" + HelperFunctions.PlatPathSeperator;
        //key is the file name, value is the line of text that contains the search term
        public Dictionary<string,string> searchResults = new Dictionary<string,string>();

        public event PropertyChangedEventHandler? PropertyChanged;
        public EventHandler? SearchValueChanged;

        public DocSearchUtil()
        {
            PropertyChanged += (async (sender,args)  => await HandleSearchChange(sender,args));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        public async Task HandleSearchChange(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "GlobalSearch")
            {
                if(String.IsNullOrWhiteSpace(GlobalSearch))
                {
                    searchResults.Clear();
                    return;
                }
                Console.WriteLine($"checking path {DocFolderPath}");
                // for each file in the docs folder that contains the search term, add it to the search results, and add the line of text that contains the search term
                var foundDocs = Directory.GetFiles(DocFolderPath);
                Console.WriteLine($"found {foundDocs.Length} document files");
                foreach (string file in foundDocs)
                {
                    Console.WriteLine($"checking file {file.Split(HelperFunctions.PlatPathSeperator).Last()} for {GlobalSearch}");
                    if ((await File.ReadAllTextAsync(file)).Contains(GlobalSearch,StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine($"found {GlobalSearch} in {file.Split(HelperFunctions.PlatPathSeperator).Last()}");
                        searchResults.TryAdd(file, File.ReadLines(file).First(line => line.Contains(GlobalSearch, StringComparison.CurrentCultureIgnoreCase)));
                    }
                }
                SearchValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
    }
}
