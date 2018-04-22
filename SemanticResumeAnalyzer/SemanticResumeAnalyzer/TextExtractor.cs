using System;
using System.IO;
using Xceed.Words.NET;
using Spire.Pdf;
using WordInterop = Microsoft.Office.Interop.Word;
using System.Text;

namespace SemanticResumeAnalyzer
{
    public enum FileType
    {
        None = 0x00,
        Pdf = 0x01,
        Doc = 0x02,
        DocX = 0x04
    }

    public class TextExtractor
    {
        public string FilePath { get; set; }
        public string Text { get; set; }

        public TextExtractor()
        {
            Text = string.Empty;
            FilePath = string.Empty;
        }

        public string GetText(string filePath)
        {
            string text = null;
            string fileExtension = null;
            FileType fileType;
  
            fileExtension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new ArgumentException("The resume file has a invalid extension!");
            }
            
            switch (fileExtension)
            {
                case ".pdf":
                    fileType = FileType.Pdf;
                    text = GetTextFromPdfFile(filePath);
                    break;
                case ".doc":
                    fileType = FileType.Doc;
                    text = GetTextFromDocFile(filePath);
                    break;
                case ".docx":
                    fileType = FileType.DocX;
                    text = GetTextFromDocXFile(filePath);
                    break;
            }

            return text;
        }

        private string GetTextFromPdfFile(string filePath)
        {
            PdfDocument pdfDoc = new PdfDocument();
            pdfDoc.LoadFromFile(filePath);

            string text = string.Empty;
            StringBuilder buff = new StringBuilder();

            foreach (PdfPageBase page in pdfDoc.Pages)
            {
                buff.Append(page.ExtractText());
            }

            text = buff.ToString();
            pdfDoc.Close();

            return text;
        }
        private string GetTextFromDocFile(string filePath)
        {
            WordInterop.Application app = new WordInterop.Application();
            WordInterop.Document doc = app.Documents.Open(filePath);

            string text = string.Empty;

            foreach (WordInterop.Paragraph p in doc.Paragraphs)
            {
                text += p.Range.Text;
            }

            doc.Close();
            app.Quit();

            return text;
        }

        private string GetTextFromDocXFile(string filePath)
        {
            string text = string.Empty;

            using (DocX docx = DocX.Load(filePath))
            {
                foreach (Paragraph p in docx.Paragraphs)
                {
                    text += (string.IsNullOrEmpty(text) ? string.Empty : Environment.NewLine) + p.Text;
                }
            }

            return text;
        }

    }
}
