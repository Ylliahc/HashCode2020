using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HashCode2020
{
    class Program
    {
        private static readonly string _bookCountTagName = "bookCount";
        private static readonly string _libraryCountTagName = "libraryCount";
        private static readonly string _scanningDaysCountTagName = "scanningDaysCount";

        private static readonly string _libraryBookCountTagName = "libraryBookCount";
        private static readonly string _libraryBookShippingCountTagName = "libraryBookShippingCount";
        private static readonly string _librarySignupProcessDaysCountTagName = "librarySignupProcessDaysCount";

        private static readonly int descriptionLine = 0;
        private static readonly int bookScoreLine = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var fileNames = new List<string>(){
                "a_example",
                "b_read_on",
                "c_incunabula",
                "d_tough_choices",
                "e_so_many_books",
                "f_libraries_of_the_world"
            };


            var points = 0;
            foreach (var fileName in fileNames)
            {
                points += Process(fileName);
            }

            

            Console.WriteLine($"Points: {points}");
            Console.WriteLine("End");

        }

        private static int Process(string fileName)
        {
            //Read file
            var textLines = File.ReadAllLines($"Contents/{fileName}.txt");
            int firstLine = 2;
            var matches = Regex.Matches(textLines[descriptionLine],
                    $"^(?<{_bookCountTagName}>\\d+) (?<{_libraryCountTagName}>\\d+) (?<{_scanningDaysCountTagName}>\\d+)");
            var bookCount = int.Parse( matches.First().Groups[_bookCountTagName].Value);
            var libraryCount = int.Parse(matches.First().Groups[_libraryCountTagName].Value);
            var scanningDaysCount = int.Parse(matches.First().Groups[_scanningDaysCountTagName].Value);

            var bookScoresArray = textLines[bookScoreLine].Split(" ");
            Dictionary<string,int> bookScores = new Dictionary<string, int>();

            for(int scoreCounter = 0; scoreCounter < bookCount; scoreCounter++)
            {
                bookScores.Add(scoreCounter.ToString(),int.Parse(bookScoresArray[scoreCounter]));
            }

            List<Library> libraries = new List<Library>();

            
            for(int i = firstLine, libraryCounter = 0; i < (libraryCount *2) + 2  ;i+=2, libraryCounter++)
            {
                var libraryMatches = Regex.Matches(textLines[i],
                    $"^(?<{_libraryBookCountTagName}>\\d+) (?<{_librarySignupProcessDaysCountTagName}>\\d+) (?<{_libraryBookShippingCountTagName}>\\d+)");
                
                var bookLine = textLines[i+1].Split(" ").ToList();

                var library = new Library(){
                    ID = libraryCounter,
                    BookCount = int.Parse(libraryMatches.First().Groups[_libraryBookCountTagName].Value),
                    Books = new List<Book>(),
                    SignupDays = int.Parse(libraryMatches.First().Groups[_librarySignupProcessDaysCountTagName].Value),
                    ShippingCount = int.Parse(libraryMatches.First().Groups[_libraryBookShippingCountTagName].Value)
                };

                foreach(var bookId in bookLine)
                {
                    var book = new Book()
                    {
                        Id = int.Parse(bookId),
                        Score = bookScores[bookId]
                    };
                    library.Books.Add(book);
                }

                libraries.Add(library);
            }

            libraries.OrderByDescending(x => x.Books.GroupBy(b => b.Id).Select(b => b.First()).Sum(b => b.Score) / (x.SignupDays + bookCount + x.ShippingCount));

            //Process
            var signupWitness = 0;
            List<Library> finalLibraries = libraries.TakeWhile(x => (signupWitness += x.SignupDays) < scanningDaysCount ).ToList();

            signupWitness = 0;
            foreach (var item in finalLibraries)
            {
                signupWitness += item.SignupDays;
                var deletionIndex = signupWitness + (item.Books.Count/item.ShippingCount);
                item.Books = item.Books.Take(deletionIndex).ToList();
            }


            //Prepare Output
            List<string> Output = new List<string>();

            Output.Add(finalLibraries.Count.ToString());

            foreach (var item in finalLibraries)
            {
                Output.Add($"{item.ID} {item.Books.Count}");
                Output.Add(string.Join(" ",item.Books.Select(b => b.Id).ToList()));
            }

            File.WriteAllLines($"Output/{fileName}.txt", Output);

            //Compute points
            var points = finalLibraries.SelectMany( l => l.Books).ToList().GroupBy(b => b.Id).Select(b => b.First()).Sum(x => x.Score);
            Console.WriteLine($"{fileName}: {points}");
            return points;
        }
    }
}
