using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFViewerMAUI.DataModels
{
    [Serializable]
    public class PDFDocumentDetails
    {
        public string DocumentDetails { get; set; }
        public string Event { get; set; }
        public string Venue { get; set; }
        public string Location { get; set; }
        public DateTime EventDateTime { get; set; } 
        public string PDFFileName { get; set; }
        public string TicketInfo { get { return !String.IsNullOrEmpty(Event) ? Event : DocumentDetails + " on " + EventDateTime.ToString("MMMM dd, yyyy"); } }

        public bool IsEmpty() { return String.IsNullOrEmpty(Event) || EventDateTime == DateTime.MinValue; }
        public bool CandidateForTicket { get; set; }

        public PDFDocumentDetails() 
        {
            EventDateTime = DateTime.MinValue;
        }

        public PDFDocumentDetails(string DocumentDetails, string Location, DateTime DocumentDate, string PDFFileName)
        {
            this.Location = Location;
            this.DocumentDetails = DocumentDetails;
            this.EventDateTime = DocumentDate;
            this.PDFFileName = PDFFileName;
            this.CandidateForTicket = false;
        }

        public PDFDocumentDetails(string EventName, string Venue, string Location, DateTime eventDateTime, string PDFFileName)
        {            
            this.Event = EventName;
            this.Venue = Venue;
            this.Location = Location;
            this.EventDateTime = eventDateTime;
            this.PDFFileName = PDFFileName;            
        }
    }
}
