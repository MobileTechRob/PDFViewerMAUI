using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFViewerMAUI.Utility
{
    class CustomPdfSplitter : PdfSplitter
    {
        int _partNumber = 1;
        string pathToFileTosplit = "";
        string fileNameNoExtension = "";
        public List<string> FilesCreated = new List<string>();

        public CustomPdfSplitter(PdfDocument pdfDocument, string pathToFileTosplit, string fileNameNoExtension) : base(pdfDocument)
        {
            this.pathToFileTosplit = pathToFileTosplit;
            this.fileNameNoExtension = fileNameNoExtension;
        }

        protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
        {
            try
            {
                string fileToCreate = pathToFileTosplit + $"\\{fileNameNoExtension}_Page_" + _partNumber++ + ".pdf";
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
