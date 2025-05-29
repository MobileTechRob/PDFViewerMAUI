
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using iText.Kernel.Pdf;
using iText.Kernel.Utils;


namespace PDFUtility
{
    public class PDFSplitter
    {

        public static void Main(string pathToFileTosplit, out int count, out string directory, out List<string> filesCreated)
        {
            int maxPageCount = 1; // create a new PDF per 2 pages from the original file
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(new FileStream(pathToFileTosplit, FileMode.Open, FileAccess.Read, FileShare.Read)));

            string directoryForSplitPDF = Path.GetDirectoryName(pathToFileTosplit);
            directory = directoryForSplitPDF;

            CustomPdfSplitter customPdfSplitter = new CustomPdfSplitter(pdfDocument, directoryForSplitPDF);
            IList<PdfDocument> splitDocuments = customPdfSplitter.SplitByPageCount(maxPageCount);

            filesCreated = customPdfSplitter.FilesCreated;

            count = splitDocuments.Count;

            foreach (PdfDocument doc in splitDocuments)
            {
                doc.Close();
            }

            pdfDocument.Close();
        }
    }
}
