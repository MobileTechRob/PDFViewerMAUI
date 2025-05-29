using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDFViewerMAUI.DataModels;
using iText;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Svg;
using System.Net.Sockets;
using iText.Svg.Converter;
using iText.Kernel.Pdf.Xobject;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout;

namespace PDFViewerMAUI.Managers
{

    public class PDFParser
    {
        public string? basePath;
        public string? origFileName;
        public string? origFileNameNoExt;
        public string? origFileNameOfTicket;
        public string? pdfWriterPath;

        private string ticketInformation;

        iText.Kernel.Pdf.PdfDocument origPdfDocument;
        PdfReader pdfReader = null;
        PdfWriter pdfWriter = null;
        List<string> ticketPaths = null;
        public string[] TicketInformationText { get; set; }
        List<PDFDocumentDetails> pDFDocumentDetails=null;

        public PDFParser()
        {
        }

        public MyPDFDocumentType ExamineDocuments(List<string> fileList, ref string errorMessage)
        {
            pDFDocumentDetails = new List<PDFDocumentDetails>();
            bool entertainmentTicket = false;
            MyPDFDocumentType documentType = MyPDFDocumentType.EntertainmentTicket;

            foreach (string file in fileList)
            {
                PDFDocumentDetails ticket = Parse(file, ref errorMessage);
                pDFDocumentDetails.Add(ticket);

                if (pDFDocumentDetails.Count == 1)
                {
                    if (!String.IsNullOrEmpty(ticket.Event))
                        entertainmentTicket = true;

                    if (pDFDocumentDetails[0].CandidateForTicket == false)
                        documentType = MyPDFDocumentType.GeneralDocument;
                }                
                else if (pDFDocumentDetails.Count <= 5)
                {
                    if ((entertainmentTicket == false) && String.IsNullOrEmpty(ticket.Event))                   
                        documentType = MyPDFDocumentType.GeneralDocument;                    
                }
                else if (documentType == MyPDFDocumentType.GeneralDocument)
                {
                    break;
                }
            }

            return documentType;
        }

        public List<PDFDocumentDetails> DocumentList()
        {
            return pDFDocumentDetails;
        }

        public PDFParser(string ticketPath)
        {
            origFileNameOfTicket = ticketPath;
            pdfReader = new PdfReader(ticketPath);
            
            origPdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
            
            basePath = Path.GetDirectoryName(ticketPath);
            origFileName = Path.GetFileName(ticketPath);

            origFileNameNoExt = origFileName;

            if (origFileName.EndsWith(".pdf"))
                origFileNameNoExt = origFileName.Remove(origFileName.Length - 4);

            ticketPaths = new List<string>();
        }

        public PDFDocumentDetails Parse(string ticketPath, ref string errorMessage)
        {
            origFileNameOfTicket = ticketPath;
            pdfReader = new PdfReader(ticketPath);

            origPdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);

            basePath = Path.GetDirectoryName(ticketPath);
            origFileName = Path.GetFileName(ticketPath);

            origFileNameNoExt = origFileName;

            if (origFileName.EndsWith(".pdf"))
                origFileNameNoExt = origFileName.Remove(origFileName.Length - 4);

            PDFDocumentDetails ticket = ExtractTicketInfo(ref errorMessage);
            ticket.PDFFileName = ticketPath;

            return ticket;
        }

        public PDFDocumentDetails ExtractTicketInfo(ref string errorMessage)
        {           
            try
            {
                ticketInformation = PdfTextExtractor.GetTextFromPage(origPdfDocument.GetFirstPage());
            }
            catch (Exception ex)
            {
                errorMessage = "ExtractTicketInfo (PdfTextExtractor.GetTextFromPage) exception : " + ex.Message;
            }

            return ConvertTextToTicketObject(ticketInformation, ref errorMessage);
        }

        public string ExtractedTextAsIs()
        {
            return ticketInformation.Replace("#","").Replace("&","and");
        }

        public PDFDocumentDetails ConvertTextToTicketObject(string extractedText, ref string errorMessage)
        {
            string[] Keywords = { "EVENT", "VENUE", "PATRON", "DATE", "TIME" };
            List<LabelAndPosition> labelAndPositions = new List<LabelAndPosition>();
            string newExtractedText = extractedText;

            foreach (string keyword in Keywords)
            {
                labelAndPositions.Add(new LabelAndPosition() { LabelText = keyword, Position = -1, Length = -1 });
            }

            int index = -1;

            foreach (string keyword in Keywords)
            {
                index = extractedText.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);

                if (labelAndPositions.Find(x => x.LabelText == keyword) != null)
                    labelAndPositions.Find(x => x.LabelText == keyword).Position = index;
            }

            int count = labelAndPositions.Count;
            int currentIndex = 0;

            PDFDocumentDetails ticket = null;

            try
            {
                bool KeepRemoving = true;
                bool itemRemoved = false;

                while (KeepRemoving == true)
                {
                    while (currentIndex < labelAndPositions.Count)
                    {
                        if (labelAndPositions[currentIndex].Position == -1)
                        {
                            labelAndPositions.RemoveAt(currentIndex);
                            itemRemoved = true;
                            break;
                        }
                        currentIndex++;
                    }

                    if (itemRemoved == true)
                    {
                        count = labelAndPositions.Count;
                        itemRemoved = false;
                    }
                    else
                        break;
                }

                count = labelAndPositions.Count - 1;
                string labelAndText = "";

                while (count >= 0)
                {
                    labelAndText = newExtractedText.Substring(labelAndPositions[count].Position);

                    labelAndPositions[count].CompleteText = labelAndText;

                    newExtractedText = newExtractedText.Substring(0, labelAndPositions[count].Position);

                    count--;
                }

                ticket = new PDFDocumentDetails();

                foreach (var item in labelAndPositions)
                {
                    if (item.LabelText == "EVENT")
                    {
                        item.CompleteText = item.CompleteText.Replace("EVENT", "").Trim().Trim('\n');
                        ticket.Event = item.CompleteText;
                        ticket.CandidateForTicket = true;
                    }
                    else if (item.LabelText == "VENUE")
                    {
                        item.CompleteText = item.CompleteText.Replace("VENUE", "").Trim().Trim('\n');
                        ticket.Venue = item.CompleteText;
                    }
                    else if (item.LabelText == "PATRON")
                    {
                        item.CompleteText = item.CompleteText.Replace("PATRON", "").Trim().Trim('\n');
                    }
                    else if (item.LabelText == "DATE")
                    {
                        DateTime dateTime;
                        item.CompleteText = item.CompleteText.Replace("DATE", "").Trim().Trim('\n');
                        if (ProcessDateTime(item.CompleteText, out dateTime))
                            ticket.EventDateTime = dateTime;
                    }
                    else if (item.LabelText == "TIME")
                    {
                        DateTime dateTime;
                        item.CompleteText = item.CompleteText.Replace("TIME", "").Trim().Trim('\n');

                        index = item.CompleteText.IndexOf("\n");

                        if (index > -1)
                            item.CompleteText = item.CompleteText.Substring(0, index);

                        if (ProcessDateTime(item.CompleteText, out dateTime))
                            ticket.EventDateTime = dateTime;
                    }

                    Console.WriteLine($"{item.LabelText}={item.CompleteText}");
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (ticket.IsEmpty())
            {
                ticket.Event = "";
                ticket.CandidateForTicket = false;

                //    if (extractedText.Contains("General Admission") || extractedText.Contains("General Adm") || extractedText.Contains("VIP"))
                //    {
                //        ticket.CandidateForTicket = true;
                //    }
            }

            return ticket;
        }

        bool ProcessDateTime(string dataTimeText, out DateTime dateTime)
        {
            string[] Months = {"January","Jan",
                        "February","Feb",
                        "March","Mar",
                        "April","Apr",
                        "May","May",
                        "June","Jun",
                        "July","Jul",
                        "August","Aug",
                        "September","Sept","Sep",
                        "October","Oct",
                        "November","Nov",
                        "December","Dec" };

            string[] dateTimeParts = dataTimeText.Split(new char[] { ' ', ',' });

            if ((dateTimeParts.Length == 1) && dateTimeParts[0].Length < 3)
            {
                dateTime = DateTime.MinValue;
                return false;
            }

            string dateTimeSegment = "";

            Array daysOfWeek = Enum.GetValues(typeof(DayOfWeek));
            DateTime timeCandidate;


            foreach (var dateTimePart in dateTimeParts)
            {
                foreach (var dayOfWeek in daysOfWeek)
                {
                    if (String.Equals(dateTimePart, dayOfWeek.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        dateTimeSegment += " " + dateTimePart;
                }

                foreach (var month in Months)
                {
                    if (String.Equals(dateTimePart, month, StringComparison.CurrentCultureIgnoreCase))
                        dateTimeSegment += " " + month;
                }

                if (IsThisJustaTime(dateTimePart))
                    dateTimeSegment += " " + dateTimePart;

                string dayOrYearCandidate = dateTimePart.Replace("th", "").Replace("nd", "");
                int day = 0;

                if (Int32.TryParse(dayOrYearCandidate, out day))
                    dateTimeSegment += " " + day.ToString();
            }

            dateTime = DateTime.Parse(dateTimeSegment);
            return true;
        }

        public bool IsThisJustaTime(string dateTimePart)
        {
            return dateTimePart.Contains(":") && (dateTimePart.Contains("AM") || dateTimePart.Contains("PM")) && (dateTimePart.Length <= 7);
        }

    }
}
