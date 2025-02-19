﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PogCounter.Models
{
    class Transcript :  ITranscript
    {
        private string fullText;
        private List<string> allWordsList;
        private bool isValidFile;
        private int wordCount;
        private string fileSource;
        private Dictionary<string, int> pogCountDictionary;
        private int pogCount;
        private double pogDensity;
        private double pogPerMinute;

        //metadata
        private string textFileName;
        private string YTLink;
        private int runtimeInMinutes;
        private string vidTitle;
        private DateTime streamDate;

        public Transcript(string fileSource, DataRow rowData) : this(fileSource)
        {
            this.textFileName = (string) rowData["textFile"];
            this.runtimeInMinutes = Convert.ToInt32( rowData["runtimeInMinutes"]);
            this.YTLink = (string) rowData["link"];
            this.vidTitle = (string)rowData["videoName"];
            var dateInTextFile = this.textFileName.Split("[")[1].Split("]")[0];
            var streamYear = Convert.ToInt32(dateInTextFile.Substring(0, 4));
            var streamMonth = Convert.ToInt32(dateInTextFile.Substring(4, 2));
            var streamDate = Convert.ToInt32(dateInTextFile.Substring(6, 2));
            this.streamDate = new DateTime(streamYear, streamMonth, streamDate);
        }
        public Transcript(string fileSource) 
        {
            this.allWordsList = new List<string>();
            this.fullText = "";
            this.wordCount = 0;
            this.fileSource = fileSource;
            this.pogCountDictionary = new Dictionary<string, int>();
            this.pogCount = 0;
            this.pogDensity = 0.0;
            this.pogPerMinute = 0.0;
            this.isValidFile = false;

            this.GetAllText();
            this.SeparateTextIntoWords();
        }


        private void GetAllText() 
        {
            if (File.Exists(this.fileSource))
            {
                this.isValidFile = true;
                var fullText = File.ReadAllText(this.fileSource);

                //clean text & lower case
                fullText = Regex.Replace(fullText, @"[\d-]", " ");
                fullText = Regex.Replace(fullText, @"[\n]", " ");
                fullText = Regex.Replace(fullText, @"[\t]", " ");
                fullText = Regex.Replace(fullText, @"[\r]", " ");
                fullText = fullText.ToLower();
                this.fullText = fullText;
            }
        }

        private void SeparateTextIntoWords() 
        {
            var splitText = this.fullText.Split(Globals.DelimiterCharacters);

            //used to combine instances that are spelled out (i.e., "P", "O", "G")
            var previousWords = "";

            foreach (var word in splitText)
            {
                if (word.Length > 1) //checks non-single letter words
                {
                    if (previousWords.Length > 0) //checks if just finished combining single-letter words
                    {
                        //adds combined single-letter words as a single word
                        this.allWordsList.Add(previousWords);
                    }
                    if (!word.Contains("[")) //checks if not an effect cue (i.e., [Music], [Laughter])
                    {
                        this.allWordsList.Add(word);
                    }

                    previousWords = ""; 
                }
                else if (word.Length > 0) //checks for single-letter word
                {
                    if (word == "p") //fence post in case of repeated "POG" 
                    {
                        if (previousWords.Length > 0)  // checks if "P O G" is repeated
                        {
                            this.allWordsList.Add(previousWords); //counts repetition as separate instances
                            previousWords = "";
                        }

                        previousWords += word;
                    }
                    else
                    {
                        previousWords = previousWords + word;
                    }
                }
                this.wordCount = this.allWordsList.Count;
            }
        }

        public List<string> GetAllWords() { return this.allWordsList; }

        public bool IsFileValid() { return this.isValidFile; }

        public int GetWordCount() { return this.wordCount; }

        public string GetFileSource() { return this.fileSource; }

        public void SetPogDictionary(Dictionary<string, int> keywordDict) 
        {
            this.pogCountDictionary = keywordDict;
            var countPog = 0;
            foreach (var key in this.pogCountDictionary.Keys)
            {
                countPog += this.pogCountDictionary[key];
            }
            this.pogCount = countPog;

            this.pogDensity = (double)this.pogCount / (double)this.wordCount;
            this.pogPerMinute = (double)this.pogCount / (double)this.runtimeInMinutes;
        }

        public int GetPogCount() { return this.pogCount; }

        public double GetPogDensity() { return this.pogDensity; }

        public void PrintDictionary() 
        {

            Console.WriteLine("### **Pog Dictionary**");
            Console.WriteLine("**Pog-like Word** | **Count**|");
            Console.WriteLine(":---: | :---:");
            foreach (var key in this.pogCountDictionary.Keys)
            {
                Console.WriteLine($"{key} | {this.pogCountDictionary[key]}");
            }
        }

        public void PrintMetadata() 
        {
            Console.WriteLine($"## [{this.vidTitle}]({this.YTLink})");

            var youtubeParts = this.YTLink.Split("=");
            var youtubeID = youtubeParts[youtubeParts.Length - 1];
            Console.WriteLine($"[![Link to {this.vidTitle}](https://img.youtube.com/vi/{youtubeID}/0.jpg)]({this.YTLink})");
            Console.WriteLine();
            Console.WriteLine($"**Total Runtime in Minutes**: {this.runtimeInMinutes}");
            Console.WriteLine();
            Console.WriteLine($"### **Pog Stats:**");
            Console.WriteLine();
            Console.WriteLine($"&nbsp;&nbsp;&nbsp;**Total number of words**: {this.wordCount}");
            Console.WriteLine();
            Console.WriteLine($"&nbsp;&nbsp;&nbsp;**Total number of Pog-like words**: {this.pogCount}");
            Console.WriteLine();
            Console.WriteLine($"&nbsp;&nbsp;&nbsp;**Pog Density (Number of Pogs / Total Words)**: {this.pogDensity}");
            Console.WriteLine();
            Console.WriteLine($"&nbsp;&nbsp;&nbsp;**Pog Per Minute (Number of Pogs / Total Runtime in Minutes)**: {this.pogPerMinute}");
            Console.WriteLine();
        }

        public string GetVidTitle() { return this.vidTitle; }
        public string GetTextFileName() { return this.textFileName; }
        public int GetRunTimeMinutes() { return this.runtimeInMinutes; }
        public string GetYTLink() { return this.YTLink; }
        public double GetPogPerMinute() { return this.pogPerMinute; }

        public DateTime GetStreamDate() { return this.streamDate; }
    }
}
