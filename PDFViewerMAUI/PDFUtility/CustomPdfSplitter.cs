using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFUtility
{
    public class CustomPdfSplitter : PdfSplitter
    {
        int _partNumber = 1;
        string pathToFileTosplit = "";
        public List<string> FilesCreated = new List<string>();

        public CustomPdfSplitter(PdfDocument pdfDocument, string pathToFileTosplit) : base(pdfDocument)
        {
            this.pathToFileTosplit = pathToFileTosplit;
        }

        protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
        {
            try
            {
                string fileToCreate = pathToFileTosplit + "\\splitDocument_" + _partNumber++ + ".pdf";
                FilesCreated.Add(fileToCreate);
                return new PdfWriter(fileToCreate);
            }
            catch (FileNotFoundException e)
            {
                throw new SystemException();
            }
        }
    }
}
