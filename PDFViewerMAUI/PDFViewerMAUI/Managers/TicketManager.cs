using PDFViewerMAUI.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFViewerMAUI.Managers
{
    public class TicketManager
    {
        private List<PDFDocumentDetails> _ticketList;
        private const string MyTickets = "MyTickets.json";
        private string myTicketsDirectory;

        public TicketManager()
        {
            _ticketList = new List<PDFDocumentDetails>();

            myTicketsDirectory = FileSystem.AppDataDirectory;
        }

        public void DeleteTickets()
        {
            _ticketList.Clear();

            if (File.Exists(myTicketsDirectory + "\\" + MyTickets))
            {
                File.Delete(myTicketsDirectory + "\\" + MyTickets);
            }
        }

        public void AddTicket(PDFDocumentDetails ticket)
        {
            _ticketList.Add(ticket);
        }

        public void SaveTicket()
        {
            string tickets = Newtonsoft.Json.JsonConvert.SerializeObject(_ticketList);
            File.WriteAllText(myTicketsDirectory + "\\" + MyTickets, tickets);
        }

        public void LoadTickets()
        {
            if (File.Exists(myTicketsDirectory + "\\" + MyTickets))
            {
                string tickets = File.ReadAllText(FileSystem.AppDataDirectory + "\\" + MyTickets);
                _ticketList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PDFDocumentDetails>>(tickets);
            }
        }

        public List<PDFDocumentDetails> Tickets()
        {
            return _ticketList;
        }
    }
}
