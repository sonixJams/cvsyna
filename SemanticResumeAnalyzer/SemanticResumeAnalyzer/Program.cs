using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticResumeAnalyzer
{
    class Program
    {
        private const string pathToResume = "D:\\abc\\Sorin\\cv engleza.doc";
        static void Main(string[] args)
        {
            TextExtractor textExtractor = new TextExtractor();
            string text = textExtractor.GetText(pathToResume);
            Console.WriteLine(text);
        }
    }
}
